using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TiltBrush;
using UnityEngine;
using Action = MirageXR.Action;



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

public enum AddOrRemove
{
    ADD,
    REMOVE
}

public class ErobsonItemManager : MonoBehaviour
{
    private static ActivityManager ActivityManager => RootObject.Instance.activityManager;


    public delegate void BitConnectedDelegate(eROBSONItems eRobsonItem);

    public static event BitConnectedDelegate OnBitConnected;

    public static void BitConnected(eROBSONItems eROBSONItem)
    {
        OnBitConnected?.Invoke(eROBSONItem);
    }

    public delegate void BitDisconnectedDelegate(eROBSONItems eROBSONItem);

    public static event BitDisconnectedDelegate OnBitDisconnected;

    public static void BitDisconnected(eROBSONItems eROBSONItem)
    {
        OnBitDisconnected?.Invoke(eROBSONItem);
    }


    /// <summary>
    /// all bits in the scene
    /// </summary>
    public static List<eROBSONItems> ERobsonItemsList
    {
        get; private set;
    }


    /// <summary>
    /// the bits which are connected by player in any mode (the active bits)
    /// </summary>
    public static List<eROBSONItems> ERobsonActiveConnectedItemsList
    {
        get; private set;
    }


    /// <summary>
    /// the bits which are connected by player in play mode (either active or not)
    /// </summary>
    public static List<eROBSONItems> ERobsonConnectedItemsListByPlayer
    {
        get; private set;
    }


    /// <summary>
    /// the bits which are connected by teacher in edit mode (either active or not)
    /// </summary>
    public static List<eROBSONItems> ERobsonConnectedItemsListByTeacher
    {
        get; private set;
    }


    public bool PromptMessageIsOpen
    {
        get; set;
    }


    public static ErobsonItemManager Instance
    {
        get; private set;
    }

    public bool CircuitParsed { get; private set; }

    private string _eRobsonDataFolder;


    /// <summary>
    /// Save the circuit data into a json file
    /// </summary>
    public void SaveJson()
    {
        try
        {
            //The circuit should only be saved on editMode
            if (!RootObject.Instance.activityManager.EditModeActive)
            {
                return;
            }

            var circuit = new ERobsonCircuit
            {
                connectedbitsList = new List<ERobsonItem>(),
            };

            foreach (var bit in ERobsonActiveConnectedItemsList)
            {
                //When OnActivitySaved is invoked not by pressing save button
                if (bit == null)
                {
                    ERobsonActiveConnectedItemsList.Clear();
                    return;
                }

                var bitToSave = new ERobsonItem
                {
                    //Save the poi Id
                    poiID = bit.poiID,
                    //Save the id (Only to be clear onjson file. This will not be used on loading)
                    id = bit.ID.ToString(),
                };

                //Save the ports info
                var portsToJson = new PortItem[bit.Ports.Length];

                for (var i = 0; i < bit.Ports.Length; i++)
                {
                    var port = bit.Ports[i];

                    var portToSave = new PortItem
                    {
                        index = i,
                    };

                    if (port.DetectedPortPole)
                    {
                        portToSave.connectedPortBitPoiId = port.DetectedPortPole.ERobsonItem.poiID;
                    }

                    portToSave.connected = port.Connected;

                    portsToJson[i] = portToSave;
                }

                bitToSave.ports = portsToJson;

                bitToSave.isActive = bit.IsActive;

                bitToSave.Dimmable = bit.Dimmable;

                if (bit.Dimmable)
                {
                    bitToSave.value = bit.Value;
                }

                var poiObject = GameObject.Find(bit.poiID);
                if (poiObject)
                {
                    var manipulator = poiObject.GetComponentInParent<ObjectManipulator>();
                    if (manipulator)
                    {
                        bitToSave.localPosition = manipulator.transform.localPosition;
                        bitToSave.localRotation = manipulator.transform.localRotation;
                    }
                }

                foreach (var connectedBit in bit.ConnectedBits)
                {
                    if (!connectedBit)
                    {
                        continue;
                    }
                    bitToSave.connectedbitsID.Add(connectedBit.poiID);
                }

                //If the bit has a real time simulated wire save the position of it
                if (bit.HasWire)
                {
                    var wireEndPosition = bit.GetComponentInChildren<FixedPort>();
                    if (wireEndPosition)
                    {
                        bitToSave.wireEndPosition = wireEndPosition.transform.localPosition;
                    }
                }

                circuit.connectedbitsList.Add(bitToSave);
            }

            var eRobsonCircuitJson = JsonUtility.ToJson(circuit);

            if (!Directory.Exists(_eRobsonDataFolder))
            {
                Directory.CreateDirectory(_eRobsonDataFolder);
            }

            var jsonPath = $"{_eRobsonDataFolder}/eRobsonCircuit.json";

            //write the json file
            File.WriteAllText(jsonPath, eRobsonCircuitJson);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }


    /// <summary>
    /// Add or remove the connected bit to the connected list
    /// </summary>
    /// <param name="bit">the bit which will be added/deleted to the list</param>
    /// <param name="addOrRemove">Add or delete</param>
    public static void AddOrRemoveFromConnectedList(eROBSONItems bit, AddOrRemove addOrRemove)
    {
        switch (addOrRemove)
        {
            case AddOrRemove.ADD:
            {
                if (!ERobsonActiveConnectedItemsList.Contains(bit))
                {
                    ERobsonActiveConnectedItemsList.Add(bit);
                }

                break;
            }
            case AddOrRemove.REMOVE:
            {
                if (ERobsonActiveConnectedItemsList.Contains(bit))
                {
                    ERobsonActiveConnectedItemsList.Remove(bit);
                }

                break;
            }
        }

        //Debug.LogError(ERobsonActiveConnectedItemsList.Count);
        //Debug.LogError(bit.ID + " " + addOrRemove);
    }




    /// <summary>
    /// All bits will lose power
    /// </summary>
    public void CutCircuitPower()
    {
        foreach (var bit in ERobsonActiveConnectedItemsList.Where(bit => bit.ID != BitID.USBPOWER))
        {
            bit.HasPower = false;
        }
    }


    private void Start()
    {
        //Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        ERobsonItemsList = new List<eROBSONItems>();
        ERobsonActiveConnectedItemsList = new List<eROBSONItems>();
        ERobsonConnectedItemsListByTeacher = new List<eROBSONItems>();
        ERobsonConnectedItemsListByPlayer = new List<eROBSONItems>();

        Subscribe();

        StartCoroutine(Init());
    }


    private void OnDestroy()
    {
        Unsubscribe();
    }


    /// <summary>
    /// Subscribe the events
    /// </summary>
    private void Subscribe()
    {
        EventManager.OnAugmentationObjectCreated += OnERobsonItemAdded;
        EventManager.OnAugmentationDeleted += OnERobsonItemDeleted;
        EventManager.OnActivitySaved += SaveJson;
        EventManager.OnActivateAction += OnActivateAction;
    }


    /// <summary>
    /// Unsubscribe the events
    /// </summary>
    private void Unsubscribe()
    {
        EventManager.OnAugmentationObjectCreated -= OnERobsonItemAdded;
        EventManager.OnAugmentationDeleted -= OnERobsonItemDeleted;
        EventManager.OnActivitySaved -= SaveJson;
        EventManager.OnActivateAction -= OnActivateAction;
    }


    /// <summary>
    /// Load the circuit data from json file
    /// </summary>
    private async void LoadERobsonCircuit()
    {
        //wait a bit for Mirage XR to load anything in the step
        await Task.Delay(500);

        //Load json file
        ERobsonCircuit circuit = null;
        var jsonPath = $"{_eRobsonDataFolder}/eRobsonCircuit.json";
        if (File.Exists(jsonPath))
        {
            circuit = JsonUtility.FromJson<ERobsonCircuit>(await File.ReadAllTextAsync(jsonPath));
        }

        var activates = ActivityManager.ActiveAction.enter.activates;

        //Add all bits which exist in the scne to the list of all bits
        foreach (var toggleObject in activates)
        {
            //Not interest in other annotation types
            if (!toggleObject.predicate.StartsWith("eRobson"))
            {
                continue;
            }

            var eRobsonItemPoiObject = GameObject.Find(toggleObject.poi);

            //eRobson object not found!
            if (!eRobsonItemPoiObject)
            {
                continue;
            }

            var eRobsonItem = eRobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();

            var timer = 0;

            //Wait to be sure we will get eROBSONItems (not more than 1 sec)
            while (!eRobsonItem && timer < 20)
            {
                eRobsonItem = eRobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();
                timer++;
                await Task.Delay(50);
            }

            //Still not found! Go for the next eRobson item
            if (eRobsonItem == null)
            {
                continue;
            }

            if (!ERobsonItemsList.Contains(eRobsonItem))
            {
                ERobsonItemsList.Add(eRobsonItem);
            }

            //Check and adjust connected bits
            if (circuit != null)
            {
                try
                {
                    foreach (var connectedBit in circuit.connectedbitsList)
                    {
                        //This connected bit is not me
                        if (connectedBit.poiID != toggleObject.poi)
                        {
                            continue;
                        }

                        //Save the loaded data in a list for using in playmode
                        eRobsonItem.LoadedData = connectedBit;

                        //Apply the loaded info to the bits in editmode
                        if (RootObject.Instance.activityManager.EditModeActive)
                        {
                            ApplySettings(eRobsonItem, connectedBit);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            //The power source should be added to connected list even if there is not other bits connected
            if (eRobsonItem.ID == BitID.USBPOWER)
            {
                AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
            }

            if (!RootObject.Instance.activityManager.EditModeActive)
            {
                if (!ERobsonConnectedItemsListByTeacher.Contains(eRobsonItem))
                {
                    ERobsonConnectedItemsListByTeacher.Add(eRobsonItem);
                }
            }
        }

        if (!RootObject.Instance.activityManager.EditModeActive)
        {
            DialogWindow.Instance.Show("Info", "Circuit order is essential for meeting test requirements.", new DialogButtonContent("Close"));
        }

        CircuitParsed = true;
    }


    //This is for debugging
    //private void Update()
    //{
    //    var temp = ERobsonActiveConnectedItemsList.FindAll(e => e.HasPower).Select(e => e.ID.ToString());
    //    Debug.LogError(string.Join(", ", temp.ToArray()));
    //}


    /// <summary>
    /// Load the bit settings from json to the prefab
    /// </summary>
    /// <param name="eRobsonItem">The bit which need settings to be added to it</param>
    /// <param name="bitFromJson">The data as ERobson class from json file</param>
    private static void ApplySettings(eROBSONItems eRobsonItem, ERobsonItem bitFromJson)
    {
        eRobsonItem.IsActive = bitFromJson.isActive && !InactiveBitsByDefault(eRobsonItem.ID);

        eRobsonItem.Value = bitFromJson.value;

        eRobsonItem.Dimmable = bitFromJson.Dimmable;

        //Load the wire position if the bit has wire
        if (eRobsonItem.HasWire)
        {
            var wireEndPosition = eRobsonItem.GetComponentInChildren<FixedPort>();
            if (wireEndPosition)
            {
                wireEndPosition.transform.localPosition = bitFromJson.wireEndPosition;
            }
        }

        var poiObject = GameObject.Find(eRobsonItem.poiID);
        if (poiObject)
        {
            var manipulator = poiObject.GetComponentInParent<ObjectManipulator>();
            if (manipulator)
            {
                var manipulatorTransform = manipulator.transform;
                manipulatorTransform.localPosition = bitFromJson.localPosition;
                manipulatorTransform.localRotation = bitFromJson.localRotation;
            }
        }


        //Find the connected bits in the scene and add them as the loaded bit's connectedBit list
        foreach (var connectedBitID in bitFromJson.connectedbitsID)
        {
            var bitObject = GameObject.Find(connectedBitID);
            if (bitObject != null)
            {
                eRobsonItem.ConnectedBits.Add(bitObject.GetComponent<eROBSONItems>());
            }
        }

        //Load the port infos and snap the connected port on editmode
        for (var i = 0; i < eRobsonItem.Ports.Length; i++)
        {
            var bitPort = eRobsonItem.Ports[i];

            foreach (var portToLoad in bitFromJson.ports)
            {
                //This port is not me
                if (portToLoad.index != i)
                {
                    continue;
                }

                bitPort.Connected = portToLoad.connected;

                //Find the port which was connected to this port
                var connectedBitToPortGameObject = GameObject.Find(portToLoad.connectedPortBitPoiId);

                if (!connectedBitToPortGameObject)
                {
                    continue;
                }

                var connectedBitToPort = connectedBitToPortGameObject.GetComponentInChildren<eROBSONItems>();
                if (connectedBitToPort && connectedBitToPort.Ports.Length > portToLoad.index)
                {
                    bitPort.DetectedPortPole = connectedBitToPort.Ports[portToLoad.index];
                }

                bitPort.ControlPortCollision(connectedBitToPortGameObject);
            }
        }

        //Check again that after loading the step, is the bit really connected or just json info shows it is connected
        foreach (var bitPort in eRobsonItem.Ports)
        {
            var ray = new Ray(bitPort.transform.position, bitPort.transform.forward * (bitPort.ReverseRay ? -1 : 1));
            // Check if the ray hits any GameObjects within the specified distance
            if (!Physics.Raycast(ray, out var hit, Port.RayDistance, LayerMask.GetMask("eRobsonPort")) && bitPort.Connected)
            {
                bitPort.Disconnect();
            }
        }
    }



    /// <summary>
    /// the bits which needs to be in active at start of step e.q. button
    /// </summary>
    /// <param name="bit"></param>
    /// <returns></returns>
    private static bool InactiveBitsByDefault(BitID bit)
    {
        return bit == BitID.I3BUTTON;
    }



    /// <summary>
    /// When a new eRobson augmentation is added to the scene
    /// </summary>
    /// <param name="eRobsonGameObject">eRobson toggleObject which is added</param>
    private static void OnERobsonItemAdded(GameObject eRobsonGameObject)
    {
        // add the power source into connect bit list
        var eRobsonItem = eRobsonGameObject.GetComponentInChildren<eROBSONItems>();

        //Couldn't find eROBSONItems component
        if (!eRobsonItem)
        {
            return;
        }

        ERobsonItemsList.Add(eRobsonItem);

        //The power source should be added to the connected bits list at the start
        if (eRobsonItem.ID == BitID.USBPOWER)
        {
            AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
        }
    }



    /// <summary>
    /// When the eRobson augmentation is removed from the scene
    /// </summary>
    /// <param name="toggleObject">eRobson toggleObject which is deleted</param>
    private static void OnERobsonItemDeleted(ToggleObject toggleObject)
    {
        // remove the power source from connect bit list
        if (toggleObject.predicate.StartsWith("eRobson"))
        {
            var eRobsonItem = GameObject.Find(toggleObject.poi).GetComponentInChildren<eROBSONItems>();
            if (eRobsonItem)
            {
                //If power source is deleted initiate all bits
                if (eRobsonItem.ID == BitID.USBPOWER &&
                    ERobsonActiveConnectedItemsList.Contains(eRobsonItem) &&
                    ERobsonActiveConnectedItemsList.FindAll(b => b.ID == BitID.USBPOWER).Count == 1)
                {
                    foreach (var bit in ERobsonItemsList)
                    {
                        bit.GetComponent<BitsBehaviourController>().Init();
                    }
                }

                //Remove the deleted bit from the erobson list
                ERobsonItemsList.Remove(eRobsonItem);

                //Remove the deleted bit from the connected erobson list
                AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.REMOVE);

                //Let adjust the circuit again after deleting this bit
                eRobsonItem.GetComponent<BitsBehaviourController>().ControlCircuit();

                Debug.Log(eRobsonItem.ID + " deleted");
            }
        }
    }


    /// <summary>
    /// When a step is activated
    /// </summary>
    private void OnActivateAction(string actionID)
    {
        StartCoroutine(Init());
    }


    /// <summary>
    /// initial the manager
    /// </summary>
    private IEnumerator Init()
    {
        CircuitParsed = false;

        while (ActivityManager.ActiveAction == null)
        {
            yield return null;
        }
        _eRobsonDataFolder = Path.Combine(ActivityManager.ActivityPath, $"eRobson/{ActivityManager.ActiveAction.id}");

        ERobsonItemsList.Clear();
        ERobsonActiveConnectedItemsList.Clear();
        ERobsonConnectedItemsListByTeacher.Clear();
        ERobsonConnectedItemsListByPlayer.Clear();

        LoadERobsonCircuit();
    }
}



[Serializable]
internal class ERobsonCircuit
{
    public List<ERobsonItem> connectedbitsList;
}


[Serializable]
public class ERobsonItem
{
    public string poiID;
    public string id;
    public PortItem[] ports;
    public bool isActive;
    public float value;
    public bool Dimmable;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public List<string> connectedbitsID = new();
    public Vector3 wireEndPosition;
}


[Serializable]
public class PortItem
{
    public int index;
    public bool connected;
    public string connectedPortBitPoiId;
}