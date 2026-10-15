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
                        var pose = CalculateQRPose(resultPoints, width, height);
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

                        var scanResult = await DecodeTextureAsync(_capturedTexture, cancellationToken, stopScanningOnNoCode: false);
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

                    var scanResult = await DecodeTextureAsync(_capturedTexture, cancellationToken, stopScanningOnNoCode: false);
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
            DecodeTextureAsync(_capturedTexture, token, stopScanningOnNoCode: true).Forget();
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
        public QRCodeScanResult DecodeTexture(Texture2D texture, bool stopScanningOnNoCode = true)
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

                var pose = CalculateQRPose(resultPoints, width, height);
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
        public async UniTask<QRCodeScanResult> DecodeTextureAsync(Texture2D texture, CancellationToken cancellationToken = default, bool stopScanningOnNoCode = false)
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
                var pose = CalculateQRPose(resultPoints, width, height);

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
        /// Calculates the 3D world pose of a detected QR code based on camera gaze/raycast.
        /// </summary>
        public static Pose CalculateQRPose(Vector2[] points, int imageWidth, int imageHeight, Camera camera = null)
        {
            if (camera == null)
            {
                camera = RootObject.Instance?.BaseCamera ?? Camera.main;
            }

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

            // Convert pixel coordinates to normalized viewport coordinates (0 to 1)
            float viewportX = Mathf.Clamp01(centerX / Mathf.Max(imageWidth, 1));
            // In Unity viewport, (0,0) is bottom-left; in image space (0,0) is top-left
            float viewportY = Mathf.Clamp01(1.0f - (centerY / Mathf.Max(imageHeight, 1)));

            Ray ray = camera != null
                ? camera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f))
                : new Ray(Vector3.zero, Vector3.forward);

            Pose pose;
            // Raycast against spatial environment
            if (Physics.Raycast(ray, out RaycastHit hit, 5.0f))
            {
                Vector3 forward = -hit.normal;
                if (forward == Vector3.zero || Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f)
                {
                    forward = camera != null ? camera.transform.forward : Vector3.forward;
                }

                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                pose = new Pose(hit.point, rotation);
            }
            else
            {
                // Fallback: place in front of camera at default working distance (0.8m)
                Vector3 position = ray.origin + ray.direction * 0.8f;
                Vector3 forward = camera != null ? camera.transform.forward : Vector3.forward;
                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                pose = new Pose(position, rotation);
            }

            // Follow MirageXR standard calibration: constrain to yaw (rotation around Y-axis)
            var euler = pose.rotation.eulerAngles;
            var yawRotation = Quaternion.Euler(0, euler.y, 0);

            return new Pose(pose.position, yawRotation);
        }

        public static Pose CalculateQRPose(ResultPoint[] resultPoints, int imageWidth, int imageHeight, Camera camera = null)
        {
            if (resultPoints == null || resultPoints.Length == 0)
            {
                return CalculateQRPose((Vector2[])null, imageWidth, imageHeight, camera);
            }

            var points = new Vector2[resultPoints.Length];
            for (int i = 0; i < resultPoints.Length; i++)
            {
                points[i] = new Vector2(resultPoints[i].X, resultPoints[i].Y);
            }
            return CalculateQRPose(points, imageWidth, imageHeight, camera);
        }
    }
}
