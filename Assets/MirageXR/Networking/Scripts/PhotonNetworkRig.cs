#if FUSION2
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class PhotonNetworkRig : NetworkBehaviour
	{
		[SerializeField] private Transform _head;
		[SerializeField] private HandController _leftHandController;
		[SerializeField] private HandController _rightHandController;
		[SerializeField] private Vector3 _headOffset;

		private PhotonHardwareManager _hardwareRig;

		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalNetworkRig => Object && Object.HasStateAuthority;

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
			}
		}

		protected virtual void ApplyLocalStateToRigParts(RigData rigData)
		{
			transform.position = rigData.playSpacePose.position;
			transform.rotation = rigData.playSpacePose.rotation;
			_head.transform.position = rigData.headPose.position + rigData.headPose.rotation * _headOffset;
			_head.transform.rotation = rigData.headPose.rotation;

			ApplyHandData(_leftHandController, rigData.leftHand);
			ApplyHandData(_rightHandController, rigData.rightHand);
		}

		private void ApplyHandData(HandController handController, HandData handData)
		{
			handController.HandPositionSetExternally = handData.IsTracked;
			if (handData.IsTracked)
			{
				foreach (KeyValuePair<XRHandJointID, Pose> jointPose in handData.JointPoses)
				{
					handController.JointsController.ApplyPoseToJoint(jointPose.Key, jointPose.Value);
				}
			}
		}

		public override void Render()
		{
			base.Render();
			if (IsLocalNetworkRig)
			{
				// Extrapolate for local user :
				// we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hardware positions
				ApplyLocalStateToRigParts(_hardwareRig.RigData);
			}
		}
	}
}
#endif