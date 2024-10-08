using UnityEngine;

namespace MirageXR
{
	public class FootController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Transform _headTarget;
		[SerializeField] private Transform _kneeHint;
		[SerializeField] private Transform _bodyTarget;
		[SerializeField] private Transform _toeTarget;
		[SerializeField] private FootController _otherFoot;
		[Header("Foot Placement")]
		[SerializeField] private bool _isLeftFoot;
		[SerializeField] private float _footHeightOffset = 0.04f;
		[SerializeField] private float _footSpacing = 0.18f;
		[Header("Stepping")]
		//[SerializeField] private float _stepThreshold = 0.2f;
		[SerializeField] private float _footAngleThreshold = 0f;
		[SerializeField] private float _stepDistance = 0.25f;
		[SerializeField] private float _stepHeight = 0.2f;
		[SerializeField] private float _stepSpeed = 3.5f;
		[Header("Debugging")]
		[SerializeField] private bool _showDebugWidgets = false;

		private static FloorManagerWithFallback _floorManager => RootObject.Instance.FloorManagerWithRaycastFallback;

		// variables for intermediate calcuations of the foot target position and the position of the knee hint
		// the value of these variables are assigned to the transforms in Update
		private Pose _currentFootTargetPose;
		private Vector3 _currentKneeHintPosition;
		// step control points: the beginning and ending of a step
		private Pose _previousFootGroundTargetPose, _footGroundTargetPose;
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
			_footGroundTargetPose = new Pose(transform.position, transform.rotation);
			_previousFootGroundTargetPose = new Pose(transform.position, transform.rotation);
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

			ControlFoot();
			ControlKnee();
		}

		private void ControlFoot()
		{
			transform.position = _currentFootTargetPose.position + _footHeightOffset * Vector3.up;
			transform.rotation = _currentFootTargetPose.rotation;

			// look for the candidate where we would currently place the foot
			Vector3 footPositionCandidate = _bodyTarget.position + (SidewaysVector * _footSpacing);
			footPositionCandidate.y = _floorManager.GetFloorHeight(footPositionCandidate);

			float distance = Vector3.Distance(footPositionCandidate, _footGroundTargetPose.position);
			float dotProduct = Vector2.Dot(
				new Vector2(_bodyTarget.forward.x, _bodyTarget.forward.z),
				new Vector2(_footGroundTargetPose.forward.x, _footGroundTargetPose.forward.z));

			// check if we need to make a step (and if we can take a step)
			// we step if the distance of the foot candidate to our current foot placement is too high
			// or if we rotated the body too far
			if (!CurrentlyStepping && !_otherFoot.CurrentlyStepping
				&& (distance > 1.05f * _stepDistance || dotProduct < _footAngleThreshold))
			{
				// initiate the step
				_stepLerpProgress = 0f;

				Vector3 stepDirection = footPositionCandidate - _previousFootGroundTargetPose.position;
				stepDirection.y = 0f;
				stepDirection.Normalize();

				_footGroundTargetPose.position = footPositionCandidate + stepDirection * _stepDistance + _footHeightOffset * Vector3.up;
				_footGroundTargetPose.rotation = Quaternion.Euler(
					_currentFootTargetPose.rotation.eulerAngles.x,
					_headTarget.eulerAngles.y,
					_currentFootTargetPose.rotation.eulerAngles.z);
			}
			// progress the step further if we are currently making a step
			if (_stepLerpProgress < 1f)
			{
				_currentFootTargetPose.position = Vector3.Lerp(_previousFootGroundTargetPose.position, _footGroundTargetPose.position, _stepLerpProgress);
				_currentFootTargetPose.position.y += Mathf.Sin(_stepLerpProgress * Mathf.PI) * _stepHeight;

				_currentFootTargetPose.rotation = Quaternion.Slerp(_previousFootGroundTargetPose.rotation, _footGroundTargetPose.rotation, _stepLerpProgress);

				_stepLerpProgress += Time.deltaTime * _stepSpeed;
			}
			else
			{
				_previousFootGroundTargetPose.position = _footGroundTargetPose.position;
				_previousFootGroundTargetPose.rotation = _footGroundTargetPose.rotation;
			}

			if (_debugWidget != null)
			{
				_debugWidget.transform.position = _footGroundTargetPose.position;
			}
		}

		private void ControlKnee()
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
