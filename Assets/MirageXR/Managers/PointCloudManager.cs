using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointCloudManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabPointCloud;
    [SerializeField] private MeshFilter _prefabSpatialMapMesh;
    [SerializeField] private Material _occlusionMaterial;
    [SerializeField] private Material _wireframeMaterial;

    private ARSession _arSession;
    private ARPointCloudManager _arPointCloudManager;
    private ARMeshManager _arMeshManager;
    private IViewManager _viewManager;
    private bool _isMeshActive;

    public async Task InitializationAsync(IViewManager viewManager)
    {
        UnityEngine.Debug.Log("Initializing [PointCloudManager] <--");
        _viewManager = viewManager;
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_VISIONOS
        return;
#endif
        var mainCamera = _viewManager.Camera;

        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>(_viewManager.BaseCamera.gameObject);

#if !UNITY_VISIONOS && !UNITY_IOS
        // if on Android, then add the pointcloud prefab (not supported on VisionOS)
        _arPointCloudManager = MirageXR.Utilities.FindOrCreateComponent<ARPointCloudManager>(_viewManager.BaseCamera.gameObject);
        _arPointCloudManager.pointCloudPrefab = _prefabPointCloud;
#else 
        Transform baseCameraTransform = _viewManager.BaseCamera.transform;
        Transform childHolder = baseCameraTransform.Find("ARMeshManager");
        if (childHolder == null)
        {
            var go = new GameObject("ARMeshManager");
            go.transform.SetParent(baseCameraTransform, false);
            childHolder = go.transform;
        }
        _arMeshManager = MirageXR.Utilities.FindOrCreateComponent<ARMeshManager>(childHolder.gameObject);
        if (_prefabSpatialMapMesh != null)
        {
            _arMeshManager.meshPrefab = _prefabSpatialMapMesh;
        }
        _arMeshManager.meshesChanged += OnMeshesChanged;
#endif

        await Task.Yield();

        LearningExperienceEngine.EventManager.OnEditModeChanged += SetAllPointCloudsActive;
        UnityEngine.Debug.Log("Initializing [PointCloudManager] -->");
    }

    public void Unsubscribe()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
        LearningExperienceEngine.EventManager.OnEditModeChanged -= SetAllPointCloudsActive;
#endif
#if UNITY_IOS || UNITY_VISIONOS
        if (_arMeshManager != null)
        {
            _arMeshManager.meshesChanged -= OnMeshesChanged;
        }
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

        if (_arPointCloudManager != null)
        {
            foreach (var arPointCloud in _arPointCloudManager.trackables)
            {
                if (arPointCloud != null)
                {
                    Destroy(arPointCloud.gameObject);
                }
            }

            Destroy(_arPointCloudManager);
            _arPointCloudManager = null;
        }

#elif UNITY_IOS || UNITY_VISIONOS

        if (_arMeshManager != null)
        {
            Destroy(_arMeshManager);
            _arMeshManager = null;
        }

#endif

        //_arSession.Reset();
        await Task.Yield();

        await InitializationAsync(_viewManager);
        //_arPointCloudManager.enabled = true;

        return true;
    }

    public void SetMeshActive(bool active)
    {
        _isMeshActive = active;
#if UNITY_ANDROID
        if (_arPointCloudManager != null)
        {
            _arPointCloudManager.enabled = active;
            foreach (var cloud in _arPointCloudManager.trackables)
            {
                if (cloud != null && cloud.gameObject != null)
                {
                    cloud.gameObject.SetActive(active);
                }
            }
        }
#elif UNITY_IOS || UNITY_VISIONOS
        if (_arMeshManager != null)
        {
            _arMeshManager.enabled = true; // Keep meshing active for occlusion
            UpdateMeshRenderers();
        }
#endif
    }

#if UNITY_IOS || UNITY_VISIONOS
    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        UpdateMeshRenderers();
    }
#endif

    public void UpdateMeshRenderers()
    {
#if UNITY_IOS || UNITY_VISIONOS
        if (_arMeshManager == null) return;
        var origin = _arMeshManager.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>();
        var trackablesParent = origin != null ? origin.TrackablesParent : _arMeshManager.transform;
        if (trackablesParent != null)
        {
            bool suppressOcclusion = ShouldSuppressOcclusion();
            foreach (Transform child in trackablesParent)
            {
                if (child != null)
                {
                    var renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        if (_isMeshActive)
                        {
                            renderer.enabled = true;
                            if (_wireframeMaterial != null) renderer.material = _wireframeMaterial;
                        }
                        else if (!suppressOcclusion && _occlusionMaterial != null)
                        {
                            renderer.enabled = true;
                            renderer.material = _occlusionMaterial;
                        }
                        else
                        {
                            renderer.enabled = false;
                        }
                    }
                }
            }
        }
#endif
    }

    private bool ShouldSuppressOcclusion()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm != null && rtm.IsRoomShowing())
        {
            var style = rtm.GetRoomTwinStyle();
            if (style == RoomTwinStyle.FullTwin || style == RoomTwinStyle.TwinVignette)
            {
                return true;
            }
        }
        return false;
    }

    private void SetAllPointCloudsActive(bool isEditMode)
    {
        bool shouldDisplay = false;
        if (isEditMode)
        {
            var gridManager = RootObject.Instance != null ? RootObject.Instance.GridManager : null;
            if (gridManager != null)
            {
                shouldDisplay = gridManager.gridEnabled && gridManager.showMesh;
            }
            else
            {
                shouldDisplay = LearningExperienceEngine.UserSettings.showGrid && LearningExperienceEngine.UserSettings.showMesh;
            }
        }

        SetMeshActive(shouldDisplay);
    }
}
