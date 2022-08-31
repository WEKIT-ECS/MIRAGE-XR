using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BitsBehaviourController : MonoBehaviour
{

    private eROBSONItems _erobsonItem;

    private void Awake()
    {
        _erobsonItem = GetComponent<eROBSONItems>();
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        ErobsonItemManager.OnBitConnected += OnItemConnected;
        ErobsonItemManager.OnBitDisconnected += OnItemDisconnected;
    }

    private void OnDisable()
    {
        ErobsonItemManager.OnBitConnected -= OnItemConnected;
        ErobsonItemManager.OnBitDisconnected -= OnItemDisconnected;
    }


    /// <summary>
    /// Initiate the bits at the start
    /// </summary>
    public void Init()
    {
        if (!_erobsonItem)
            return;

        switch (_erobsonItem.ID)
        {
            case BitID.O2LONGLED:
            case BitID.O6BUZZER:
            case BitID.O9BARGRAPH:
            case BitID.O13FAN:
            case BitID.P3USBPOWERCONNECTOR:
            case BitID.W2BRANCH:
            case BitID.W7FORK:
                _erobsonItem.IsActive = true;
                break;
            case BitID.USBPOWER:
                _erobsonItem.IsActive = true;
                _erobsonItem.HasPower = true;
                break;
            case BitID.I3BUTTON:
            case BitID.I5SLIDEDIMMER:
            case BitID.I11PRESSURESENSOR:
            case BitID.I18MOTIONSENSOR:
            case BitID.O25DCMOTOR:
                _erobsonItem.IsActive = false;
                break;
        }
    }



    /// <summary>
    /// Toggle the status of the bit
    /// </summary>
    public void ActivatingToggle()
    {
        _erobsonItem.IsActive = !_erobsonItem.IsActive;
        if (_erobsonItem.IsActive)
        {
            _erobsonItem.HasPower = true;
        }

        ControlCircuit();
    }


    /// <summary>
    /// Control every bit in this circuit and active/diactive it if it is not connected to power sourse 
    /// </summary>
    private void ControlCircuit()
    {
        ErobsonItemManager.eRobsonItemsList.OrderBy(e => e.connectedTime);
        //Check all bits and deactivate all bits after this deactivated bit
        foreach (var eRobsonItem in ErobsonItemManager.eRobsonItemsList)
        {
            //Check the bits which are connected to this bit
            var hasConnectedPower = HasConnectedPower(eRobsonItem);

            if ((hasConnectedPower && eRobsonItem.IsActive) || eRobsonItem.ID == BitID.USBPOWER)
            {
                eRobsonItem.HasPower = true;
                ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
                BitActionToggle(eRobsonItem, true);
            }
            else
            {
                eRobsonItem.HasPower = false;
                ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.REMOVE);
                BitActionToggle(eRobsonItem, false);
            }
        }
    }



    /// <summary>
    /// Controll if is power connected from the source to the given bit
    /// This will check all bits from the source to the given bit
    /// </summary>
    /// <param name="bit"></param>
    /// <returns></returns>
    private bool HasConnectedPower(eROBSONItems bit)
    {
        foreach (var connectedbit in bit.connectedbits)
        {
            if (connectedbit.HasPower && connectedbit.IsActive)
            {
                //Debug.Log($"{bit.name} is connected to {connectedbit} which is active and connected.");
                return true;
            }
            //Debug.Log($"{bit.name} is connected to {connectedbit} which is inactive or disconnected.");
        }

        return false;
    }


    /// <summary>
    /// The behaviour of the bit after connecting
    /// </summary>
    private void OnItemConnected(eROBSONItems bit)
    {
        if (bit != _erobsonItem)
            return;

        ControlCircuit();
    }


    /// <summary>
    /// The behaviour of the bit after disconnecting
    /// </summary>
    private void OnItemDisconnected(eROBSONItems bit)
    {
        if (bit != _erobsonItem)
            return;

        ControlCircuit();
    }


    /// <summary>
    /// The actions which any bit will do on active/deactive status
    /// </summary>
    /// <param name="active"></param>
    private void BitActionToggle(eROBSONItems bit, bool value)
    {
        switch (bit.ID)
        {
            case BitID.I3BUTTON:
                break;
            case BitID.I5SLIDEDIMMER:
                break;
            case BitID.I11PRESSURESENSOR:
                break;
            case BitID.I18MOTIONSENSOR:
                break;
            case BitID.O2LONGLED:
                bit.GetComponentInChildren<Light>().enabled = value;
                break;
            case BitID.O6BUZZER:
                break;
            case BitID.O9BARGRAPH:
                break;
            case BitID.O13FAN:
            case BitID.O25DCMOTOR:
                bit.GetComponent<Animator>().SetBool("ON", value);
                break;
            case BitID.P3USBPOWERCONNECTOR:
                break;
            case BitID.W2BRANCH:
                break;
            case BitID.W7FORK:
                break;
            case BitID.USBPOWER:
                break;
            default:
                break;
        }
    }

}
