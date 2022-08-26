using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;

public class ErobsonItemManager : MonoBehaviour
{

    #region Erobson Events
    public delegate void BitConnectedDelegate();
    public static event BitConnectedDelegate OnBitConnected;
    public static void BitConnected()
    {
        OnBitConnected?.Invoke();
    }


    public delegate void BitDisconnectedDelegate();
    public static event BitDisconnectedDelegate OnBitDisconnected;
    public static void BitDisconnected()
    {
        OnBitDisconnected?.Invoke();
    }
    #endregion

    public static List<eROBSONItems> eRobsonItemsList
    {
        get; private set;
    }

    public static ErobsonItemManager Instance
    {
        get; private set;
    }


    private void Start()
    {

        //Singleton
        if(Instance == null)
        {
            Instance = this;
        }else if(Instance != this)
        {
            Destroy(gameObject);
        }

        eRobsonItemsList = new List<eROBSONItems>();
    }

    private void OnEnable()
    {
        EventManager.OnAugmentationObjectCreated += OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted += OnErobsonItemDeleted;
    }

    private void OnDisable()
    {
        EventManager.OnAugmentationObjectCreated -= OnErobsonItemAdded;
        EventManager.OnAugmentationDeleted -= OnErobsonItemDeleted;
    }


    /// <summary>
    /// When a new erobson augmentation is added to the scene
    /// </summary>
    /// <param name="toggleObjectGameObject"></param>
    private void OnErobsonItemAdded(GameObject toggleObjectGameObject)
    {
        var eRobsonItem = toggleObjectGameObject.GetComponent<eROBSONItems>();
        if (eRobsonItem)
        {
            eRobsonItemsList.Add(eRobsonItem);
        }

        foreach (var item in eRobsonItemsList)
        {
            Debug.LogError(item.name);
        }
    }

    /// <summary>
    /// When the erobson augmentation is removed from the scene
    /// </summary>
    /// <param name="toggleObject"></param>
    private void OnErobsonItemDeleted(ToggleObject toggleObject)
    {
        if (toggleObject.predicate.StartsWith("eRobson"))
        {
            eRobsonItemsList.Remove(GameObject.Find(toggleObject.poi).GetComponentInChildren<eROBSONItems>());
        }
    }

}
