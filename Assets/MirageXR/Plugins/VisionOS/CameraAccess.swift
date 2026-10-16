import ARKit
import AVFoundation
import Foundation
import SwiftUI
import UnityFramework
import RealityKit
import MetalKit
import Accelerate

// MARK: - Shared state
//
// All mutable state below is accessed both from the Unity main thread (via the
// @_cdecl entry points) and from the Swift concurrency thread pool (capture task).
// Every access MUST be guarded by `stateLock`.

private let stateLock = NSLock()
private let mtlDevice: MTLDevice = MTLCreateSystemDefaultDevice()!
private var textureCache: CVMetalTextureCache? = nil
private var commandQueue: MTLCommandQueue? = nil

/// Capture states reported to Unity via `getCaptureState()`.
private enum CaptureState: Int32 {
    case idle = 0
    case starting = 1
    case running = 2
    case failed = 3
}

private var captureState: CaptureState = .idle
private var arKitSession: ARKitSession? = nil
private var captureTask: Task<Void, Never>? = nil
private var eventsTask: Task<Void, Never>? = nil
/// Incremented on every start/stop. A capture task only publishes frames while
/// its generation is still the current one, so stale tasks can never overwrite state.
private var sessionGeneration: Int = 0
private var authorizationGranted: Bool = false

/// Ring buffer of textures. The writer never writes into the texture that is
/// currently published (`latestIndex`) or currently acquired by Unity (`lockedIndex`).
private let ringSize = 3
private var ringTextures: [MTLTexture?] = [nil, nil, nil]
private var latestIndex: Int = -1
private var lockedIndex: Int = -1
private var currentFrameId: Int64 = 0
private var textureWidth: Int = 1920
private var textureHeight: Int = 1080

public typealias NativePermissionCallback = @convention(c) (Bool) -> Void

// MARK: - Permissions

@_cdecl("VisionOS_IsCameraPermissionGranted")
public func visionOS_IsCameraPermissionGranted() -> Bool {
    let semaphore = DispatchSemaphore(value: 0)
    var granted = false
    Task {
        let session = ARKitSession()
        let status = await session.queryAuthorization(for: [.cameraAccess])
        granted = (status[.cameraAccess] == .allowed)
        semaphore.signal()
    }
    _ = semaphore.wait(timeout: .now() + 1.0)
    if granted {
        stateLock.lock()
        authorizationGranted = true
        stateLock.unlock()
    }
    return granted
}

@_cdecl("VisionOS_RequestCameraPermission")
public func visionOS_RequestCameraPermission(_ callback: @escaping NativePermissionCallback) {
    Task {
        let session = ARKitSession()
        let status = await session.requestAuthorization(for: [.cameraAccess])
        let granted = (status[.cameraAccess] == .allowed)
        stateLock.lock()
        authorizationGranted = granted
        stateLock.unlock()
        callback(granted)
    }
}

// MARK: - Session lifecycle

/// Creates the ARKitSession object if needed. Does not start any data provider.
@_cdecl("prepareCapture")
public func prepareCapture() {
    stateLock.lock()
    if arKitSession == nil {
        arKitSession = ARKitSession()
    }
    stateLock.unlock()
}

/// Starts streaming camera frames. Idempotent: if a stream is already starting or
/// running, this call does nothing. If a previous stream is still shutting down,
/// the new stream waits for it to finish before running a new CameraFrameProvider.
@_cdecl("startCapture")
public func startCapture() {
    stateLock.lock()
    if captureState == .starting || captureState == .running {
        stateLock.unlock()
        return
    }

    if arKitSession == nil {
        arKitSession = ARKitSession()
    }
    let session = arKitSession!
    sessionGeneration += 1
    let generation = sessionGeneration
    let previousTask = captureTask
    let alreadyAuthorized = authorizationGranted
    captureState = .starting
    stateLock.unlock()

    print("[CameraAccess] Starting capture (generation \(generation))")

    eventsTask = Task {
        for await event in session.events {
            switch event {
            case .authorizationChanged(let type, let status):
                print("[CameraAccess] Authorization changed: \(type) -> \(status)")
                if type == .cameraAccess && status != .allowed {
                    setFailed(generation: generation, reason: "camera access revoked")
                }
            case .dataProviderStateChanged(_, let newState, let error):
                print("[CameraAccess] Data provider state: \(newState) error: \(String(describing: error))")
                if error != nil {
                    setFailed(generation: generation, reason: "data provider error")
                }
            default:
                break
            }
        }
    }

    let task = Task {
        // Make sure the previous provider/session is completely shut down before
        // running a new CameraFrameProvider (only one can be active at a time).
        if let previousTask = previousTask {
            await previousTask.value
        }
        guard isCurrent(generation), !Task.isCancelled else { return }

        if !alreadyAuthorized {
            let authResult = await session.requestAuthorization(for: [.cameraAccess])
            guard authResult[.cameraAccess] == .allowed else {
                print("[CameraAccess] Camera access not allowed: \(authResult)")
                setFailed(generation: generation, reason: "not authorized")
                return
            }
            stateLock.lock()
            authorizationGranted = true
            stateLock.unlock()
        }

        let formats = CameraVideoFormat.supportedVideoFormats(for: .main, cameraPositions: [.left])
        guard let videoFormat = formats.first else {
            print("[CameraAccess] No supported video format found")
            setFailed(generation: generation, reason: "no video format")
            return
        }

        // Running a new provider immediately after a previous one stopped can fail
        // transiently, so retry a few times.
        var cameraFrameProvider: CameraFrameProvider? = nil
        for attempt in 1...3 {
            guard isCurrent(generation), !Task.isCancelled else { return }
            let provider = CameraFrameProvider()
            do {
                try await session.run([provider])
                cameraFrameProvider = provider
                break
            } catch {
                print("[CameraAccess] ARKitSession.run failed (attempt \(attempt)): \(error)")
                try? await Task.sleep(nanoseconds: 300_000_000)
            }
        }

        guard let provider = cameraFrameProvider else {
            setFailed(generation: generation, reason: "session run failed")
            return
        }

        guard let frameUpdates = provider.cameraFrameUpdates(for: videoFormat) else {
            print("[CameraAccess] cameraFrameUpdates returned nil")
            setFailed(generation: generation, reason: "no frame updates")
            return
        }

        stateLock.lock()
        if sessionGeneration == generation {
            captureState = .running
        }
        stateLock.unlock()
        print("[CameraAccess] ARKit camera stream running (generation \(generation))")

        for await cameraFrameUpdate in frameUpdates {
            guard isCurrent(generation), !Task.isCancelled else { break }
            processFrame(cameraFrameUpdate.primarySample.pixelBuffer, generation: generation)
        }
        print("[CameraAccess] Frame update loop finished (generation \(generation))")
    }

    stateLock.lock()
    // Only store if nothing else started/stopped in the meantime.
    if sessionGeneration == generation {
        captureTask = task
    } else {
        task.cancel()
    }
    stateLock.unlock()
}

/// Stops streaming and frees the ARKitSession and all textures.
@_cdecl("stopCapture")
public func stopCapture() {
    stateLock.lock()
    sessionGeneration += 1
    let session = arKitSession
    arKitSession = nil
    let task = captureTask
    // Keep a reference so a subsequent start can await full shutdown.
    // The task itself is cancelled below.
    let evTask = eventsTask
    eventsTask = nil
    captureState = .idle
    ringTextures = Array(repeating: nil, count: ringSize)
    latestIndex = -1
    lockedIndex = -1
    if let cache = textureCache {
        CVMetalTextureCacheFlush(cache, 0)
    }
    stateLock.unlock()

    print("[CameraAccess] Stopping capture")
    task?.cancel()
    evTask?.cancel()
    session?.stop()
}

// MARK: - Frame access for Unity

@_cdecl("getCaptureState")
public func getCaptureState() -> Int32 {
    stateLock.lock()
    defer { stateLock.unlock() }
    return captureState.rawValue
}

@_cdecl("getFrameId")
public func getFrameId() -> Int64 {
    stateLock.lock()
    defer { stateLock.unlock() }
    return currentFrameId
}

@_cdecl("getTextureWidth")
public func getTextureWidth() -> Int32 {
    stateLock.lock()
    defer { stateLock.unlock() }
    return Int32(textureWidth)
}

@_cdecl("getTextureHeight")
public func getTextureHeight() -> Int32 {
    stateLock.lock()
    defer { stateLock.unlock() }
    return Int32(textureHeight)
}

/// Returns the latest complete frame (or nil) without locking it. Prefer
/// `acquireLatestTexture` / `releaseTexture` for safe reads.
@_cdecl("getTexture")
public func getTexture() -> UnsafeMutableRawPointer? {
    stateLock.lock()
    defer { stateLock.unlock() }
    guard latestIndex >= 0, let tex = ringTextures[latestIndex] else { return nil }
    return Unmanaged.passUnretained(tex).toOpaque()
}

/// Locks the latest complete frame so the capture thread will not write into it
/// until `releaseTexture()` is called. Returns a consistent snapshot of frame id
/// and dimensions together with the texture pointer.
@_cdecl("acquireLatestTexture")
public func acquireLatestTexture(_ outFrameId: UnsafeMutablePointer<Int64>,
                                 _ outWidth: UnsafeMutablePointer<Int32>,
                                 _ outHeight: UnsafeMutablePointer<Int32>) -> UnsafeMutableRawPointer? {
    stateLock.lock()
    defer { stateLock.unlock() }
    guard latestIndex >= 0, let tex = ringTextures[latestIndex] else {
        outFrameId.pointee = currentFrameId
        outWidth.pointee = 0
        outHeight.pointee = 0
        return nil
    }
    lockedIndex = latestIndex
    outFrameId.pointee = currentFrameId
    outWidth.pointee = Int32(tex.width)
    outHeight.pointee = Int32(tex.height)
    return Unmanaged.passUnretained(tex).toOpaque()
}

@_cdecl("releaseTexture")
public func releaseTexture() {
    stateLock.lock()
    lockedIndex = -1
    stateLock.unlock()
}

// MARK: - Helpers

private func isCurrent(_ generation: Int) -> Bool {
    stateLock.lock()
    defer { stateLock.unlock() }
    return sessionGeneration == generation
}

private func setFailed(generation: Int, reason: String) {
    stateLock.lock()
    if sessionGeneration == generation {
        captureState = .failed
    }
    stateLock.unlock()
    print("[CameraAccess] Capture failed: \(reason)")
}

private func processFrame(_ pixelBuffer: CVPixelBuffer, generation: Int) {
    let pixelBufferBGRA: CVPixelBuffer
    do {
        guard let converted = try pixelBuffer.toBGRA() else { return }
        pixelBufferBGRA = converted
    } catch {
        print("[CameraAccess] Skipping frame, conversion failed: \(error)")
        return
    }

    let width = CVPixelBufferGetWidth(pixelBufferBGRA)
    let height = CVPixelBufferGetHeight(pixelBufferBGRA)

    stateLock.lock()
    if textureCache == nil {
        CVMetalTextureCacheCreate(kCFAllocatorDefault, nil, mtlDevice, nil, &textureCache)
    }
    let cacheRef = textureCache
    stateLock.unlock()
    guard let cache = cacheRef else { return }

    var cvTexture: CVMetalTexture?
    let status = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                                                           cache,
                                                           pixelBufferBGRA,
                                                           nil,
                                                           .bgra8Unorm_srgb,
                                                           width,
                                                           height,
                                                           0,
                                                           &cvTexture)
    guard status == kCVReturnSuccess,
          let imageTexture = cvTexture,
          let sourceTexture = CVMetalTextureGetTexture(imageTexture) else { return }

    // Pick a ring slot that is neither published nor locked by Unity.
    stateLock.lock()
    guard sessionGeneration == generation else {
        stateLock.unlock()
        return
    }
    var writeIndex = 0
    for i in 0..<ringSize where i != latestIndex && i != lockedIndex {
        writeIndex = i
        break
    }
    var target = ringTextures[writeIndex]
    if target == nil || target!.width != width || target!.height != height {
        let descriptor = MTLTextureDescriptor.texture2DDescriptor(pixelFormat: sourceTexture.pixelFormat,
                                                                  width: width,
                                                                  height: height,
                                                                  mipmapped: false)
        descriptor.usage = [.shaderRead]
        target = mtlDevice.makeTexture(descriptor: descriptor)
        ringTextures[writeIndex] = target
    }
    if commandQueue == nil {
        commandQueue = mtlDevice.makeCommandQueue()
    }
    let queue = commandQueue
    stateLock.unlock()

    guard let destination = target,
          let commandBuffer = queue?.makeCommandBuffer(),
          let blitEncoder = commandBuffer.makeBlitCommandEncoder() else { return }

    blitEncoder.copy(from: sourceTexture,
                     sourceSlice: 0, sourceLevel: 0,
                     sourceOrigin: MTLOrigin(x: 0, y: 0, z: 0),
                     sourceSize: MTLSizeMake(width, height, 1),
                     to: destination, destinationSlice: 0, destinationLevel: 0,
                     destinationOrigin: MTLOrigin(x: 0, y: 0, z: 0))
    blitEncoder.endEncoding()
    commandBuffer.commit()
    commandBuffer.waitUntilCompleted()

    // Publish the completed frame.
    stateLock.lock()
    if sessionGeneration == generation && ringTextures[writeIndex] === destination {
        latestIndex = writeIndex
        textureWidth = width
        textureHeight = height
        currentFrameId += 1
    }
    stateLock.unlock()
}

// MARK: - Pixel format conversion

enum CameraAccessError: Error {
    case conversionFailed(vImage_Error)
    case bufferUnavailable
}

extension CVPixelBuffer {
    public func toBGRA() throws -> CVPixelBuffer? {
        let pixelBuffer = self
        let pixelFormat = CVPixelBufferGetPixelFormatType(pixelBuffer)
        guard pixelFormat == kCVPixelFormatType_420YpCbCr8BiPlanarFullRange else { return pixelBuffer }

        CVPixelBufferLockBaseAddress(pixelBuffer, .readOnly)
        defer { CVPixelBufferUnlockBaseAddress(pixelBuffer, .readOnly) }

        guard let yImage = VImage(pixelBuffer: pixelBuffer, plane: 0),
              let cbcrImage = VImage(pixelBuffer: pixelBuffer, plane: 1),
              let outPixelBuffer = CVPixelBuffer.make(width: yImage.width, height: yImage.height, format: kCVPixelFormatType_32BGRA) else {
            throw CameraAccessError.bufferUnavailable
        }

        CVPixelBufferLockBaseAddress(outPixelBuffer, [])
        defer { CVPixelBufferUnlockBaseAddress(outPixelBuffer, []) }

        guard var argbImage = VImage(pixelBuffer: outPixelBuffer) else {
            throw CameraAccessError.bufferUnavailable
        }
        try argbImage.draw(yBuffer: yImage.buffer, cbcrBuffer: cbcrImage.buffer)
        argbImage.permute(channelMap: [3, 2, 1, 0])
        return outPixelBuffer
    }
}

struct VImage {
    let width: Int
    let height: Int
    let bytesPerRow: Int
    var buffer: vImage_Buffer
    init?(pixelBuffer: CVPixelBuffer, plane: Int) {
        guard let rawBuffer = CVPixelBufferGetBaseAddressOfPlane(pixelBuffer, plane) else { return nil }
        self.width = CVPixelBufferGetWidthOfPlane(pixelBuffer, plane)
        self.height = CVPixelBufferGetHeightOfPlane(pixelBuffer, plane)
        self.bytesPerRow = CVPixelBufferGetBytesPerRowOfPlane(pixelBuffer, plane)
        self.buffer = vImage_Buffer(
            data: UnsafeMutableRawPointer(mutating: rawBuffer),
            height: vImagePixelCount(height),
            width: vImagePixelCount(width),
            rowBytes: bytesPerRow)
    }
    init?(pixelBuffer: CVPixelBuffer) {
        guard let rawBuffer = CVPixelBufferGetBaseAddress(pixelBuffer) else { return nil }
        self.width = CVPixelBufferGetWidth(pixelBuffer)
        self.height = CVPixelBufferGetHeight(pixelBuffer)
        self.bytesPerRow = CVPixelBufferGetBytesPerRow(pixelBuffer)
        self.buffer = vImage_Buffer(
            data: UnsafeMutableRawPointer(mutating: rawBuffer),
            height: vImagePixelCount(height),
            width: vImagePixelCount(width),
            rowBytes: bytesPerRow)
    }
    mutating func draw(yBuffer: vImage_Buffer, cbcrBuffer: vImage_Buffer) throws {
        try buffer.draw(yBuffer: yBuffer, cbcrBuffer: cbcrBuffer)
    }
    mutating func permute(channelMap: [UInt8]) {
        buffer.permute(channelMap: channelMap)
    }
}

extension CVPixelBuffer {
    func with<T>(_ closure: ((_ pixelBuffer: CVPixelBuffer) -> T)) -> T {
        CVPixelBufferLockBaseAddress(self, .readOnly)
        let result = closure(self)
        CVPixelBufferUnlockBaseAddress(self, .readOnly)
        return result
    }
    static func make(width: Int, height: Int, format: OSType) -> CVPixelBuffer? {
        var pixelBuffer: CVPixelBuffer? = nil
        CVPixelBufferCreate(kCFAllocatorDefault,
                            width,
                            height,
                            format,
                            [String(kCVPixelBufferIOSurfacePropertiesKey): [
                                "IOSurfaceOpenGLESFBOCompatibility": true,
                                "IOSurfaceOpenGLESTextureCompatibility": true,
                                "IOSurfaceCoreAnimationCompatibility": true,
                            ]] as CFDictionary,
                            &pixelBuffer)
        return pixelBuffer
    }
}

/// Conversion matrix is constant, so generate it once.
private let ycbcrToARGBMatrix: vImage_YpCbCrToARGB = {
    var pixelRange = vImage_YpCbCrPixelRange(Yp_bias: 0, CbCr_bias: 128, YpRangeMax: 255, CbCrRangeMax: 255, YpMax: 255, YpMin: 1, CbCrMax: 255, CbCrMin: 0)
    var matrix = vImage_YpCbCrToARGB()
    vImageConvert_YpCbCrToARGB_GenerateConversion(kvImage_YpCbCrToARGBMatrix_ITU_R_709_2, &pixelRange, &matrix, kvImage420Yp8_CbCr8, kvImageARGB8888, UInt32(kvImageNoFlags))
    return matrix
}()

extension vImage_Buffer {
    mutating func draw(yBuffer: vImage_Buffer, cbcrBuffer: vImage_Buffer) throws {
        var yBuffer = yBuffer
        var cbcrBuffer = cbcrBuffer
        var conversionMatrix = ycbcrToARGBMatrix
        let error = vImageConvert_420Yp8_CbCr8ToARGB8888(&yBuffer, &cbcrBuffer, &self, &conversionMatrix, nil, 255, UInt32(kvImageNoFlags))
        if error != kvImageNoError {
            throw CameraAccessError.conversionFailed(error)
        }
    }
    mutating func permute(channelMap: [UInt8]) {
        vImagePermuteChannels_ARGB8888(&self, &self, channelMap, 0)
    }
}
