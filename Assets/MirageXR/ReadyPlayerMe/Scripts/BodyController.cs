using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class BodyController : MonoBehaviour
    {
        [SerializeField] private float interpolationSpeed = 1f;

        [SerializeField] private Transform _headBone;
        [SerializeField] private Transform _hipBone;
        [SerializeField] private Transform _headTarget;
        [SerializeField] private Vector3 _headPlacementOffset;

        private Vector3 _headBodyOffset;

        private void Start()
        {
            _headBodyOffset = _hipBone.position - _headBone.position;
        }

        private void Update()
        {
            // move the hip into a plausible position based on the head target
            transform.position = _headTarget.position + _headBodyOffset + (_headTarget.rotation * _headPlacementOffset);
            float yaw = _headTarget.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(transform.rotation.x, yaw, transform.rotation.z),
                interpolationSpeed * Time.deltaTime);
        }
    }
}
