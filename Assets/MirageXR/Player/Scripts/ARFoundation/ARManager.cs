using MirageXR;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.XR.ARFoundation;
#endif

public class ARManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _prefabPlace;
    [SerializeField] private GameObject _prefabPointCloud;

#if UNITY_ANDROID || UNITY_IOS
    private ARPlaneManager _arPlaneManager;
    private ARPointCloudManager _arPointCloudManager;
#endif

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        Init();
#endif
    }

    private void Init()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!_mainCamera) _mainCamera = Camera.main;
        if (!_mainCamera) return;

        var cameraParent = _mainCamera.transform.parent.gameObject;

        _arPlaneManager = cameraParent.AddComponent<ARPlaneManager>();
        _arPointCloudManager = cameraParent.AddComponent<ARPointCloudManager>();

        _arPlaneManager.planePrefab = _prefabPlace;
        _arPointCloudManager.pointCloudPrefab = _prefabPointCloud;

        _arPlaneManager.enabled = false;
        _arPointCloudManager.enabled = false;
        EventManager.OnEditModeChanged += EnablePlaneDetection;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        EventManager.OnEditModeChanged -= EnablePlaneDetection;
#endif
    }

#if UNITY_ANDROID || UNITY_IOS
    private void EnablePlaneDetection(bool value)
    {
        _arPlaneManager.enabled = value;
        _arPointCloudManager.enabled = value;
        SetAllPlanesActive(value);
        SetAllPointCloudsActive(value);
    }

    private void SetAllPlanesActive(bool value)
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
    }

    private void SetAllPointCloudsActive(bool value)
    {
        foreach (var cloud in _arPointCloudManager.trackables)
        {
            cloud.gameObject.SetActive(value);
        }
    }
#endif
}
