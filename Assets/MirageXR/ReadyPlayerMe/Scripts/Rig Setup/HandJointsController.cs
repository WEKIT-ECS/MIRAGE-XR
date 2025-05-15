using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandJointsController : AvatarBaseController
	{
		[field: SerializeField]
		public Handedness HandSide { get; set; }

		private Dictionary<XRHandJointID, float> originalLengths = new Dictionary<XRHandJointID, float>();

		private void Start()
		{
			for (int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
			{
				XRHandJointID jointId = XRHandJointIDUtility.FromIndex(i);

				if (jointId == XRHandJointID.Wrist)
				{
					// the wrist is the root bone, so we cannot find a parent for it
					continue;
				}

				Transform endBone = AvatarRefs.Rig.Bones.GetSide(HandSide).Arm.Hand.GetBoneByJointID(jointId);
				if (endBone == null)
				{
					continue;
				}
				Transform startBone = endBone.parent;
				originalLengths.Add(jointId, Vector3.Distance(startBone.position, endBone.position));
			}
		}

		public void ApplyPoseToJoint(XRHandJointID jointId, Pose pose)
		{
			HandIKData handIk = AvatarRefs.Rig.IK.GetSide(HandSide).Hand;
			if (handIk.HasHandBoneIKTarget(jointId))
			{
				handIk.GetHandBoneIKTarget(jointId).transform.position = pose.position;
				handIk.GetHandBoneIKTarget(jointId).transform.rotation = pose.rotation * Quaternion.Euler(90, 0, 0);

				if (jointId != XRHandJointID.Wrist)
				{

				}
			}
		}
	}
}