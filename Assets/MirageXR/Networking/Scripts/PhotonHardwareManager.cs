using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	/// <summary>
	/// Collects infos about the MRTK hardware rig and provides it in a way that the MRTK network rig can read it
	/// </summary>
	public class PhotonHardwareManager : MonoBehaviour
	{
		private Transform _headTransform;
		private HandTrackingManager _handTrackingManager;

		private RigData _rigData = default;

		public RigData RigData
		{
			get
			{
				GetRigData();
				return _rigData;
			}
		}

		private void Awake()
		{
			_handTrackingManager = GetComponent<HandTrackingManager>();
		}

		private void GetRigData()
		{
			var headTransform = RootObject.Instance.BaseCamera.transform;
			_rigData.headPose.position = headTransform.position;
			_rigData.headPose.rotation = headTransform.rotation;
			_rigData.playSpacePose.position = headTransform.parent.position;
			_rigData.playSpacePose.rotation = headTransform.parent.rotation;
			_rigData.leftHand = _handTrackingManager.LeftHandData;
			_rigData.rightHand = _handTrackingManager.RightHandData;
		}
	}

	public struct RigData
	{
		public Pose playSpacePose;
		public Pose headPose;
		public HandData leftHand;
		public HandData rightHand;

		public HandData GetHand(bool left)
		{
			if (left)
			{
				return leftHand;
			}
			else
			{
				return rightHand;
			}
		}

		public HandData GetHand(int side)
		{
            if (side != 0 && side != 1)
            {
				Debug.LogError($"side needs to be 0 for left or 1 for right but was {side}. Cannot choose side.");
				return null;
            }
            return GetHand(side == 0);
		}
	}
}
