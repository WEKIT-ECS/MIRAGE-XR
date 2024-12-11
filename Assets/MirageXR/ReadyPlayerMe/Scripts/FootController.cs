using UnityEngine;

namespace MirageXR
{
	public class FootController : MonoBehaviour
	{
		[field: Header("Foot Placement")]
		[field: SerializeField] public bool IsLeftFoot { get; set; }
		[SerializeField] private float _footHeightOffset = 0.04f;
		[SerializeField] private float _footSpacing = 0.12f;
		[field: Header("Stepping")]
		[field: SerializeField] private float FootAngleThreshold { get; set; } = 0f;
		[field: SerializeField] private float StepDistance { get; set; } = 0.3f;
		[field: SerializeField] private float StepHeight { get; set; } = 0.25f;
		[field: SerializeField] private float StepSpeed { get; set; } = 3.5f;
		[field: Header("Debugging")]
		[field: SerializeField] private bool ShowDebugWidgets { get; set; } = false;

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

		private RigReferences _rigRefs;

		private FootController _otherFoot;
		public FootController OtherFoot
		{
			get
			{
				if (_otherFoot == null)
				{
					_otherFoot = _rigRefs.IK.GetSide(!IsLeftFoot).Foot.Target.gameObject.GetComponent<FootController>();
				}
				return _otherFoot;
			}
		}

		private Vector3 SidewaysVector
		{
			get
			{
				float direction = IsLeftFoot ? -1f : 1f;
				return _rigRefs.IK.HipsTarget.right * direction;
			}
		}

		public bool CurrentlyStepping
		{
			get => _stepLerpProgress < 1f;
		}

		public void SetRigReferences(RigReferences rigReferences)
		{
			_rigRefs = rigReferences;
		}

		private void Start()
		{
            if (_rigRefs == null)
            {
				_rigRefs = gameObject.GetComponentInParent<RigReferences>();
            }
            _footRotationOffset = transform.rotation;
			_currentFootTargetPose = new Pose(transform.position, transform.rotation);
			_footGroundTargetPose = new Pose(transform.position, transform.rotation);
			_previousFootGroundTargetPose = new Pose(transform.position, transform.rotation);
			_stepLerpProgress = 1f;
		}

		private void Update()
		{
			if (ShowDebugWidgets && _debugWidget == null)
			{
				_debugWidget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				_debugWidget.name = "FootGround_" + (IsLeftFoot ? "Left" : "Right");
				_debugWidget.transform.localScale = 0.1f * Vector3.one;
			}
			else if (!ShowDebugWidgets && _debugWidget != null)
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
			Vector3 footPositionCandidate = _rigRefs.IK.HipsTarget.position + (SidewaysVector * _footSpacing);
			footPositionCandidate.y = _floorManager.GetFloorHeight(footPositionCandidate);

			float distance = Vector3.Distance(footPositionCandidate, _footGroundTargetPose.position);
			float dotProduct = Vector2.Dot(
				new Vector2(_rigRefs.IK.HipsTarget.forward.x, _rigRefs.IK.HipsTarget.forward.z),
				new Vector2(_footGroundTargetPose.forward.x, _footGroundTargetPose.forward.z));

			// check if we need to make a step (and if we can take a step)
			// we step if the distance of the foot candidate to our current foot placement is too high
			// or if we rotated the body too far
			if (!CurrentlyStepping && !OtherFoot.CurrentlyStepping
				&& (distance > 1.05f * StepDistance || dotProduct < FootAngleThreshold))
			{
				// initiate the step
				_stepLerpProgress = 0f;

				Vector3 stepDirection = footPositionCandidate - _previousFootGroundTargetPose.position;
				stepDirection.y = 0f;
				stepDirection.Normalize();

				_footGroundTargetPose.position = footPositionCandidate + stepDirection * StepDistance + _footHeightOffset * Vector3.up;
				_footGroundTargetPose.rotation = Quaternion.Euler(
					_currentFootTargetPose.rotation.eulerAngles.x,
					_rigRefs.IK.HeadTarget.eulerAngles.y,
					_currentFootTargetPose.rotation.eulerAngles.z);
			}
			// progress the step further if we are currently making a step
			if (_stepLerpProgress < 1f)
			{
				_currentFootTargetPose.position = Vector3.Lerp(_previousFootGroundTargetPose.position, _footGroundTargetPose.position, _stepLerpProgress);
				_currentFootTargetPose.position.y += Mathf.Sin(_stepLerpProgress * Mathf.PI) * StepHeight;

				_currentFootTargetPose.rotation = Quaternion.Slerp(_previousFootGroundTargetPose.rotation, _footGroundTargetPose.rotation, _stepLerpProgress);

				_stepLerpProgress += Time.deltaTime * StepSpeed;
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
			_currentKneeHintPosition = Vector3.Lerp(_rigRefs.IK.HipsTarget.position, _currentFootTargetPose.position, 0.5f);
			// move the knee to the front to ensure correct bending
			_currentKneeHintPosition += _rigRefs.IK.HipsTarget.forward * 0.5f;
			// move the knee slightly outwards to the sides
			_currentKneeHintPosition += SidewaysVector * 0.05f;
			_rigRefs.IK.GetSide(IsLeftFoot).Foot.KneeHint.position = _currentKneeHintPosition;
		}
	}
}
