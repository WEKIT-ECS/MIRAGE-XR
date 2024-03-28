using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointCloudManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabPointCloud;

    private ARSession _arSession;
    private ARPointCloudManager _arPointCloudManager;

    public async Task<bool> InitializationAsync()
    {
#if !UNITY_ANDROID && !UNITY_IOS
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
        _arPointCloudManager = MirageXR.Utilities.FindOrCreateComponent<ARPointCloudManager>(cameraParent);

        _arPointCloudManager.pointCloudPrefab = _prefabPointCloud;

        await Task.Yield();

        EventManager.OnEditModeChanged += SetAllPointCloudsActive;
        return true;
    }

    public void Unsubscribe()
    {
        EventManager.OnEditModeChanged -= SetAllPointCloudsActive;
    }

    public async Task<bool> ResetAsync()
    {
#if !UNITY_ANDROID && !UNITY_IOS
        return true;
#endif
        if (!_arSession)
        {
            Debug.LogError("ARSession is null");
            return false;
        }

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

        //_arSession.Reset();
        await Task.Yield();

        await InitializationAsync();
        //_arPointCloudManager.enabled = true;

        return true;
    }

    private void SetAllPointCloudsActive(bool value)
    {
        foreach (var cloud in _arPointCloudManager.trackables)
        {
            cloud.gameObject.SetActive(value);
        }
    }
}
