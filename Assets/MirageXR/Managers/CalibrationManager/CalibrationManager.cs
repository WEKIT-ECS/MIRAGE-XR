using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public class CalibrationManager : ICalibrationManager
    {
        private static LearningExperienceEngine.BrandManager brandManager =>
            LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;

        private static ImageTargetManagerWrapper imageTargetManager => RootObject.Instance.ImageTargetManager;

        private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;

        private static float ANIMATION_TIME = 5f;
        private static float IMAGE_TARGET_WIGTH = 0.19f;

        public UnityEvent OnCalibrationStarted => _onCalibrationStarted;
        public UnityEvent OnCalibrationCanceled => _onCalibrationCanceled;
        public UnityEvent OnCalibrationFinished => _onCalibrationFinished;

        public Transform Anchor => _anchor;
        public bool IsInitialized => _isInitialized;
        public float AnimationTime => ANIMATION_TIME;
        public bool IsCalibrated => _isCalibrated;

        private readonly UnityEvent _onCalibrationStarted = new ();
        private readonly UnityEvent _onCalibrationCanceled = new ();
        private readonly UnityEvent _onCalibrationFinished = new ();

        private Transform _anchor;
        private ImageTargetModel _imageTargetModel;
        private IImageTarget _imageTarget;
        private bool _isEnabled;
        private bool _isRecalibration;
        private bool _isWaitingForImageTarget;
        private CalibrationTool _calibrationTool;
        private Transform _arAnchor;
        private GameObject _debugSphere;
        private bool _isCalibrated;
        private bool _isInitialized;
        private IAssetBundleManager _assetsManager;
        private IAuthorizationManager _authorizationManager;

        public UniTask WaitForInitialization()
        {
            return UniTask.WaitUntil(() => _isInitialized);
        }

        public async UniTask InitializationAsync(IAssetBundleManager assetsManager, IAuthorizationManager authorizationManager)
        {
            UnityEngine.Debug.Log("Initializing [CalibrationManager] <--");
            _assetsManager = assetsManager;
            _authorizationManager = authorizationManager;

            await _authorizationManager.WaitForInitialization();
            await _assetsManager.WaitForInitialization();

            _anchor = CreateAnchor();
            _imageTargetModel = new ImageTargetModel
            {
                name = "calibrationImageTarget",
                width = IMAGE_TARGET_WIGTH,
                texture2D = brandManager.CalibrationMarker,
                prefab = _assetsManager.GetCalibrationToolPrefab().gameObject,
                useLimitedTracking = false,
            };

            LearningExperienceEngine.EventManager.OnSetPoseSynchronizerTargetToCalibrationAnchor += OnSetCalibrationAnchor;
            _isInitialized = true;
            UnityEngine.Debug.Log("Initializing [CalibrationManager] -->");
        }

        public void EnableCalibration(bool isRecalibration = false)
        {
            EnableCalibrationAsync(isRecalibration).AsAsyncVoid();
        }

        public void DisableCalibration()
        {
            if (!_isEnabled)
            {
                return;
            }

            _isRecalibration = false;
            _isEnabled = false;

            if (_imageTarget != null && _calibrationTool != null)
            {
                _calibrationTool.Disable();
            }
        }

        private async Task EnableCalibrationAsync(bool isRecalibration = false)
        {
            _isEnabled = true;
            _isRecalibration = isRecalibration;

            if (_isWaitingForImageTarget)
            {
                return;
            }

            _imageTarget = imageTargetManager.GetImageTarget(_imageTargetModel.name);

            if (_imageTarget == null || !_calibrationTool)
            {
                var value = await CreateCalibrationTool(isRecalibration);
                if (!value)
                {
                    Debug.LogError("Unable to create imageTarget");
                    return;
                }
            }

            if (!InitCalibrationTool(_imageTarget))
            {
                Debug.LogError("Unable to create calibrationTool");
                return;
            }

            if (_isEnabled)
            {
                _calibrationTool.Enable();
            }
            else
            {
                _calibrationTool.Disable();
            }
        }

        private async Task<bool> CreateCalibrationTool(bool isRecalibration = false)
        {
            _isRecalibration = isRecalibration;
            _isEnabled = true;
            _isWaitingForImageTarget = true;

            try
            {
                _imageTarget = await imageTargetManager.AddImageTarget(_imageTargetModel);
                return _imageTarget != null;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return false;
            }
            finally
            {
                _isWaitingForImageTarget = false;
            }
        }

        private bool InitCalibrationTool(IImageTarget imageTarget)
        {
            _calibrationTool = imageTarget.targetObject.GetComponent<CalibrationTool>();
            if (!_calibrationTool)
            {
                Debug.LogError($"{nameof(CalibrationTool)} cannot be found");
                return false;
            }

            _calibrationTool.Initialization(ANIMATION_TIME);

            return true;
        }

        public void StartCalibration()
        {
            _onCalibrationStarted.Invoke();
        }

        public void CancelCalibration()
        {
            _onCalibrationCanceled.Invoke();
        }

        public void FinishCalibration(Pose pose)
        {
            OnCalibrationFinishedAsync(pose);
        }

        public Pose GetAnchorPositionAsync()
        {
            return _anchor.GetPose();
        }

        public void SetAnchorPosition(Pose pose)
        {
            UpdateAnchorPosition(pose);
        }

        public async Task ApplyCalibrationAsync(bool resetAnchor)
        {
            await LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.CalibrateWorkplace(
                resetAnchor);
            _isCalibrated = true;
        }

        private void OnCalibrationFinishedAsync(Pose pose)
        {
            SetAnchorPosition(pose);
            //await ApplyCalibrationAsync(_isRecalibration);
            DisableCalibration();
            _onCalibrationFinished.Invoke();
            //TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.CALIBRATION_FINISHED);
        }

        private void UpdateAnchorPosition(Pose pose)
        {
            var arAnchor = floorManager.CreateAnchor(pose);

            if (!arAnchor)
            {
                Debug.LogError("Can't create arAnchor");
                return;
            }

            _anchor.SetParent(arAnchor.transform);
            _anchor.localPosition = Vector3.zero;
            _anchor.localRotation = Quaternion.identity;

            if (_arAnchor)
            {
                Object.Destroy(_arAnchor.gameObject);
            }

            _arAnchor = arAnchor;
        }

        private static Transform CreateAnchor()
        {
            var obj = new GameObject("Anchor");
            var anchorTransform = obj.transform;
            anchorTransform.position = Vector3.zero;
            anchorTransform.rotation = Quaternion.identity;
            anchorTransform.localScale = Vector3.one;

//#if UNITY_EDITOR
//        anchorTransform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
//        anchorTransform.position = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 2f), Random.Range(-3f, 3f));
//#endif

            //var capsule = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //capsule.transform.SetParent(anchorTransform, true);
            //capsule.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            return anchorTransform;
        }

        private void OnSetCalibrationAnchor(LearningExperienceEngine.PoseSynchronizer pose)
        {
            Debug.LogTrace("[CalibrationManager] OnGetCalibrationAnchor callback received: setting PoseSynchronizer target to calibration anchor.");
            pose.target = Anchor;
        }
    }
}