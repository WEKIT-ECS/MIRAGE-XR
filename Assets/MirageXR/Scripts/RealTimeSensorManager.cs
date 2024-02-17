using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MQTT;
using System.Threading.Tasks;
using System;

namespace MirageXR
{
    public class RealTimeSensorManager : MonoBehaviour
    {

        // TODO: re-add sensor UI
        // UI related variables for environment and human.
        public GameObject SensorButton;

        public Image PanelHumanFill;
        public Image ButtonHumanFill;

        public Image PanelEnvFill;
        public Image ButtonEnvFill;

        private int _redHuman;
        private int _yellowHuman;
        private int _redEnv;
        private int _yellowEnv;

        public Color32 Green;
        public Color32 Yellow;
        public Color32 Red;

        private byte _alpha = 128;

        private AudioSource _audio;
        public AudioClip Connected;
        public AudioClip Disconnected;
        public AudioClip Beep;

        private int _beepCounter;
        public int YellowBeep = 60;
        public int RedBeep = 30;

        // MQTT connection variables.
        public List<HumEnvMqttBehaviour> Connections = new List<HumEnvMqttBehaviour>();

        public List<SensorDisplay> Values = new List<SensorDisplay>();

        // Overall sensor connection states.
        private bool _sensorsConnected;
        private bool _sensorsConnectedPrevious;

        public bool IsActive;

        // There can only be one...
        public static RealTimeSensorManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            // SensorButton.SetActive(false);
        }

        private void OnEnable()
        {
            EventManager.OnClearAll += Clear;
            EventManager.OnPlayerReset += Clear;
        }

        private void OnDisable()
        {
            EventManager.OnClearAll -= Clear;
            EventManager.OnPlayerReset -= Clear;
        }

        private void Clear()
        {
            Debug.Log("Clearing connections!");
            // Clear lists.
            Values.Clear();
            Connections.Clear();
            _sensorsConnected = false;
            _sensorsConnectedPrevious = false;
            // SensorButton.SetActive(false);
        }

        // Use this for initialization
        private void Start()
        {
            _audio = GetComponent<AudioSource>();
            // SensorButton.SetActive(false);
        }

        void SetColor(string category, string color)
        {
            if (category.Equals("human"))
            {
                switch (color)
                {
                    case "red":
                        ButtonHumanFill.color = Red;
                        PanelHumanFill.color = Red;
                        break;
                    case "yellow":
                        ButtonHumanFill.color = Yellow;
                        PanelHumanFill.color = Yellow;
                        break;
                    case "green":
                        ButtonHumanFill.color = Green;
                        PanelHumanFill.color = Green;
                        break;
                }
            }
            else
            {
                switch (color)
                {
                    case "red":
                        ButtonEnvFill.color = EnvironmentColor(Red);
                        PanelEnvFill.color = EnvironmentColor(Red);
                        break;
                    case "yellow":
                        ButtonEnvFill.color = EnvironmentColor(Yellow);
                        PanelEnvFill.color = EnvironmentColor(Yellow);
                        break;
                    case "green":
                        ButtonEnvFill.color = EnvironmentColor(Green);
                        PanelEnvFill.color = EnvironmentColor(Green);
                        break;
                }
            }
        }

        private Color32 EnvironmentColor(Color32 color)
        {
            var panelFillColor = color;
            panelFillColor.a = _alpha;
            return panelFillColor;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (IsActive)
            {
                var connectionsCount = 0;

                if (Connections.Count > 0)
                    _sensorsConnected = true;
                else
                    _sensorsConnected = false;

                foreach (var connection in Connections)
                {
                    if (!connection.IsActive)
                        _sensorsConnected = false;
                    else
                        connectionsCount++;
                }

                if (_sensorsConnected && connectionsCount > 0)
                {
                    if (_sensorsConnected != _sensorsConnectedPrevious)
                    {
                        // SensorButton.SetActive(true);
                        _audio.clip = Connected;
                        // _audio.Play();
                    }

                    _redHuman = 0;
                    _yellowHuman = 0;
                    _redEnv = 0;
                    _yellowEnv = 0;

                    foreach (var variable in Values)
                    {
                        if (variable.State == SensorDisplay.SensorState.Yellow)
                        {
                            if (variable.Category == SensorDisplay.SensorCategory.Human)
                            {
                                _yellowHuman++;
                            }
                            else
                            {
                                _yellowEnv++;
                            }
                        }

                        else if (variable.State == SensorDisplay.SensorState.Red)
                        {
                            if (variable.Category == SensorDisplay.SensorCategory.Human)
                            {
                                _redHuman++;
                            }
                            else
                            {
                                _redEnv++;
                            }
                        }
                    }

                    SetColor("human", "green");

                    if (_yellowHuman != 0)
                        SetColor("human", "yellow");

                    if (_redHuman != 0)
                        SetColor("human", "red");

                    SetColor("environment", "green");

                    if (_yellowEnv != 0)
                        SetColor("environment", "yellow");

                    if (_redEnv != 0)
                        SetColor("environment", "red");

                    _beepCounter++;

                    if ((_redEnv != 0 || _redHuman != 0) && _beepCounter >= RedBeep)
                    {
                        PlayBeep();
                    }

                    else if ((_yellowEnv != 0 || _yellowHuman != 0) && _beepCounter >= YellowBeep)
                    {
                        PlayBeep();
                    }
                }

                // Sensor connection lost.
                else
                {
                    if (_sensorsConnected != _sensorsConnectedPrevious)
                    {
                        // SensorButton.SetActive(false);
                        _audio.clip = Disconnected;
                        _audio.Play();
                    }
                }

                _sensorsConnectedPrevious = _sensorsConnected;
            }
        }

        private void PlayBeep()
        {
            _audio.clip = Beep;
            _audio.Play();
            _beepCounter = 0;
        }
    }
}