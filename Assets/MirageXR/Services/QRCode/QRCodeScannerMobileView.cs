using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Screen-space mobile UI view for the QR code scanner.
    /// Positioned in layer order behind the home screen menu panel, providing a responsive
    /// reticle, status text, and camera preview thumbnail.
    /// Detects activity QR codes, triggers spatial calibration, and opens activities.
    /// </summary>
    public class QRCodeScannerMobileView : PopupBase
    {
        [Header("UI Elements")]
        [SerializeField] private RawImage _cameraPreview;
        [SerializeField] private TMP_Text _textStatus;
        [SerializeField] private GameObject _scanningReticle;

        [Header("Scanner Component")]
        [SerializeField] private QRCodeScanner _scanner;

        private Action<QRCodeScanResult> _onResultCallback;
        private bool _isProcessingResult;
        public event Action OnScannerClosed;

        protected override bool TryToGetArguments(params object[] args)
        {
            _showBackground = false;
            if (args != null && args.Length > 0 && args[0] is Action<QRCodeScanResult> callback)
            {
                _onResultCallback = callback;
            }
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            if (_scanner == null)
            {
                _scanner = gameObject.GetComponent<QRCodeScanner>() ?? gameObject.AddComponent<QRCodeScanner>();
            }

            _scanner.RequireActivityLink = true;
            _scanner.OnCameraStarted += OnCameraStarted;
            _scanner.OnQRCodeDetected += OnQRCodeDetected;
            _scanner.OnInvalidQRCodeDetected += OnInvalidQRCodeDetected;
            _scanner.OnScanError += OnScanError;

            if (_textStatus != null)
            {
                _textStatus.text = "Align QR Code within frame...";
            }

            ConfigureLayoutForDevice();

            StartScanningAsync().Forget();
        }

        private void ConfigureLayoutForDevice()
        {
            float horizontalAnchor = PlatformManager.GetDeviceFormat() == DeviceFormat.Tablet ? 0.35f : 0.5f;

            if (_scanningReticle != null && _scanningReticle.transform is RectTransform reticleRect)
            {
                reticleRect.anchorMin = new Vector2(horizontalAnchor, 0.5f);
                reticleRect.anchorMax = new Vector2(horizontalAnchor, 0.5f);
                reticleRect.anchoredPosition = new Vector2(0f, 30f);
            }

            if (_textStatus != null && _textStatus.transform is RectTransform statusRect)
            {
                statusRect.anchorMin = new Vector2(horizontalAnchor, 0.5f);
                statusRect.anchorMax = new Vector2(horizontalAnchor, 0.5f);
                statusRect.anchoredPosition = new Vector2(0f, -140f);
            }
        }

        private async UniTaskVoid StartScanningAsync()
        {
            if (_scanner != null)
            {
                await _scanner.StartScanningAsync();
            }
        }

        private void OnCameraStarted(Texture texture)
        {
            if (_cameraPreview != null && texture != null)
            {
                _cameraPreview.texture = texture;
                _cameraPreview.gameObject.SetActive(true);
                var aspectFitter = _cameraPreview.GetComponent<AspectRatioFitter>();
                if (aspectFitter != null && texture.height > 0)
                {
                    float aspect = (float)texture.width / texture.height;
                    if (texture is WebCamTexture webcam && (webcam.videoRotationAngle == 90 || webcam.videoRotationAngle == 270))
                    {
                        aspect = (float)texture.height / texture.width;
                    }
                    aspectFitter.aspectRatio = aspect;
                }

                if (texture is WebCamTexture cam)
                {
                    _cameraPreview.rectTransform.localEulerAngles = new Vector3(0, 0, -cam.videoRotationAngle);
                    float scaleY = cam.videoVerticallyMirrored ? -1f : 1f;
                    _cameraPreview.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                }
                else
                {
                    _cameraPreview.rectTransform.localEulerAngles = Vector3.zero;
                    _cameraPreview.rectTransform.localScale = Vector3.one;
                }
            }
        }

        private void OnInvalidQRCodeDetected(string rawText)
        {
            if (_textStatus != null)
            {
                _textStatus.text = "Not an activity QR code.\nKeep scanning...";
            }
        }

        private void OnScanError(string error)
        {
            if (_textStatus != null)
            {
                _textStatus.text = error;
            }
        }

        private void OnQRCodeDetected(QRCodeScanResult result)
        {
            if (result == null || _isProcessingResult)
            {
                return;
            }

            // If detected code is not a valid activity, inform the user and keep scanning
            if (result.ActivityId == Guid.Empty)
            {
                if (_textStatus != null)
                {
                    _textStatus.text = "Not an activity QR code.\nKeep scanning...";
                }
                return;
            }

            if (_onResultCallback != null)
            {
                Close();
                _onResultCallback.Invoke(result);
                return;
            }

            // Default processing flow: load activity while view remains active for feedback
            ProcessScanResultAsync(result).Forget();
        }

        private async UniTaskVoid ProcessScanResultAsync(QRCodeScanResult result)
        {
            _isProcessingResult = true;

            if (_scanner != null)
            {
                _scanner.StopScanning();
            }

            // Restore the home screen so it is rolled up when returning home
            RootView_v2.Instance?.activityListView?.ExpandPanel();

            if (_textStatus != null)
            {
                _textStatus.text = "Loading activity...";
            }

            var uprightPose = new Pose(result.WorldPose.position, result.WorldPose.rotation);
            Debug.Log($"[QRCodeScannerMobileView] Opening activity {result.ActivityId} with marker position: {uprightPose.position}");

            var loadView = LoadView.Instance;
            if (loadView != null)
            {
                loadView.Show();
            }

            bool loadSuccess = false;
            try
            {
                await RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(result.ActivityId);
                loadSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QRCodeScannerMobileView] Activity with id {result.ActivityId} not found or failed to load: {ex.Message}");
            }
            finally
            {
                if (loadView != null)
                {
                    loadView.Hide();
                }
            }

            if (loadSuccess)
            {
                // Calibrate task station to marker position (upright) after activity view is instantiated
                try
                {
                    var calibrationManager = RootObject.Instance?.CalibrationManager;
                    if (calibrationManager != null)
                    {
                        calibrationManager.FinishCalibration(uprightPose, false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[QRCodeScannerMobileView] Calibration note: {ex.Message}");
                }

                // Open activity with its menu minimized so the user can see AR markers in the camera view
                RootView_v2.Instance?.activityView?.MinimizeMenu();

                Close();
            }
            else
            {
                // Keep scanner open and ask user to scan again
                _isProcessingResult = false;
                if (_textStatus != null)
                {
                    _textStatus.text = "Activity not found.\nPlease scan another QR code...";
                }

                StartScanningAsync().Forget();
            }
        }

        public override void Close()
        {
            if (_scanner != null)
            {
                _scanner.StopScanning();
            }

            OnScannerClosed?.Invoke();
            base.Close();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_scanner != null)
            {
                _scanner.OnCameraStarted -= OnCameraStarted;
                _scanner.OnQRCodeDetected -= OnQRCodeDetected;
                _scanner.OnInvalidQRCodeDetected -= OnInvalidQRCodeDetected;
                _scanner.OnScanError -= OnScanError;
                _scanner.StopScanning();
            }
        }
    }
}
