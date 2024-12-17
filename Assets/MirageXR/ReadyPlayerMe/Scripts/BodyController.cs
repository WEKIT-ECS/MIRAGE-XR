using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class BodyController : AvatarBaseController
    {
        [field: SerializeField] public float InterpolationSpeed { get; set; } = 1f;
        [field: SerializeField] public Vector3 HeadPlacementOffset { get; set; }

		private Vector3 _headBodyOffset;

        private void Start()
        {
            _headBodyOffset = AvatarRefs.Rig.Bones.Hips.position - AvatarRefs.Rig.Bones.Head.position;
        }

        private void Update()
        {
            // move the hip into a plausible position based on the head target
            transform.position = AvatarRefs.Rig.IK.HeadTarget.position + _headBodyOffset + (AvatarRefs.Rig.IK.HeadTarget.rotation * HeadPlacementOffset);
            float yaw = AvatarRefs.Rig.IK.HeadTarget.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(transform.rotation.x, yaw, transform.rotation.z),
                InterpolationSpeed * Time.deltaTime);
        }
    }
}
