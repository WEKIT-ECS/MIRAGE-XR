using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// Controls the movement and positioning of a hand in an avatar.
	/// </summary>
	public class HandController : AvatarBaseController
	{

		[Header("Configuration Values")]

		[Tooltip("The offset from the hip to the hand.")]
		[SerializeField] private Vector3 _handHipOffset = new Vector3(0.3f, 0, 0);
		[Tooltip("Inertia factor affecting the hand's movement.")]
		[SerializeField] private float _handInertia = 0.995f;
		[Tooltip("Damping factor reducing the hand's acceleration over time.")]
		[SerializeField] private float _handDamping = 0.95f;
		[Tooltip("Speed at which the wrist rotates to match the lower arm bone.")]
		[SerializeField] private float _wristRotationSpeed = 1f;
		[Tooltip("Factor determining how wide the elbow should be positioned relative to the hips.")]
		[SerializeField] private float _elbowWideness = 0.8f;
		/// <summary>
		/// Indicates whether this controller is for the left hand.
		/// </summary>
		[field: Tooltip("Indicates whether this controller is for the left hand.")]
		[field: SerializeField] public bool IsLeftHand { get; set; }
		/// <summary>
		/// Offset applied to the wrist rotation.
		/// </summary>
		[field: Tooltip("Offset applied to the wrist rotation.")]
		[field: SerializeField] public Quaternion WristRotationOffset = Quaternion.identity;

		/// <summary>
		/// If checked, the position of the hand target is left as is; otherwise, the script will find a natural position for the hands
		/// </summary>
		[field: Tooltip("If checked, the position of the hand target is left as is so that other scripts can position the hand; otherwise, the script will find a natural position for the hand target")]
		[field: SerializeField]
		public bool HandPositionSetExternally { get; set; } = true;

		// Current position of the the elbow hint.
		private Vector3 _currentElbowHintPosition;

		// Target position for the hand.
		private Vector3 _handTargetPosition;

		// Acceleration vector for the hand movement.
		private Vector3 _handAcceleration;

		/// <summary>
		/// Gets the Transform representing the elbow hint for this hand.
		/// </summary>
		private Transform ElbowHint
		{
			get
			{
				return AvatarRefs.Rig.IK.GetSide(IsLeftHand).Hand.ElbowHint;
			}
		}

		/// <summary>
		/// Gets the Transform representing the lower arm bone for this side of the body.
		/// </summary>
		private Transform LowerArmBone
		{
			get
			{
				return AvatarRefs.Rig.Bones.GetSide(IsLeftHand).Arm.Lower;
			}
		}

		/// <summary>
		/// Gets the controller responsible for managing the joints of this hand.
		/// </summary>
		public HandJointsController JointsController
		{
			get => AvatarRefs.GetSide(IsLeftHand).HandJointsController;
		}

		// Updates the position and rotation of the hand based on configuration values and target positions.
		private void Update()
		{
			// Calculate the new elbow hint position.
			_currentElbowHintPosition = Vector3.Lerp(AvatarRefs.Rig.IK.HeadTarget.position, AvatarRefs.Rig.IK.HipsTarget.position, 0.8f);
			_currentElbowHintPosition -= AvatarRefs.Rig.IK.HipsTarget.forward;
			float sideFactor = IsLeftHand ? -1f : 1f;
			_currentElbowHintPosition += sideFactor * AvatarRefs.Rig.IK.HipsTarget.right * _elbowWideness;
			ElbowHint.position = _currentElbowHintPosition;

			if (!HandPositionSetExternally)
			{
				// set the hand to a plausible position relative to the body / hip
				_handTargetPosition = AvatarRefs.Rig.IK.HipsTarget.position +
					AvatarRefs.Rig.IK.HipsTarget.rotation *
					Vector3.Scale(new Vector3(sideFactor, 0, 0), _handHipOffset);
				_handAcceleration = _handDamping * (_handInertia * _handAcceleration + (1f - _handInertia) * (_handTargetPosition - transform.position));
				transform.position += _handAcceleration;
				Quaternion targetRotation = LowerArmBone.rotation * WristRotationOffset;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _wristRotationSpeed);
			}
		}
	}
}
