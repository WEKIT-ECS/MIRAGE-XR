using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// Collects infos about the MRTK hardware rig and provides it in a way that the MRTK network rig can read it
	/// </summary>
	public class MRTKHardwareRig : MonoBehaviour
	{
		private Transform _headTransform;

		private RigState _rigState = default;
		private IMixedRealityHandJointService _handJointService;

		public RigState RigState
		{
			get
			{
				GetRigData();
				return _rigState;
			}
		}

		private void Awake()
		{
			_headTransform = Camera.main.transform;
		}

		private void Start()
		{
			_handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
		}

		private void GetRigData()
		{
			_rigState.playSpacePose.position = transform.position;
			_rigState.playSpacePose.rotation = transform.rotation;
			_rigState.headPose.position = _headTransform.position;
			_rigState.headPose.rotation = _headTransform.rotation;
			GetHandData(Handedness.Left, ref _rigState.leftHandState);
			GetHandData(Handedness.Right, ref _rigState.rightHandState);
		}

		private void GetHandData(Handedness hand, ref HandRigState handRigState)
		{
			if (!_handJointService.IsHandTracked(hand))
			{
				handRigState.handPresent = false;
				handRigState.handPose.position = Vector3.zero;
				handRigState.handPose.rotation = Quaternion.identity;
				return;
			}
			else
			{
				Transform handTransform = _handJointService.RequestJointTransform(TrackedHandJoint.Wrist, hand);
				handRigState.handPresent = true;
				handRigState.handPose.position = handTransform.position;
				handRigState.handPose.rotation = handTransform.rotation;
			}
		}
	}

	public struct RigState
	{
		public Pose playSpacePose;
		public Pose headPose;
		public HandRigState leftHandState;
		public HandRigState rightHandState;
	}

	public struct HandRigState
	{
		public bool handPresent;
		public Pose handPose;
	}
}
