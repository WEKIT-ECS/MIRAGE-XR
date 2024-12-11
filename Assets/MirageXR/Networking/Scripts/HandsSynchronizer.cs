#if FUSION2
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandsSynchronizer : BaseNetworkedAvatarController
	{
		/// <summary>
		/// number of currently synchronized joints
		/// </summary>
		public const int JOINTS_COUNT = 25;

		[Networked]
		public int LeftTracked { get; set; }

		[Networked, Capacity(JOINTS_COUNT)]
		public NetworkDictionary<XRHandJointID, Quaternion> NetworkedLeftJoints => default;

		[Networked]
		public int RightTracked { get; set; }

		[Networked, Capacity(JOINTS_COUNT)]
		public NetworkDictionary<XRHandJointID, Quaternion> NetworkedRightJoints => default;

		private HandData _extractedData = new HandData();

		private NetworkDictionary<XRHandJointID, Quaternion> GetNetworkedJoints(bool leftSide)
		{
			if (leftSide)
			{
				return NetworkedLeftJoints;
			}
			else
			{
				return NetworkedRightJoints;
			}
		}

		private bool IsSideTracked(bool leftSide)
		{
			if (leftSide)
			{
				return LeftTracked > 0;
			}
			else
			{
				return RightTracked > 0;
			}
		}

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
			for (int i = 0; i < 2; i++)
			{
				bool left = i == 0;
				ApplySingleHandDataToRig(left, rigData.GetHand(left));
			}
		}

		private void ApplySingleHandDataToRig(bool isLeftSide, HandData handData)
		{
			_avatarRefs.OfflineReferences.GetSide(isLeftSide).HandController.HandPositionSetExternally = handData.IsTracked;
			if (handData.IsTracked)
			{
				foreach (KeyValuePair<XRHandJointID, Pose> jointPose in handData.JointPoses)
				{
					// do not set the position of the wrist on the remote side
					// this would interfere with the NetworkTransform
					if (!IsLocalController && jointPose.Key == XRHandJointID.Wrist)
					{
						continue;
					}
					_avatarRefs.OfflineReferences.GetSide(isLeftSide).HandJointsController.ApplyPoseToJoint(jointPose.Key, jointPose.Value);
				}
			}
		}

		public void ApplyRemoteHandsDataToRig()
		{
			for (int i = 0; i < 2; i++)
			{
				ApplyRemoteSingleHandDataToRig(i == 0);
			}
		}

		private void ApplyRemoteSingleHandDataToRig(bool isLeftSide)
		{
			HandController handController = _avatarRefs.OfflineReferences.GetSide(isLeftSide).HandController;

			_extractedData.HandSide = handController.JointsController.HandSide;
			_extractedData.IsTracked = IsSideTracked(isLeftSide);
			_extractedData.JointPoses.Clear();

			NetworkDictionary<XRHandJointID, Quaternion> jointData = GetNetworkedJoints(isLeftSide);

			foreach (KeyValuePair<XRHandJointID, Quaternion> networkedRotation in jointData)
			{
				_extractedData.JointPoses.Add(networkedRotation.Key, new Pose(Vector3.zero, networkedRotation.Value));
			}

			ApplySingleHandDataToRig(isLeftSide, _extractedData);
		}
	}
}
#endif