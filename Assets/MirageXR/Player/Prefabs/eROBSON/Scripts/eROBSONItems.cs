using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using MirageXR;
using System.Collections.Generic;
using System;

public enum BitID
{
    I3BUTTON,
    I5SLIDEDIMMER,
    I11PRESSURESENSOR,
    I18MOTIONSENSOR,
    O2LONGLED,
    O6BUZZER,
    O9BARGRAPH,
    O13FAN,
    O25DCMOTOR,
    P3USBPOWERCONNECTOR,
    USBPOWER,
    W2BRANCH,
    W7FORK
}

public class eROBSONItems : MirageXRPrefab
{

    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private ToggleObject myObj;

    [Tooltip("The bit id")]
    [SerializeField] private BitID id;

    public string poiID
    {
        get
        {
            return myObj.poi;
        }
    }

    public BitID ID => id;

    private BitsBehaviourController MyBehaviourController;


    private Port[] ports;

    public Port[] Ports => ports;

    public bool IsMoving { get; private set; }

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


    [HideInInspector]
    public List<eROBSONItems> connectedbits = new List<eROBSONItems>();

    private async void Awake()
    {
        //create eRobson manager
        //Create the erobson managers
        if (ErobsonItemManager.Instance == null)
        {
            // Get the prefab from the references
            var erobsonManagers = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("eROBSON/Prefabs/ErobsonImageMarkerController");
            // if the prefab reference has been found successfully
            if (erobsonManagers != null)
            {
                var erobsonManagersPrefab = Instantiate(erobsonManagers, Vector3.zero, Quaternion.identity);
                var erobsonItemManager = erobsonManagersPrefab.GetComponent<ErobsonItemManager>();
                if (erobsonItemManager)
                    erobsonItemManager.Subscribe();
            }
        }
    }

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += SetEditorState;
    }

    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= SetEditorState;
    }


    private void Start()
    {
        SetEditorState(activityManager.EditModeActive);

        ports = GetComponentsInChildren<Port>();
        MyBehaviourController = GetComponent<BitsBehaviourController>();
        gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationStarted.AddListener(OnMovingItem);
        gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationEnded.AddListener(OnItemStoppedMoving);
    }

    private void OnMovingItem(ManipulationEventData arg0)
    {
        IsMoving = true;
    }

    private void OnItemStoppedMoving(ManipulationEventData arg0)
    {
        IsMoving = false;
    }



    public void DisableManipulation()
    {
        gameObject.GetComponentInParent<ObjectManipulator>().enabled = false;
    }

    public void EnableManipulation()
    {
        gameObject.GetComponentInParent<ObjectManipulator>().enabled = true;
    }


    private void SetEditorState(bool editModeActive)
    {

        var boundsControl = GetComponent<BoundsControl>();
        if (boundsControl != null)
        {
            boundsControl.Active = editModeActive;
        }
    }

    public override bool Init(ToggleObject obj)
    {
        myObj = obj;

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


    public void CopySettings(eROBSONItems bit)
    {
        HasPower = bit.HasPower;
        IsActive = bit.IsActive;
        connectedbits = bit.connectedbits;
    }
}
