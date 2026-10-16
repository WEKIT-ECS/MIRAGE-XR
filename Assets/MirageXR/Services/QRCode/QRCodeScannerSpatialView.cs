using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Head-locked spatial view for the QR code scanner.
    /// Floats directly in front of the camera as an optical HUD reticle with corner guides,
    /// a red horizontal scanning indicator, and a live corner thumbnail preview.
    /// Detects activity QR codes, sets spatial calibration to the code position, and loads activities.
    /// </summary>
    public class QRCodeScannerSpatialView : PopupBase
    {
        [Header("Positioning")]
        [SerializeField] private float _distanceFromCamera = 0.30f;

        [Header("UI Elements")]
        [SerializeField] private Button _btnClose;
        [SerializeField] private RawImage _cameraPreview;
        [SerializeField] private TMP_Text _textStatus;
        [SerializeField] private GameObject _scanningReticle;

        [Header("Scanner Component")]
        [SerializeField] private QRCodeScanner _scanner;

        private Action<QRCodeScanResult> _onResultCallback;

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

            AttachToCamera();

            if (_btnClose != null)
            {
                _btnClose.onClick.AddListener(Close);
            }

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

        /// <summary>
        /// Parents this HUD reticle to the camera at a fixed relative position in front of the user.
        /// </summary>
        public void AttachToCamera()
        {
            var cam = RootObject.Instance?.BaseCamera ?? Camera.main;
            if (cam != null)
            {
                transform.SetParent(cam.transform, false);
                transform.localPosition = new Vector3(0f, 0f, _distanceFromCamera);
                transform.localRotation = Quaternion.identity;
            }
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

            // Valid activity found: close HUD and process
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
            var spatialDialog = MenuManager.Instance?.Dialog;
            if (spatialDialog != null)
            {
                spatialDialog.ShowMiddle(title, message, "OK", () => { });
            }
            else if (DialogWindow.Instance != null)
            {
                DialogWindow.Instance.Show(title, message, new DialogButtonContent("Ok"));
            }
            else
            {
                Debug.LogWarning($"[QRCodeScannerSpatialView] {title}: {message}");
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
                Debug.LogWarning($"[QRCodeScannerSpatialView] QR code does not contain a valid activity link: {result.RawText}");
                ShowDialog("Invalid QR Code",
                    "The scanned QR code is not a recognized activity link.\n\n" + result.RawText);
                return;
            }

            // Upright pose with yaw rotation from detected QR marker pose
            var uprightPose = new Pose(result.WorldPose.position, result.WorldPose.rotation);

            Debug.Log($"[QRCodeScannerSpatialView] Opening activity {result.ActivityId} with marker position: {uprightPose.position}");

            // Open / load the activity and calibrate once loaded
            OpenActivityAsync(result.ActivityId, uprightPose).Forget();
        }

        private static async UniTaskVoid OpenActivityAsync(Guid activityId, Pose uprightPose)
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
                Debug.LogError($"[QRCodeScannerSpatialView] Failed to load activity {activityId}: {ex.Message}");
                ShowDialog("Activity Not Found",
                    $"Activity with ID '{activityId}' could not be opened or does not exist.");
                return;
            }
            finally
            {
                if (loadView != null)
                {
                    loadView.Hide();
                }
            }

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
                Debug.LogError($"[QRCodeScannerSpatialView] Error during calibration: {ex.Message}");
            }
        }

        public override void Close()
        {
            if (_scanner != null)
            {
                _scanner.StopScanning();
            }

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
