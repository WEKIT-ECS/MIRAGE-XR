using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class AvatarControllersInitializer : AvatarInitializer
	{
		public override int Priority => -1; // needs to run after the IK initializer

		[Header("Hand Config")]
		[SerializeField] private Quaternion _leftHandWristRotationOffset = Quaternion.Euler(0,340,0);
		[SerializeField] private Quaternion _rightHandWristRotationOffset = Quaternion.Euler(0, 20, 0);

		public override void InitializeAvatar(GameObject avatar)
		{
			RigReferences rigRefs = avatar.GetComponent<RigReferences>();

			BodyController bodyController = rigRefs.IK.HipsConstraint.gameObject.AddComponent<BodyController>();
			bodyController.SetRigReferences(rigRefs);

			SetupHand(true, rigRefs);
			SetupHand(false, rigRefs);
		}

		private void SetupHand(bool isLeftHand, RigReferences rigRefs)
		{
			GameObject handTarget = rigRefs.IK.GetSide(isLeftHand).Hand.Target.gameObject;

			HandController handController = handTarget.AddComponent<HandController>();
			handController.SetRigReferences(rigRefs);
			handController.IsLeftHand = isLeftHand;
			handController.WristRotationOffset = isLeftHand ? _leftHandWristRotationOffset : _rightHandWristRotationOffset;

			HandJointsController handJointsController = handTarget.AddComponent<HandJointsController>();
			handJointsController.SetRigReferences(rigRefs);
			handJointsController.HandSide = isLeftHand ? Handedness.Left : Handedness.Right;
		}
	}
}
