using i5.Toolkit.Core.VerboseLogging;
using System;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Controls Rotation Machine based on sensor input.
    /// </summary>
    [RequireComponent(typeof(RotationMachine))]
    public class SmartRotationController : MonoBehaviour
    {
        // Rotation machine
        private RotationMachine _rotationMachine;

        // Sprite renderer
        private SpriteRenderer _spriteRenderer;

        // Data stream to be monitored.
        private DeviceMqttBehaviour.SensorVariable _stream;

        private float _min;
        private float _max;

        // Are we ready or not?
        private bool _isReady = false;

        private void Start()
        {
            // Get and setup the rotation machine.
            _rotationMachine = GetComponent<RotationMachine>();
            _rotationMachine.ActiveAxis = RotationMachine.Axis.Z;
            _rotationMachine.Speed = 1.5f;

            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Attach data stream to controller.
        /// </summary>
        /// <param name="sensor">Sensor id.</param>
        /// <param name="key">Stream key id.</param>
        /// <param name="min">Minimum value for the between comparison.</param>
        /// <param name="max">Maximum value for the between comparison.</param>
        /// <returns></returns>
        public bool AttachStream(string sensor, string key, float min, float max)
        {
            var sensorCounter = 0;
            var streamCounter = 0;

            try
            {
                // Let's do the easy bit first.
                if (min >= max)
                    throw new ArgumentException("Minimum value not smaller than maximum.");

                _min = min;
                _max = max;

                foreach (Transform obj in GameObject.Find("Sensors").transform)
                {
                    // We are only interested if a match is found.
                    if (obj.name != sensor)
                        continue;

                    sensorCounter++;

                    // Get sensor object streams.
                    var streams = obj.GetComponent<DeviceMqttBehaviour>().Values;

                    if (streams == null)
                        throw new MissingComponentException(sensor + " doesn't contain any data streams.");

                    // If we got any...
                    foreach (var stream in streams)
                    {
                        // Once again, we are looking only for a match.
                        if (stream.Key != key)
                            continue;

                        // Check that the value is numerical
                        if (stream.Type.Equals("string") || stream.Type.Equals("bool"))
                            throw new ArgumentException("Stream is not numerical.");

                        // Attach stream as the monitored stream.
                        _stream = stream;

                        streamCounter++;
                    }
                }

                if (sensorCounter == 0)
                    throw new ArgumentException("Sensor " + sensor + " couldn't be found.");

                if (streamCounter == 0)
                    throw new ArgumentException("Stream " + key + " couldn't be found.");

                _isReady = true;
                return true;
            }
            catch (Exception e)
            {
                AppLog.LogException(e);
                return false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If everything is initialised properly.
            if (_isReady)
            {
                // If value is too low, potentiometer should be rotated clockwise.
                if (float.Parse(_stream.Value) < _min)
                {
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.flipX = false;
                    _rotationMachine.ActiveDirection = RotationMachine.Direction.CW;
                }

                // If value is too high, potentiometer should be rotated counter-clockwise.
                else if (float.Parse(_stream.Value) > _max)
                {
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.flipX = true;
                    _rotationMachine.ActiveDirection = RotationMachine.Direction.CCW;
                }

                // If value is within the sweet spot, don't touch it! (This goes out to you Kaj...)
                else
                {
                    _spriteRenderer.enabled = false;
                }
            }
        }
    }
}