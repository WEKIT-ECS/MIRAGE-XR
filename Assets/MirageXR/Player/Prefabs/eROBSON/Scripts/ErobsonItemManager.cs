using MirageXR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Action = MirageXR.Action;

public enum AddOrRemove
{
    ADD,
    REMOVE
}

public class ErobsonItemManager : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;


    public delegate void BitConnectedDelegate(eROBSONItems eROBSONItem);

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


    public static List<eROBSONItems> ERobsonItemsList
    {
        get; private set;
    }

    public static List<eROBSONItems> ERobsonConnectedItemsList
    {
        get; private set;
    }

    public static ErobsonItemManager Instance
    {
        get; private set;
    }

    public bool CircuitParsed { get; private set; }

    private string _eRobsonDataFolder;

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
        ERobsonConnectedItemsList = new List<eROBSONItems>();

        Subscribe();

        StartCoroutine(Init());
    }


    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Subscribe()
    {
        EventManager.OnAugmentationObjectCreated += OnERobsonItemAdded;
        EventManager.OnAugmentationDeleted += OnERobsonItemDeleted;
        EventManager.OnActivitySaved += SaveJson;
        EventManager.OnStepActivatedStamp += OnActivateAction;
    }

    private void Unsubscribe()
    {
        EventManager.OnAugmentationObjectCreated -= OnERobsonItemAdded;
        EventManager.OnAugmentationDeleted -= OnERobsonItemDeleted;
        EventManager.OnActivitySaved -= SaveJson;
        EventManager.OnStepActivatedStamp -= OnActivateAction;
    }


    /// <summary>
    /// Load the circuit data from json file
    /// </summary>
    private async void LoadRobsonCircuit()
    {
        //Load json file
        ERobsonCircuit circuit = null;
        var jsonPath = $"{_eRobsonDataFolder}/eRobsonCircuit.json";
        if (File.Exists(jsonPath))
        {
            circuit = JsonUtility.FromJson<ERobsonCircuit>(await File.ReadAllTextAsync(jsonPath));
        }

        var activates = activityManager.ActiveAction.enter.activates;

        //Add all bits which exist in the scne to the list of all bits
        foreach (var toggleObject in activates)
        {
            if (toggleObject.predicate.StartsWith("eRobson"))
            {

                var erobsonItemPoiObject = GameObject.Find(toggleObject.poi);

                //eRobson object not found!
                if (!erobsonItemPoiObject)
                {
                    continue;
                }

                var erobsonItem = erobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();

                var timer = 0;

                //Wait to be sure we will get eROBSONItems (not more than 1 sec)
                while (!erobsonItem && timer < 20)
                {
                    erobsonItem = erobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();
                    timer++;
                    await Task.Delay(50);
                }

                //Still not found! Go for the next eRobson item
                if (erobsonItem == null)
                {
                    continue;
                }

                if (!ERobsonItemsList.Contains(erobsonItem))
                {
                    ERobsonItemsList.Add(erobsonItem);
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
                            erobsonItem.LoadedData = connectedBit;

                            //Apply the loaded info to the bits in editmode
                            if (RootObject.Instance.activityManager.EditModeActive)
                            {
                                ApplySettings(erobsonItem, connectedBit);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                //The power source should be added to connected list even if there is not other bits connected
                if (erobsonItem.ID == BitID.USBPOWER)
                {
                    AddOrRemoveFromConnectedList(erobsonItem, AddOrRemove.ADD);
                }
            }
        }

        CircuitParsed = true;
    }


    /// <summary>
    /// Load the bit settings from json to the prefab
    /// </summary>
    /// <param name="bit"></param>
    private static void ApplySettings(eROBSONItems eROBSONItem, ERobsonItem bitFromJson)
    {
        eROBSONItem.IsActive = bitFromJson.isActive;

        eROBSONItem.Value = bitFromJson.value;

        eROBSONItem.Dimmable = bitFromJson.Dimmable;


        var poiObject = GameObject.Find(eROBSONItem.poiID);
        if (poiObject)
        {
            var manipulator = poiObject.GetComponentInParent<ObjectManipulator>();
            if (manipulator)
            {
                manipulator.transform.localPosition = bitFromJson.localPosition;
                manipulator.transform.localRotation = bitFromJson.localRotation;
            }
        }


        //Find the connected bits in the scene and add them as the loaded bit's connectedBit list
        foreach (var connectedBitID in bitFromJson.connectedbitsID)
        {
            var bit = GameObject.Find(connectedBitID);
            if (bit != null)
            {
                eROBSONItem.ConnectedBits.Add(bit.GetComponent<eROBSONItems>());
            }
        }

        //Load the port infos and snap the connected port on editmode
        for (var i = 0; i < eROBSONItem.Ports.Length; i++)
        {
            var bitPort = eROBSONItem.Ports[i];

            foreach (var portToLoad in bitFromJson.ports)
            {
                //This port is not me
                if (portToLoad.index != i)
                {
                    continue;
                }

                bitPort.Connected = portToLoad.connected;

                if (bitPort.PortIsMovable)
                {
                    bitPort.PortPosition = portToLoad.position;
                }

                //Find the port which was connected to this port
                var connectedBitToPortGameObject = GameObject.Find(portToLoad.connectedPortBitPoiId);
                if (connectedBitToPortGameObject)
                {
                    var connectedBitToPort = connectedBitToPortGameObject.GetComponentInChildren<eROBSONItems>();
                    bitPort.DetectedPortPole = connectedBitToPort.Ports[portToLoad.index];
                }
            }
        }
    }



    /// <summary>
    /// When a new eRobson augmentation is added to the scene
    /// </summary>
    /// <param name="eRobsonGameObject"></param>
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
    /// When the erobson augmentation is removed from the scene
    /// </summary>
    /// <param name="toggleObject"></param>
    private static void OnERobsonItemDeleted(ToggleObject toggleObject)
    {
        // remove the power source from connect bit list
        if (toggleObject.predicate.StartsWith("eRobson"))
        {
            var eRobsonItem = GameObject.Find(toggleObject.poi).GetComponentInChildren<eROBSONItems>();
            if (eRobsonItem)
            {
                //If power source is deleted initiate all bits
                if (eRobsonItem.ID == BitID.USBPOWER)
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
    private void OnActivateAction(string deviceId, Action activatedAction, string timestamp)
    {
        StartCoroutine(Init());
    }


    private IEnumerator Init()
    {
        CircuitParsed = false;

        while (activityManager.ActiveAction == null)
        {
            yield return null;
        }
        _eRobsonDataFolder = Path.Combine(activityManager.ActivityPath, $"eRobson/{activityManager.ActiveAction.id}");

        ERobsonItemsList.Clear();
        ERobsonConnectedItemsList.Clear();

        LoadRobsonCircuit();
    }



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

            foreach (var bit in ERobsonConnectedItemsList)
            {
                //When OnActivitySaved is invoked not by pressing save button
                if (bit == null)
                {
                    ERobsonConnectedItemsList.Clear();
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

                    if (port.PortIsMovable)
                    {
                        portToSave.position = port.PortPosition;
                    }

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
                    bitToSave.connectedbitsID.Add(connectedBit.poiID);
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
    /// <param name="bit"></param>
    /// <param name="addOrRemove"></param>
    public static void AddOrRemoveFromConnectedList(eROBSONItems bit, AddOrRemove addOrRemove)
    {
        if (addOrRemove == AddOrRemove.ADD)
        {
            if (!ERobsonConnectedItemsList.Contains(bit))
                ERobsonConnectedItemsList.Add(bit);
        }
        else if (addOrRemove == AddOrRemove.REMOVE)
        {
            if (ERobsonConnectedItemsList.Contains(bit))
                ERobsonConnectedItemsList.Remove(bit);
        }

        //Debug.LogError(eRobsonConnectedItemsList.Count);
        //Debug.LogError(bit.ID + " " + addOrRemove);
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
    public List<string> connectedbitsID = new ();
}


[Serializable]
public class PortItem
{
    public int index;
    public bool connected;
    public string connectedPortBitPoiId;
    public Vector3 position;
}