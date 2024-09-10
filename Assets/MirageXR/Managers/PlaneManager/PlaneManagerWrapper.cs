using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventPlaneIdVector3 : UnityEvent<PlaneId, Vector3> { }

public class UnityEventPlaneIdPlaneId : UnityEvent<PlaneId, PlaneId> { }

public struct PlaneId
{
    public static PlaneId InvalidId = new PlaneId(0, 0);

    public ulong subId1;
    public ulong subId2;

    public PlaneId(ulong subId1, ulong subId2)
    {
        this.subId1 = subId1;
        this.subId2 = subId2;
    }

    public static bool operator ==(PlaneId lhs, PlaneId rhs) => lhs.subId1 == rhs.subId1 && lhs.subId2 == rhs.subId2;

    public static bool operator !=(PlaneId lhs, PlaneId rhs) => lhs.subId1 != rhs.subId1 || lhs.subId2 != rhs.subId2;
}

public class PlaneManagerWrapper : MonoBehaviour
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
    [SerializeField] private GameObject _prefabARFoundationPlane;
    [SerializeField] private GameObject _prefabEditorPlane;
    [SerializeField] private UnityEvent _onDetectionEnabled = new UnityEvent();
    [SerializeField] private UnityEvent _onDetectionDisabled = new UnityEvent();

    public UnityEventPlaneIdVector3 onPlaneClicked => _manager.onPlaneClicked;
    public UnityEventPlaneIdPlaneId onPlaneRemoved => _manager.onPlaneRemoved;

    public UnityEvent onDetectionEnabled => _onDetectionEnabled;

    public UnityEvent onDetectionDisabled => _onDetectionDisabled;

    private IPlaneManager _manager;

    public async Task InitializationAsync()
    {
        _manager = CreateManager();

        if (_manager == null)
        {
            return;
        }

        try
        {
            var result = await _manager.InitializationAsync();
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

    public Task<bool> ResetAsync()
    {
        return _manager.ResetAsync();
    }

    public void EnablePlanes()
    {
        _onDetectionEnabled.Invoke();
        _manager.EnablePlanes();
    }

    public void DisablePlanes()
    {
        _onDetectionDisabled.Invoke();
        _manager.DisablePlanes();
    }

    public void SelectPlane(PlaneId planeId)
    {
        _manager.SelectPlane(planeId);
    }

    public GameObject GetPlane(PlaneId planeId)
    {
        return _manager.GetPlane(planeId);
    }

    public GameObject GetRandomPlane()
    {
        return _manager.GetRandomPlane();
    }

    public void UpdatePlanes()
    {
        _manager.UpdatePlanes();
    }

    public void Dispose()
    {
        _manager?.Dispose();
    }

    private IPlaneManager CreateManager()
    {
        switch (_forceManagerType)
        {
            case ForceManagerType.Default:
                return CreateDefaultManager();
            case ForceManagerType.Editor:
                var managerEditor = gameObject.AddComponent<PlaneManagerEditor>();
                managerEditor.prefabPlane = _prefabEditorPlane;
                return managerEditor;
            case ForceManagerType.ARFoundation:
                var managerARFoundation = gameObject.AddComponent<PlaneManagerARFoundation>();
                managerARFoundation.prefabPlane = _prefabARFoundationPlane;
                return managerARFoundation;
            case ForceManagerType.MRTK:
                return gameObject.AddComponent<PlaneManagerMRTK>();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IPlaneManager CreateDefaultManager()
    {
#if UNITY_EDITOR
        var manager = gameObject.AddComponent<PlaneManagerEditor>();
        manager.prefabPlane = _prefabEditorPlane;
#elif UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS
        var manager = gameObject.AddComponent<PlaneManagerARFoundation>();
        manager.prefabPlane = _prefabARFoundationPlane;
#else
        var manager = gameObject.AddComponent<PlaneManagerMRTK>();
#endif
        return manager;
    }
}
