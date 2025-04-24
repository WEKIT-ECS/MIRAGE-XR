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
        MRTK,
    }

    [SerializeField] private ForceManagerType _forceManagerType = ForceManagerType.Default;
    [SerializeField] private GameObject _prefabARFoundationAnchor;
    [SerializeField] private GameObject _prefabEditorAnchor;

    private IFloorManager _floorManager;

    public async Task InitializationAsync()
    {
        UnityEngine.Debug.Log("Initializing [FloorManagerWrapper] <--");
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS || VISION_OS
        _forceManagerType = ForceManagerType.ARFoundation;
#endif
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
        UnityEngine.Debug.Log("Initializing [FloorManagerWrapper] -->");
    }

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

    public void SetFloor(PlaneId planeId, Vector3 position)
    {
        _floorManager.SetFloor(planeId, position);
        RootObject.Instance.PlaneManager.SelectPlane(planeId);
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
#elif UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS
        var floorManager = gameObject.AddComponent<FloorManagerARFoundation>();
        floorManager.prefabAnchor = _prefabARFoundationAnchor;
#else
        var floorManager = gameObject.AddComponent<FloorManagerMRTK>();
#endif
        return floorManager;
    }
}
