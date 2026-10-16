using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_WSA || UNITY_EDITOR
using System.Linq;
using UnityEngine.Windows.WebCam;
#endif
#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class NativeCameraController
{
    private const int DEFAULT_MAX_SIZE = 1024;

    private static ARCameraManager _cachedARCameraManager;
    private static int _arStreamRefCount;
    private static long _arFrameCounter;

    public static void TakePicture(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        TakePictureVisionOS(callback, showHolograms, maxSize);
#elif UNITY_EDITOR
        TakePictureEditor(callback, maxSize);
#elif UNITY_WSA
        TakePictureUWP(callback, showHolograms, maxSize);
#else
        if (HasActiveARCameraStream())
        {
            TakePictureARFoundation(callback, maxSize);
        }
        else
        {
            TakePictureMobile(callback, maxSize);
        }
#endif
    }

    public static void PrepareCapture()
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        prepareCapture();
#endif
    }

    /// <summary>
    /// Whether continuous frame streaming via <see cref="StartCameraStream"/> is supported on this platform.
    /// </summary>
    public static bool SupportsCameraStream
    {
        get
        {
#if UNITY_VISIONOS && !UNITY_EDITOR
            return true;
#else
            return HasActiveARCameraStream();
#endif
        }
    }

    /// <summary>
    /// Starts (or joins) a continuous native camera stream. Calls are reference counted;
    /// every call must be balanced by <see cref="StopCameraStream"/>.
    /// </summary>
    public static void StartCameraStream()
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        AcquireVisionOSStream();
#else
        if (HasActiveARCameraStream())
        {
            AcquireARFoundationStream();
        }
#endif
    }

    /// <summary>
    /// Leaves the camera stream. When no user is left, the native session is stopped and freed immediately.
    /// </summary>
    public static void StopCameraStream()
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        ReleaseVisionOSStream(0f);
#else
        ReleaseARFoundationStream();
#endif
    }

    /// <summary>
    /// Copies the latest camera frame into <paramref name="target"/> if it is newer than <paramref name="lastFrameId"/>.
    /// <paramref name="target"/> is reused when its size matches; otherwise it is destroyed and recreated.
    /// Returns false if no new frame is available.
    /// </summary>
    public static bool TryGetLatestFrame(ref long lastFrameId, ref Texture2D target, int maxSize = DEFAULT_MAX_SIZE)
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        return TryCopyLatestVisionOSFrame(ref lastFrameId, ref target, maxSize);
#else
        if (HasActiveARCameraStream())
        {
            return TryCopyLatestARFoundationFrame(ref lastFrameId, ref target, maxSize);
        }
        return false;
#endif
    }

    public static void StartRecordingVideo(string filePath, Action<bool, string> callback, bool showHolograms = false)
    {
#if UNITY_WSA
        StartVideoRecordingUWPAsync(filePath, callback, showHolograms: showHolograms);
#else
        StartVideoRecordingMobileAsync(filePath, callback);
#endif
    }

    public static Texture2D GetVideoThumbnail(string filePath)
    {
        return NativeCamera.GetVideoThumbnail(filePath, markTextureNonReadable: false);
    }

    public static void StopRecordVideo()
    {
#if UNITY_WSA
        StopVideoRecordingUWP();
#else
        StopVideoRecordingMobile();
#endif
    }

    private static void TakePictureMobile(Action<bool, Texture2D> callback, int maxSize = DEFAULT_MAX_SIZE)
    {
        NativeCamera.TakePicture(path =>
        {
            try
            {
                if (path == null)
                {
                    Debug.LogError("Couldn't get picture");
                    callback(false, null);
                    return;
                }

                var texture = NativeCamera.LoadImageAtPath(path, maxSize, false);
                if (texture == null)
                {
                    Debug.LogError($"Couldn't load texture from {path}");
                }

                File.Delete(path);
                callback(texture != null, texture);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                callback(false, null);
            }
        }, maxSize);
    }

#if UNITY_VISIONOS && !UNITY_EDITOR
    private const int CAPTURE_STATE_IDLE = 0;
    private const int CAPTURE_STATE_STARTING = 1;
    private const int CAPTURE_STATE_RUNNING = 2;
    private const int CAPTURE_STATE_FAILED = 3;

    /// <summary>How long the camera stays running after a single TakePicture, so repeated shots are fast.</summary>
    private const float SINGLE_SHOT_KEEP_ALIVE_SECONDS = 5f;
    private const float FRAME_TIMEOUT_SECONDS = 5f;
    private const float RESTART_THROTTLE_SECONDS = 2f;

    [DllImport("__Internal")]
    private static extern void prepareCapture();

    [DllImport("__Internal")]
    private static extern void startCapture();

    [DllImport("__Internal")]
    private static extern void stopCapture();

    [DllImport("__Internal")]
    private static extern long getFrameId();

    [DllImport("__Internal")]
    private static extern int getCaptureState();

    [DllImport("__Internal")]
    private static extern IntPtr acquireLatestTexture(out long frameId, out int width, out int height);

    [DllImport("__Internal")]
    private static extern void releaseTexture();

    private static int _streamRefCount;
    private static CancellationTokenSource _keepAliveCts;
    private static float _lastRestartTime = -100f;
    private static Texture2D _externalTexture;
    private static RenderTexture _copyRenderTexture;

    private static void TakePictureVisionOS(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {
        CaptureVisionOSTextureAsync(callback, maxSize).Forget();
    }

    private static async UniTaskVoid CaptureVisionOSTextureAsync(Action<bool, Texture2D> callback, int maxSize)
    {
        Texture2D result = null;
        var success = false;

        AcquireVisionOSStream();
        try
        {
            // Require a frame newer than "now" so we never return a stale picture.
            long lastFrameId = getFrameId();
            float deadline = Time.realtimeSinceStartup + FRAME_TIMEOUT_SECONDS;

            while (Time.realtimeSinceStartup < deadline)
            {
                if (TryCopyLatestVisionOSFrame(ref lastFrameId, ref result, maxSize))
                {
                    success = true;
                    break;
                }

                await UniTask.Delay(50, ignoreTimeScale: true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NativeCameraController] Exception during visionOS camera capture: {ex}");
        }
        finally
        {
            ReleaseVisionOSStream(SINGLE_SHOT_KEEP_ALIVE_SECONDS);
        }

        if (!success)
        {
            Debug.LogError($"[NativeCameraController] Failed to acquire a new camera frame from visionOS (state: {getCaptureState()}).");
            if (result != null)
            {
                UnityEngine.Object.Destroy(result);
            }
            callback?.Invoke(false, null);
            return;
        }

        callback?.Invoke(true, result);
    }

    private static void AcquireVisionOSStream()
    {
        _streamRefCount++;
        CancelKeepAlive();
        EnsureVisionOSNativeRunning();
    }

    private static void ReleaseVisionOSStream(float keepAliveSeconds)
    {
        _streamRefCount = Math.Max(0, _streamRefCount - 1);
        if (_streamRefCount > 0)
        {
            return;
        }

        if (keepAliveSeconds <= 0f)
        {
            CancelKeepAlive();
            StopVisionOSNative();
            return;
        }

        StopAfterKeepAliveAsync(keepAliveSeconds).Forget();
    }

    private static async UniTaskVoid StopAfterKeepAliveAsync(float seconds)
    {
        CancelKeepAlive();
        _keepAliveCts = new CancellationTokenSource();
        var token = _keepAliveCts.Token;

        var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: token)
            .SuppressCancellationThrow();

        if (cancelled || _streamRefCount > 0)
        {
            return;
        }

        StopVisionOSNative();
    }

    private static void CancelKeepAlive()
    {
        if (_keepAliveCts != null)
        {
            _keepAliveCts.Cancel();
            _keepAliveCts.Dispose();
            _keepAliveCts = null;
        }
    }

    private static void EnsureVisionOSNativeRunning()
    {
        var state = getCaptureState();
        if (state == CAPTURE_STATE_STARTING || state == CAPTURE_STATE_RUNNING)
        {
            return;
        }

        if (state == CAPTURE_STATE_FAILED)
        {
            ReleaseCopyResources();
            stopCapture();
        }

        _lastRestartTime = Time.realtimeSinceStartup;
        prepareCapture();
        startCapture();
    }

    private static void RestartVisionOSIfFailed()
    {
        if (_streamRefCount <= 0 || getCaptureState() != CAPTURE_STATE_FAILED)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _lastRestartTime < RESTART_THROTTLE_SECONDS)
        {
            return;
        }

        Debug.LogWarning("[NativeCameraController] visionOS camera stream failed, restarting.");
        EnsureVisionOSNativeRunning();
    }

    private static void StopVisionOSNative()
    {
        // Release Unity's handle on the native textures before the native side frees them.
        ReleaseCopyResources();
        stopCapture();
    }

    private static void ReleaseCopyResources()
    {
        if (_externalTexture != null)
        {
            UnityEngine.Object.Destroy(_externalTexture);
            _externalTexture = null;
        }

        if (_copyRenderTexture != null)
        {
            _copyRenderTexture.Release();
            UnityEngine.Object.Destroy(_copyRenderTexture);
            _copyRenderTexture = null;
        }
    }

    private static bool TryCopyLatestVisionOSFrame(ref long lastFrameId, ref Texture2D target, int maxSize)
    {
        if (getFrameId() <= lastFrameId)
        {
            RestartVisionOSIfFailed();
            return false;
        }

        IntPtr texturePtr = acquireLatestTexture(out long frameId, out int width, out int height);
        try
        {
            if (texturePtr == IntPtr.Zero || width <= 0 || height <= 0 || frameId <= lastFrameId)
            {
                return false;
            }

            if (_externalTexture == null || _externalTexture.width != width || _externalTexture.height != height)
            {
                if (_externalTexture != null)
                {
                    UnityEngine.Object.Destroy(_externalTexture);
                }
                _externalTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.BGRA32, false, false, texturePtr);
            }
            _externalTexture.UpdateExternalTexture(texturePtr);

            var scale = maxSize > 0 ? Mathf.Min(1f, (float)maxSize / Mathf.Max(width, height)) : 1f;
            var targetWidth = Mathf.Max(1, Mathf.RoundToInt(width * scale));
            var targetHeight = Mathf.Max(1, Mathf.RoundToInt(height * scale));

            if (_copyRenderTexture == null || _copyRenderTexture.width != targetWidth || _copyRenderTexture.height != targetHeight)
            {
                if (_copyRenderTexture != null)
                {
                    _copyRenderTexture.Release();
                    UnityEngine.Object.Destroy(_copyRenderTexture);
                }
                _copyRenderTexture = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
                _copyRenderTexture.Create();
            }

            // Native texture is top-down; flip vertically.
            Graphics.Blit(_externalTexture, _copyRenderTexture, new Vector2(1, -1), new Vector2(0, 1));

            if (target == null || target.width != targetWidth || target.height != targetHeight)
            {
                if (target != null)
                {
                    UnityEngine.Object.Destroy(target);
                }
                target = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            }

            var previousActive = RenderTexture.active;
            RenderTexture.active = _copyRenderTexture;
            // ReadPixels synchronises with the GPU, so the native texture is no longer in use afterwards.
            target.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            target.Apply(false);
            RenderTexture.active = previousActive;

            lastFrameId = frameId;
            return true;
        }
        finally
        {
            releaseTexture();
        }
    }
#endif

    public static bool HasActiveARCameraStream()
    {
        var manager = GetARCameraManager();
        return manager != null && manager.enabled && (manager.subsystem?.running ?? false);
    }

    private static ARCameraManager GetARCameraManager()
    {
        if (_cachedARCameraManager != null && _cachedARCameraManager.gameObject != null)
        {
            return _cachedARCameraManager;
        }

        var baseCamera = MirageXR.RootObject.Instance?.BaseCamera;
        if (baseCamera != null)
        {
            _cachedARCameraManager = baseCamera.GetComponentInChildren<ARCameraManager>();
            if (_cachedARCameraManager != null)
            {
                return _cachedARCameraManager;
            }
        }

        _cachedARCameraManager = UnityEngine.Object.FindAnyObjectByType<ARCameraManager>();
        return _cachedARCameraManager;
    }

    private static void AcquireARFoundationStream()
    {
        _arStreamRefCount++;
    }

    private static void ReleaseARFoundationStream()
    {
        _arStreamRefCount = Math.Max(0, _arStreamRefCount - 1);
    }

    private static bool TryCopyLatestARFoundationFrame(ref long lastFrameId, ref Texture2D target, int maxSize)
    {
        var manager = GetARCameraManager();
        if (manager == null || !manager.enabled)
        {
            return false;
        }

        if (!manager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            return false;
        }

        using (cpuImage)
        {
            long currentTimestamp = (long)(cpuImage.timestamp * 1000000);
            if (currentTimestamp > 0 && currentTimestamp <= lastFrameId)
            {
                return false;
            }

            int width = cpuImage.width;
            int height = cpuImage.height;

            float scale = maxSize > 0 ? Mathf.Min(1f, (float)maxSize / Mathf.Max(width, height)) : 1f;
            int targetWidth = Mathf.Max(1, Mathf.RoundToInt(width * scale));
            int targetHeight = Mathf.Max(1, Mathf.RoundToInt(height * scale));

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, width, height),
                outputDimensions = new Vector2Int(targetWidth, targetHeight),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            int size = cpuImage.GetConvertedDataSize(conversionParams);
            using (var nativeBuffer = new NativeArray<byte>(size, Allocator.Temp))
            {
                cpuImage.Convert(conversionParams, nativeBuffer);

                bool shouldRotate = Screen.width < Screen.height && targetWidth > targetHeight;
                if (shouldRotate)
                {
                    int finalWidth = targetHeight;
                    int finalHeight = targetWidth;

                    using (var rotatedBuffer = new NativeArray<byte>(size, Allocator.Temp))
                    {
                        unsafe
                        {
                            uint* src = (uint*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(nativeBuffer);
                            uint* dst = (uint*)NativeArrayUnsafeUtility.GetUnsafePtr(rotatedBuffer);

                            for (int y = 0; y < targetHeight; y++)
                            {
                                int srcRowOffset = y * targetWidth;
                                for (int x = 0; x < targetWidth; x++)
                                {
                                    dst[x * finalWidth + y] = src[srcRowOffset + x];
                                }
                            }
                        }

                        if (target == null || target.width != finalWidth || target.height != finalHeight)
                        {
                            if (target != null)
                            {
                                UnityEngine.Object.Destroy(target);
                            }
                            target = new Texture2D(finalWidth, finalHeight, TextureFormat.RGBA32, false);
                        }

                        target.LoadRawTextureData(rotatedBuffer);
                        target.Apply(false);
                    }
                }
                else
                {
                    if (target == null || target.width != targetWidth || target.height != targetHeight)
                    {
                        if (target != null)
                        {
                            UnityEngine.Object.Destroy(target);
                        }
                        target = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
                    }

                    target.LoadRawTextureData(nativeBuffer);
                    target.Apply(false);
                }
            }

            _arFrameCounter++;
            lastFrameId = currentTimestamp > 0 ? currentTimestamp : _arFrameCounter;
            return true;
        }
    }

    private static void TakePictureARFoundation(Action<bool, Texture2D> callback, int maxSize = DEFAULT_MAX_SIZE)
    {
        CaptureARFoundationTextureAsync(callback, maxSize).Forget();
    }

    private static async UniTaskVoid CaptureARFoundationTextureAsync(Action<bool, Texture2D> callback, int maxSize)
    {
        Texture2D result = null;
        bool success = false;
        long lastFrameId = 0;
        float deadline = Time.realtimeSinceStartup + 3.0f;

        while (Time.realtimeSinceStartup < deadline)
        {
            if (TryCopyLatestARFoundationFrame(ref lastFrameId, ref result, maxSize))
            {
                success = true;
                break;
            }
            await UniTask.Delay(50, ignoreTimeScale: true);
        }

        if (!success)
        {
            Debug.LogError("[NativeCameraController] Failed to capture picture from ARFoundation stream.");
            if (result != null)
            {
                UnityEngine.Object.Destroy(result);
            }
            callback?.Invoke(false, null);
            return;
        }

        callback?.Invoke(true, result);
    }

    private static void StartVideoRecordingMobileAsync(string filePath, Action<bool, string> callback)
    {
        NativeCamera.RecordVideo(path =>
        {
            try
            {
#if UNITY_EDITOR
                if (path != null)
                {
                    File.Copy(path, filePath);
                    callback.Invoke(true, filePath);
                }
                else
                {
                    Debug.LogWarning("Fake video-recording in editor: No file was selected. Returning false");
                    callback.Invoke(false, null);
                }
#else
                if (path != null)
                {
                    File.Move(path, filePath);
                    callback.Invoke(true, filePath);
                }
                else
                {
                    Debug.LogWarning("No file was selected.");
                    callback.Invoke(false, null);
                }
#endif

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                callback.Invoke(false, filePath);
            }
        });
    }

    private static void StopVideoRecordingMobile()
    {
        Debug.Log($"no need to call this function ({nameof(StopVideoRecordingMobile)}) for mobile devices");
    }

#if UNITY_EDITOR
    private static void TakePictureEditor(Action<bool, Texture2D> callback, int maxSize = DEFAULT_MAX_SIZE)
    {
#if UNITY_EDITOR_WIN
        if (PhotoCapture.SupportedResolutions.Any())
        {
            TakePictureUWP(callback, false, maxSize);
            return;
        }
#endif
        CaptureEditorWebCamAsync(callback, maxSize).Forget();
    }

    private static async UniTaskVoid CaptureEditorWebCamAsync(Action<bool, Texture2D> callback, int maxSize)
    {
        WebCamTexture webcam = null;
        try
        {
            webcam = new WebCamTexture(maxSize, maxSize, 30);
            webcam.Play();

            float elapsed = 0f;
            while (webcam.width <= 16 && elapsed < 2.0f)
            {
                await UniTask.Delay(100);
                elapsed += 0.1f;
            }

            if (webcam.width > 16)
            {
                var tex = new Texture2D(webcam.width, webcam.height, TextureFormat.RGBA32, false);
                tex.SetPixels32(webcam.GetPixels32());
                tex.Apply();
                webcam.Stop();
                UnityEngine.Object.Destroy(webcam);
                callback?.Invoke(true, tex);
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NativeCameraController] Editor WebCam capture note: {ex.Message}");
        }

        if (webcam != null)
        {
            if (webcam.isPlaying) webcam.Stop();
            UnityEngine.Object.Destroy(webcam);
        }

        string pickedFile = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select Image", "", new string[] { "Image files", "png,jpg,jpeg", "All files", "*" });
        if (!string.IsNullOrEmpty(pickedFile) && File.Exists(pickedFile))
        {
            byte[] fileData = File.ReadAllBytes(pickedFile);
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                callback?.Invoke(true, tex);
                return;
            }
        }

        callback?.Invoke(false, null);
    }
#endif

#if UNITY_WSA || UNITY_EDITOR
    private static void TakePictureUWP(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {
        var resolutions = PhotoCapture.SupportedResolutions.Where(t => t.height <= maxSize && t.width <= maxSize).ToList();

        if (resolutions.Count == 0)
        {
            Debug.LogWarning($"No extension was found for the specified parameters (maxSize == {maxSize}). The maximum available resolution will be used.");
            resolutions = PhotoCapture.SupportedResolutions.ToList();
        }

        if (resolutions.Count == 0)
        {
            Debug.LogWarning("Could not take image. No resolutions are available. Is the webcam accessbile?");
            callback(false, null);
            return;
        }

        var cameraResolution = resolutions.OrderByDescending(res => res.width * res.height).First();

        PhotoCapture photoCapture;

        PhotoCapture.CreateAsync(showHolograms, captureObject =>
        {
            photoCapture = captureObject;

            if (photoCapture != null)
            {
                var cameraParameters = new CameraParameters
                {
                    hologramOpacity = showHolograms ? 1.0f : 0.0f,
                    cameraResolutionWidth = cameraResolution.width,
                    cameraResolutionHeight = cameraResolution.height,
                    pixelFormat = CapturePixelFormat.BGRA32
                };

                photoCapture.StartPhotoModeAsync(cameraParameters, result =>
                {
                    if (result.success)
                    {
                        photoCapture.TakePhotoAsync((captureResult, frame) =>
                        {
                            var texture = new Texture2D(cameraResolution.width, cameraResolution.height);
                            frame.UploadImageDataToTexture(texture);
                            photoCapture.StopPhotoModeAsync(photoCaptureResult =>
                            {
                                photoCapture.Dispose();
                                callback(true, texture);
                            });
                        });
                    }
                    else
                    {
                        callback(false, null);
                    }
                });
            }
            else
            {
                callback(false, null);
            }
        });
    }
#endif

#if UNITY_WSA || UNITY_EDITOR
    private static VideoCapture _videoCapture;
    private static string _filePath;
    private static Action<bool, string> _recordCallback;

    private static void StartVideoRecordingUWPAsync(string filePath, Action<bool, string> callback,
        VideoCapture.AudioState audioState = VideoCapture.AudioState.MicAudio, bool showHolograms = false)
    {

#if UNITY_EDITOR
        string pickedFile = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select video", "", new string[] { "Video files", "mp4,mov,wav,avi", "All files", "*" });
        if (callback != null)
        {
            var isValidPath = !string.IsNullOrEmpty(pickedFile);
            callback.Invoke(isValidPath, isValidPath ? pickedFile : null);
            ResetVideoRecorder();
        }
        return;
#endif


        if (_videoCapture != null && _videoCapture.IsRecording)
        {
            Debug.Log("Is already recording");
            return;
        }

        VideoCapture.CreateAsync(showHolograms, captureObject =>
        {
            if (captureObject != null)
            {
                _videoCapture = captureObject;

                Debug.Log("onVideoCaptureCreated");

                var parameters = CreateCameraVideoParameters();
                _videoCapture.StartVideoModeAsync(parameters, audioState, result =>
                {
                    Debug.Log("start recording video");
                    if (result.success)
                    {
                        _videoCapture.StartRecordingAsync(filePath, captureResult =>
                        {
                            Debug.Log("Started Recording Video!");
                            _filePath = filePath;
                            _recordCallback = callback;
                        });
                    }
                    else
                    {
                        Debug.LogError("VideoCaptureResult was not successful");
                        _recordCallback.Invoke(false, null);
                        ResetVideoRecorder();
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to create VideoCapture Instance!");
                _recordCallback.Invoke(false, null);
                ResetVideoRecorder();
            }
        });
    }

    private static void ResetVideoRecorder()
    {
        _recordCallback = null;
        _videoCapture?.Dispose();
        _videoCapture = null;
    }

    private static void StopVideoRecordingUWP()
    {
        if (_videoCapture == null || !_videoCapture.IsRecording)
        {
            Debug.LogError("No recording taking place");
            return;
        }

        _videoCapture.StopRecordingAsync(result =>
        {
            Debug.Log("Stopped Recording Video!");
            _videoCapture.StopVideoModeAsync(captureResult =>
            {
                _recordCallback.Invoke(captureResult.success, captureResult.success ? _filePath : null);
                ResetVideoRecorder();
                Debug.Log(captureResult);
            });
        });
    }

    private static CameraParameters CreateCameraVideoParameters()
    {
        Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).Last();
        float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending(fps => fps).First();

        var cameraParameters = new CameraParameters
        {
            hologramOpacity = 0.0f,
            frameRate = cameraFramerate,
            cameraResolutionWidth = cameraResolution.width,
            cameraResolutionHeight = cameraResolution.height,
            pixelFormat = CapturePixelFormat.BGRA32
        };
        return cameraParameters;
    }
#endif
}