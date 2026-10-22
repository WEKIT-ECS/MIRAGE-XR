using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class FloorManagerWrapper : MonoBehaviour
{
    [Serializable]
    public enum ForceManagerType
    {
        Default,
        Editor,
        ARFoundation,
    }

    [SerializeField] private ForceManagerType _forceManagerType = ForceManagerType.Default;
    [SerializeField] private GameObject _prefabARFoundationAnchor;
    [SerializeField] private GameObject _prefabEditorAnchor;

    private IFloorManager _floorManager;

    public async Task InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        UnityEngine.Debug.Log("Initializing [FloorManagerWrapper] <--");
#if UNITY_WSA
        _forceManagerType = ForceManagerType.ARFoundation;
#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS || VISION_OS)
        _forceManagerType = ForceManagerType.ARFoundation;
#else
        _forceManagerType = ForceManagerType.Editor;
#endif
        _floorManager = CreateFloorManager();

        if (_floorManager == null)
        {
            return;
        }

        _floorManager.OnFloorLevelChanged += (level) => OnFloorLevelChanged?.Invoke(level);

        try
        {
            var result = await _floorManager.InitializationAsync(viewManager, planeManager);
            if (!result)
            {
                Debug.Log("FloorManagerWrapper: unable to initialize");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        UnityEngine.Debug.Log("Initializing [FloorManagerWrapper] -->");
    }

    public event Action<float> OnFloorLevelChanged;

    public float floorLevel => _floorManager != null ? _floorManager.floorLevel : -1.6f;

    public bool isFloorDetected => _floorManager != null && _floorManager.isFloorDetected;

    public bool isManualFloor => _floorManager != null && _floorManager.isManualFloor;

    public Task<bool> ResetAsync()
    {
        return _floorManager.ResetAsync();
    }

    public Transform CreateAnchor(Pose pose)
    {
        return _floorManager.CreateAnchor(pose);
    }

    public void SetFloor(PlaneId planeId, Vector3 position)
    {
        _floorManager.SetFloor(planeId, position);
        RootObject.Instance.PlaneManager.SelectPlane(planeId);
    }

    public void ForceDetectFloor()
    {
        _floorManager?.ForceDetectFloor();
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
                return managerEditor;
            case ForceManagerType.ARFoundation:
                var managerARFoundation = gameObject.AddComponent<FloorManagerARFoundation>();
                managerARFoundation.prefabAnchor = _prefabARFoundationAnchor;
                return managerARFoundation;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IFloorManager CreateDefaultImageTargetManager()
    {
#if UNITY_EDITOR
        var floorManager = gameObject.AddComponent<FloorManagerEditor>();
        floorManager.prefabAnchor = _prefabEditorAnchor;
#elif UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS
        var floorManager = gameObject.AddComponent<FloorManagerARFoundation>();
        floorManager.prefabAnchor = _prefabARFoundationAnchor;
#else
        var floorManager = gameObject.AddComponent<FloorManagerARFoundation>();
        floorManager.prefabAnchor = _prefabARFoundationAnchor;
#endif
        return floorManager;
    }
}
