using System;
using System.Globalization;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using UnityEngine;

public class PoiEditor : MonoBehaviour
{
    private ObjectManipulator objectManipulator;

    private float modelMagnification = 0.0f;
    public float ModelMagnification
    {
        get
        {
            return modelMagnification;
        }
        set
        {
            modelMagnification = value;
        }
    }

    private Poi poi;
    public bool ICanRotate
    {
        get; set;
    }

    public bool iCanScale
    {
        get; set;
    }

    public Poi GetMyPoi()
    {
        return poi;
    }

    public void Initialize(Poi poi)
    {
        this.poi = poi;
    }

    public void UpdateManipulationOptions(GameObject ObjectToUpdate)
    {
        SetAugmentationSpecificManipulation(ObjectToUpdate);
    }

    private void Start()
    {
        SetAllObjectManipulationOptions();
        objectManipulator.OnManipulationEnded.AddListener(OnChanged);

        OnEditModeChanged(RootObject.Instance.activityManager.EditModeActive);
        //SetPoiData();
    }

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        if (!objectManipulator)
        {
            objectManipulator = GetOrAddComponent<ObjectManipulator>();
        }

        objectManipulator.enabled = editModeActive;
    }

    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        return gameObject.GetComponent<T>() ? gameObject.GetComponent<T>() : gameObject.AddComponent<T>();
    }

    public void OnChanged(ManipulationEventData data)
    {
        SetPoiData();
        EventManager.NotifyOnAugmentationPoiChanged();
    }

    private void SetPoiData()
    {
        string taskStationId = transform.parent.name;
        var workplaceManager = RootObject.Instance.workplaceManager;
        Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(taskStationId));
        Transform originT = GameObject.Find(detectable.id).transform;

        Vector3 offset = Utilities.CalculateOffset(transform.position, transform.rotation, originT.position, originT.rotation);
        poi.offset = $"{offset.x.ToString(CultureInfo.InvariantCulture)}, {offset.y.ToString(CultureInfo.InvariantCulture)}, {offset.z.ToString(CultureInfo.InvariantCulture)}";
        poi.x_offset = offset.x;
        poi.y_offset = offset.y;
        poi.z_offset = offset.z;

        // rotation should be saved
        if (ICanRotate)
        {
            poi.rotation = Math.Round(transform.localRotation.eulerAngles.x, 2).ToString() + ", " + Math.Round(transform.localRotation.eulerAngles.y, 2).ToString() + ", " + Math.Round(transform.localRotation.eulerAngles.z, 2).ToString();
        }

        // also scale can be adjusted using the object manipulator
        if (iCanScale)
        {
            poi.scale = Math.Round(transform.localScale.x, 2).ToString() + ", " + Math.Round(transform.localScale.y, 2).ToString() + ", " + Math.Round(transform.localScale.z, 2).ToString();
        }
    }


    /// <summary>
    /// Optional orientation constraints and controls for each augmentation type
    /// </summary>
    private void SetAllObjectManipulationOptions()
    {
        int numChildren = transform.childCount;
        for (int ch = 0; ch < numChildren; ch++)
        {
            GameObject child = transform.GetChild(ch).gameObject;
            SetAugmentationSpecificManipulation(child);
        }

        // ensure default interaction is present
        objectManipulator = GetOrAddComponent<ObjectManipulator>();
    }

    private void SetAugmentationSpecificManipulation(GameObject prefabObject)
    {
        // check type of augmentation
        string objectName = prefabObject.name.ToLower();

        if (objectName.Contains("video") || objectName.Contains("image"))
        {
            Debug.Log("adding video prefab interaction options");
            GetOrAddComponent<ObjectManipulator>();
        }

        // add other augmentation types here
        // ...
        // ...

    }
}
