using Fusion;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class MRTKNetworkRig : NetworkBehaviour
	{
		[SerializeField] private Transform _head;
		[SerializeField] private Transform _leftHand;
		[SerializeField] private Transform _rightHand;
		[SerializeField] private Pose _leftHandOffset;
		[SerializeField] private Pose _rightHandOffset;

		private MRTKHardwareRig _hardwareRig;
		private Pose _leftHandDefaultPoseOffset;
		private Pose _rightHandDefaultPoseOffset;

		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalNetworkRig => Object && Object.HasStateAuthority;

		private void Awake()
		{
			_leftHandDefaultPoseOffset = new Pose(
				_leftHand.position - _head.position,
				_leftHand.rotation);
			_rightHandDefaultPoseOffset = new Pose(
				_rightHand.position - _head.position,
				_rightHand.localRotation);
		}

		public override void Spawned()
		{
			base.Spawned();
			if (IsLocalNetworkRig)
			{
				_hardwareRig = FindObjectOfType<MRTKHardwareRig>();
				if (_hardwareRig == null)
				{
					Debug.LogError("Missing MRTK Hardware Rig", this);
				}
			}
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			// Update the rig at each network tick for local player. The NetworkTransform will forward this to other players
			if (IsLocalNetworkRig && _hardwareRig)
			{
				RigState rigState = _hardwareRig.RigState;
				ApplyLocalStateToRigParts(rigState);
			}
		}

		protected virtual void ApplyLocalStateToRigParts(RigState rigState)
		{
			//transform.position = rigState.playSpacePose.position;
			//transform.rotation = rigState.playSpacePose.rotation;
			if (rigState.leftHandState.handPresent)
			{
				_leftHand.transform.position = rigState.leftHandState.handPose.position + _leftHandOffset.position;
				_leftHand.transform.rotation = rigState.leftHandState.handPose.rotation * _leftHandOffset.rotation;
			}
			else
			{
				_leftHand.transform.localPosition = _head.position + _leftHandDefaultPoseOffset.position;
				_leftHand.transform.localRotation = _leftHandDefaultPoseOffset.rotation;
			}
			if (rigState.rightHandState.handPresent)
			{

				_rightHand.transform.position = rigState.rightHandState.handPose.position + _rightHandOffset.position;
				_rightHand.transform.rotation = rigState.rightHandState.handPose.rotation * _rightHandOffset.rotation;
			}
			else
			{
				_rightHand.transform.localPosition = _head.position + _rightHandDefaultPoseOffset.position;
				_rightHand.transform.localRotation = _rightHandDefaultPoseOffset.rotation;
			}
			_head.transform.position = rigState.headPose.position;
			_head.transform.rotation = rigState.headPose.rotation;
		}

		public override void Render()
		{
			base.Render();
			if (IsLocalNetworkRig)
			{
				// Extrapolate for local user :
				// we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hardware positions

				RigState rigState = _hardwareRig.RigState;

				ApplyLocalStateToRigParts(rigState);
			}
		}
	}
}
