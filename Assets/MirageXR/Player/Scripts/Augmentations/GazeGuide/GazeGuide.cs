using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class GazeGuide : MonoBehaviour
    {
        [SerializeField] private bool _isVisible;
        private bool _isFound;
        private float _counter = 1.5f;
        private float _timer = 1.5f;

        public GameObject Parent { get; set; }

        // Inbuild Unity function launched when object is first rendered with any camera.
        private void OnBecameVisible ()
        {
            // Raise the flag when object has become visible.
            _isVisible = true;
        }

        // Inbuild Unity function launched when object is no more rendered with any cameras.
        private void OnBecameInvisible ()
        {
            // Lower the flag when object has become invisible.
            _isVisible = false;
        }

        private void Update ()
        {
            // If object has entered the view, count time the timer.
            if (_isVisible && !_isFound)
            {
                _counter -= Time.deltaTime;
            }

            // If object leaves the view before timer is done, reset the timer.
            if (!_isVisible && !_isFound)
            {
                _counter = _timer;
            }

            // If timer reaches zero.
            if (_counter <= 0)
            {
                // Raise the flag.
                _isFound = true;

                // Disable the gaze guide.
                Parent.GetComponent<LineRenderer> ().enabled = false;

                // Disable this object.
                //gameObject.SetActive (false);
            }
        }
    }
}