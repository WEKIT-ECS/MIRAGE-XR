using MirageXR;
using UnityEngine;

public class SpatialMapManager : MonoBehaviour
{
    private MeshCollider[] spatialMapMeshs;

#if UNITY_WSA
    private void Start()
    {
        UpdateSpatialMapColliders(RootObject.Instance.activityManager.EditModeActive);
        //set colliders when first loading an activity
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
                    spatialMapMesh.enabled = !editMode;
            }
            //loop though spatial map colliders and set to enabled or disabled depending on the editmode status
        }
        catch (System.Exception e)
        {
            Debug.Log("Spatial Awareness System game object not found");
        }
    }
#endif
}
