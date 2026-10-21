using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Keeps a GameObject within the view cone of the main camera (lazy follow / tagalong).
    /// Native replacement for MRTK RadialView.
    /// </summary>
    public class RadialView : MonoBehaviour
    {
        [SerializeField] private float _minDistance = 1.0f;
        [SerializeField] private float _maxDistance = 2.0f;
        [SerializeField] private float _minViewDegree = 0f;
        [SerializeField] private float _maxViewDegree = 30f;
        [SerializeField] private float _lerpTime = 4f;

        private Transform _cameraTransform;

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 camPos = _cameraTransform.position;
            Vector3 camFwd = _cameraTransform.forward;

            Vector3 toObj = transform.position - camPos;
            float dist = toObj.magnitude;
            if (dist < 0.001f)
            {
                dist = _minDistance;
                toObj = camFwd * dist;
            }

            Vector3 objDir = toObj / dist;
            float angle = Vector3.Angle(camFwd, objDir);

            if (angle > _maxViewDegree || dist < _minDistance || dist > _maxDistance)
            {
                Vector3 targetPos = camPos + camFwd * Mathf.Clamp(dist, _minDistance, _maxDistance);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * _lerpTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - camPos), Time.deltaTime * _lerpTime);
            }
        }
    }
}
