using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZXing;
using ZXing.Common;

namespace MirageXR
{
    public class QRCodeScanResult
    {
        public string RawText { get; set; }
        public Guid ActivityId { get; set; }
        public Pose WorldPose { get; set; }
        public ResultPoint[] ResultPoints { get; set; }
    }

    /// <summary>
    /// QR code scanner supporting continuous live camera feed decoding (in PlayMode/WebCam)
    /// and native camera snapshot capture via NativeCameraController.TakePicture.
    /// Runs ZXing barcode decoding on background threads and calculates 3D world pose for detected QR codes.
    /// </summary>
    public class QRCodeScanner : MonoBehaviour
    {
        private const int DEFAULT_MAX_SIZE = 1024;

        [SerializeField] private int _requestedWidth = 640;
        [SerializeField] private int _requestedHeight = 480;
        [SerializeField] private int _requestedFPS = 30;
        [SerializeField] private float _scanIntervalSeconds = 0.15f;
        [SerializeField] private int _maxPictureSize = DEFAULT_MAX_SIZE;
        [SerializeField] private bool _showHolograms = false;
        [SerializeField] private bool _requireActivityLink = false;

        public event Action<QRCodeScanResult> OnQRCodeDetected;
        public event Action<string> OnScanError;
        public event Action<string> OnInvalidQRCodeDetected;
        public event Action<Texture2D> OnCameraStarted;

        public bool RequireActivityLink
        {
            get => _requireActivityLink;
            set => _requireActivityLink = value;
        }

        private WebCamTexture _webcamTexture;
        private Texture2D _previewTexture;
        private Texture2D _capturedTexture;
        private BarcodeReaderGeneric _barcodeReader;
        private CancellationTokenSource _scanCts;
        private bool _isScanning;
        private Pose? _singleShotCapturePose;

        public bool IsScanning => _isScanning;
        public Texture2D CameraTexture => _previewTexture ?? _capturedTexture;

        private void Awake()
        {
            InitializeReader();
        }

        private void OnDestroy()
        {
            StopScanning();
            CleanupTexture();
        }

        private void OnDisable()
        {
            StopScanning();
        }

        private void InitializeReader()
        {
            _barcodeReader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true,
                PureBarcode = false
            }
            };
        }

        private void CleanupTexture()
        {
            if (_capturedTexture != null)
            {
                Destroy(_capturedTexture);
                _capturedTexture = null;
            }
            if (_previewTexture != null)
            {
                Destroy(_previewTexture);
                _previewTexture = null;
            }
        }

        /// <summary>
        /// Begins scanning. In Editor and desktop environments with a webcam, starts a live feed
        /// and continuously checks frames until a QR code is detected. On platforms without direct
        /// webcam stream access, falls back to NativeCameraController.TakePicture.
        /// </summary>
        public void StartScanning()
        {
            if (_isScanning)
            {
                return;
            }

            _scanCts?.Cancel();
            _scanCts?.Dispose();
            _scanCts = new CancellationTokenSource();
            _isScanning = true;

#if UNITY_VISIONOS && !UNITY_EDITOR
            StartNativeCameraContinuousScanning(_scanCts.Token).Forget();
#elif UNITY_EDITOR || UNITY_STANDALONE
            StartWebCamContinuousScanning();
#else
            if (WebCamTexture.devices.Length > 0)
            {
                StartWebCamContinuousScanning();
            }
            else
            {
                StartNativeCameraContinuousScanning(_scanCts.Token).Forget();
            }
#endif
        }

        public async UniTask StartScanningAsync()
        {
            StartScanning();
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Starts continuous webcam feed and background detection loop.
        /// </summary>
        private void StartWebCamContinuousScanning()
        {
            try
            {
                if (_webcamTexture == null)
                {
                    _webcamTexture = new WebCamTexture(_requestedWidth, _requestedHeight, _requestedFPS);
                }

                _webcamTexture.Play();
                ScanLoopAsync(_scanCts.Token).Forget();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QRCodeScanner] Error starting WebCamTexture: {ex.Message}. Falling back to NativeCameraController.");
                StartNativeCameraContinuousScanning(_scanCts.Token).Forget();
            }
        }

        private async UniTaskVoid ScanLoopAsync(CancellationToken cancellationToken)
        {
            // Wait until camera feed provides valid dimensions
            float elapsed = 0f;
            while (_webcamTexture != null && _webcamTexture.width <= 16 && !cancellationToken.IsCancellationRequested && elapsed < 3.0f)
            {
                await UniTask.Delay(100, cancellationToken: cancellationToken);
                elapsed += 0.1f;
            }

            while (_isScanning && !cancellationToken.IsCancellationRequested)
            {
                if (_webcamTexture == null || !_webcamTexture.isPlaying)
                {
                    await UniTask.Yield(cancellationToken);
                    continue;
                }

                if (_webcamTexture.didUpdateThisFrame)
                {
                    int width = _webcamTexture.width;
                    int height = _webcamTexture.height;
                    Color32[] pixels = _webcamTexture.GetPixels32();

                    // Safely update Texture2D preview for PolySpatial compatibility (never assign WebCamTexture to CanvasRenderer)
                    if (_previewTexture == null || _previewTexture.width != width || _previewTexture.height != height)
                    {
                        if (_previewTexture != null)
                        {
                            Destroy(_previewTexture);
                        }
                        _previewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                        OnCameraStarted?.Invoke(_previewTexture);
                    }
                    _previewTexture.SetPixels32(pixels);
                    _previewTexture.Apply(false);

                    var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
                    Pose? capturePose = currentCam != null
                        ? new Pose(currentCam.transform.position, currentCam.transform.rotation)
                        : (Pose?)null;

                    Result decodeResult = null;
                    try
                    {
                        decodeResult = await UniTask.RunOnThreadPool(() =>
                        {
                            var luminance = new UnityColor32LuminanceSource(pixels, width, height);
                            return _barcodeReader.Decode(luminance);
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[QRCodeScanner] Error decoding frame: {ex.Message}");
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (decodeResult != null && !string.IsNullOrEmpty(decodeResult.Text))
                    {
                        var rawText = decodeResult.Text;
                        var resultPoints = decodeResult.ResultPoints;

                        await UniTask.SwitchToMainThread(cancellationToken);
                        var pose = CalculateQRPose(resultPoints, width, height, currentCam, capturePose);
                        QRCodeActivityParser.TryParseActivityId(rawText, out var activityId);

                        var scanResult = new QRCodeScanResult
                        {
                            RawText = rawText,
                            ActivityId = activityId,
                            WorldPose = pose,
                            ResultPoints = resultPoints
                        };

                        if (_requireActivityLink && activityId == Guid.Empty)
                        {
                            Debug.LogWarning($"[QRCodeScanner] Detected non-activity QR code: {rawText}. Continuing scan...");
                            OnInvalidQRCodeDetected?.Invoke(rawText);
                        }
                        else
                        {
                            Debug.Log($"[QRCodeScanner] QR code detected: {decodeResult.Text}");
                            _isScanning = false;
                            StopCamera();
                            OnQRCodeDetected?.Invoke(scanResult);
                            break;
                        }
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_scanIntervalSeconds), cancellationToken: cancellationToken);
            }
        }

        private async UniTaskVoid StartNativeCameraContinuousScanning(CancellationToken cancellationToken)
        {
            if (NativeCameraController.SupportsCameraStream)
            {
                await RunStreamingNativeCameraLoopAsync(cancellationToken);
            }
            else
            {
                await RunSnapshotNativeCameraLoopAsync(cancellationToken);
            }
        }

        private async UniTask RunStreamingNativeCameraLoopAsync(CancellationToken cancellationToken)
        {
            if (RootObject.Instance?.ImageTargetManager != null)
            {
                RootObject.Instance.ImageTargetManager.enabled = false;
            }

            NativeCameraController.StartCameraStream();
            long lastFrameId = 0;
            Texture2D streamTexture = null;
            Texture2D previousNotifiedTexture = null;

            try
            {
                while (_isScanning && !cancellationToken.IsCancellationRequested)
                {
                    bool hasNewFrame = NativeCameraController.TryGetLatestFrame(ref lastFrameId, ref streamTexture, _maxPictureSize);

                    if (hasNewFrame && streamTexture != null)
                    {
                        var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
                        Pose? capturePose = currentCam != null
                            ? new Pose(currentCam.transform.position, currentCam.transform.rotation)
                            : (Pose?)null;

                        if (_capturedTexture != streamTexture)
                        {
                            if (_capturedTexture != null)
                            {
                                Destroy(_capturedTexture);
                            }
                            _capturedTexture = streamTexture;
                        }

                        if (previousNotifiedTexture != _capturedTexture)
                        {
                            previousNotifiedTexture = _capturedTexture;
                            OnCameraStarted?.Invoke(_capturedTexture);
                        }

                        var scanResult = await DecodeTextureAsync(_capturedTexture, cancellationToken, stopScanningOnNoCode: false, cameraPoseAtCapture: capturePose);
                        if (scanResult != null && (!_requireActivityLink || scanResult.ActivityId != Guid.Empty))
                        {
                            break;
                        }
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_scanIntervalSeconds), ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScanner] Exception during camera stream scanning: {ex}");
            }
            finally
            {
                NativeCameraController.StopCameraStream();

                if (RootObject.Instance?.ImageTargetManager != null)
                {
                    RootObject.Instance.ImageTargetManager.enabled = true;
                }
            }
        }

        private async UniTask RunSnapshotNativeCameraLoopAsync(CancellationToken cancellationToken)
        {
            if (RootObject.Instance?.ImageTargetManager != null)
            {
                RootObject.Instance.ImageTargetManager.enabled = false;
            }

            try
            {
                while (_isScanning && !cancellationToken.IsCancellationRequested)
                {
                    var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
                    Pose? capturePose = currentCam != null
                        ? new Pose(currentCam.transform.position, currentCam.transform.rotation)
                        : (Pose?)null;

                    var tcs = new UniTaskCompletionSource<(bool success, Texture2D texture)>();

                    try
                    {
                        NativeCameraController.TakePicture((success, tex) =>
                        {
                            tcs.TrySetResult((success, tex));
                        }, _showHolograms, _maxPictureSize);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[QRCodeScanner] Error invoking NativeCameraController: {ex.Message}");
                        tcs.TrySetResult((false, null));
                    }

                    var (success, capturedTex) = await tcs.Task;

                    if (cancellationToken.IsCancellationRequested || !_isScanning)
                    {
                        if (capturedTex != null)
                        {
                            Destroy(capturedTex);
                        }
                        break;
                    }

                    if (!success || capturedTex == null)
                    {
                        Debug.LogWarning("[QRCodeScanner] Native camera frame capture returned unsuccessful. Retrying in 1s...");
                        await UniTask.Delay(TimeSpan.FromSeconds(1f), ignoreTimeScale: true, cancellationToken: cancellationToken);
                        continue;
                    }

                    CleanupTexture();
                    _capturedTexture = capturedTex;
                    OnCameraStarted?.Invoke(_capturedTexture);

                    var scanResult = await DecodeTextureAsync(_capturedTexture, cancellationToken, stopScanningOnNoCode: false, cameraPoseAtCapture: capturePose);
                    if (scanResult != null && (!_requireActivityLink || scanResult.ActivityId != Guid.Empty))
                    {
                        break;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScanner] Exception during snapshot scanning: {ex}");
            }
            finally
            {
                if (RootObject.Instance?.ImageTargetManager != null)
                {
                    RootObject.Instance.ImageTargetManager.enabled = true;
                }
            }
        }

        /// <summary>
        /// Captures a single image via NativeCameraController.TakePicture wrapper method.
        /// Temporarily disables ImageTargetManager to avoid hardware camera locks.
        /// </summary>
        public void CaptureWithNativeCamera()
        {
            if (RootObject.Instance?.ImageTargetManager != null)
            {
                RootObject.Instance.ImageTargetManager.enabled = false;
            }

            var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
            _singleShotCapturePose = currentCam != null
                ? new Pose(currentCam.transform.position, currentCam.transform.rotation)
                : (Pose?)null;

            try
            {
                NativeCameraController.TakePicture(OnPictureTaken, _showHolograms, _maxPictureSize);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScanner] Error invoking NativeCameraController.TakePicture: {ex.Message}");
                if (RootObject.Instance?.ImageTargetManager != null)
                {
                    RootObject.Instance.ImageTargetManager.enabled = true;
                }
                _isScanning = false;
                _singleShotCapturePose = null;
                OnScanError?.Invoke($"Camera error: {ex.Message}");
            }
        }

        /// <summary>
        /// Explicit wrapper method initiating camera capture via NativeCameraController.TakePicture.
        /// </summary>
        public void CaptureImage()
        {
            CaptureWithNativeCamera();
        }

        private void OnPictureTaken(bool result, Texture2D texture2D)
        {
            // Restore ImageTargetManager tracking
            if (RootObject.Instance?.ImageTargetManager != null)
            {
                RootObject.Instance.ImageTargetManager.enabled = true;
            }

            var capturePose = _singleShotCapturePose;
            _singleShotCapturePose = null;

            if (!_isScanning)
            {
                if (texture2D != null)
                {
                    Destroy(texture2D);
                }
                return;
            }

            if (!result || texture2D == null)
            {
                _isScanning = false;
                OnScanError?.Invoke("Failed to capture image from camera.");
                return;
            }

            CleanupTexture();
            _capturedTexture = texture2D;
            OnCameraStarted?.Invoke(_capturedTexture);

            var token = _scanCts != null ? _scanCts.Token : CancellationToken.None;
            DecodeTextureAsync(_capturedTexture, token, stopScanningOnNoCode: true, cameraPoseAtCapture: capturePose).Forget();
        }

        /// <summary>
        /// Releases webcam texture resources.
        /// </summary>
        public void StopCamera()
        {
            if (_webcamTexture != null)
            {
                if (_webcamTexture.isPlaying)
                {
                    _webcamTexture.Stop();
                }
                Destroy(_webcamTexture);
                _webcamTexture = null;
            }
        }

        /// <summary>
        /// Stops active scanning and releases camera resources.
        /// </summary>
        public void StopScanning()
        {
            _isScanning = false;
            _singleShotCapturePose = null;

            if (_scanCts != null)
            {
                _scanCts.Cancel();
                _scanCts.Dispose();
                _scanCts = null;
            }

            StopCamera();
            CleanupTexture();

            if (RootObject.Instance?.ImageTargetManager != null)
            {
                RootObject.Instance.ImageTargetManager.enabled = true;
            }
        }

        /// <summary>
        /// Synchronously decodes a QR code from a Texture2D.
        /// </summary>
        public QRCodeScanResult DecodeTexture(Texture2D texture, bool stopScanningOnNoCode = true, Pose? cameraPoseAtCapture = null)
        {
            if (texture == null)
            {
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("Invalid image texture.");
                }
                return null;
            }

            int width = texture.width;
            int height = texture.height;
            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScanner] Failed to read pixels from texture: {ex.Message}");
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("Could not read image data.");
                }
                return null;
            }

            Result decodeResult = null;
            try
            {
                var luminance = new UnityColor32LuminanceSource(pixels, width, height);
                decodeResult = _barcodeReader.Decode(luminance);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QRCodeScanner] Error decoding frame: {ex.Message}");
            }

            if (decodeResult != null && !string.IsNullOrEmpty(decodeResult.Text))
            {
                Debug.Log($"[QRCodeScanner] QR code detected: {decodeResult.Text}");

                var rawText = decodeResult.Text;
                var resultPoints = decodeResult.ResultPoints;

                var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
                var pose = CalculateQRPose(resultPoints, width, height, currentCam, cameraPoseAtCapture);
                QRCodeActivityParser.TryParseActivityId(rawText, out var activityId);

                var scanResult = new QRCodeScanResult
                {
                    RawText = rawText,
                    ActivityId = activityId,
                    WorldPose = pose,
                    ResultPoints = resultPoints
                };

                if (_requireActivityLink && activityId == Guid.Empty)
                {
                    if (stopScanningOnNoCode)
                    {
                        _isScanning = false;
                    }
                    OnInvalidQRCodeDetected?.Invoke(rawText);
                }
                else
                {
                    _isScanning = false;
                    OnQRCodeDetected?.Invoke(scanResult);
                }
                return scanResult;
            }
            else
            {
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("No QR code detected in the captured picture. Please try again.");
                }
                return null;
            }
        }

        /// <summary>
        /// Decodes a QR code from the specified texture on a background thread.
        /// </summary>
        public async UniTask<QRCodeScanResult> DecodeTextureAsync(Texture2D texture, CancellationToken cancellationToken = default, bool stopScanningOnNoCode = false, Pose? cameraPoseAtCapture = null)
        {
            if (texture == null)
            {
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("Invalid image texture.");
                }
                return null;
            }

            int width = texture.width;
            int height = texture.height;
            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScanner] Failed to read pixels from texture: {ex.Message}");
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("Could not read image data.");
                }
                return null;
            }

            Result decodeResult = null;
            try
            {
                decodeResult = await UniTask.RunOnThreadPool(() =>
                {
                    var luminance = new UnityColor32LuminanceSource(pixels, width, height);
                    return _barcodeReader.Decode(luminance);
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QRCodeScanner] Error decoding frame: {ex.Message}");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (decodeResult != null && !string.IsNullOrEmpty(decodeResult.Text))
            {
                Debug.Log($"[QRCodeScanner] QR code detected: {decodeResult.Text}");

                var rawText = decodeResult.Text;
                var resultPoints = decodeResult.ResultPoints;

                await UniTask.SwitchToMainThread(cancellationToken);
                var currentCam = RootObject.Instance?.BaseCamera ?? Camera.main;
                var pose = CalculateQRPose(resultPoints, width, height, currentCam, cameraPoseAtCapture);

                QRCodeActivityParser.TryParseActivityId(rawText, out var activityId);

                var scanResult = new QRCodeScanResult
                {
                    RawText = rawText,
                    ActivityId = activityId,
                    WorldPose = pose,
                    ResultPoints = resultPoints
                };

                if (_requireActivityLink && activityId == Guid.Empty)
                {
                    if (stopScanningOnNoCode)
                    {
                        _isScanning = false;
                    }
                    OnInvalidQRCodeDetected?.Invoke(rawText);
                }
                else
                {
                    _isScanning = false;
                    OnQRCodeDetected?.Invoke(scanResult);
                }
                return scanResult;
            }
            else
            {
                if (stopScanningOnNoCode)
                {
                    _isScanning = false;
                    OnScanError?.Invoke("No QR code detected in the captured picture. Please try again.");
                }
                return null;
            }
        }

        /// <summary>
        /// Standard physical width/height of a QR code displayed on a mobile screen (5.4cm).
        /// </summary>
        public const float DEFAULT_PHYSICAL_QR_SIZE_METERS = 0.054f;

        /// <summary>
        /// Default fallback scan distance if finder points cannot be measured optically (0.30m).
        /// </summary>
        public const float DEFAULT_SCAN_DISTANCE_METERS = 0.30f;

        /// <summary>
        /// Calculates the 3D world pose of a detected QR code based on pinhole camera optics
        /// from the detected finder pattern pixel coordinates and known physical size.
        /// </summary>
        public static Pose CalculateQRPose(
            Vector2[] points,
            int imageWidth,
            int imageHeight,
            Camera camera = null,
            Pose? cameraPoseAtCapture = null,
            float physicalQrSizeMeters = DEFAULT_PHYSICAL_QR_SIZE_METERS,
            float nominalDistanceMeters = DEFAULT_SCAN_DISTANCE_METERS)
        {
            if (camera == null)
            {
                camera = RootObject.Instance?.BaseCamera ?? Camera.main;
            }

            Pose capturePose = cameraPoseAtCapture ?? (camera != null
                ? new Pose(camera.transform.position, camera.transform.rotation)
                : Pose.identity);

            float centerX = 0f;
            float centerY = 0f;

            if (points != null && points.Length > 0)
            {
                foreach (var pt in points)
                {
                    centerX += pt.x;
                    centerY += pt.y;
                }
                centerX /= points.Length;
                centerY /= points.Length;
            }
            else
            {
                centerX = imageWidth * 0.5f;
                centerY = imageHeight * 0.5f;
            }

            // Convert pixel coordinates to normalized viewport coordinates (0 to 1).
            // In Unity Color32 / ZXing texture space, (0,0) is bottom-left, matching Unity viewport.
            float viewportX = Mathf.Clamp01(centerX / Mathf.Max(imageWidth, 1));
            float viewportY = Mathf.Clamp01(centerY / Mathf.Max(imageHeight, 1));

            float vFov = camera != null ? camera.fieldOfView : 60f;
            Vector3 localDir;
            if (camera != null)
            {
                Ray currentRay = camera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f));
                localDir = camera.transform.InverseTransformDirection(currentRay.direction);
            }
            else
            {
                // Fallback field of view ~60 degrees vertical
                float aspect = (float)Mathf.Max(imageWidth, 1) / Mathf.Max(imageHeight, 1);
                float halfHeight = Mathf.Tan(vFov * 0.5f * Mathf.Deg2Rad);
                float halfWidth = halfHeight * aspect;
                localDir = new Vector3((viewportX - 0.5f) * 2f * halfWidth, (viewportY - 0.5f) * 2f * halfHeight, 1f).normalized;
            }

            Vector3 rayDir = (capturePose.rotation * localDir).normalized;

            // Optical distance calculation using pinhole camera geometry:
            // D = (physicalSize * focalLengthPixels) / apparentPixelSize
            float focalLengthPixels = (float)imageHeight / (2f * Mathf.Tan(vFov * 0.5f * Mathf.Deg2Rad));
            float distance = nominalDistanceMeters;

            if (points != null && points.Length >= 2)
            {
                float maxFinderSpan = 0f;
                if (points.Length >= 3)
                {
                    float d01 = Vector2.Distance(points[0], points[1]);
                    float d12 = Vector2.Distance(points[1], points[2]);
                    float d02 = Vector2.Distance(points[0], points[2]);
                    float[] dists = { d01, d12, d02 };
                    Array.Sort(dists);
                    maxFinderSpan = dists[1]; // Second shortest distance represents the orthogonal edge span
                }
                else
                {
                    maxFinderSpan = Vector2.Distance(points[0], points[1]);
                }

                // In standard QR codes, finder pattern centers span ~80% of total module width
                float apparentQRPixelSize = maxFinderSpan / 0.80f;
                if (apparentQRPixelSize > 10f)
                {
                    float calculatedDistance = (physicalQrSizeMeters * focalLengthPixels) / apparentQRPixelSize;
                    distance = Mathf.Clamp(calculatedDistance, 0.15f, 0.60f);
                }
            }

            Vector3 position = capturePose.position + rayDir * distance;

            // Orient facing the camera along horizontal plane (constrain to upright yaw rotation)
            Vector3 forward = Vector3.ProjectOnPlane(-rayDir, Vector3.up).normalized;
            if (forward == Vector3.zero)
            {
                forward = capturePose.rotation * Vector3.forward;
            }
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var euler = rotation.eulerAngles;
            var yawRotation = Quaternion.Euler(0, euler.y, 0);

            return new Pose(position, yawRotation);
        }

        public static Pose CalculateQRPose(
            ResultPoint[] resultPoints,
            int imageWidth,
            int imageHeight,
            Camera camera = null,
            Pose? cameraPoseAtCapture = null,
            float physicalQrSizeMeters = DEFAULT_PHYSICAL_QR_SIZE_METERS,
            float nominalDistanceMeters = DEFAULT_SCAN_DISTANCE_METERS)
        {
            if (resultPoints == null || resultPoints.Length == 0)
            {
                return CalculateQRPose((Vector2[])null, imageWidth, imageHeight, camera, cameraPoseAtCapture, physicalQrSizeMeters, nominalDistanceMeters);
            }

            var points = new Vector2[resultPoints.Length];
            for (int i = 0; i < resultPoints.Length; i++)
            {
                points[i] = new Vector2(resultPoints[i].X, resultPoints[i].Y);
            }
            return CalculateQRPose(points, imageWidth, imageHeight, camera, cameraPoseAtCapture, physicalQrSizeMeters, nominalDistanceMeters);
        }
    }
}
