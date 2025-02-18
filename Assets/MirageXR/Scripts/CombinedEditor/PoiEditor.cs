using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using MirageXR;
using UnityEngine;

public class PoiEditor : MonoBehaviour
{
    private ObjectManipulator _objectManipulator;

    private BoundsControl _boundsControl;
    private LearningExperienceEngine.ToggleObject _obj;
    private bool isLocked = false;
    private bool _boundsControlActive = false;

    private float _modelMagnification = 0.0f;

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    public float ModelMagnification
    {
        get => _modelMagnification;
        set => _modelMagnification = value;
    }

    private LearningExperienceEngine.Poi _poi;

    public bool canRotate
    {
        get; set;
    }

    public bool canScale
    {
        get; set;
    }

    public LearningExperienceEngine.Poi GetMyPoi()
    {
        return _poi;
    }

    public void Initialize(LearningExperienceEngine.Poi poi)
    {
        this._poi = poi;
    }

    public void UpdateManipulationOptions(GameObject ObjectToUpdate)
    {
        SetAugmentationSpecificManipulation(ObjectToUpdate);
    }

    private void Start()
    {
        SetAllObjectManipulationOptions();
        _objectManipulator.OnManipulationEnded.AddListener(OnChanged);

        OnEditModeChanged(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive);
        //SetPoiData();
    }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        if (_boundsControl && _boundsControlActive)
        {
            if (isLocked)
            {
                _boundsControl.enabled = false;
            }
            else
            {
                _boundsControl.enabled = editModeActive;
            }
        }

        if (_objectManipulator)
        {
            if (isLocked)
            {
                _objectManipulator.enabled = false;
            }
            else
            {
                _objectManipulator.enabled = editModeActive;
            }
        }
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        return gameObject.GetComponent<T>() ? gameObject.GetComponent<T>() : gameObject.AddComponent<T>();
    }

    public void OnChanged(ManipulationEventData data)
    {
        OnChanged();
    }

    public void OnChanged()
    {
        SetPoiData();
        activityManager.SaveData();
        LearningExperienceEngine.EventManager.NotifyOnAugmentationPoiChanged();
    }

    private Vector3 GetOffset()
    {
        var taskStationId = transform.parent.name;
        var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
        var detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(taskStationId));
        var annotationStartingPoint = ActionEditor.Instance.GetDefaultAugmentationStartingPoint();
        var originT = GameObject.Find(detectable.id);   // TODO: replace by direct reference to the object
        if (!originT)
        {
            Debug.LogError($"Can't find detectable {detectable.id}");
            return annotationStartingPoint.transform.position;
        }

        var detectableBehaviour = originT.GetComponent<DetectableBehaviour>();

        if (!detectableBehaviour)
        {
            Debug.LogError($"Can't find DetectableBehaviour");
            return annotationStartingPoint.transform.position;
        }

        var attachedObject = detectableBehaviour.AttachedObject;
        return attachedObject.transform.InverseTransformPoint(transform.position);
    }

    private void SetPoiData()
    {
        var offset = GetOffset();

        _poi.offset = MirageXR.Utilities.Vector3ToString(offset);
        _poi.x_offset = offset.x;
        _poi.y_offset = offset.y;
        _poi.z_offset = offset.z;

        if (canRotate)
        {
            _poi.rotation = MirageXR.Utilities.Vector3ToString(transform.localEulerAngles);
        }

        if (canScale)
        {
            _poi.scale = MirageXR.Utilities.Vector3ToString(transform.localScale);
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
        _objectManipulator = GetOrAddComponent<ObjectManipulator>();
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

    public void EnableBoundsControl(bool enabled)
    {
        _boundsControlActive = enabled;

        if (_boundsControlActive)
        {
            _boundsControl = GetOrAddComponent<BoundsControl>();

            _boundsControl.RotateStopped.AddListener(OnChanged);
            _boundsControl.ScaleStopped.AddListener(OnChanged);

            _boundsControl.enabled = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;
        }
        else
        {
            GetOrAddComponent<BoundsControl>().enabled = enabled;
        }
    }

    public void EnableBoxCollider(bool enabled)
    {
        var collider = GetOrAddComponent<BoxCollider>();
        if (collider)
        {
            collider.enabled = enabled;
        }
    }

    public void IsLocked(bool locked)
    {
        isLocked = locked;
        if (activityManager.EditModeActive)
        {
            GetOrAddComponent<ObjectManipulator>().enabled = !locked;
        }
    }
}
