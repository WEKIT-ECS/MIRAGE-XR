using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using MirageXR;
using i5.Toolkit.Core.VerboseLogging;

namespace MirageXR
{
    public class Detect : MirageXRPrefab
    {
        // Gaze manager.
        private GazeProvider gazeProvider;
        private bool _isDetected;
        private bool _isTrigger;

        // Guide line starting point object.
        private Transform _startingPoint;

        // Trigger related variables.
        private bool _isCompleted;
        private float _duration = 0f;
        private float _target = 5f;

        // Visualization components.
        private LineRenderer _lineRend;
        private Image _timerCircle;
        private Image _triangles;
        private RectTransform _trianglesTransform;
        private Image _centerDot;

        // Audio component.
        private AudioSource _audio;

        private float _rotationFactor = 0.35f;

        // Use this for initialization
        private void Awake()
        {
            // Disable the default gaze guide since we are using a custom one...
            UseGuide = false;

            // Attach components


            _startingPoint = GameObject.FindGameObjectWithTag("MainCamera").transform;

            _lineRend = GetComponent<LineRenderer>();
            _timerCircle = transform.Find("TimerCircle").GetComponent<Image>();
            _triangles = transform.Find("Triangles").GetComponent<Image>();
            _trianglesTransform = transform.Find("Triangles").GetComponent<RectTransform>();
            _centerDot = transform.Find("Center").GetComponent<Image>();

            _audio = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            // Set scale, if defined in the action step configuration.
            if (!obj.scale.Equals(0))
                GetComponent<RectTransform>().localScale = new Vector3(obj.scale, obj.scale, obj.scale) / 2048;

            // If scaling is not set, default to 5 cm symbol.
            else
                GetComponent<RectTransform>().localScale = new Vector3(0.05f, 0.05f, 0.05f) / 2048;

            // Set target duration if defined in the action configuration.
            if (!obj.duration.Equals(0))
                _target = obj.duration;

            // If duration is not set, default to 1.5 seconds.
            else
                _target = 1.5f;

            // Shouldn't be a trigger when initialized with a toggle object.
            _isTrigger = false;

            // If all went well, return true.
            return true;
        }

        // Update is called once per frame
        private void Update()
        {
            // Line renderer is enabled once a proper location reference is received.
            if (_lineRend.enabled)
            {
                // Set default state.
                _isDetected = false;

                // Check if user is gazing this symbol.
                if (gazeProvider.GazeTarget)
                {
                    var hitObject = gazeProvider.GazeTarget; //_gazeManager.HitObject;

                    if (hitObject.transform.parent != null)
                    {
                        // Cursor will hit the child objects so check the parent for match.
                        if (hitObject.transform.parent.gameObject == gameObject)
                            _isDetected = true;
                    }
                }

                // Rotate triangles. The closer you get, the slower the rotation.
                _trianglesTransform.Rotate(Vector3.forward, Vector3.Distance(transform.position, _startingPoint.position) * _rotationFactor);

                // If not detected and not yet completed, draw the line between the symbol and the cursor.
                if (!_isDetected && !_isCompleted)
                {
                    _lineRend.enabled = true;
                    _lineRend.SetPosition(0, _startingPoint.position);
                    _lineRend.SetPosition(1, transform.position);
                    _centerDot.enabled = true;
                    _triangles.enabled = true;
                    _timerCircle.enabled = true;
                }

                // Detected but not yet completed.
                else if (_isDetected && !_isCompleted)
                {
                    _lineRend.enabled = false;
                    _centerDot.enabled = false;
                    _triangles.enabled = true;
                    _timerCircle.enabled = true;
                }

                // When completed, hide everything except the main circles.
                else
                {
                    _lineRend.enabled = false;
                    _centerDot.enabled = false;
                    _triangles.enabled = false;
                    _timerCircle.enabled = false;
                }

                // When detected, calculate duration.
                if (_isDetected && !_isCompleted)
                {
                    _duration += Time.deltaTime;
                }

                else
                {
                    _duration = 0;
                }

                // Fill in the timer circle based on the duration (percentage of the target).
                _timerCircle.fillAmount = _duration / _target;

                // When target is met, do magic.
                if (_duration >= _target && !_isCompleted)
                {
                    // Do trigger stuff.
                    if (_isTrigger)
                    {

                    }

                    // Ding dong.
                    _audio.Play();

                    // Wohoo!
                    _isCompleted = true;
                }
            }
        }
    }
}