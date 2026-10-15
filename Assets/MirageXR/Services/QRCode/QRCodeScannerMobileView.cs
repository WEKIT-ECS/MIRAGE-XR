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

            StartScanningAsync().Forget();
        }

        private async UniTaskVoid StartScanningAsync()
        {
            if (_scanner != null)
            {
                await _scanner.StartScanningAsync();
            }
        }

        private void OnCameraStarted(Texture2D texture)
        {
            if (_cameraPreview != null && texture != null)
            {
                _cameraPreview.texture = texture;
                _cameraPreview.gameObject.SetActive(true);
                var aspectFitter = _cameraPreview.GetComponent<AspectRatioFitter>();
                if (aspectFitter != null && texture.height > 0)
                {
                    aspectFitter.aspectRatio = (float)texture.width / texture.height;
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
            if (result == null)
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

            // Valid activity found: close view and process
            Close();

            if (_onResultCallback != null)
            {
                _onResultCallback.Invoke(result);
                return;
            }

            // Default processing flow
            ProcessScanResult(result);
        }

        public static void ShowDialog(string title, string message)
        {
            if (DialogWindow.Instance != null)
            {
                DialogWindow.Instance.Show(title, message, new DialogButtonContent("Ok"));
            }
            else if (RootView_v2.Instance != null && RootView_v2.Instance.dialog != null)
            {
                RootView_v2.Instance.dialog.ShowMiddle(title, message, "OK", () => { });
            }
            else
            {
                Debug.LogWarning($"[QRCodeScannerMobileView] {title}: {message}");
            }
        }

        public static void ProcessScanResult(QRCodeScanResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.ActivityId == Guid.Empty)
            {
                Debug.LogWarning($"[QRCodeScannerMobileView] QR code does not contain a valid activity link: {result.RawText}");
                ShowDialog("Invalid QR Code", "The scanned QR code is not a recognized activity link.\n\n" + result.RawText);
                return;
            }

            Debug.Log($"[QRCodeScannerMobileView] Calibrating to QR code position: {result.WorldPose.position} and opening activity {result.ActivityId}");

            // 1. Immediately calibrate activity world origin to QR code position
            try
            {
                var calibrationManager = RootObject.Instance?.CalibrationManager;
                if (calibrationManager != null)
                {
                    calibrationManager.FinishCalibration(result.WorldPose, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScannerMobileView] Error during calibration: {ex.Message}");
            }

            // 2. Open / load the activity
            OpenActivityAsync(result.ActivityId).Forget();
        }

        private static async UniTaskVoid OpenActivityAsync(Guid activityId)
        {
            var loadView = LoadView.Instance;
            if (loadView != null)
            {
                loadView.Show();
            }

            try
            {
                await RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(activityId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QRCodeScannerMobileView] Failed to load activity {activityId}: {ex.Message}");
                ShowDialog("Activity Not Found", $"Activity with ID '{activityId}' could not be opened or does not exist.");
            }
            finally
            {
                if (loadView != null)
                {
                    loadView.Hide();
                }
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
