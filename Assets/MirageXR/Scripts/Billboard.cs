using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Standalone billboard component to orient objects towards the camera.
    /// Replaces Microsoft.MixedReality.Toolkit.UI.Billboard.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        public enum PivotAxis
        {
            // Rotate about all axes
            Free,
            // Rotate around the Y axis only
            Y,
            // Rotate around the X axis only
            X,
            // Rotate around the Z axis only
            Z
        }

        [SerializeField]
        [Tooltip("The axis about which the object will rotate.")]
        private PivotAxis _pivotAxis = PivotAxis.Y;

        [SerializeField]
        [Tooltip("Whether to face away from the camera instead of towards it.")]
        private bool _flipForward = false;

        [SerializeField]
        [Tooltip("Smooth transition speed. Set to 0 or negative for instant rotation.")]
        private float _smoothSpeed = 0f;

        private Camera _targetCamera;

        public PivotAxis Axis
        {
            get => _pivotAxis;
            set => _pivotAxis = value;
        }

        public bool FlipForward
        {
            get => _flipForward;
            set => _flipForward = value;
        }

        public Camera TargetCamera
        {
            get => _targetCamera;
            set => _targetCamera = value;
        }

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            UpdateOrientation(true);
        }

        private void LateUpdate()
        {
            UpdateOrientation(false);
        }

        private void UpdateOrientation(bool immediate)
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
                if (_targetCamera == null) return;
            }

            Vector3 directionToCamera = _targetCamera.transform.position - transform.position;
            if (_flipForward)
            {
                directionToCamera = -directionToCamera;
            }

            if (directionToCamera.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = transform.rotation;

            switch (_pivotAxis)
            {
                case PivotAxis.Y:
                    directionToCamera.y = 0;
                    if (directionToCamera.sqrMagnitude > 0.0001f)
                    {
                        targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
                    }
                    break;

                case PivotAxis.X:
                    directionToCamera.x = 0;
                    if (directionToCamera.sqrMagnitude > 0.0001f)
                    {
                        targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
                    }
                    break;

                case PivotAxis.Z:
                    directionToCamera.z = 0;
                    if (directionToCamera.sqrMagnitude > 0.0001f)
                    {
                        targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
                    }
                    break;

                case PivotAxis.Free:
                default:
                    targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, _targetCamera.transform.up);
                    break;
            }

            if (immediate || _smoothSpeed <= 0f)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _smoothSpeed);
            }
        }
    }
}
