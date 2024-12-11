using Cysharp.Threading.Tasks.Triggers;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class AvatarNetworkControllerInitializer : AvatarInitializer
	{
		[Header("Speaker")]
		[SerializeField] private Transform _speakerInstance;
		[SerializeField] private Vector3 _speakerPositionOffset = new Vector3(0, 0.013f, 0.012f);

		[Header("Name Placement")]
		[SerializeField] private Vector3 nameLabelOffset = new Vector3(0, 0.93f, -0.01f);

		public override int Priority => -10; // after the non-networked initializers

		public override void InitializeAvatar(GameObject avatar)
		{
			NetworkedAvatarReferences avatarRefs = GetComponent<NetworkedAvatarReferences>();

			AddPhotonComponents(avatar, avatarRefs);

			RelativePositionPlacement relativePositioning = avatarRefs.NameLabel.GetComponent<RelativePositionPlacement>();
			relativePositioning.Target = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			relativePositioning.Offset = nameLabelOffset;
		}

		private void AddPhotonComponents(GameObject avatar, NetworkedAvatarReferences avatarRefs)
		{
			avatar.AddComponent<NetworkTransform>();
			avatarRefs.OfflineReferences.Rig.IK.HeadTarget.gameObject.AddComponent<NetworkTransform>();
			for (int i = 0; i < 2; i++)
			{
				NetworkTransform handNetworkTransform = avatarRefs.OfflineReferences.Rig.IK.GetSide(i).Hand.Target.gameObject.AddComponent<NetworkTransform>();
				handNetworkTransform.DisableSharedModeInterpolation = true;
			}			

			RelativePositionPlacement relativeSpeakerPositioning = _speakerInstance.GetComponent<RelativePositionPlacement>();
			if (relativeSpeakerPositioning == null)
			{
				relativeSpeakerPositioning = _speakerInstance.gameObject.AddComponent<RelativePositionPlacement>();
			}
			relativeSpeakerPositioning.Target = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			relativeSpeakerPositioning.Offset = _speakerPositionOffset;
		}
	}
}
