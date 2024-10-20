using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	/// <summary>
	/// Manager for access to hand tracking data
	/// </summary>
	public class HandTrackingManager : MonoBehaviour
	{
		private List<XRHandSubsystem> _handSubsystemCandidates = new List<XRHandSubsystem>();
		private XRHandSubsystem _activeHandSubsystem;

		[SerializeField] private bool StartTrackingOnInitialize = false;

		[field: SerializeField]
		public bool GetDataBeforeRender { get; set; } = true;

		[field: SerializeField]
		public bool GetDataOnDynamic { get; set; } = false;

		public bool IsTracking { get; private set; }

		public HandData[] HandData { get; private set; } =
		{
			new HandData()
			{
				HandSide = Handedness.Left,
				IsTracked = false
			},
			new HandData()
			{
				HandSide = Handedness.Right,
				IsTracked = false
			}
		};

		public HandData LeftHandData { get => HandData[0]; }
		public HandData RightHandData { get => HandData[1]; }

		public HandData GetHandStatus(Handedness handedness)
		{
			int index = (int)handedness - 1;
			if (index < 0)
			{
				return null;
			}
			if (HandData[index].HandSide != handedness)
			{
				Debug.LogError($"Assertment failed: Handedness of retrieved hand data does not match requested handedness. Hand Data: {HandData[index].HandSide} vs. requested for {handedness}", this);
				return null;
			}
			return HandData[index];
		}

		private void Start()
		{
			if (StartTrackingOnInitialize)
			{
				StartTracking();
			}
		}

		public void StartTracking()
		{
			if (IsTracking)
			{
				return;
			}

			if (_activeHandSubsystem == null || !_activeHandSubsystem.running)
			{
				SearchActiveHandSubsystem();
			}
			else
			{
				SubscribeHandSubsystem();
			}
			IsTracking = true;
		}

		public void StopTracking()
		{
			if (!IsTracking)
			{
				return;
			}

			UnsubscribeHandSubsystem();
			IsTracking = false;
		}

		private void Update()
		{
			if (IsTracking && (_activeHandSubsystem == null || !_activeHandSubsystem.running))
			{
				SearchActiveHandSubsystem();
			}
		}

		private void SearchActiveHandSubsystem()
		{
			Debug.Log("Searching for hand subsystem...", this);
			SubsystemManager.GetSubsystems(_handSubsystemCandidates);

			bool foundActiveHandSubsystem = false;
			for (int i = 0; i < _handSubsystemCandidates.Count; i++)
			{
				XRHandSubsystem handSubsystem = _handSubsystemCandidates[i];
				if (handSubsystem.running)
				{
					UnsubscribeHandSubsystem();
					_activeHandSubsystem = handSubsystem;
					foundActiveHandSubsystem = true;
					break;
				}
			}

			if (!foundActiveHandSubsystem)
			{
				return;
			}
			Debug.Log("Discovered an active hand subsystem. Subscribing to hand events...", this);

			SubscribeHandSubsystem();
		}

		private void SubscribeHandSubsystem()
		{
			if (_activeHandSubsystem != null)
			{
				_activeHandSubsystem.trackingAcquired += OnTrackingAcquired;
				_activeHandSubsystem.trackingLost += OnTrackingLost;
				_activeHandSubsystem.updatedHands += OnUpdateHands;
			}
		}

		private void UnsubscribeHandSubsystem()
		{
			if (_activeHandSubsystem != null)
			{
				_activeHandSubsystem.trackingAcquired -= OnTrackingAcquired;
				_activeHandSubsystem.trackingLost -= OnTrackingLost;
				_activeHandSubsystem.updatedHands -= OnUpdateHands;
			}
		}

		private void OnUpdateHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags flags, XRHandSubsystem.UpdateType type)
		{
			if ((!GetDataBeforeRender || type != XRHandSubsystem.UpdateType.BeforeRender)
				&& (!GetDataOnDynamic || type != XRHandSubsystem.UpdateType.Dynamic))
			{
				return;
			}

			bool leftHandTracked = subsystem.leftHand.isTracked;
			bool rightHandTracked = subsystem.rightHand.isTracked;

			if (leftHandTracked)
			{
				LeftHandData.IsTracked = leftHandTracked;
				TrackHand(subsystem.leftHand);
			}
			if (rightHandTracked)
			{
				RightHandData.IsTracked = rightHandTracked;
				TrackHand(subsystem.rightHand);
			}
		}

		private void TrackHand(XRHand hand)
		{
			HandData handData = GetHandStatus(hand.handedness);
			if (handData == null)
			{
				Debug.LogError($"Could not get hand tracking data for {hand.handedness} hand", this);
				return;
			}

			handData.JointPoses.Clear();

			for (int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
			{
				XRHandJoint joint = hand.GetJoint(XRHandJointIDUtility.FromIndex(i));

				if (joint.TryGetPose(out Pose pose))
				{
					handData.JointPoses.Add(joint.id, pose);
				}
			}
		}

		private void OnTrackingLost(XRHand hand)
		{
			HandData handData = GetHandStatus(hand.handedness);
			handData.IsTracked = false;
		}

		private void OnTrackingAcquired(XRHand hand)
		{
			HandData handData = GetHandStatus(hand.handedness);
			handData.IsTracked = true;
		}
	}

	public class HandData
	{
		public Handedness HandSide { get; set; }

		public bool IsTracked { get; set; } = false;

		public Dictionary<XRHandJointID, Pose> JointPoses { get; set; } = new Dictionary<XRHandJointID, Pose>();
	}


}
