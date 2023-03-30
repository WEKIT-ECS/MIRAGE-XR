using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;

public class CalibrationManager : MonoBehaviour
{
    private static ImageTargetManagerWrapper imageTargetManager => RootObject.Instance.imageTargetManager;

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
    private string _targetName;
    private bool _isEnabled;
    private bool _isRecalibration;
    private CalibrationTool _calibrationTool;


    public void Initialization()
    {
        _anchor = CreateAnchor();
        _imageTargetModel = new ImageTargetModel
        {
            name = "calibrationImageTarget",
            width = 0.3f,
            texture2D = _targetImage,
            prefab = _calibrationImageTargetPrefab,
            useLimitedTracking = false,
        };
    }

    public void EnableCalibration(bool isRecalibration = false)
    {
        EnableCalibrationAsync(isRecalibration).AsAsyncVoid();
    }

    private async Task EnableCalibrationAsync(bool isRecalibration = false)
    {
        if (_isEnabled)
        {
            return;
        }

        bool result;
        (result, _targetName) = await imageTargetManager.TryAddImageTarget(_imageTargetModel);
        if (result)
        {
            imageTargetManager.onTargetCreated.AddListener(OnImageTargetCreated);
        }

        _isRecalibration = isRecalibration;
        _isEnabled = true;
    }

    public void DisableCalibration()
    {
        if (!_isEnabled)
        {
            return;
        }

        imageTargetManager.TryRemoveImageTarget(_targetName);
        _isRecalibration = false;
        _isEnabled = false;
    }

    private void OnImageTargetCreated(IImageTarget imageTarget)
    {
        if (imageTarget.imageTargetName != _targetName)
        {
            return;
        }

        _calibrationTool = imageTarget.targetObject.GetComponent<CalibrationTool>();
        if (!_calibrationTool)
        {
            AppLog.LogError($"{nameof(CalibrationTool)} cannot be found");
            return;
        }

        _calibrationTool.Initialization(ANIMATION_TIME);
        _calibrationTool.onCalibrationStarted.AddListener(OnCalibrationStarted);
        _calibrationTool.onCalibrationCanceled.AddListener(OnCalibrationCanceled);
        _calibrationTool.onCalibrationFinished.AddListener(OnCalibrationFinished);

        imageTargetManager.onTargetCreated.RemoveListener(OnImageTargetCreated);
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
        _anchor.transform.position = _calibrationTool.transform.position;
        _anchor.transform.rotation = _calibrationTool.transform.rotation;
    }

    private static Transform CreateAnchor()
    {
        var obj = new GameObject("Anchor");
        var anchorTransform = obj.transform;
        anchorTransform.position = Vector3.zero;
        anchorTransform.rotation = Quaternion.identity;
        anchorTransform.localScale = Vector3.one;

#if UNITY_EDITOR
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(anchorTransform, true);
        capsule.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
#endif
        return anchorTransform;
    }
}
