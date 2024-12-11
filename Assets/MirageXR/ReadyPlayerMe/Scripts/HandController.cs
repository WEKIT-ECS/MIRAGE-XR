using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class HandController : AvatarBaseController
	{

		[Header("Configuration Values")]

		[SerializeField] private Vector3 _handHipOffset = new Vector3(0.3f, 0, 0);
		[SerializeField] private float _handInertia = 0.995f;
		[SerializeField] private float _handDamping = 0.95f;
		[SerializeField] private float _wristRotationSpeed = 1f;
		[SerializeField] private float _elbowWideness = 0.8f;
		[field: SerializeField] public bool IsLeftHand { get; set; }
		[field: SerializeField] public Quaternion WristRotationOffset = Quaternion.identity;

		/// <summary>
		/// If checked, the position of the hand target is left as is; otherwise, the script will find a natural position for the hands
		/// </summary>
		[field: Tooltip("If checked, the position of the hand target is left as is so that other scripts can position the hand; otherwise, the script will find a natural position for the hand target")]
		[field: SerializeField]
		public bool HandPositionSetExternally { get; set; } = true;

		private Vector3 _currentElbowHintPosition;

		private Vector3 _handTargetPosition;
		private Vector3 _handAcceleration;

		private Transform ElbowHint
		{
			get
			{
				return _avatarRefs.Rig.IK.GetSide(IsLeftHand).Hand.ElbowHint;
			}
		}

		private Transform LowerArmBone
		{
			get
			{
				return _avatarRefs.Rig.Bones.GetSide(IsLeftHand).Arm.Lower;
			}
		}

		public HandJointsController JointsController
		{
			get => _avatarRefs.GetSide(IsLeftHand).HandJointsController;
		}

		private void Update()
		{
			_currentElbowHintPosition = Vector3.Lerp(_avatarRefs.Rig.IK.HeadTarget.position, _avatarRefs.Rig.IK.HipsTarget.position, 0.8f);
			_currentElbowHintPosition -= _avatarRefs.Rig.IK.HipsTarget.forward;
			float sideFactor = IsLeftHand ? -1f : 1f;
			_currentElbowHintPosition += sideFactor * _avatarRefs.Rig.IK.HipsTarget.right * _elbowWideness;
			ElbowHint.position = _currentElbowHintPosition;

			if (!HandPositionSetExternally)
			{
				// set the hand to a plausible position relative to the body / hip
				_handTargetPosition = _avatarRefs.Rig.IK.HipsTarget.position +
					_avatarRefs.Rig.IK.HipsTarget.rotation *
					Vector3.Scale(new Vector3(sideFactor, 0, 0), _handHipOffset);
				_handAcceleration = _handDamping * (_handInertia * _handAcceleration + (1f - _handInertia) * (_handTargetPosition - transform.position));
				transform.position += _handAcceleration;
				Quaternion targetRotation = LowerArmBone.rotation * WristRotationOffset;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _wristRotationSpeed);
			}
		}
	}
}
