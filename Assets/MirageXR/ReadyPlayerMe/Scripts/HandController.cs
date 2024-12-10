using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class HandController : MonoBehaviour
	{

		[Header("Configuration Values")]

		[SerializeField] private Vector3 _handHipOffset = new Vector3(0.3f, 0, 0);
		[SerializeField] private float _handInertia = 0.995f;
		[SerializeField] private float _handDamping = 0.95f;
		[SerializeField] private float _wristRotationSpeed = 1f;
		[SerializeField] private Quaternion _wristRotationOffset = Quaternion.identity;
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

		private RigReferences _rigRefs;
		private HandJointsController _jointsController;

		public HandJointsController JointsController
		{
			get
			{
				if (_jointsController == null)
				{
					_jointsController = GetComponent<HandJointsController>();
				}
				return _jointsController;
			}
		}

		private Transform ElbowHint
		{
			get
			{
				if (IsLeftHand)
				{
					return _rigRefs.IK.LeftElbowHint;
				}
				else
				{
					return _rigRefs.IK.RightElbowHint;
				}
			}
		}

		private Transform LowerArmBone
		{
			get
			{
				if (IsLeftHand)
				{
					return _rigRefs.Bones.LeftLowerArm;
				}
				else
				{
					return _rigRefs.Bones.RightLowerArm;
				}
			}
		}

		public void SetRigReferences(RigReferences rigReferences)
		{
			_rigRefs = rigReferences;
		}

		private void Start()
		{
			if (_rigRefs == null)
			{
				_rigRefs = GetComponentInParent<RigReferences>();
			}
		}

		private void Update()
		{
			_currentElbowHintPosition = Vector3.Lerp(_rigRefs.IK.HeadTarget.position, _rigRefs.IK.HipsTarget.position, 0.8f);
			_currentElbowHintPosition -= _rigRefs.IK.HipsTarget.forward;
			float sideFactor = IsLeftHand ? -1f : 1f;
			_currentElbowHintPosition += sideFactor * _rigRefs.IK.HipsTarget.right * _elbowWideness;
			ElbowHint.position = _currentElbowHintPosition;

			if (!HandPositionSetExternally)
			{
				// set the hand to a plausible position relative to the body / hip
				_handTargetPosition = _rigRefs.IK.HipsTarget.position +
					_rigRefs.IK.HipsTarget.rotation *
					Vector3.Scale(new Vector3(sideFactor, 0, 0), _handHipOffset);
				_handAcceleration = _handDamping * (_handInertia * _handAcceleration + (1f - _handInertia) * (_handTargetPosition - transform.position));
				transform.position += _handAcceleration;
				Quaternion targetRotation = LowerArmBone.rotation * WristRotationOffset;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _wristRotationSpeed);
			}
		}
	}
}
