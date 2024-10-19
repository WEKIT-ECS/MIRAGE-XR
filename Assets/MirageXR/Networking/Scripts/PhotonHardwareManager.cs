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
			_headTransform = Camera.main.transform;
			_handTrackingManager = GetComponent<HandTrackingManager>();
		}

		private void GetRigData()
		{
			_rigData.headPose.position = _headTransform.position;
			_rigData.headPose.rotation = _headTransform.rotation;
			_rigData.playSpacePose.position = _headTransform.parent.position;
			_rigData.playSpacePose.rotation = _headTransform.parent.rotation;
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
	}
}
