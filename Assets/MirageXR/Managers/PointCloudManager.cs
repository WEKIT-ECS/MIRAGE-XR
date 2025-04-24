using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointCloudManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabPointCloud;
    [SerializeField] private MeshFilter _prefabSpatialMapMesh;

    private ARSession _arSession;
    private ARPointCloudManager _arPointCloudManager;
    private ARMeshManager _arMeshManager;

    public async Task<bool> InitializationAsync()
    {
        UnityEngine.Debug.Log("Initializing [PointCloudManager] <--");
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_VISIONOS
        return true;
#endif
        var mainCamera = Camera.main;

        if (!mainCamera)
        {
            Debug.Log("Can't find camera main");
            return false;
        }

        var cameraParent = mainCamera.transform.parent ? mainCamera.transform.parent.gameObject : mainCamera.gameObject;

        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>(cameraParent);

#if !UNITY_VISIONOS && !UNITY_IOS
        // if on Android, then add the pointcloud prefab (not supported on VisionOS)
        _arPointCloudManager = MirageXR.Utilities.FindOrCreateComponent<ARPointCloudManager>(cameraParent);
        _arPointCloudManager.pointCloudPrefab = _prefabPointCloud;
/* #else 
        // ar mesh manager is not working on the device build 
        _arMeshManager = MirageXR.Utilities.FindOrCreateComponent<ARMeshManager>(cameraParent);
        _arMeshManager.meshPrefab = _prefabSpatialMapMesh; */
#endif

        await Task.Yield();

        LearningExperienceEngine.EventManager.OnEditModeChanged += SetAllPointCloudsActive;
        UnityEngine.Debug.Log("Initializing [PointCloudManager] -->");
        return true;
    }

    public void Unsubscribe()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
        LearningExperienceEngine.EventManager.OnEditModeChanged -= SetAllPointCloudsActive;
#endif
    }

    public async Task<bool> ResetAsync()
    {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_VISIONOS
        return true;
#endif
        if (!_arSession)
        {
            Debug.LogError("ARSession is null");
            return false;
        }

#if UNITY_ANDROID

        if (!_arPointCloudManager)
        {
            Debug.LogError("ARPointCloudManager is null");
            return false;
        }

        //_arPointCloudManager.enabled = false;
        foreach (var arPointCloud in _arPointCloudManager.trackables)
        {
            Destroy(arPointCloud.gameObject);
        }

        Destroy(_arPointCloudManager);

#elif UNITY_IOS || UNITY_VISIONOS

        // TODO clean up the mesh
        if (!_arMeshManager)
        {
            Debug.LogError("ARMeshManager is null");
            return false;
        }
        Destroy(_arMeshManager);

#endif

        //_arSession.Reset();
        await Task.Yield();

        await InitializationAsync();
        //_arPointCloudManager.enabled = true;

        return true;
    }

    private void SetAllPointCloudsActive(bool value)
    {
#if UNITY_ANDROID
        foreach (var cloud in _arPointCloudManager.trackables)
        {
            cloud.gameObject.SetActive(value);
        }
#elif UNITY_IOS || UNITY_VISIONOS
        _arMeshManager.enabled = value;
#endif
    }
}
