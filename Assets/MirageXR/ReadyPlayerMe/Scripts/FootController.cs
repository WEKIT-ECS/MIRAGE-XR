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
		//[SerializeField] private float _stepThreshold = 0.2f;
		[SerializeField] private float _stepDistance = 0.25f;
		[SerializeField] private float _stepHeight = 0.2f;
		[SerializeField] private float _stepSpeed = 3.5f;
		[Header("Debugging")]
		[SerializeField] private bool _showDebugWidgets = false;

		private static FloorManagerWithFallback _floorManager => RootObject.Instance.floorManagerWithRaycastFallback;

		// variables for intermediate calcuations of the foot target position and the position of the knee hint
		// the value of these variables are assigned to the transforms in Update
		private Pose _currentFootTargetPose;
		private Vector3 _currentKneeHintPosition;
		// step control points: the beginning and ending of a step
		private Pose _previousFootTargetPose, _newFootTargetPose;
		// keeps track of the current step progress; 0: step not started; 1: step completed
		private float _stepLerpProgress = 0f;

		private Quaternion _footRotationOffset = Quaternion.identity;

		private GameObject _debugWidget;

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
			_footRotationOffset = transform.rotation;
			_currentFootTargetPose = new Pose(transform.position, transform.rotation);
			_newFootTargetPose = new Pose(transform.position, transform.rotation);
			_previousFootTargetPose = new Pose(transform.position, transform.rotation);
			_stepLerpProgress = 1f;
		}

		private void Update()
		{
			if (_showDebugWidgets && _debugWidget == null)
			{
				_debugWidget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				_debugWidget.name = "FootGround_" + (_isLeftFoot ? "Left" : "Right");
				_debugWidget.transform.localScale = 0.1f * Vector3.one;
			}
			else if (!_showDebugWidgets && _debugWidget != null)
			{
				Destroy(_debugWidget);
			}

			PlaceFootTarget();
			PlaceKneeHint();
		}

		private void PlaceFootTarget()
		{
			transform.position = _currentFootTargetPose.position + _footHeightOffset * Vector3.up;
			transform.rotation = _currentFootTargetPose.rotation;

			// look for the candidate where we would currently place the foot
			Vector3 projectedVirtualFootPosition = _headTarget.position + (SidewaysVector * _footSpacing);
			projectedVirtualFootPosition.y = _floorManager.GetFloorHeight(projectedVirtualFootPosition);

			float distance = Vector3.Distance(projectedVirtualFootPosition, _newFootTargetPose.position);
			float dotProduct = Vector2.Dot(
				new Vector2(_headTarget.forward.x, _headTarget.forward.z),
				new Vector2(_newFootTargetPose.forward.x, _newFootTargetPose.forward.z));

			// check if we need to make a step (and if we can take a step)
			if (!CurrentlyStepping && !_otherFoot.CurrentlyStepping
				&& (distance > 1.05f * _stepDistance || dotProduct < 0))
			{
				_stepLerpProgress = 0f;

				Vector3 stepDirection = projectedVirtualFootPosition - _previousFootTargetPose.position;
				stepDirection.y = 0f;
				stepDirection.Normalize();

				_newFootTargetPose.position = projectedVirtualFootPosition + stepDirection * _stepDistance + _footHeightOffset * Vector3.up;
				_newFootTargetPose.rotation = Quaternion.Euler(
					_currentFootTargetPose.rotation.eulerAngles.x,
					_headTarget.eulerAngles.y,
					_currentFootTargetPose.rotation.eulerAngles.z);
			}
			// progress the step further if we are currently making a step
			if (_stepLerpProgress < 1f)
			{
				_currentFootTargetPose.position = Vector3.Lerp(_previousFootTargetPose.position, _newFootTargetPose.position, _stepLerpProgress);
				_currentFootTargetPose.position.y += Mathf.Sin(_stepLerpProgress * Mathf.PI) * _stepHeight;

				_currentFootTargetPose.rotation = Quaternion.Slerp(_previousFootTargetPose.rotation, _newFootTargetPose.rotation, _stepLerpProgress);

				_stepLerpProgress += Time.deltaTime * _stepSpeed;
			}
			else
			{
				_previousFootTargetPose.position = _newFootTargetPose.position;
				_previousFootTargetPose.rotation = _newFootTargetPose.rotation;
			}

			if (_debugWidget != null)
			{
				_debugWidget.transform.position = _newFootTargetPose.position;
			}
		}

		private void PlaceKneeHint()
		{
			_currentKneeHintPosition = Vector3.Lerp(_bodyTarget.position, _currentFootTargetPose.position, 0.5f);
			// move the knee to the front to ensure correct bending
			_currentKneeHintPosition += _bodyTarget.forward * 0.5f;
			// move the knee slightly outwards to the sides
			_currentKneeHintPosition += SidewaysVector * 0.05f;
			_kneeHint.position = _currentKneeHintPosition;
		}
	}
}
