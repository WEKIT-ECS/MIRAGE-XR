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

		[Header("Offset Configs")]
		[SerializeField] private Quaternion _leftHandWristRotationOffset = Quaternion.Euler(0, 340, 0);
		[SerializeField] private Quaternion _rightHandWristRotationOffset = Quaternion.Euler(0, 20, 0);
		[SerializeField] private Vector3 _loadingDisplayOffset = new Vector3(0, 0.4f, 0);

		public override void InitializeAvatar(GameObject avatar)
		{
			AvatarReferences avatarRefs = GetComponent<AvatarReferences>();
			avatarRefs.Rig = avatar.GetComponent<RigReferences>();

			BodyController bodyController = avatarRefs.Rig.IK.HipsConstraint.gameObject.AddComponent<BodyController>();
			bodyController.SetReferences(avatarRefs);

			for (int i = 0; i < 2; i++)
			{
				SetupHand(i == 0, avatarRefs);
				SetupFoot(i == 0, avatarRefs);
			}

			avatarRefs.VisibilityController.Initialize();

			RelativePositionPlacement relativePositioning = avatarRefs.LoadingDisplay.GetComponent<RelativePositionPlacement>();
			relativePositioning.Target = avatarRefs.Rig.IK.HeadTarget;
			relativePositioning.Offset = _loadingDisplayOffset;
		}

		private void SetupHand(bool isLeftHand, AvatarReferences avatarRefs)
		{
			GameObject handTarget = avatarRefs.Rig.IK.GetSide(isLeftHand).Hand.Target.gameObject;

			HandController handController = handTarget.AddComponent<HandController>();
			handController.SetReferences(avatarRefs);
			handController.IsLeftHand = isLeftHand;
			handController.WristRotationOffset = isLeftHand ? _leftHandWristRotationOffset : _rightHandWristRotationOffset;
			handController.HandPositionSetExternally = false;
			avatarRefs.GetSide(isLeftHand).HandController = handController;

			HandJointsController handJointsController = handTarget.AddComponent<HandJointsController>();
			handJointsController.SetReferences(avatarRefs);
			handJointsController.HandSide = isLeftHand ? Handedness.Left : Handedness.Right;
			avatarRefs.GetSide(isLeftHand).HandJointsController = handJointsController;
		}

		private void SetupFoot(bool isLeftFoot, AvatarReferences avatarRefs)
		{
			GameObject footTarget = avatarRefs.Rig.IK.GetSide(isLeftFoot).Foot.Target.gameObject;

			FootController footController = footTarget.AddComponent<FootController>();
			footController.SetReferences(avatarRefs);
			footController.IsLeftFoot = isLeftFoot;
			avatarRefs.GetSide(isLeftFoot).FootController = footController;
		}

		public override void CleanupAvatar(GameObject avatar)
		{
			base.CleanupAvatar(avatar);
			AvatarReferences avatarRefs = GetComponent<AvatarReferences>();
			for (int i = 0; i < 2; i++)
			{
				CleanupHand(i == 0, avatarRefs);
			}
		}

		private void CleanupHand(bool isLeft, AvatarReferences avatarRefs)
		{
			DestroyImmediate(avatarRefs.GetSide(isLeft).HandJointsController);
			DestroyImmediate(avatarRefs.GetSide(isLeft).HandController);	
		}
	}
}
