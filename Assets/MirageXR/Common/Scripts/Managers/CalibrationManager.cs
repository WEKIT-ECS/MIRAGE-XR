using System;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class CalibrationManager : MonoBehaviour
{
    private static ImageTargetManagerWrapper imageTargetManager => RootObject.Instance.imageTargetManager;

    private static FloorManager floorManager => RootObject.Instance.floorManager;

    private static float ANIMATION_TIME = 5f;

    [SerializeField] private GameObject _calibrationImageTargetPrefab;
    [SerializeField] private Texture2D _targetImage;
    [SerializeField] private UnityEvent _onCalibrationStarted = new UnityEvent();
    [SerializeField] private UnityEvent _onCalibrationCanceled = new UnityEvent();
    [SerializeField] private UnityEvent _onCalibrationFinished = new UnityEvent();

    public UnityEvent onCalibrationStarted => _onCalibrationStarted;

    public UnityEvent onCalibrationCanceled => _onCalibrationCanceled;

    public UnityEvent onCalibrationFinished => _onCalibrationFinished;

    public Transform anchor => _anchor;

    public float animationTime => ANIMATION_TIME;

    private Transform _anchor;
    private ImageTargetModel _imageTargetModel;
    private IImageTarget _imageTarget;
    private ARAnchorManager _arAnchorManager;
    private bool _isEnabled;
    private bool _isRecalibration;
    private bool _isWaitingForImageTarget;
    private CalibrationTool _calibrationTool;
    private ARAnchor _arAnchor;

    public async Task<bool> InitializationAsync()
    {
        var mainCamera = Camera.main;

        if (!mainCamera)
        {
            Debug.LogError("Can't find camera main");
            return false;
        }

        var cameraParent = mainCamera.transform.parent ? mainCamera.transform.parent.gameObject : mainCamera.gameObject;

        _anchor = CreateAnchor();
        _imageTargetModel = new ImageTargetModel
        {
            name = "calibrationImageTarget",
            width = 0.3f,
            texture2D = _targetImage,
            prefab = _calibrationImageTargetPrefab,
            useLimitedTracking = false,
        };

        _arAnchorManager = Utilities.FindOrCreateComponent<ARAnchorManager>(cameraParent);

        return true;
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

        if (_imageTarget != null && _calibrationTool)
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
                AppLog.LogError("Unable to create imageTarget");
                return;
            }
        }

        if (!InitCalibrationTool(_imageTarget))
        {
            AppLog.LogError("Unable to create calibrationTool");
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
            AppLog.LogError(e.ToString());
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
            AppLog.LogError($"{nameof(CalibrationTool)} cannot be found");
            return false;
        }

        _calibrationTool.Initialization(ANIMATION_TIME);
        _calibrationTool.onCalibrationStarted.RemoveAllListeners();
        _calibrationTool.onCalibrationStarted.AddListener(OnCalibrationStarted);
        _calibrationTool.onCalibrationCanceled.RemoveAllListeners();
        _calibrationTool.onCalibrationCanceled.AddListener(OnCalibrationCanceled);
        _calibrationTool.onCalibrationFinished.RemoveAllListeners();
        _calibrationTool.onCalibrationFinished.AddListener(OnCalibrationFinished);

        return true;
    }

    private void OnCalibrationStarted()
    {
        _onCalibrationStarted.Invoke();
    }

    private void OnCalibrationCanceled()
    {
        _onCalibrationCanceled.Invoke();
    }

    private void OnCalibrationFinished()
    {
        OnCalibrationFinishedAsync().AsAsyncVoid();
    }

    private async Task OnCalibrationFinishedAsync()
    {
        UpdateAnchorPosition();
        await RootObject.Instance.workplaceManager.CalibrateWorkplace(_anchor, _isRecalibration);
        DisableCalibration();
        _onCalibrationFinished.Invoke();
    }

    private void UpdateAnchorPosition()
    {
        var arAnchor = floorManager.CreateAnchor(new Pose(_calibrationTool.transform.position, Quaternion.identity));

        if (!arAnchor)
        {
            Debug.LogError("Can't create arAnchor");
            return;
        }

        var capsule = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        capsule.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        //_anchor.transform.position = _calibrationTool.transform.position;
        //_anchor.transform.rotation = Quaternion.AngleAxis(_calibrationTool.transform.rotation.eulerAngles.y, Vector3.up);

        _anchor.SetParent(arAnchor.transform);
        _anchor.localPosition = Vector3.zero;
        _anchor.localRotation = Quaternion.identity;

        if (_arAnchor)
        {
            Destroy(_arAnchor.gameObject);
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

        if (DBManager.developMode)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(anchorTransform, true);
            capsule.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }

        return anchorTransform;
    }
}
