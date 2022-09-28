using MirageXR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public enum AddOrRemove
{
    ADD,
    REMOVE
}

public class ErobsonItemManager : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    #region Erobson Events
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
    #endregion

    public static List<eROBSONItems> eRobsonItemsList
    {
        get; private set;
    }

    public static List<eROBSONItems> eRobsonConnectedItemsList
    {
        get; private set;
    }

    public static List<ERobsonItem> ConnectedBitsLoadedData
    {
        get; private set;
    }

    public static ErobsonItemManager Instance
    {
        get; private set;
    }

    private string eRobsonDataFolder;

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

        DontDestroyOnLoad(gameObject);

        eRobsonItemsList = new List<eROBSONItems>();
        eRobsonConnectedItemsList = new List<eROBSONItems>();

        StartCoroutine(Init());

        Subscribe();
    }


    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Subscribe()
    {
        EventManager.OnAugmentationObjectCreated += OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted += OnErobsonItemDeleted;
        EventManager.OnActivitySaved += SaveJson;
        EventManager.OnActivityStarted += OnActivateAction;
    }

    private void Unsubscribe()
    {
        EventManager.OnAugmentationObjectCreated -= OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted -= OnErobsonItemDeleted;
        EventManager.OnActivitySaved -= SaveJson;
        EventManager.OnActivityStarted -= OnActivateAction;
    }



    /// <summary>
    /// Load the circuit data from json file
    /// </summary>
    private async void LoadeRobsonCircuit()
    {
        //Load json file
        ERobsonCircuit circuit = null;
        var jsonpath = $"{eRobsonDataFolder}/eRobsonCircuit.json";
        if (File.Exists(jsonpath))
        {
            circuit = JsonUtility.FromJson<ERobsonCircuit>(File.ReadAllText(jsonpath));
        }

        var activates = activityManager.ActiveAction.enter.activates;

        //Add all bits which exist in the scne to the list of all bits
        foreach (var toggleObject in activates)
        {
            if (toggleObject.predicate.StartsWith("eRobson"))
            {

                var erobsonItemPoiObject = GameObject.Find(toggleObject.poi);

                if (erobsonItemPoiObject)
                {
                    var erobsonItem = erobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();

                    while (!erobsonItem)
                    {
                        erobsonItem = erobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();
                        await Task.Delay(30);
                    }

                    if (erobsonItem != null)
                    {
                        if (!eRobsonItemsList.Contains(erobsonItem))
                        {
                            eRobsonItemsList.Add(erobsonItem);
                        }

                        //Check and adjust connected bits
                        if (circuit != null)
                        {
                            try
                            {
                                foreach (var connectedBit in circuit.connectedbitsList)
                                {

                                    if (connectedBit.PoiID == toggleObject.poi)
                                    {
                                        //Save the loaded data in a list for using in playmode
                                        erobsonItem.LoadedData = connectedBit;

                                        //Apply the loaded info to the bits in editmode
                                        if (RootObject.Instance.activityManager.EditModeActive)
                                        {
                                            ApplySettings(erobsonItem, connectedBit);
                                        }

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
            }
        }


    }


    /// <summary>
    /// Load the bit settings from json to the prefab
    /// </summary>
    /// <param name="bit"></param>
    private void ApplySettings(eROBSONItems eROBSONItem, ERobsonItem bitFromJson)
    {
        eROBSONItem.IsActive = bitFromJson.IsActive;

        eROBSONItem.Value = bitFromJson.Value;

        eROBSONItem.Dimmable = bitFromJson.Dimmable;

        eROBSONItem.transform.parent.localPosition = bitFromJson.position;
        eROBSONItem.transform.parent.localRotation = bitFromJson.rotation;

        //Find the the connected bits in the scene and add them as the loaded bit's connectedBit list
        foreach (var connectedBitID in bitFromJson.connectedbitsID)
        {
            var bit = GameObject.Find(connectedBitID);
            if (bit != null)
            {
                eROBSONItem.connectedbits.Add(bit.GetComponent<eROBSONItems>());
            }
        }

        //Load the port infos and snap the connected port on editmode
        for (int i = 0; i < eROBSONItem.Ports.Length; i++)
        {
            var bitPort = eROBSONItem.Ports[i];

            foreach (var portToLoad in bitFromJson.Ports)
            {
                if (portToLoad.index == i)
                {
                    bitPort.Connected = portToLoad.Connected;

                    //Find the port which was connected to this port
                    var connectedBitToPortGameObject = GameObject.Find(portToLoad.ConnectedPortBitPoiId);
                    if (connectedBitToPortGameObject)
                    {
                        var connectedBitToPort = connectedBitToPortGameObject.GetComponentInChildren<eROBSONItems>();
                        bitPort.DetectedPortPole = connectedBitToPort.Ports[portToLoad.index];
                    }

                }
            }
        }
    }



    /// <summary>
    /// When a new erobson augmentation is added to the scene
    /// </summary>
    /// <param name="toggleObjectGameObject"></param>
    private void OnErobsonItemAdded(GameObject erobsonGameObject)
    {
        // add the power source into connect bit list
        var eRobsonItem = erobsonGameObject.GetComponentInChildren<eROBSONItems>();
        if (eRobsonItem)
        {
            eRobsonItemsList.Add(eRobsonItem);

            //The power source should be added to the connected bits list at the start
            if (eRobsonItem.ID == BitID.USBPOWER)
            {
                AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
            }
        }
    }



    /// <summary>
    /// When the erobson augmentation is removed from the scene
    /// </summary>
    /// <param name="toggleObject"></param>
    private void OnErobsonItemDeleted(ToggleObject toggleObject)
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
                    foreach (var bit in eRobsonItemsList)
                    {
                        bit.GetComponent<BitsBehaviourController>().Init();
                    }
                }

                eRobsonItemsList.Remove(eRobsonItem);

                //Let adjust the circuit again after deleting this bit
                eRobsonItem.GetComponent<BitsBehaviourController>().ControlCircuit();

                Debug.Log(eRobsonItem.ID + " deleted");
            }
        }
    }


    /// <summary>
    /// When a step is activated
    /// </summary>
    private void OnActivateAction()
    {
        StartCoroutine(Init());
    }


    IEnumerator Init()
    {
        while(activityManager.ActiveAction == null)
        {
            yield return null;
        }
        eRobsonDataFolder = Path.Combine(activityManager.ActivityPath, $"eRobson/{activityManager.ActiveAction.id}");

        eRobsonItemsList.Clear();
        eRobsonConnectedItemsList.Clear();

        LoadeRobsonCircuit();
    }



    /// <summary>
    /// Save the circuit data into a json file
    /// </summary>
    private void SaveJson()
    {
        //The circuit should only be saved on editmode
        if (!RootObject.Instance.activityManager.EditModeActive)
            return;

        var circuit = new ERobsonCircuit();
        circuit.connectedbitsList = new List<ERobsonItem>();

        foreach (var bit in eRobsonConnectedItemsList)
        {
            var bitTosave = new ERobsonItem();

            //Save the poi Id
            bitTosave.PoiID = bit.poiID;

            //Save the id (Only to be clear onjson file. This will not be used on loading)
            bitTosave.ID = bit.ID.ToString();

            //Save the ports info
            var portsToJson = new PortItem[bit.Ports.Length];

            for (int i = 0; i < bit.Ports.Length; i++)
            {
                var p = bit.Ports[i];

                var portToSave = new PortItem();
                portToSave.index = i;

                if (p.DetectedPortPole)
                {
                    portToSave.ConnectedPortBitPoiId = p.DetectedPortPole.ERobsonItem.poiID;
                }

                portToSave.Connected = p.Connected;
                portsToJson[i] = portToSave;
            }

            bitTosave.Ports = portsToJson;

            bitTosave.IsActive = bit.IsActive;

            bitTosave.Dimmable = bit.Dimmable;

            if (bit.Dimmable)
            {
                bitTosave.Value = bit.Value;
            }

            bitTosave.position = bit.transform.parent.localPosition;

            bitTosave.rotation = bit.transform.parent.localRotation;


            foreach (var connectedBitm in bit.connectedbits)
            {
                bitTosave.connectedbitsID.Add(connectedBitm.poiID);
            }

            circuit.connectedbitsList.Add(bitTosave);
        }

        string eRobsonCircuitJson = JsonUtility.ToJson(circuit);

        if (!Directory.Exists(eRobsonDataFolder))
            Directory.CreateDirectory(eRobsonDataFolder);

        var jsonpath = $"{eRobsonDataFolder}/eRobsonCircuit.json";

        //write the json file
        File.WriteAllText(jsonpath, eRobsonCircuitJson);

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
            if (!eRobsonConnectedItemsList.Contains(bit))
                eRobsonConnectedItemsList.Add(bit);
        }
        else if (addOrRemove == AddOrRemove.REMOVE)
        {
            if (eRobsonConnectedItemsList.Contains(bit))
                eRobsonConnectedItemsList.Remove(bit);
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
    public string PoiID;
    public string ID;
    public PortItem[] Ports;
    public bool IsActive;
    public float Value;
    public bool Dimmable;
    public Vector3 position;
    public Quaternion rotation;
    public List<string> connectedbitsID = new List<string>();
}


[Serializable]
public class PortItem
{
    public int index;
    public bool Connected;
    public string ConnectedPortBitPoiId;
}