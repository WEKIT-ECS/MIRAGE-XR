using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class HandController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Transform _headTarget;
		[SerializeField] private Transform _bodyTarget;
		[SerializeField] private Transform _elbowHint;
		[SerializeField] private Transform _lowerArmBone;
		[SerializeField] private bool _isLeftHand;

		[Header("Configuration Values")]
		[SerializeField] private Vector3 _handHipOffset = new Vector3(0.3f, 0, 0);
		[SerializeField] private float _handInertia = 0.995f;
		[SerializeField] private float _handDamping = 0.95f;
		[SerializeField] private float _wristRotationSpeed = 1f;
		[SerializeField] private Quaternion _wristRotationOffset = Quaternion.identity;
		[SerializeField] private float _elbowWideness = 0.8f;

		private Vector3 _currentElbowHintPosition;


		private Vector3 _handTargetPosition;
		private Vector3 _handAcceleration;

		/// <summary>
		/// If checked, the position of the hand target is left as is; otherwise, the script will find a natural position for the hands
		/// </summary>
		[field: Tooltip("If checked, the position of the hand target is left as is so that other scripts can position the hand; otherwise, the script will find a natural position for the hand target")]
		[field: SerializeField]
		public bool HandPositionSetExternally { get; set; } = true;

		private void Update()
		{
			_currentElbowHintPosition = Vector3.Lerp(_headTarget.position, _bodyTarget.position, 0.8f);
			_currentElbowHintPosition -= _bodyTarget.forward;
			float sideFactor = _isLeftHand ? -1f : 1f;
			_currentElbowHintPosition += sideFactor * _bodyTarget.right * _elbowWideness;
			_elbowHint.position = _currentElbowHintPosition;

			if (!HandPositionSetExternally)
			{
				// set the hand to a plausible position relative to the body / hip
				_handTargetPosition = _bodyTarget.position +
					_bodyTarget.rotation *
					Vector3.Scale(new Vector3(sideFactor, 0, 0), _handHipOffset);
				_handAcceleration = _handDamping * (_handInertia * _handAcceleration + (1f - _handInertia) * (_handTargetPosition - transform.position));
				transform.position += _handAcceleration;
				Quaternion targetRotation = _lowerArmBone.rotation * _wristRotationOffset;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _wristRotationSpeed);
			}
		}
	}
}
