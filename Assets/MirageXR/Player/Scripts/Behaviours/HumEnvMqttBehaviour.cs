using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MQTT;
using UnityEngine;

namespace MirageXR
{
    public class HumEnvMqttBehaviour : MonoBehaviour
    {

        public MqttConnection Mqtt;
        private Sensor _sensor;
        private GameObject _prefab;

        private RectTransform _humanContainer;
        private RectTransform _environmentContainer;

        private bool _readyToSubscribe;
        private bool _subcribed;

        private List<SensorDisplay> _values = new List<SensorDisplay>();

        public bool IsActive;

        private void Awake()
        {
            Mqtt = new MqttConnection();

            _prefab = Resources.Load<GameObject>("Prefabs/HumEnvSensorDisplayPrefab");

            _humanContainer = GameObject.FindGameObjectWithTag("HumanSensorContainer").GetComponent<RectTransform>();
            _environmentContainer = GameObject.FindGameObjectWithTag("EnvironmentSensorContainer").GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Mqtt.OnMessageReceivedAsync += HandleMessagesAsync;
            Mqtt.OnConnectionEstablished += ConnectionEstablished;
            Mqtt.OnConnectionDisconnected += ConnectionLost;

            EventManager.OnClearAll += Delete;
        }

        private async void OnDisable()
        {
            Mqtt.OnMessageReceivedAsync -= HandleMessagesAsync;
            Mqtt.OnConnectionEstablished -= ConnectionEstablished;
            Mqtt.OnConnectionDisconnected -= ConnectionLost;

            EventManager.OnClearAll -= Delete;

            await Mqtt.DisconnectAsync();
        }

        public async Task<bool> Init(Sensor sensor)
        {
            _sensor = sensor;

            Debug.Log("Starting connection to sensors...");

            // Generate randomized client id.
            var clientId = "WEKIT-";

            // Full uri e.g. "mqtt://192.168.0.1"
            var fullUri = _sensor.uri;

            if (!fullUri.StartsWith("mqtt://"))
            {
                Debug.Log("Tried to create sensor without mqtt connection: " + _sensor.id);
                return false;
            }

            // Get the actual url by simply removing the mqtt bit.
            fullUri = fullUri.Replace("mqtt://", "");

            var uriComponents = fullUri.Split(':');

            if (uriComponents.Length != 2)
            {
                Debug.Log("Sensor uri doesn't contain proper url + port definition: " + _sensor.id);
                return false;
            }

            var url = uriComponents[0];
            var port = int.Parse(uriComponents[1]);

            if (!string.IsNullOrEmpty(_sensor.username))
            {
                if (string.IsNullOrEmpty(_sensor.password))
                {
                    Debug.Log("Sensor connection username without a password: " + _sensor.id);
                    return false;
                }

                await Mqtt.ConnectAsync(url, port, _sensor.username, _sensor.password, clientId);
            }

            else
            {
                await Mqtt.ConnectAsync(url, port);
            }

            return true;
        }

        private void ConnectionEstablished(bool success)
        {
            if (success)
            {
                RealTimeSensorManager.Instance.Connections.Add(this);
                IsActive = true;

                _readyToSubscribe = true;
            }
        }

        private void ConnectionLost(bool success)
        {
            IsActive = false;
        }

        private async void Subscribe()
        {
            // Handle subscribtions.
            foreach (var data in _sensor.data)
            {
                // Subscribe to MQTT topic.
                await Mqtt.SubscribeAsync(data.key);

                RectTransform container;

                // Instantiate sensor display to correct container.
                if (_sensor.type.Equals("human"))
                {
                    container = _humanContainer;
                }
                else
                {
                    container = _environmentContainer;
                }

                var display = Instantiate(_prefab, Vector3.zero, Quaternion.identity, container);
                display.name = data.name;

                display.GetComponent<RectTransform>().localPosition = Vector3.zero;
                display.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
                display.GetComponent<RectTransform>().localScale = Vector3.one;

                var sensorDisplay = display.GetComponent<SensorDisplay>();
                sensorDisplay.Key = data.key;
                _values.Add(sensorDisplay);
                RealTimeSensorManager.Instance.Values.Add(sensorDisplay);

                // Configure sensor display.
                sensorDisplay.Name = data.name;
                sensorDisplay.Unit = data.unit;

                switch (_sensor.type)
                {
                    case "human":
                        sensorDisplay.Category = SensorDisplay.SensorCategory.Human;
                        break;
                    case "environment":
                        sensorDisplay.Category = SensorDisplay.SensorCategory.Environment;
                        break;
                }

                switch (data.type)
                {
                    case "int":
                        sensorDisplay.Type = SensorDisplay.SensorValueType.Int;
                        break;
                    case "float":
                        sensorDisplay.Type = SensorDisplay.SensorValueType.Float;
                        break;
                }

                // Get range.
                var rangeValues = data.range.Split(';');

                float.TryParse(rangeValues[0], out float rangeMin);

                float.TryParse(rangeValues[1], out float rangeMax);

                sensorDisplay.Min = rangeMin;
                sensorDisplay.Max = rangeMax;

                // Get limits.
                var greenLimits = data.green;

                if (greenLimits.StartsWith("between:"))
                {
                    greenLimits = greenLimits.Replace("between:", "");
                    var greenValues = greenLimits.Split(';');

                    float.TryParse(greenValues[0], out float greenMin);

                    float.TryParse(greenValues[1], out float greenMax);

                    sensorDisplay.GreenLimitMin = greenMin;
                    sensorDisplay.GreenLimitMax = greenMax;
                }

                // TODO: Handle other states... Check DeviceMqttBehaviour.cs for implementations...

                var yellowLimits = data.yellow;

                if (yellowLimits.StartsWith("between:"))
                {
                    yellowLimits = yellowLimits.Replace("between:", "");
                    var yellowValues = yellowLimits.Split(';');

                    float.TryParse(yellowValues[0], out float yellowMin);

                    float.TryParse(yellowValues[1], out float yellowMax);

                    sensorDisplay.YellowLimitMin = yellowMin;
                    sensorDisplay.YellowLimitMax = yellowMax;
                }

                // Red is the rest...
            }
        }

        private async Task HandleMessagesAsync(string topic, string message)
        {
            await Task.Run(() => HandleMessages(topic, message));
        }

        private void HandleMessages(string topic, string message)
        {
            foreach (var value in _values)
            {
                if (value.Key.Equals(topic))
                {
                    value.Value = message;
                }
            }
        }

        private void Update()
        {
            if (_readyToSubscribe && !_subcribed)
            {
                Subscribe();
                _subcribed = true;
            }
        }

        private void Delete()
        {
            Destroy(gameObject);
        }
    }
}