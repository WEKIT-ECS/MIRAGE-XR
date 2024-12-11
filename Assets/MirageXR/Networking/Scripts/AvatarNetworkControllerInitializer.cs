using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class AvatarNetworkControllerInitializer : AvatarInitializer
	{
		[SerializeField] private Transform _speakerInstance;
		[SerializeField] private Vector3 _speakerPositionOffset = new Vector3(0, 0.013f, 0.012f);

		public override int Priority => -10; // after the non-networked initializers

		public override void InitializeAvatar(GameObject avatar)
		{
			NetworkedAvatarReferences avatarRefs = GetComponent<NetworkedAvatarReferences>();

			AddPhotonComponents(avatar, avatarRefs);
			AddNetworkedControllers(avatar, avatarRefs);
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

			_speakerInstance.parent = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			_speakerInstance.localPosition = _speakerPositionOffset;
			_speakerInstance.localRotation = Quaternion.identity;
		}

		private void AddNetworkedControllers(GameObject avatar, NetworkedAvatarReferences avatarRefs)
		{
			NetworkedAvatarVisibilityController networkedAvatarVisibilityController = avatar.AddComponent<NetworkedAvatarVisibilityController>();
			networkedAvatarVisibilityController.SetReferences(avatarRefs);
			avatarRefs.NetworkedVisibilityController = networkedAvatarVisibilityController;
		}
	}
}
