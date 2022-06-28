using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using MQTT;
using UnityEngine;
using UnityEngine.UI;
using MirageXR;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class DeviceMqttBehaviour : MonoBehaviour
{

    private MqttConnection _mqtt;

    private Sensor _sensor;

    private Transform _container;

    [Serializable]
    public class SensorVariable
    {
        public string Name;

        public string Key;
        public string Value;

        public string Type;
        public string Unit;

        public string Green;
        public string Yellow;
        public string Red;
        public string Normal;
        public string Disabled;

        public SensorValueDisplay ValueDisplay;
    }

    private GameObject _sensorDisplay;
    private SensorContainer _sensorContainer;

    public List<SensorVariable> Values = new List<SensorVariable>();

    private bool _connectionEstablished;
    private bool _connectionEstablishedPrevious;

    private void OnEnable()
    {
        _mqtt.OnMessageReceivedAsync += HandleMessages;
        _mqtt.OnConnectionEstablished += ConnectionEstablished;
        _mqtt.OnConnectionDisconnected += ConnectionLost;
        EventManager.OnClearAll += Delete;
    }

    private void OnDisable()
    {
        _mqtt.OnMessageReceivedAsync -= HandleMessages;
        _mqtt.OnConnectionEstablished -= ConnectionEstablished;
        _mqtt.OnConnectionDisconnected -= ConnectionLost;
        EventManager.OnClearAll -= Delete;
    }

    private async void Awake()
    {
        _mqtt = new MqttConnection();

        var sensorDisplayPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("SensorContainerPrefab");

        if (sensorDisplayPrefab)
        {
            _sensorDisplay = Instantiate(sensorDisplayPrefab, Vector3.zero, Quaternion.identity);
            _sensorDisplay.GetComponent<RectTransform>().localPosition = Vector3.zero;
            _sensorDisplay.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
            _sensorContainer = _sensorDisplay.GetComponent<SensorContainer>();
        }

    }

    public async Task<bool> Init(Sensor sensor)
    {
        _sensor = sensor;

        // Generate randomized client id.
        var clientId = "WEKIT-" + +DateTime.Now.Ticks;

        // Trim uri.
        var path = _sensor.uri.Replace("mqtt://", "");

        // The format should be 192.168.0.1:port
        var splitPath = path.Split(':');
        if (!splitPath.Length.Equals(2))
            return false;

        var url = path.Split(':')[0];
        var port = int.Parse(path.Split(':')[1]);

        // Check if username and password are set...
        if (!string.IsNullOrEmpty(_sensor.username))
        {
            // Require both.
            if (string.IsNullOrEmpty(_sensor.password))
                return false;
            try
            {
                await _mqtt.ConnectAsync(url, port, _sensor.username, _sensor.password, clientId);
                Debug.Log(_sensor.id + " connected.");
            }
            catch
            {
                return false;
            }
        }

        // Connection without username and password.
        else
        {
            try
            {
                await _mqtt.ConnectAsync(url, port, _sensor.username, _sensor.password, clientId);
                Debug.Log(_sensor.id + " connected.");
            }
            catch
            {
                return false;
            }
        }

        // All good!
        return true;
    }

    private void ConnectionEstablished(bool success)
    {
        if (success)
        {
            _connectionEstablished = true;
        }
    }

    private void ConnectionLost(bool success)
    {
        if (success)
        {
            _connectionEstablished = false;
        }
    }

    private async void ConnectionEstablishedRoutine()
    {
        Debug.Log("Connection established.");
        // Instantiate a sensor container.

        // _sensorDisplay.name = _sensor.id;

        // Set name & display title.
        _sensorContainer.gameObject.name = _sensor.id;
        _sensorContainer.Title = _sensor.name;

        // Set container.
        _container = _sensorContainer.Container;

        // Create value pairs && subscribe.
        foreach (var data in _sensor.data)
        {
            var sensorVariable = new SensorVariable
            {
                Name = data.name,
                Key = data.key
            };

            if (string.IsNullOrEmpty(data.type))
                sensorVariable.Type = "string";
            else
                sensorVariable.Type = data.type;

            sensorVariable.Unit = data.unit;

            sensorVariable.Normal = data.normal;
            sensorVariable.Green = data.green;
            sensorVariable.Yellow = data.yellow;
            sensorVariable.Red = data.red;
            sensorVariable.Disabled = data.disabled;

            var valuePrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("ValuePrefab");
            if (valuePrefab == null) return;
            sensorVariable.ValueDisplay = Instantiate(valuePrefab.GetComponent<SensorValueDisplay>());

            var variableObject = sensorVariable.ValueDisplay.gameObject;

            variableObject.name = data.key;

            var variableRect = variableObject.GetComponent<RectTransform>();

            variableRect.SetParent(_container);
            variableRect.localPosition = Vector3.zero;
            variableRect.localEulerAngles = Vector3.zero;
            variableRect.localScale = Vector3.one;

            // Subscribe to topic.
            await _mqtt.SubscribeAsync(data.key);

            // Add to list of sensor variables.
            Values.Add(sensorVariable);
        }
    }

    private async Task HandleMessages(string topic, string message)
    {
        await Task.Run(() => HandleMessagesRoutine(topic, message));
    }

    private void HandleMessagesRoutine(string topic, string message)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            var value = Values[i];
            if (topic.Equals(value.Key))
            {
                Values[i].Value = message;

                value.ValueDisplay.Value = value.Name + ": " + message;

                if (!string.IsNullOrEmpty(value.Unit))
                    value.ValueDisplay.Value += " " + value.Unit;

                switch (value.Type)
                {
                    case "float":
                    case "int":
                        if (CheckFloatValue(message, value.Green))
                            value.ValueDisplay.SetState("green");
                        else if (CheckFloatValue(message, value.Yellow))
                            value.ValueDisplay.SetState("yellow");
                        else if (CheckFloatValue(message, value.Red))
                            value.ValueDisplay.SetState("red");

                        else if (CheckFloatValue(message, value.Normal))
                            value.ValueDisplay.SetState("normal");
                        else if (CheckFloatValue(message, value.Disabled))
                            value.ValueDisplay.SetState("disabled");


                        break;
                    case "string":

                        if (message.Equals(value.Green))
                            value.ValueDisplay.SetState("green");
                        else if (message.Equals(value.Yellow))
                            value.ValueDisplay.SetState("yellow");
                        else if (message.Equals(value.Red))
                            value.ValueDisplay.SetState("red");
                        else if (message.Equals(value.Normal))
                            value.ValueDisplay.SetState("normal");
                        else if (message.Equals(value.Disabled))
                            value.ValueDisplay.SetState("disabled");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Check float value against sensor configuration.
    /// </summary>
    /// <param name="value">Received value.</param>
    /// <param name="state">State definition string.</param>
    /// <returns></returns>
    private bool CheckFloatValue(string value, string state)
    {

        // Must be able to parse as float.
        if (float.TryParse(value, out float floatValue))
        {
            // The given state has to be defined.
            if (!string.IsNullOrEmpty(state))
            {
                // Handling for range between 2 values.
                if (state.StartsWith("between:"))
                {
                    // Trim state.
                    state = state.Replace("between:", "");

                    // Get limits.
                    var limits = state.Split(';');

                    // We need exactly 2 values for the comparison!
                    if (limits.Length == 2)
                    {

                        // Check if we can parse also the limits as float.
                        if (float.TryParse(limits[0], out float min) && float.TryParse(limits[1], out float max))
                        {
                            // Do the actual comparison.
                            return (floatValue >= min && floatValue <= max);
                        }
                    }
                }

                // Handling for greater than.
                else if (state.StartsWith("greater:"))
                {
                    // Trim state.
                    state = state.Replace("greater:", "");


                    // Check if we can get the comparison value as float.
                    if (float.TryParse(state, out float comparison))
                    {
                        // Do the actual comparison;
                        return floatValue > comparison;
                    }
                }

                // Handling for smaller than.
                else if (state.StartsWith("smaller:"))
                {
                    // Trim state.
                    state = state.Replace("smaller:", "");


                    // Check if we can get the comparison value as float.
                    if (float.TryParse(state, out float comparison))
                    {
                        // Do the actual comparison.
                        return floatValue < comparison;
                    }
                }

                // Handling for equal.
                else if (state.StartsWith("equals:"))
                {
                    // Trim state.
                    state = state.Replace("equals:", "");


                    // Check if we can get the comparison value as float.
                    if (float.TryParse(state, out float comparison))
                    {
                        return floatValue.Equals(comparison);
                    }
                }
            }
        }
        return false;
    }

    public void LinkDisplay(Transform anchor)
    {
        _sensorDisplay.transform.SetParent(anchor);
        _sensorDisplay.GetComponent<RectTransform>().localPosition = Vector3.zero;
        _sensorDisplay.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;

        // Turn tagalong off if attached to a device.
        _sensorDisplay.GetComponent<RadialView>().enabled = false;
    }

    private void Delete()
    {
        _mqtt.Disconnect();
        Destroy(gameObject);
    }

    private void Update()
    {
        _sensorDisplay.name = _sensor.id;

        if (_connectionEstablished && _connectionEstablished != _connectionEstablishedPrevious)
            ConnectionEstablishedRoutine();
        _connectionEstablishedPrevious = _connectionEstablished;
    }
}
