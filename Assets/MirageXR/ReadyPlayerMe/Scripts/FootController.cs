using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class FootController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Transform _headTarget;
		[SerializeField] private Transform _kneeHint;
		[SerializeField] private Transform _bodyTarget;
		[SerializeField] private FootController _otherFoot;
		[Header("Foot Placement")]
		[SerializeField] private bool _isLeftFoot;
		[SerializeField] private float _footHeightOffset = 0.04f;
		[SerializeField] private float _footSpacing = 0.18f;
		[Header("Stepping")]
		[SerializeField] private Vector3 _footOffset = Vector3.zero;
		[SerializeField] private float _stepThreshold = 0.2f;
		[SerializeField] private float _stepDistance = 0.2f;
		[SerializeField] private float _stepHeight = 0.2f;
		[SerializeField] private float _stepSpeed = 3.5f;

		private static FloorManagerWithFallback _floorManager => RootObject.Instance.floorManagerWithRaycastFallback;

		// variables for intermediate calcuations of the foot target position and the position of the knee hint
		// the value of these variables are assigned to the transforms in Update
		private Vector3 _currentFootTargetPosition, _currentKneeHintPosition;
		// step control points: the beginning and ending of a step
		private Vector3 _previousFootTargetPosition, _newFootTargetPosition;
		// keeps track of the current step progress; 0: step not started; 1: step completed
		private float _stepLerpProgress = 0f;

		private GameObject _debugCube;

		private Vector3 SidewaysVector
		{
			get
			{
				float direction = _isLeftFoot ? -1f : 1f;
				return _bodyTarget.right * direction;
			}
		}

		public bool CurrentlyStepping
		{
			get => _stepLerpProgress < 1f;
		}

		private void Start()
		{
			_debugCube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			_debugCube.transform.localScale = 0.1f * Vector3.one;

			_currentFootTargetPosition = transform.position;
			_newFootTargetPosition = transform.position;
			_previousFootTargetPosition = transform.position;
			_stepLerpProgress = 1f;
		}

		private void Update()
		{
			PlaceFootTarget();
			PlaceKneeHint();
		}

		private void PlaceFootTarget()
		{
			transform.position = _currentFootTargetPosition + _footHeightOffset * Vector3.up;

			// look for the candidate where we would currently place the foot
			Vector3 projectedBodyPosition = _headTarget.position + (SidewaysVector * _footSpacing);
			projectedBodyPosition.y = _floorManager.GetFloorHeight(_bodyTarget.position);

			// check if we need to make a step (and if we can take a step)
			if (!CurrentlyStepping && !_otherFoot.CurrentlyStepping
				&& Vector3.Distance(projectedBodyPosition, _newFootTargetPosition) > _stepDistance)
			{
				_stepLerpProgress = 0f;

				Vector3 stepDirection = projectedBodyPosition - _newFootTargetPosition;
				stepDirection.y = 0f;
				stepDirection.Normalize();

				_newFootTargetPosition = projectedBodyPosition + stepDirection * _stepDistance + _footOffset;
			}
			// progress the step further if we are currently making a step
			if (_stepLerpProgress < 1f)
			{
				_currentFootTargetPosition = Vector3.Lerp(_previousFootTargetPosition, _newFootTargetPosition, _stepLerpProgress);
				_currentFootTargetPosition.y += Mathf.Sin(_stepLerpProgress * Mathf.PI) * _stepHeight;

				_stepLerpProgress += Time.deltaTime * _stepSpeed;
			}
			else
			{
				_previousFootTargetPosition = _newFootTargetPosition;
			}

			_debugCube.transform.position = _newFootTargetPosition;
		}

		private void PlaceKneeHint()
		{
			_currentKneeHintPosition = Vector3.Lerp(_bodyTarget.position, _currentFootTargetPosition, 0.5f);
			// move the knee to the front to ensure correct bending
			_currentKneeHintPosition += _bodyTarget.forward * 0.5f;
			// move the knee slightly outwards to the sides
			_currentKneeHintPosition += SidewaysVector * 0.05f;
			_kneeHint.position = _currentKneeHintPosition;
		}
	}
}
