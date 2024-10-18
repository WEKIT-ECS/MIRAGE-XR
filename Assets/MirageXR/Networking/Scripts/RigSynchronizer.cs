#if FUSION2
using Fusion;
using UnityEngine;

namespace MirageXR
{
	public class RigSynchronizer : NetworkBehaviour
	{
		[SerializeField] private Transform _head;
		[SerializeField] private Vector3 _headOffset;

		private PhotonHardwareManager _hardwareRig;

		private HandsSynchronizer _handSynchronizer;

		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalNetworkRig => Object && Object.HasStateAuthority;

		private void Awake()
		{
			_handSynchronizer = GetComponent<HandsSynchronizer>();
		}

		public override void Spawned()
		{
			base.Spawned();
			if (IsLocalNetworkRig)
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
			if (IsLocalNetworkRig && _hardwareRig != null)
			{
				ApplyLocalStateToRigParts(_hardwareRig.RigData);
				_handSynchronizer.StoreHandsData(_hardwareRig.RigData);
			}
		}

		protected virtual void ApplyLocalStateToRigParts(RigData rigData)
		{
			transform.position = rigData.playSpacePose.position;
			transform.rotation = rigData.playSpacePose.rotation;
			_head.transform.position = rigData.headPose.position + rigData.headPose.rotation * _headOffset;
			_head.transform.rotation = rigData.headPose.rotation;
			_handSynchronizer.ApplyHandsDataToRig(_hardwareRig.RigData);
		}

		public override void Render()
		{
			base.Render();
			if (IsLocalNetworkRig)
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
				_handSynchronizer.ApplyRemoteHandsDataToRig();
			}
		}
	}
}
#endif