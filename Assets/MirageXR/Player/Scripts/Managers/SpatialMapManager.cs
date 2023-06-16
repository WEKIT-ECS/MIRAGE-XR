using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using MirageXR;
using UnityEngine;

public class SpatialMapManager : MonoBehaviour
{
    private MeshCollider[] spatialMapMeshs;
    
    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

#if UNITY_WSA
    private void Start()
    {
        UpdateSpatialMapColliders(RootObject.Instance.activityManager.EditModeActive);
        //set colliders when first loading an activity

        floorManager.onDetectionEnabled.AddListener(OnDetectionEnabled);
        floorManager.onDetectionDisabled.AddListener(OnDetectionDisabled);
    }

    private void OnDetectionDisabled()
    {
        SpatialMapMeshVisible(false);
        UpdateSpatialMapColliders(RootObject.Instance.activityManager.EditModeActive);
    }

    private void OnDetectionEnabled()
    {
        SpatialMapMeshVisible(true);
        if (RootObject.Instance.activityManager.EditModeActive)
        {
            UpdateSpatialMapColliders(!RootObject.Instance.activityManager.EditModeActive);
        }
    }


    private void OnEnable()
    {
        EventManager.OnEditModeChanged += UpdateSpatialMapColliders;
    }


    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= UpdateSpatialMapColliders;

    }
    //set colliders when edit mode is changed


    private void OnDestroy()
    {
        UpdateSpatialMapColliders(false);
        //set colliders when active when this object is destroyed
    }

    private void UpdateSpatialMapColliders(bool editMode) {

        if (spatialMapMeshs == null) {
            spatialMapMeshs = GameObject.Find("Spatial Awareness System").GetComponentsInChildren<MeshCollider>();
            //find the spatial map colliders if they are not set
            //TODO; Find a more efficent way of finding the colliders without using 'GameObject.Find'
        }

        foreach (MeshCollider spatialMapMesh in spatialMapMeshs)
        {
            if(spatialMapMesh)
                spatialMapMesh.enabled = !editMode;
        }
        //loop though spatial map colliders and set to enabled or disabled depending on the editmode status
    }

    private void SpatialMapMeshVisible(bool visible)
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (visible)
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
        }
        else
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        }
    }
#endif
}
