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
        var mainCamera = Camera.main;

        if (!mainCamera)
        {
            Debug.LogError("Can't find camera main");
            return false;
        }

        var cameraParent = mainCamera.transform.parent ? mainCamera.transform.parent.gameObject : mainCamera.gameObject;

        _arSession = Utilities.FindOrCreateComponent<ARSession>(cameraParent);
        _arPointCloudManager = Utilities.FindOrCreateComponent<ARPointCloudManager>(cameraParent);

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

        _arPointCloudManager.enabled = false;

        _arSession.Reset();
        await Task.Yield();

        _arPointCloudManager.enabled = true;

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
