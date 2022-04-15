using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MQTT;
using UnityEngine;
using UnityEngine.UI;
using MirageXR;

public class WearableManager : MonoBehaviour
{

    private MqttConnection _mqtt;

    public string BrokerUrl = "192.168.0.1";
    public int BrokerPort = 1883;

    public string ButtonState = "none";
    private string _previousButtonState = "";
    public float Humidity;
    public float Temperature;

    public Text EnvironmentText;

    private void Awake()
    {
        _mqtt = new MqttConnection();
    }

    private void OnEnable()
    {
        _mqtt.OnConnectionEstablished += MqttConnected;
        _mqtt.OnMessageReceivedAsync += HandleMessagesAsync;
    }

    private async void OnDisable()
    {
        _mqtt.OnConnectionEstablished -= MqttConnected;
        _mqtt.OnMessageReceivedAsync -= HandleMessagesAsync;
        await _mqtt.DisconnectAsync();
    }

	// Use this for initialization
	async void Start ()
	{
	    await _mqtt.ConnectAsync(BrokerUrl, BrokerPort);
	}

    private async void MqttConnected(bool success)
    {
        if (success)
        {
            await _mqtt.SubscribeAsync("buttonState");
            await _mqtt.SubscribeAsync("humidity");
            await _mqtt.SubscribeAsync("temperature");
            await _mqtt.PublishAsync("buttonStateReset", "none");
        }
    }

    private async Task HandleMessagesAsync(string topic, string message)
    {
        await Task.Run(() => MessageHandler(topic, message));
    }

    private void MessageHandler(string topic, string message)
    {
        switch (topic)
        {
            case "buttonState":
                ButtonState = message;
                break;
            case "humidity":
                Humidity = float.Parse(message);
                break;
            case "temperature":
                Temperature = float.Parse(message);
                break;
        }
    }

    private void HandleButton(string state)
    {
        switch (state)
        {
            case "airtap":
                EventManager.Tap();
                break;

            case "next":
                EventManager.Next("touch");
                break;

            case "back":
                EventManager.Previous("touch");
                break;

            case "toggle_menu":
                EventManager.ToggleMenu();
                break;

            case "toggle_guides":
                EventManager.ToggleGuides();
                break;
           
            case "toggle_lock":
                EventManager.ToggleLock();
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
	    if (!ButtonState.Equals("none") && !ButtonState.Equals(_previousButtonState))
	    {
	        HandleButton(ButtonState);
	    }

	    _previousButtonState = ButtonState;

	    if (!Humidity.Equals(0))
	        EnvironmentText.text = "Temperature: " + Temperature + " C\n" + "Humidity: " + Humidity + " %";
	    else
	        EnvironmentText.text = "";
	}
}
