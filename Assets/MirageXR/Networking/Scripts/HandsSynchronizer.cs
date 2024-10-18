#if FUSION2
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandsSynchronizer : NetworkBehaviour
	{
		/// <summary>
		/// number of currently synchronized joints
		/// </summary>
		public const int JOINTS_COUNT = 25;

		[SerializeField] private HandController _leftHandController;
		[SerializeField] private HandController _rightHandController;

		[Networked]
		public int LeftTracked { get; set; }

		[Networked, Capacity(JOINTS_COUNT)]
		public NetworkDictionary<XRHandJointID, Quaternion> NetworkedLeftJoints => default;

		[Networked]
		public int RightTracked { get; set; }

		[Networked, Capacity(JOINTS_COUNT)]
		public NetworkDictionary<XRHandJointID, Quaternion> NetworkedRightJoints => default;

		private bool IsLocalNetworkRig => Object && Object.HasStateAuthority;

		private HandData _extractedData = new HandData();

		public void StoreHandsData(RigData rigData)
		{
			StoreSingleHandData(rigData.leftHand, NetworkedLeftJoints);
			LeftTracked = rigData.leftHand.IsTracked ? 1 : 0;
			StoreSingleHandData(rigData.rightHand, NetworkedRightJoints);
			RightTracked = rigData.rightHand.IsTracked ? 1 : 0;
		}

		private void StoreSingleHandData(HandData handData, NetworkDictionary<XRHandJointID, Quaternion> networkDictionary)
		{
			networkDictionary.Clear();
			foreach (KeyValuePair<XRHandJointID, Pose> jointPose in handData.JointPoses)
			{
				networkDictionary.Add(jointPose.Key, jointPose.Value.rotation);
			}
		}

		public void ApplyHandsDataToRig(RigData rigData)
		{
			ApplySingleHandDataToRig(_leftHandController, rigData.leftHand);
			ApplySingleHandDataToRig(_rightHandController, rigData.rightHand);
		}

		private void ApplySingleHandDataToRig(HandController handController, HandData handData)
		{
			handController.HandPositionSetExternally = handData.IsTracked;
			if (handData.IsTracked)
			{
				foreach (KeyValuePair<XRHandJointID, Pose> jointPose in handData.JointPoses)
				{
					// do not set the position of the wrist on the remote side
					// this would interfere with the NetworkTransform
					if (!IsLocalNetworkRig && jointPose.Key == XRHandJointID.Wrist)
					{
						continue;
					}
					handController.JointsController.ApplyPoseToJoint(jointPose.Key, jointPose.Value);
				}
			}
		}

		public void ApplyRemoteHandsDataToRig()
		{
			ApplyRemoteSingleHandDataToRig(_leftHandController, LeftTracked > 0, NetworkedLeftJoints);
			ApplyRemoteSingleHandDataToRig(_rightHandController, RightTracked > 0, NetworkedRightJoints);
		}

		private void ApplyRemoteSingleHandDataToRig(HandController handController, bool tracked, NetworkDictionary<XRHandJointID, Quaternion> jointData)
		{
			_extractedData.HandSide = handController.JointsController.HandSide;
			_extractedData.IsTracked = tracked;
			_extractedData.JointPoses.Clear();

			foreach (KeyValuePair<XRHandJointID, Quaternion> networkedRotation in jointData)
			{
				_extractedData.JointPoses.Add(networkedRotation.Key, new Pose(Vector3.zero, networkedRotation.Value));
			}

			ApplySingleHandDataToRig(handController, _extractedData);
		}
	}
}
#endif