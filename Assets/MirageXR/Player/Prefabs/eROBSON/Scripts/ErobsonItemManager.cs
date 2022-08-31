using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
using System;
using System.IO;

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
        }else if(Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        eRobsonItemsList = new List<eROBSONItems>();
        eRobsonConnectedItemsList = new List<eROBSONItems>();

        eRobsonDataFolder = Path.Combine(activityManager.ActivityPath, $"eRobson/{activityManager.ActiveAction.id}");

        LoadeRobsonCircuit();
    }


    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Subscribe()
    {
        EventManager.OnAugmentationObjectCreated += OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted += OnErobsonItemDeleted;
        EventManager.ActivitySaveButtonClicked += SaveJson;
        EventManager.OnActivityStarted += OnActivateAction;
    }

    private void Unsubscribe()
    {
        EventManager.OnAugmentationObjectCreated -= OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted -= OnErobsonItemDeleted;
        EventManager.ActivitySaveButtonClicked -= SaveJson;
        EventManager.OnActivityStarted -= OnActivateAction;
    }


    private void LoadeRobsonCircuit()
    {

        //Load json file
        eRobsonCircuit circuit = null;
        var jsonpath = $"{eRobsonDataFolder}/eRobsonCircuit.json";
        if (File.Exists(jsonpath))
        {
            circuit = JsonUtility.FromJson<eRobsonCircuit>(File.ReadAllText(jsonpath));
        }

        //Add all bits which exist in the scne to the list of all bits
        foreach (var toggleObject in activityManager.ActiveAction.enter.activates)
        {
            if (toggleObject.predicate.StartsWith("eRobson"))
            {
                var erobsonItemPoiObject = GameObject.Find(toggleObject.poi);
                if (erobsonItemPoiObject)
                {
                    var erobsonItem = erobsonItemPoiObject.GetComponentInChildren<eROBSONItems>();
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
                                    if (connectedBit.poiID == toggleObject.poi)
                                    {
                                        erobsonItem.CopySettings(connectedBit);
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }


                        if (erobsonItem.ID == BitID.USBPOWER && !eRobsonConnectedItemsList.Contains(erobsonItem))
                        {
                            eRobsonConnectedItemsList.Add(erobsonItem);
                        }
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
        }
    }

    /// <summary>
    /// When the erobson augmentation is removed from the scene
    /// </summary>
    /// <param name="toggleObject"></param>
    private void OnErobsonItemDeleted(ToggleObject toggleObject)
    {
        // remove the power source from connect bit list
        if (toggleObject.predicate.StartsWith("eRobson") )
        {
            var eRobsonItem = GameObject.Find(toggleObject.poi).GetComponentInChildren<eROBSONItems>();
            if (eRobsonItem)
            {
                eRobsonItemsList.Remove(eRobsonItem);
            }
        }
    }

    private void OnActivateAction()
    {
        eRobsonItemsList.Clear();
        eRobsonConnectedItemsList.Clear();

        LoadeRobsonCircuit();
    }


    private void SaveJson()
    {
        var circuit = new eRobsonCircuit();
        circuit.connectedbitsList = new List<eROBSONItems>();

        foreach (var bit in eRobsonConnectedItemsList)
        {
            if (bit.connectedbits.Count > 0)
            {
                circuit.connectedbitsList.Add(bit);
            }
        }

        string eRobsonCircuitJson = JsonUtility.ToJson(circuit);

        if (!Directory.Exists(eRobsonDataFolder))
            Directory.CreateDirectory(eRobsonDataFolder);

        var jsonpath = $"{eRobsonDataFolder}/eRobsonCircuit.json";

        //write the json file
        File.WriteAllText(jsonpath, eRobsonCircuitJson);

        Debug.Log("Circuit saved on " + jsonpath);
    }


    public static void AddOrRemoveFromConnectedList(eROBSONItems bit, AddOrRemove addOrRemove)
    {
        if(addOrRemove == AddOrRemove.ADD)
        {
            if (!eRobsonConnectedItemsList.Contains(bit))
                eRobsonConnectedItemsList.Add(bit);
        }else if(addOrRemove == AddOrRemove.REMOVE)
        {
            if (eRobsonConnectedItemsList.Contains(bit))
                eRobsonConnectedItemsList.Remove(bit);
        }
    }
}

[Serializable]
class eRobsonCircuit
{
    public List<eROBSONItems> connectedbitsList;
}
