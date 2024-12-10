using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class BodyController : MonoBehaviour
    {
        [field: SerializeField] public float InterpolationSpeed { get; set; } = 1f;
        [field: SerializeField] public Vector3 HeadPlacementOffset { get; set; }

		private Vector3 _headBodyOffset;
        private RigReferences _rigRefs;

        public void SetRigReferences(RigReferences rigReferences)
        {
            _rigRefs = rigReferences;
        }

        private void Start()
        {
            if (_rigRefs == null)
            {
                _rigRefs = GetComponentInParent<RigReferences>();
            }
            _headBodyOffset = _rigRefs.Bones.Hips.position - _rigRefs.Bones.Head.position;
        }

        private void Update()
        {
            // move the hip into a plausible position based on the head target
            transform.position = _rigRefs.IK.HeadTarget.position + _headBodyOffset + (_rigRefs.IK.HeadTarget.rotation * HeadPlacementOffset);
            float yaw = _rigRefs.IK.HeadTarget.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(transform.rotation.x, yaw, transform.rotation.z),
                InterpolationSpeed * Time.deltaTime);
        }
    }
}
