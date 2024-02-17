using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Rotation machine handles object rotation.
    /// </summary>
    public class RotationMachine : MonoBehaviour
    {
        // Used for setting CW / CCW rotation.
        private float _speedFactor = 1f;

        // Rotation axis selection.
        public enum Axis { None, X, Y, Z }
        public Axis ActiveAxis = Axis.X;

        // Rotation direction selection
        public enum Direction { CW, CCW }
        public Direction ActiveDirection = Direction.CW;

        public float Speed = 1f;

        // Update is called once per frame
        private void Update()
        {
            if (ActiveDirection == Direction.CW)
            {
                _speedFactor = -1f;
            }
            else
            {
                _speedFactor = 1f;
            }

            switch (ActiveAxis)
            {
                case Axis.X:
                    transform.Rotate(_speedFactor * Speed, 0, 0);
                    break;
                case Axis.Y:
                    transform.Rotate(0, _speedFactor * Speed, 0);
                    break;
                case Axis.Z:
                    transform.Rotate(0, 0, _speedFactor * Speed);
                    break;
                case Axis.None:
                default:
                    break;
            }
        }
    }
}