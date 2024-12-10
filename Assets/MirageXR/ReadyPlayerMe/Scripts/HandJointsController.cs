using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandJointsController : MonoBehaviour
	{
		[field: SerializeField]
		public Handedness HandSide { get; set; }

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
		}

		public void ApplyPoseToJoint(XRHandJointID jointId, Pose pose)
		{
			HandIKData handIk = _rigRefs.IK.GetSide(HandSide).Hand;
			if (handIk.HasHandBoneIKTarget(jointId))
			{
				handIk.GetHandBoneIKTarget(jointId).transform.position = pose.position;
				handIk.GetHandBoneIKTarget(jointId).transform.rotation = pose.rotation * Quaternion.Euler(90, 0, 0);
			}
		}
	}
}