using MirageXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PoiEditor : MonoBehaviour
{
    private XRGrabInteractable _grabInteractable;
    private SimpleBoundingBox _boundsControl;
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

    private void Awake()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool isEditMode)
    {
        EnableBoundsControl(_boundsControlActive && isEditMode);
        if (_grabInteractable != null)
        {
            _grabInteractable.enabled = isEditMode && !isLocked;
        }
    }

    public T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public void OnChanged()
    {
        if (canRotate)
        {
            _poi.rotation = MirageXR.Utilities.Vector3ToString(transform.localEulerAngles);
        }

        if (canScale)
        {
            _poi.scale = MirageXR.Utilities.Vector3ToString(transform.localScale);
        }
    }

    private void SetAllObjectManipulationOptions()
    {
        int numChildren = transform.childCount;
        for (int ch = 0; ch < numChildren; ch++)
        {
            GameObject child = transform.GetChild(ch).gameObject;
            SetAugmentationSpecificManipulation(child);
        }

        SetupGrabInteractable();
    }

    private void SetAugmentationSpecificManipulation(GameObject prefabObject)
    {
        string objectName = prefabObject.name.ToLower();
        if (objectName.Contains("video") || objectName.Contains("image"))
        {
            SetupGrabInteractable();
        }
    }

    private void SetupGrabInteractable()
    {
        _grabInteractable = GetOrAddComponent<XRGrabInteractable>();
        var rb = GetOrAddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        _grabInteractable.enabled = activityManager.EditModeActive && !isLocked;
    }

    public void EnableBoundsControl(bool enabled)
    {
        _boundsControlActive = enabled;

        if (_boundsControlActive)
        {
            _boundsControl = GetOrAddComponent<SimpleBoundingBox>();
            _boundsControl.Target = transform;
            _boundsControl.RotateStopped.RemoveListener(OnChanged);
            _boundsControl.ScaleStopped.RemoveListener(OnChanged);
            _boundsControl.RotateStopped.AddListener(OnChanged);
            _boundsControl.ScaleStopped.AddListener(OnChanged);

            _boundsControl.enabled = activityManager.EditModeActive;
        }
        else
        {
            var bb = GetComponent<SimpleBoundingBox>();
            if (bb != null)
            {
                bb.enabled = enabled;
            }
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
        if (_boundsControl != null)
        {
            _boundsControl.enabled = !locked && activityManager.EditModeActive;
        }
        if (_grabInteractable != null)
        {
            _grabInteractable.enabled = !locked && activityManager.EditModeActive;
        }
    }
}
