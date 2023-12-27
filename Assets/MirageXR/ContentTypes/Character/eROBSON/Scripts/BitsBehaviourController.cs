using Castle.Core.Internal;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BitsBehaviourController : MonoBehaviour
{
    private static readonly int SpeedString = Animator.StringToHash("SpeedString");
    private static readonly int OnString = Animator.StringToHash("ON");

    private eROBSONItems _eRobsonItem;

    private List<eROBSONItems> sortedBits;

    private bool _circuitControlling;

    private bool _userConnectionChecking;

    private bool _circuitIsSorted;

    private void Awake()
    {
        _eRobsonItem = GetComponent<eROBSONItems>();
        sortedBits = new List<eROBSONItems>();
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
        _circuitControlling = false;
        _eRobsonItem.IsActive = !_eRobsonItem.IsActive;
        _ = ControlCircuit();
    }



    /// <summary>
    /// Control every bit in this circuit and activate/deactivate it if it is not connected to power source
    /// </summary>
    public async Task ControlCircuit(bool checkConnections = false)
    {
        try
        {
            if (_circuitControlling)
            {
                return;
            }

            //We are controlling, just wait
            _circuitControlling = true;

            var eRobsonItemsList = ErobsonItemManager.ERobsonItemsList;

            //Order the list of bits by "connectedTime" variable
            // Order only once and avoid unnecessary list creation
            eRobsonItemsList.Sort((x, y) => x.connectedTime.CompareTo(y.connectedTime));

            // Find index for USBPOWER
            var idx = eRobsonItemsList.FindIndex(i => i.ID == BitID.USBPOWER);

            // Move power source item to first if it is not already and it is connected to the circuit
            if (idx > 0 && ErobsonItemManager.ERobsonActiveConnectedItemsList.Contains(eRobsonItemsList[idx]))
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

                    eRobsonItem.connectedTime = DateTime.MinValue;
                    ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.REMOVE);
                    BitActionToggle(eRobsonItem, false);
                    continue;
                }

                //Add into the connected list and activate
                eRobsonItem.HasPower = true;

                eRobsonItem.connectedTime = DateTime.Now;
                ErobsonItemManager.AddOrRemoveFromConnectedList(eRobsonItem, AddOrRemove.ADD);
                BitActionToggle(eRobsonItem, true);
            }

            //set the value text
            _eRobsonItem.SetValueText(_eRobsonItem.ID);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ControlCircuit: {e.Message}");
        }
        finally
        {
            // In play mode
            if (!RootObject.Instance.activityManager.EditModeActive && checkConnections)
            {
                if (!_userConnectionChecking && ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Count == ErobsonItemManager.ERobsonConnectedItemsListByTeacher.Count)
                {
                    _userConnectionChecking = true;
                    ComparePlayerCircuit();
                }
            }

            _circuitControlling = false;
        }
    }



    /// <summary>
    /// Sorts the bits based on their pole connections, starting from the USB power bit.
    /// </summary>
    /// <param name="unsortedBits">A list of unsorted eROBSONItems bits.</param>
    /// <returns>A list of eROBSONItems bits sorted according to their pole connections.</returns>
    public List<eROBSONItems> SortBitsByPoleConnection(List<eROBSONItems> unsortedBits)
    {
        if (_circuitIsSorted)
        {
            return sortedBits;
        }

        sortedBits.Clear();
        eROBSONItems currentBit = FindUsbPowerBit(); // Find Usb power

        while (currentBit != null)
        {
            if (sortedBits.Contains(currentBit))
            {
                // Break the loop if the currentBit is already in sortedBits to avoid an infinite loop
                break;
            }

            sortedBits.Add(currentBit);
            currentBit = FindNextBit(currentBit); // Find next connected bit
        }

        // Check if the last bit needs to be added separately
        var lastBit = unsortedBits.Except(sortedBits)
            .FirstOrDefault(b => b.ID != BitID.USBPOWER);
        if (lastBit != null)
        {
            sortedBits.Add(lastBit);
        }

        return sortedBits;
    }


    /// <summary>
    /// Finds the USB power bit from the list of connected items.
    /// </summary>
    /// <returns>The USB power bit if found, otherwise null.</returns>
    private eROBSONItems FindUsbPowerBit()
    {
        var usbPower = ErobsonItemManager.ERobsonConnectedItemsListByPlayer.FirstOrDefault(b => b.ID == BitID.USBPOWER);

        return usbPower;
    }



    /// <summary>
    /// Finds the next bit connected to the current bit based on pole connection rules.
    /// </summary>
    /// <param name="currentBit">The current bit from which to find the next connected bit.</param>
    /// <returns>The next connected bit if found, otherwise null.</returns>
    private eROBSONItems FindNextBit(eROBSONItems currentBit)
    {
        if (!currentBit)
        {
            return null;
        }

        // Check each port of the current bit
        foreach (var port in currentBit.Ports)
        {
            // Skip if the port is not negative or USB, or if it's not connected
            if ((port.Pole != Pole.NEGATIVE && port.Pole != Pole.USB) || !port.Connected)
            {
                continue;
            }

            // Check if the DetectedPortPole is null to avoid null reference error
            if (port.DetectedPortPole == null)
            {
                continue;
            }

            // Get the bit connected to this port
            var connectedBit = port.DetectedPortPole.ERobsonItem;

            // Verify that the connected bit's port is either positive or USB
            if (connectedBit != null && (port.DetectedPortPole.Pole == Pole.POSITIVE || port.DetectedPortPole.Pole == Pole.USB))
            {
                return connectedBit;
            }
        }

        // If no next bit found or if all next bits have null DetectedPortPole
        return null;
    }




    /// <summary>
    /// Control if the user in playmode has connected the bits same as the editor
    /// </summary>
    private void ComparePlayerCircuit()
    {
        bool allConnectedCorrectly = true;

        var sortedERobsonConnectedItemsListByPlayer = SortBitsByPoleConnection(ErobsonItemManager.ERobsonConnectedItemsListByPlayer);
        _circuitIsSorted = transform;

        // Assuming that the counts of both lists are already verified to be equal before calling this method.
        for (int i = 0; i < ErobsonItemManager.ERobsonConnectedItemsListByTeacher.Count; i++)
        {
            var teacherBit = ErobsonItemManager.ERobsonConnectedItemsListByTeacher[i];
            var playerBit = sortedERobsonConnectedItemsListByPlayer[i];

            // Check if the playerBit is connected correctly and in the same order as the teacherBit.
            if (!IsBitConnectedCorrectlyAndInOrder(playerBit, teacherBit))
            {
                allConnectedCorrectly = false;
                break;
            }
        }
        if (ErobsonItemManager.Instance.PromptMessageIsOpen)
        {
            return;
        }

        ErobsonItemManager.Instance.PromptMessageIsOpen = true;

        // Display result
        if (allConnectedCorrectly)
        {
            RootView_v2.Instance.dialog.ShowMiddle(
           "Success!",
           "Circuit connected correctly",
           "OK", () => _userConnectionChecking = false,
           "OK", () => _userConnectionChecking = false,
           true);
        }
        else
        {
            RootView_v2.Instance.dialog.ShowMiddle(
            "Warning!",
            "You connected the bits wrong",
            "OK", () => _userConnectionChecking = false,
            "OK", () => _userConnectionChecking = false,
            true);
        }

        _userConnectionChecking = false;
    }



    /// <summary>
    /// Checks if a bit in the player's circuit is connected correctly and in the same order as in the teacher's circuit.
    /// </summary>
    /// <param name="playerBit">The bit from the player's circuit to be validated.</param>
    /// <param name="teacherBit">The corresponding bit from the teacher's circuit that serves as a reference for the correct connections and order.</param>
    /// <returns>
    /// Returns true if the playerBit is connected in the same way and order as the teacherBit.
    /// Specifically, it verifies that each bit connected to the teacherBit is also connected to the player
    /// Bit at the same index, ensuring the order of connections is consistent.
    /// Returns false if the playerBit is missing, if any of the required connections are missing, or if the connections are not in the correct order.
    /// </returns>
    private bool IsBitConnectedCorrectlyAndInOrder(eROBSONItems playerBit, eROBSONItems teacherBit)
    {
        if (playerBit == null || teacherBit == null || playerBit.ID != teacherBit.ID)
        {
            return false; // Either player bit or teacher bit is not present or they are not the same bit
        }

        if (playerBit.ConnectedBits.Count != teacherBit.ConnectedBits.Count)
        {
            return false; // The number of connections does not match
        }

        for (int i = 0; i < teacherBit.ConnectedBits.Count; i++)
        {
            if (playerBit.ConnectedBits[i].ID != teacherBit.ConnectedBits[i].ID)
            {
                return false; // A connection is not in the correct order
            }
        }

        return true; // All connections are present and in the correct order
    }



    /// <summary>
    /// Check if any dimmable bit exist in the circuit within the given bit
    /// If exist return the average value otherwise return -1
    /// </summary>
    /// <param name="eRobsonItem">The dimmable bit to be calculated for its value</param>
    /// <returns>float</returns>
    public static float CalculateValue(eROBSONItems eRobsonItem)
    {
        var ERobsonActiveConnectedItemsList = ErobsonItemManager.ERobsonActiveConnectedItemsList;
        var dimmingToBeCalculated = ERobsonActiveConnectedItemsList.FindAll(b => b.Dimmable && ERobsonActiveConnectedItemsList.IndexOf(b) < ERobsonActiveConnectedItemsList.IndexOf(eRobsonItem));

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
        // Check if the current bit is the power source itself
        if (bit.ID == BitID.USBPOWER)
        {
            return true; // Power source always has power
        }

        // Iterate through each connected bit to check for power
        foreach (var connectedBit in bit.ConnectedBits)
        {
            if (connectedBit == null)
            {
                continue; // Skip null connected bits
            }

            // Special case for USB power connector
            if ((bit.ID == BitID.P3USBPOWERCONNECTOR && connectedBit.ID == BitID.USBPOWER) ||
            (bit.ID == BitID.USBPOWER && connectedBit.ID == BitID.P3USBPOWERCONNECTOR))
            {
                bit.Ports[0].Connected = true; //usb port of usb power connecting
                connectedBit.Ports[0].Connected = true; //usb power of p3 use power is connecting
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

        // No connected bit has power
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

        if (!ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Contains(bit))
        {
            if (bit.IsMoving || bit.ID == BitID.USBPOWER)
            {
                ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Add(bit);
            }
        }

        _circuitIsSorted = false;

        _ = ControlCircuit(true);
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
        if ((bit.ID == BitID.USBPOWER || bit.ID == BitID.P3USBPOWERCONNECTOR) && !ErobsonItemManager.ERobsonActiveConnectedItemsList.Exists(b => b.ID == BitID.USBPOWER))
        {
            //disconnect the output ports
            var portsToDisconnect = bit.Ports.FindAll(p => p.Pole == Pole.USB || p.Pole == Pole.NEGATIVE);
            foreach (var port in portsToDisconnect)
            {
                port.Connected = false;
            }

            ErobsonItemManager.Instance.CutCircuitPower();

            //by disconnecting usb power the circuit should be rebuilded agin from start
            if (bit.ID is BitID.USBPOWER)
            {
                ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Clear();
            }
        }

        if (ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Contains(bit))
        {
            if (bit.IsMoving || bit.ID == BitID.USBPOWER)
            {
                ErobsonItemManager.ERobsonConnectedItemsListByPlayer.Remove(bit);
            }
        }

        _circuitIsSorted = false;

        _ = ControlCircuit(true);
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

        await ControlCircuit();

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
        if (!bit)
        {
            return;
        }

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