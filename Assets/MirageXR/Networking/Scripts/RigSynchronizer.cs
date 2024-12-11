#if FUSION2
using Fusion;
using UnityEngine;

namespace MirageXR
{
	public class RigSynchronizer : BaseNetworkedAvatarController
	{
		[SerializeField] private Vector3 _headOffset;

		private PhotonHardwareManager _hardwareRig;

		public override void Spawned()
		{
			base.Spawned();
			if (IsLocalController)
			{
				_hardwareRig = FindObjectOfType<PhotonHardwareManager>();
				if (_hardwareRig == null)
				{
					Debug.LogError("Missing Photon Hardware Manager", this);
				}
			}
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			// Update the rig at each network tick for local player. The NetworkTransform will forward this to other players
			if (IsLocalController && _hardwareRig != null)
			{
				ApplyLocalStateToRigParts(_hardwareRig.RigData);
				AvatarRefs.HandsSynchronizer.StoreHandsData(_hardwareRig.RigData);
			}
		}

		protected virtual void ApplyLocalStateToRigParts(RigData rigData)
		{
			if (!AvatarRefs.OfflineReferences.AvatarInstantiated || AvatarRefs.OfflineReferences.Rig == null)
			{
				return;
			}
			transform.position = rigData.playSpacePose.position;
			transform.rotation = rigData.playSpacePose.rotation;
			Transform headTarget = AvatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			headTarget.transform.position = rigData.headPose.position + rigData.headPose.rotation * _headOffset;
			headTarget.transform.rotation = rigData.headPose.rotation;
			AvatarRefs.HandsSynchronizer.ApplyHandsDataToRig(_hardwareRig.RigData);
		}

		public override void Render()
		{
			base.Render();
			if (IsLocalController)
			{
				if (_hardwareRig != null)
				{
					// Extrapolate for local user :
					// we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hardware positions
					ApplyLocalStateToRigParts(_hardwareRig.RigData);
				}
			}
			else
			{
				// remote side:
				// head and hand positions are automatically synchronized by NetworkTransform components
				// by setting the GameObject positions.

				// now, we need to restore the hand joint data
				AvatarRefs.HandsSynchronizer.ApplyRemoteHandsDataToRig();
			}
		}
	}
}
#endif