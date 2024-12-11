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

        protected override void Start()
        {
            base.Start();
            _headBodyOffset = _avatarRefs.Rig.Bones.Hips.position - _avatarRefs.Rig.Bones.Head.position;
        }

        private void Update()
        {
            // move the hip into a plausible position based on the head target
            transform.position = _avatarRefs.Rig.IK.HeadTarget.position + _headBodyOffset + (_avatarRefs.Rig.IK.HeadTarget.rotation * HeadPlacementOffset);
            float yaw = _avatarRefs.Rig.IK.HeadTarget.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(transform.rotation.x, yaw, transform.rotation.z),
                InterpolationSpeed * Time.deltaTime);
        }
    }
}
