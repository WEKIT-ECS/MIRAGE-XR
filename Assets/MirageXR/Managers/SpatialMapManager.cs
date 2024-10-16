using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using MirageXR;
using UnityEngine;

public class SpatialMapManager : MonoBehaviour
{
    private MeshCollider[] spatialMapMeshs;
    
    private static PlaneManagerWrapper planeManager => RootObject.Instance.PlaneManager;

#if UNITY_WSA
    private void Start()
    {
        UpdateSpatialMapColliders(RootObject.Instance.LEE.activityManager.EditModeActive);
        //set colliders when first loading an activity

        planeManager.onDetectionEnabled.AddListener(OnDetectionEnabled);
        planeManager.onDetectionDisabled.AddListener(OnDetectionDisabled);
    }

    private void OnDetectionDisabled()
    {
        SpatialMapMeshVisible(false);
        UpdateSpatialMapColliders(RootObject.Instance.LEE.activityManager.EditModeActive);
    }

    private void OnDetectionEnabled()
    {
        SpatialMapMeshVisible(true);
        if (RootObject.Instance.LEE.activityManager.EditModeActive)
        {
            UpdateSpatialMapColliders(!RootObject.Instance.LEE.activityManager.EditModeActive);
        }
    }


    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += UpdateSpatialMapColliders;
    }


    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= UpdateSpatialMapColliders;

    }
    //set colliders when edit mode is changed


    private void OnDestroy()
    {
        UpdateSpatialMapColliders(false);
        //set colliders when active when this object is destroyed
    }

    private void UpdateSpatialMapColliders(bool editMode) {
        try
        {
            if (spatialMapMeshs == null)
            {
                spatialMapMeshs = GameObject.Find("Spatial Awareness System").GetComponentsInChildren<MeshCollider>();
                //find the spatial map colliders if they are not set
                //TODO; Find a more efficent way of finding the colliders without using 'GameObject.Find'
            }

            foreach (MeshCollider spatialMapMesh in spatialMapMeshs)
            {
                if (spatialMapMesh)
                {
                    spatialMapMesh.enabled = !editMode;
                }
            }
            //loop though spatial map colliders and set to enabled or disabled depending on the editmode status
        }
        catch (System.Exception e)
        {
            Debug.Log("Spatial Awareness System game object not found");
        }
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
