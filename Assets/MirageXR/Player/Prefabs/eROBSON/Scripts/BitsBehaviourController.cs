using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
using System.Linq;
using System.Threading.Tasks;

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
        }
    }



    /// <summary>
    /// Toggle the status of the bit
    /// </summary>
    public async void ActivatingToggle()
    {
        _erobsonItem.IsActive = !_erobsonItem.IsActive;

        //Check all bits and deactivate all bits after this deactivated bit
        foreach (var eRobsonItem in ErobsonItemManager.eRobsonItemsList)
        {
            if(eRobsonItem.connectedbits.Find(b => b.IsActive) == null)
            {
                var isPowerInCircuit = await IsPowerInCircuit(_erobsonItem);
                if (!isPowerInCircuit)
                {
                    _erobsonItem.HasPower = false;
                    OnItemDisconnected();
                }
                else
                {
                    OnItemConnected();
                }
            }
        }
    }


    /// <summary>
    /// Controll if is power connected from the source to the given bit
    /// This will check all bits from the source to the given bit
    /// </summary>
    /// <param name="bit"></param>
    /// <returns></returns>
    private async Task<bool> IsPowerInCircuit(eROBSONItems bit)
    {
        foreach (var connectedbit in bit.connectedbits)
        {
            if(connectedbit.HasPower && connectedbit.IsActive)
            {
                return true;
            }
            
            if(connectedbit.connectedbits.Count == 0)
            {
                return false;
            }
            else
            {
                return await IsPowerInCircuit(connectedbit);
            }
        }

        return false;
    }


    /// <summary>
    /// The behaviour of the bit after connecting
    /// </summary>
    private async void OnItemConnected()
    {
        if (!_erobsonItem)
            return;

        var isPowerInCircuit = await IsPowerInCircuit(_erobsonItem);
        if (isPowerInCircuit)
        {
            _erobsonItem.HasPower = true;
            BitActionToggle(_erobsonItem.HasPower);
        }
    }


    /// <summary>
    /// The behaviour of the bit after disconnecting
    /// </summary>
    private async void OnItemDisconnected()
    {
        if (!_erobsonItem)
            return;

        var isPowerInCircuit = await IsPowerInCircuit(_erobsonItem);
        if (!isPowerInCircuit)
        {
            _erobsonItem.HasPower = false;
            BitActionToggle(_erobsonItem.HasPower);
        }
            
    }


    /// <summary>
    /// The actions which any bit will do on active/deactive status
    /// </summary>
    /// <param name="active"></param>
    private void BitActionToggle(bool active)
    {
        switch (_erobsonItem.ID)
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
                _erobsonItem.GetComponentInChildren<Light>().enabled = active;
                break;
            case BitID.O6BUZZER:
                break;
            case BitID.O9BARGRAPH:
                break;
            case BitID.O13FAN:
                break;
            case BitID.O25DCMOTOR:
                break;
            case BitID.P3USBPOWERCONNECTOR:
                break;
            case BitID.W2BRANCH:
                break;
            case BitID.W7FORK:
                break;
            case BitID.USBPOWER:
                _erobsonItem.HasPower = true; //USB power always has power
                break;
            default:
                break;
        }
    }

}
