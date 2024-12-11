using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandJointsController : AvatarBaseController
	{
		[field: SerializeField]
		public Handedness HandSide { get; set; }

		public void ApplyPoseToJoint(XRHandJointID jointId, Pose pose)
		{
			HandIKData handIk = _avatarRefs.Rig.IK.GetSide(HandSide).Hand;
			if (handIk.HasHandBoneIKTarget(jointId))
			{
				handIk.GetHandBoneIKTarget(jointId).transform.position = pose.position;
				handIk.GetHandBoneIKTarget(jointId).transform.rotation = pose.rotation * Quaternion.Euler(90, 0, 0);
			}
		}
	}
}