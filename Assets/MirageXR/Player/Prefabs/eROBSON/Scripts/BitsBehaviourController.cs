using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using Obi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BitsBehaviourController : MonoBehaviour
{
    private static readonly int SpeedString = Animator.StringToHash("SpeedString");
    private static readonly int OnString = Animator.StringToHash("ON");

    private eROBSONItems _eRobsonItem;


    /// <summary>
    /// Circuit is under control and should not another control starts
    /// </summary>
    private bool CircuitControlling
    {
        get; set;
    }

    /// <summary>
    /// Initiate the bits at the start
    /// </summary>
    public void Init()
    {
        if (!_eRobsonItem)
        {
            return;
        }

        _eRobsonItem.IsActive = true;
        _eRobsonItem.HasPower = false;
        _eRobsonItem.Dimmable = false;

        switch (_eRobsonItem.ID)
        {
            case BitID.O2LONGLED:
            case BitID.O6BUZZER:
            case BitID.O9BARGRAPH:
            case BitID.O13FAN:
            case BitID.P3USBPOWERCONNECTOR:
            case BitID.W2BRANCH:
            case BitID.W7FORK:
                break;
            case BitID.USBPOWER:
                _eRobsonItem.HasPower = true;
                break;
            case BitID.I3BUTTON:
                _eRobsonItem.IsActive = false;
                break;
            case BitID.I5SLIDEDIMMER:
                _eRobsonItem.Dimmable = true;
                _eRobsonItem.Value = 0.5f;
                break;
            case BitID.I11PRESSURESENSOR:
            case BitID.I18MOTIONSENSOR:
            case BitID.O25DCMOTOR:
                _eRobsonItem.Dimmable = true;
                _eRobsonItem.Value = 0.0f;
                break;
        }
    }



    /// <summary>
    /// Set the value of the bits sensor
    /// </summary>
    /// <param name="bitValue">This is the value which sensor has been registered such as slider value</param>
    public void SetValue(float bitValue)
    {
        _eRobsonItem.Value = bitValue;
    }



    /// <summary>
    /// Toggle the active state of the bit. Uses for i3 button
    /// </summary>
    public void BitActivatingToggle()
    {
        _eRobsonItem.IsActive = !_eRobsonItem.IsActive;
        ControlCircuit();
    }



    /// <summary>
    /// Control every bit in this circuit and activate/deactivate it if it is not connected to power source
    /// </summary>
    public async void ControlCircuit()
    {
        try
        {
            if (CircuitControlling)
            {
                return;
            }

            //We are controlling, just wait
            CircuitControlling = true;

            var eRobsonItemsList = ErobsonItemManager.ERobsonItemsList;

            //Order the list of bits by "connectedTime" variable
            eRobsonItemsList = eRobsonItemsList.OrderBy(e => e.connectedTime).ToList();

            // Find index for USBPOWER
            var idx = eRobsonItemsList.FindIndex(i => i.ID == BitID.USBPOWER);

            // Move power source item to first if it is not already and it is connected to the circuit
            if (idx > 0 && ErobsonItemManager.ERobsonConnectedItemsList.Contains(eRobsonItemsList[idx]))
            {
                MoveToTop(eRobsonItemsList, idx);
            }

            //Check all bits and deactivate all bits after this deactivated bit
            foreach (var eRobsonItem in eRobsonItemsList)
            {
                //Check the bits which are connected to this bit
                var hasConnectedPower = await HasConnectedPowerUpToCurrentModule(eRobsonItem);

                //remove from the connected list and deactivate
                if (!eRobsonItem.IsActive || (eRobsonItem.Dimmable && eRobsonItem.Value <= 0) || !hasConnectedPower)
                {
                    //USBPower always has power
                    if (eRobsonItem.ID != BitID.USBPOWER)
                    {
                        eRobsonItem.HasPower = false;
                    }

                    ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.REMOVE);
                    BitActionToggle(eRobsonItem, false);
                    continue;
                }

                //Add into the connected list and activate
                eRobsonItem.HasPower = true;
                ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
                BitActionToggle(eRobsonItem, true);
            }

            //set the value text
            _eRobsonItem.SetValueText(_eRobsonItem.ID);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            CircuitControlling = false;
        }
    }


    private void Awake()
    {
        _eRobsonItem = GetComponent<eROBSONItems>();
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
    /// Check if any dimmable bit exist in the circuit within the given bit
    /// If exist return the average value otherwise return -1
    /// </summary>
    /// <param name="eRobsonItem">The dimmable bit to be calculated for its value</param>
    /// <returns>float</returns>
    public static float CalculateValue(eROBSONItems eRobsonItem)
    {
        var eRobsonConnectedItemsList = ErobsonItemManager.ERobsonConnectedItemsList;
        var dimmingToBeCalculated = eRobsonConnectedItemsList.FindAll(b => b.Dimmable && eRobsonConnectedItemsList.IndexOf(b) < eRobsonConnectedItemsList.IndexOf(eRobsonItem));

        if (dimmingToBeCalculated.Count == 0)
        {
            return -1;
        }


        float valueSum = 0;
        float counter = 0;
        foreach (var bit in dimmingToBeCalculated)
        {
            valueSum += bit.Value;
            counter++;
        }

        return valueSum / counter;
    }


    /// <summary>
    /// Control if is power connected from the source to the given bit
    /// This will check all bits from the source to the given bit
    /// </summary>
    /// <param name="bit">The bit that needs to be checked for power</param>
    /// <returns>true if the bit has power</returns>
    private static async Task<bool> HasConnectedPowerUpToCurrentModule(eROBSONItems bit)
    {
        // Check each connected bit
        foreach (var connectedBit in bit.ConnectedBits)
        {
            if (connectedBit == null)
            {
                // If the connected bit is null, continue to the next bit
                continue;
            }


            //if USBPOWER connecting into P3USBPOWERCONNECTOR
            if ((bit.ID == BitID.USBPOWER && connectedBit.ID == BitID.P3USBPOWERCONNECTOR) ||
                (bit.ID == BitID.P3USBPOWERCONNECTOR && connectedBit.ID == BitID.USBPOWER))
            {
                return true;
            }


            // If the connected bit is not activated or does not have power, continue to the next bit
            if (!connectedBit.HasPower)
            {
                continue;
            }

            // If the connected bit has power and is activated, increment the count of connected bits with power
            if (connectedBit.HasPower)
            {
                return true;
            }


            //No more connected bits
            if (connectedBit == bit)
            {
                continue;
            }

            // If the connected bit is not the current bit, recursively check all of its connected bits
            if (await HasConnectedPowerUpToCurrentModule(connectedBit))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// The behaviour of the bit after connecting
    /// </summary>
    /// <param name="bit">The bit which is connected</param>
    private void OnItemConnected(eROBSONItems bit)
    {
        if (bit != _eRobsonItem)
        {
            return;
        }

        ControlCircuit();
    }


    /// <summary>
    /// The behaviour of the bit after disconnecting
    /// </summary>
    /// <param name="bit">The bit which is disconnected</param>
    private void OnItemDisconnected(eROBSONItems bit)
    {
        if (bit != _eRobsonItem)
        {
            return;
        }

        //No power source any more
        if (bit.ID == BitID.USBPOWER && !ErobsonItemManager.ERobsonConnectedItemsList.Exists(b => b.ID == BitID.USBPOWER))
        {
            ErobsonItemManager.Instance.CutCircuitPower();
        }

        ControlCircuit();
    }


    /// <summary>
    /// Set the value of sliders
    /// </summary>
    public async void SetValue()
    {
        var pinchSlider = GetComponentInChildren<PinchSlider>();
        if (!pinchSlider)
        {
            return;
        }

        _eRobsonItem.Value = pinchSlider.SliderValue;

        ControlCircuit();

        await Task.Delay(100);
    }


    /// <summary>
    /// Turn on or off the power indicator light depends on it has power(connected) or not
    /// </summary>
    /// <param name="bit">The bit which its light indicator should be toggled</param>
    private static void SwitchPowerIndicatorLight(eROBSONItems bit)
    {
        //Turn on/off the power indicators
        if (bit && bit.IndicatorLight)
        {
            bit.IndicatorLight.SwitchLight(bit.HasPower);
        }
    }


    /// <summary>
    /// The actions which any bit will do on activate/deactivate status
    /// </summary>
    /// <param name="bit">The bit which will do some actions</param>
    /// <param name="status">the status of the bit sensor, e.g. on or off</param>
    private async void BitActionToggle(eROBSONItems bit, bool status)
    {
        //Turn on/off the power indicators
        SwitchPowerIndicatorLight(bit);

        var temp = CalculateValue(bit);
        var averageValue = temp == -1 ? 2 : temp * 2; //If no dimmable use default

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
                await ControlLedLight(bit, status, averageValue);
                break;
            case BitID.O6BUZZER:
                await ControlBuzzerSound(bit, status, averageValue);
                break;
            case BitID.O9BARGRAPH:
                await ControlBarGraph(bit, status, averageValue);
                break;
            case BitID.O13FAN:
            case BitID.O25DCMOTOR:
                var anim = bit.GetComponent<Animator>();
                anim.SetBool(OnString, status);
                anim.SetFloat(SpeedString, averageValue);
                break;
            case BitID.P3USBPOWERCONNECTOR:
                break;
            case BitID.W2BRANCH:
                break;
            case BitID.W7FORK:
                break;
            case BitID.USBPOWER:
                break;
        }
    }



    /// <summary>
    /// Turn on/off the led light
    /// </summary>
    /// <param name="bit"></param>
    /// <param name="status"></param>
    /// <param name="averageValue"></param>
    /// <returns></returns>
    private async Task ControlLedLight(eROBSONItems bit, bool status, float averageValue)
    {
        var light = bit.GetComponentInChildren<Light>();
        light.enabled = status;
        light.intensity = averageValue;

        //Change the material of the light bulb
        var lightMeshRenderer = light.GetComponentInParent<MeshRenderer>();
        Material lightMaterial;
        if (status)
        {
            lightMaterial = await ReferenceLoader.GetAssetReferenceAsync<Material>("eROBSON/Materials/LightOnMaterial");
        }
        else
        {
            lightMaterial = await ReferenceLoader.GetAssetReferenceAsync<Material>("eROBSON/Materials/LightOffMaterial");
        }

        lightMeshRenderer.material = lightMaterial;
    }




    /// <summary>
    /// control the buzzer and play the buzz sound regarding to the power
    /// </summary>
    /// <param name="bit"></param>
    /// <param name="status"></param>
    /// <param name="averageValue"></param>
    /// <returns></returns>
    private async Task ControlBuzzerSound(eROBSONItems bit, bool status, float averageValue)
    {
        var audios = bit.GetComponentsInChildren<AudioSource>();
        var buzzAudioSource = audios.FirstOrDefault(a => a.gameObject != bit.gameObject);

        if (!buzzAudioSource)
        {
            Debug.LogError("Buzzer needs an audio source in one of the children, in addition to the main game object");
            return;
        }

        buzzAudioSource.enabled = status;
        buzzAudioSource.volume = averageValue / 2; //normalized

        if (status)
        {
            buzzAudioSource.Play();
        }

        await Task.CompletedTask;
    }



    /// <summary>
    /// Control the bargraph and turn on/off the lights
    /// </summary>
    /// <param name="bit"></param>
    /// <param name="averageValue"></param>
    /// <returns></returns>
    private async Task ControlBarGraph(eROBSONItems bit, bool status, float averageValue)
    {
        bit.TryGetComponent<BargraphController>(out var bargraphController);
        if (!bargraphController)
        {
            return;
        }

        var power = averageValue / 2; //normalized
        bargraphController.TurnOnLights(status, power);

        await Task.CompletedTask;
    }




    /// <summary>
    /// Move an item from "index" to "0" in a list
    /// </summary>
    /// <param name="list">The list which contains the item</param>
    /// <param name="index">index of the item which should be as the first item in the list</param>
    private static void MoveToTop(IList<eROBSONItems> list, int index)
    {
        var item = list[index];
        for (var i = index; i > 0; i--)
        {
            list[i] = list[i - 1];
        }
        list[0] = item;
    }

}
