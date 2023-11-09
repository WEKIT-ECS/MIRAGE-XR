using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using MirageXR;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class eROBSONItems : MirageXRPrefab
{
    private ToggleObject _myObj;

    [Tooltip("The bit id")]
    [SerializeField] private BitID id;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private bool hasWire;
    [SerializeField] private Interactable cwArrowButton;
    [SerializeField] private Interactable ccwArrowButton;

    private IndicatorLight indicatorLight;

    public IndicatorLight IndicatorLight => indicatorLight;

    public string poiID
    {
        get
        {
            return _myObj.poi;
        }
    }

    public BitID ID => id;



    public bool BitIsLocked => _myObj is { positionLock: true };

    private Port[] ports;
    private List<eROBSONItems> _connectedBits;

    public Port[] Ports => ports;

    public bool IsMoving { get; private set; }

    public bool HasWire => hasWire;

    public bool IsActive
    {
        get; set;
    }

    public bool HasPower
    {
        get; set;
    }

    public float Value
    {
        get; set;
    }

    public bool Dimmable
    {
        get; set;
    }


    public DateTime connectedTime
    {
        get; set;
    }

    public ERobsonItem LoadedData
    {
        get; set;
    }

    public List<eROBSONItems> ConnectedBits => _connectedBits;

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += EditModeState;
        EventManager.OnAugmentationLocked += OnLock;
    }

    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= EditModeState;
        EventManager.OnAugmentationLocked -= OnLock;
    }

    private ObjectManipulator _manipulator;


    private async void Awake()
    {
        if (ErobsonItemManager.Instance != null) return;


        //Create the eRobson managers
        // Get the prefab from the references
        var eRobsonManagers = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("eROBSON/Prefabs/Common/ErobsonImageMarkerController");
        // if the prefab reference has been found successfully
        if (eRobsonManagers != null)
        {
            Instantiate(eRobsonManagers, Vector3.zero, Quaternion.identity);
        }
    }

    private void Start()
    {
        _connectedBits = new List<eROBSONItems>();
        indicatorLight = GetComponentInChildren<IndicatorLight>();
        ports = GetComponentsInChildren<Port>();
        cwArrowButton.OnClick.AddListener(ClockWiseRotation);
        ccwArrowButton.OnClick.AddListener(CounterClockWiseRotation);

        _manipulator = gameObject.GetComponentInParent<ObjectManipulator>();
        if (_manipulator)
        {
            _manipulator.OnManipulationStarted.AddListener(OnMovingItem);
            _manipulator.OnManipulationEnded.AddListener(OnItemStoppedMoving);
        }

        EnableManipulation();
    }



    private void ClockWiseRotation()
    {
        _manipulator.transform.Rotate(0, 90, 0, Space.Self);
    }

    private void CounterClockWiseRotation()
    {
        _manipulator.transform.Rotate(0, -90, 0, Space.Self);
    }



    /// <summary>
    /// When the user moves this item
    /// </summary>

    private void OnMovingItem(ManipulationEventData arg0)
    {
        IsMoving = !_myObj.positionLock;

    }

    /// <summary>
    /// When the user stop moving this item
    /// </summary>
    /// <param name="arg0"></param>
    private void OnItemStoppedMoving(ManipulationEventData arg0)
    {
        IsMoving = false;
        ErobsonItemManager.Instance.SaveJson();
    }



    /// <summary>
    /// Disable manupulation
    /// </summary>
    public void DisableManipulation()
    {
        gameObject.GetComponentInParent<ObjectManipulator>().enabled = false;
    }


    /// <summary>
    /// Enable manipulation
    /// </summary>
    public void EnableManipulation()
    {
        if (BitIsLocked || !_manipulator)
        {
            return;
        }

        _manipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move;
        _manipulator.enabled = true;
    }


    /// <summary>
    /// Set the value text if the bit has this feature
    /// </summary>
    public void SetValueText(BitID bitID)
    {
        if (valueText != null)
        {
            switch (bitID)
            {
                case BitID.I5SLIDEDIMMER:
                    valueText.text = Value.ToString("n2");
                    break;
                case BitID.I3BUTTON:
                    valueText.text = IsActive ? "ON" : "OFF";
                    break;
            }

        }
    }

    public override bool Init(ToggleObject obj)
    {
        _myObj = obj;

        // Try to set the parent and if it fails, terminate initialization.
        if (!SetParent(obj))
        {
            Debug.Log("Couldn't set the parent.");
            return false;
        }

        name = obj.predicate;
        obj.text = name;

        // Set scaling if defined in action configuration.
        var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
        transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);

        // If everything was ok, return base result.
        return base.Init(obj);
    }


    /// <summary>
    /// On edit mode switching
    /// </summary>
    /// <param name="editMode"></param>
    private void EditModeState(bool editMode)
    {
        //Manipulation should be enabled always for eRobson
        EnableManipulation();
    }


    private void OnLock(string id, bool locked)
    {
        if (id == _myObj.poi)
        {
            _myObj.positionLock = locked;

            //The rotation arrow is visible when the bit is unlocked
            ccwArrowButton.transform.parent.parent.gameObject.SetActive(!locked); //"degreeArrows" object

            var poiEditor = GetComponentInParent<PoiEditor>();

            if (poiEditor)
            {
                poiEditor.IsLocked(_myObj.positionLock);

                if (poiEditor.transform.GetComponent<BoundsControl>() && RootObject.Instance.activityManager.EditModeActive)
                {
                    poiEditor.EnableBoundsControl(!_myObj.positionLock);
                }
            }
        }
    }
}
