using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FloorManagerWrapper : MonoBehaviour
{
    [Serializable]
    public enum ForceManagerType
    {
        Default,
        Editor,
        ARFoundation,
        MRTK,
    }

    [SerializeField] private ForceManagerType _forceManagerType = ForceManagerType.Default;
    [SerializeField] private GameObject _prefabARFoundationAnchor;
    [SerializeField] private GameObject _prefabARFoundationPlane;
    [SerializeField] private GameObject _prefabEditorAnchor;
    [SerializeField] private GameObject _prefabEditorPlane;

    [SerializeField] private UnityEvent _onDetectionEnabled = new UnityEvent();
    [SerializeField] private UnityEvent _onDetectionDisabled = new UnityEvent();

    public UnityEvent onDetectionEnabled => _onDetectionEnabled;

    public UnityEvent onDetectionDisabled => _onDetectionDisabled;

    private IFloorManager _floorManager;

    public IFloorManager manager => _floorManager;

    public async Task InitializationAsync()
    {
        _floorManager = CreateFloorManager();

        if (_floorManager == null)
        {
            return;
        }

        try
        {
            var result = await _floorManager.InitializationAsync();
            if (!result)
            {
                Debug.Log("FloorManagerWrapper: unable to initialize");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public bool enableColliders => _floorManager.enableColliders;

    public bool showPlanes => _floorManager.showPlanes;

    public float floorLevel => _floorManager.floorLevel;

    public bool isFloorDetected => _floorManager.isFloorDetected;

    public Task<bool> ResetAsync()
    {
        return _floorManager.ResetAsync();
    }

    public Transform CreateAnchor(Pose pose)
    {
        return _floorManager.CreateAnchor(pose);
    }

    public void SetFloor(IPlaneBehaviour floor)
    {
        _floorManager.SetFloor(floor);
    }

    public void EnableFloorDetection(Action onFloorDetected)
    {
        _onDetectionEnabled.Invoke();
        _floorManager.EnableFloorDetection(onFloorDetected);
    }

    public void DisableFloorDetection()
    {
        onDetectionDisabled.Invoke();
        _floorManager.DisableFloorDetection();
    }

    public void Dispose()
    {
        _floorManager.Dispose();
    }

    private IFloorManager CreateFloorManager()
    {
        switch (_forceManagerType)
        {
            case ForceManagerType.Default:
                return CreateDefaultImageTargetManager();
            case ForceManagerType.Editor:
                var managerEditor = gameObject.AddComponent<FloorManagerEditor>();
                managerEditor.prefabAnchor = _prefabEditorAnchor;
                managerEditor.prefabPlane = _prefabEditorPlane;
                return managerEditor;
            case ForceManagerType.ARFoundation:
                var managerARFoundation = gameObject.AddComponent<FloorManagerARFoundation>();
                managerARFoundation.prefabAnchor = _prefabARFoundationAnchor;
                managerARFoundation.prefabPlane = _prefabARFoundationPlane;
                return managerARFoundation;
            case ForceManagerType.MRTK:
                return gameObject.AddComponent<FloorManagerMRTK>();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IFloorManager CreateDefaultImageTargetManager()
    {
#if UNITY_EDITOR
        var floorManager = gameObject.AddComponent<FloorManagerEditor>();
        floorManager.prefabAnchor = _prefabEditorAnchor;
        floorManager.prefabPlane = _prefabEditorPlane;
#elif UNITY_IOS || UNITY_ANDROID
        var floorManager = gameObject.AddComponent<FloorManagerARFoundation>();
        floorManager.prefabAnchor = _prefabARFoundationAnchor;
        floorManager.prefabPlane = _prefabARFoundationPlane;
#else
        var floorManager = gameObject.AddComponent<FloorManagerMRTK>();
#endif
        return floorManager;
    }
}
