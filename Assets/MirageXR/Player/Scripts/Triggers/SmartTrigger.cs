using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class SmartTrigger : MonoBehaviour
    {
        // Flag for preventing update to run without content.
        private bool _isActive;

        // Shoud we launch the trigger or not.
        private bool _trigger = true;

        // Action id of the action using this trigger.
        private string _actionId;

        // Sensor values.

        // Value(s) to be monitored.
        private readonly List<DeviceMqttBehaviour.SensorVariable> _keys = new List<DeviceMqttBehaviour.SensorVariable>();

        // Target value(s) that will launch the trigger.
        private readonly List<string> _targets = new List<string>();

        // Determines how to compare the values.
        private string _operator;

        // Used for between calculations.
        private float _min;
        private float _max;

        // Duration
        private float _duration;
        private float counter;

        private void OnEnable()
        {
            EventManager.OnPlayerReset += Delete;
        }

        private void OnDisable()
        {
            EventManager.OnPlayerReset -= Delete;
        }

        private void Delete()
        {
            Destroy(gameObject);
        }

        public bool CreateTrigger(string actionId, Trigger trigger)
        {
            return CreateTrigger(actionId, trigger.id, trigger.data, trigger.option, trigger.value, trigger.duration);
        }

        /// <summary>
        /// Create basic smart sensor that simply looks for value match.
        /// </summary>
        /// <param name="actionId">Action id off the triggering action.</param>
        /// <param name="sensorId">Sensor id.</param>
        /// <param name="keyId">Sensor value to monitor.</param>
        /// <param name="option">Comparison operator (null if not set).</param>
        /// <param name="targetValue">Target value that will launch the trigger.</param>
        /// <param name="duration">Duration how long the match has to be active for the trigger to trigger.</param>
        public bool CreateTrigger(string actionId, string sensorId, string keyId, string option, string targetValue, float duration)
        {
            try
            {
                if (string.IsNullOrEmpty(actionId))
                    throw new ArgumentException("Action id not set.");

                if (string.IsNullOrEmpty(sensorId))
                    throw new ArgumentException("Sensor id not set.");

                if (string.IsNullOrEmpty(keyId))
                    throw new ArgumentException("Key id not set.");

                var keyArray = keyId.Split(';');

                if (string.IsNullOrEmpty(targetValue))
                    throw new ArgumentException("Target value not set.");

                var targetArray = targetValue.Split(';');

                if (float.IsNaN(duration))
                    duration = 0f;

                _duration = duration;

                // Search for sensor.
                var sensorObject = GameObject.Find("Sensors/" + sensorId);

                if (sensorObject == null)
                    throw new MissingComponentException("Sensor " + sensorId + " not found.");

                // If we have a sensor, look for key.
                var sensorBehaviour = sensorObject.GetComponent<DeviceMqttBehaviour>();

                if (sensorBehaviour == null)
                    throw new MissingComponentException(sensorId + " doesn't have a sensor object attached.");

                var counter = 0;

                // Ok, now it gets a bit complicated and we have to handle things based on the possible operator...
                switch (option)
                {
                    // If operator is not set, we look for just a single value match.
                    case null:
                    case "":
                    case "none":
                    // Greater than and smaller than also operate on a single value.
                    case "greater":
                    case "smaller":
                        // If option is null or empty, set operator to "none" just in case...
                        if (string.IsNullOrEmpty(option))
                            _operator = "none";

                        // If we are looking for a single value, the key data can't contain multiple values.
                        if (keyArray.Length > 1)
                            throw new ArgumentException("Multiple key values for single value comparison.");

                        // And same thing for target values.
                        if (targetArray.Length > 1)
                            throw new ArgumentException("Multiple target values for single value comparison.");

                        // Ok the rest is quite straight forward...

                        foreach (var sensorValue in sensorBehaviour.Values)
                        {
                            if (sensorValue.Key == keyId)
                            {
                                // Ok, final check. Let's prevent greater than etc. comparisons between strings and bools...
                                if (_operator != "none" && (sensorValue.Type.Equals("string") || sensorValue.Type.Equals("bool")))
                                    throw new ArgumentException("Can't do numerical comparison on strings or booleans.");

                                // If key is found, attach the value as the monitored value.
                                _keys.Add(sensorValue);

                                // Set also the target.
                                _targets.Add(targetValue);

                                // Set operator.
                                _operator = option;

                                // And finally set the action id.
                                _actionId = actionId;

                                counter++;
                            }
                        }

                        if (counter == 0)
                            throw new MissingComponentException("Key " + keyId + " not found in " + sensorId);

                        break;

                    // Ok now the "and" operator:
                    case "and":

                        // If we are looking for multiple values for comparison, we have to have more than one.
                        if (keyArray.Length == 1)
                            throw new ArgumentException("Single key value for and comparison.");

                        // And same thing for target values.
                        if (targetArray.Length == 1)
                            throw new ArgumentException("Single target value for and comparison.");

                        // Then make sure that we have matching numbers of keys and targets.
                        if (keyArray.Length != targetValue.Split(';').Length)
                            throw new ArgumentException("Key and target count doesn't match.");

                        // Uh. Now let's handle the variables...
                        foreach (var sensorValue in sensorBehaviour.Values)
                        {
                            for (int i = 0; i < keyArray.Length; i++)
                            {
                                if (sensorValue.Key == keyArray[i])
                                {
                                    // If key is found, attach the value as the monitored value.
                                    _keys.Add(sensorValue);

                                    // Set also the target.
                                    _targets.Add(targetArray[i]);

                                    // Set operator.
                                    _operator = option;

                                    // And finally set the action id.
                                    _actionId = actionId;

                                    counter++;
                                }
                            }
                        }

                        if (counter == 0)
                            throw new MissingComponentException("Key ids " + keyId + " not found in " + sensorId);

                        break;

                    // Now the between:
                    case "between":

                        // We are checking only one value that should be between two values.
                        if (keyArray.Length > 1)
                            throw new ArgumentException("Key count not suitable for between operator.");

                        // ...Two values.
                        if (targetArray.Length != 2)
                            throw new ArgumentException("Target value count not suitable for between operator.");

                        // Now let's get our min and max...
                        var min = 0f;
                        if (!float.TryParse(targetArray[0], out min))
                            throw new ArgumentException("Min value not a numerical value.");

                        // Got min.
                        _min = min;

                        var max = 0f;
                        if (!float.TryParse(targetArray[1], out max))
                            throw new ArgumentException("Max value not a numerical value.");

                        // Got max.
                        _max = max;

                        // Let's check that they make sense...
                        if (min >= max)
                            throw new ArgumentException("Min greater or equal compared to max.");


                        // Uh. Now let's handle the variables...
                        foreach (var sensorValue in sensorBehaviour.Values)
                        {
                            if (sensorValue.Key == keyId)
                            {
                                // Ok, final check. Let's between comparisons with strings and bools...
                                if (sensorValue.Type.Equals("string") || sensorValue.Type.Equals("bool"))
                                    throw new ArgumentException("Can't do numerical comparison on strings or booleans.");

                                // If key is found, attach the value as the monitored value.
                                _keys.Add(sensorValue);

                                // Set operator.
                                _operator = option;

                                // And finally set the action id.
                                _actionId = actionId;

                                counter++;
                            }
                        }

                        if (counter == 0)
                            throw new MissingComponentException("Key ids " + keyId + " not found in " + sensorId);

                        break;

                    // Now just handle the unknown operator handling:
                    default:
                        _operator = "none";
                        throw new ArgumentException("Unknown operator: " + option);
                }
                return true;
            }
            catch (Exception e)
            {
                AppLog.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Make the trigger active.
        /// </summary>
        public void Activate()
        {
            _isActive = true;
        }

        /// <summary>
        /// Launch the trigger;
        /// </summary>
        private async void DoTrigger()
        {
            await RootObject.Instance.activityManager.DeactivateAction(_actionId);
            RootObject.Instance.activityManager.MarkCompleted(_actionId);
            GameObject.Find("SystemSounds").SendMessage("PlayIotTrigger", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Standard Unity loop.
        /// </summary>
        private void Update()
        {
            if (_isActive)
            {
                _trigger = true;

                try
                {
                    // Loop through all the key values.
                    for (int i = 0; i < _keys.Count; i++)
                    {
                        // Now handle things based on the operator.
                        switch (_operator)
                        {
                            // Defaults to single value comparison and "and".
                            default:
                                // Do things properly based on variable type.
                                switch (_keys[i].Type)
                                {
                                    // Defaults to string and bool.
                                    default:

                                        // If value and target doesn't match, we don't trigger.
                                        if (_keys[i].Value != _targets[i])
                                            _trigger = false;

                                        break;

                                    // Now the floats
                                    case "float":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(float.Parse(_keys[i].Value).Equals(float.Parse(_targets[i]))))
                                            _trigger = false;

                                        break;

                                    // And finally the ints.
                                    // Now the floats
                                    case "int":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(int.Parse(_keys[i].Value).Equals(int.Parse(_targets[i]))))
                                            _trigger = false;

                                        break;
                                }
                                break;

                            // Now the greater than operator.
                            case "greater":

                                // Let's do this properly based on the variable type.
                                switch (_keys[i].Type)
                                {
                                    // First the floats
                                    case "float":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(float.Parse(_keys[i].Value) > float.Parse(_targets[i])))
                                            _trigger = false;

                                        break;

                                    // Then the ints.
                                    case "int":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(int.Parse(_keys[i].Value) > int.Parse(_targets[i])))
                                            _trigger = false;

                                        break;

                                    // Shouldn't be possible at this stage, but let's make sure...
                                    default:
                                        throw new ArgumentException("Can't do numerical comparison on strings or booleans.");
                                }

                                break;

                            // Now the smaller than operator.
                            case "smaller":

                                // Let's do this properly based on the variable type.
                                switch (_keys[i].Type)
                                {
                                    // First the floats
                                    case "float":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(float.Parse(_keys[i].Value) < float.Parse(_targets[i])))
                                            _trigger = false;

                                        break;

                                    // Then the ints.
                                    case "int":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(int.Parse(_keys[i].Value) < int.Parse(_targets[i])))
                                            _trigger = false;

                                        break;

                                    // Shouldn't be possible at this stage, but let's make sure...
                                    default:
                                        throw new ArgumentException("Can't do numerical comparison on strings or booleans.");
                                }

                                break;

                            // Now the between operator.
                            case "between":

                                // Let's do this properly based on the variable type.
                                switch (_keys[i].Type)
                                {
                                    // First the floats
                                    case "float":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(float.Parse(_keys[i].Value) > _min && float.Parse(_keys[i].Value) < _max))
                                            _trigger = false;

                                        break;

                                    // Then the ints.
                                    case "int":

                                        // If value and target doesn't match, we don't trigger.
                                        if (!(int.Parse(_keys[i].Value) > _min && int.Parse(_keys[i].Value) < _max))
                                            _trigger = false;

                                        break;

                                    // Shouldn't be possible at this stage, but let's make sure...
                                    default:
                                        throw new ArgumentException("Can't do numerical comparison on strings or booleans.");
                                }

                                break;
                        }
                    }

                    // Reset the counter when trigger values don't match.
                    if (!_trigger)
                        counter = _duration;

                    // Count down the timer.
                    if (_trigger && counter > 0)
                        counter -= Time.deltaTime;

                    // Trigger done!
                    if (_trigger && counter <= 0)
                    {
                        _isActive = false;
                        DoTrigger();
                    }
                }
                catch (Exception e)
                {
                    AppLog.LogException(e);
                    throw;
                }
            }
        }
    }
}