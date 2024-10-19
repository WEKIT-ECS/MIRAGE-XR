using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class HandJointsController : MonoBehaviour
	{
		[SerializeField] private List<BoneEntry> _boneMapping = new List<BoneEntry>();
		[SerializeField] private RigBuilder _rigBuilder;

		[SerializeField] private bool drawJointTargets = false;

		[field: SerializeField]
		public Handedness HandSide { get; private set; }


		private Dictionary<XRHandJointID, Transform> constraintTargets = new Dictionary<XRHandJointID, Transform>();

		private void Awake()
		{
			GenerateConstraintTargets();
		}

		private void GenerateConstraintTargets()
		{
			// the wrist is a special case because we already have an IK constraint set up for it in the editor
			constraintTargets.Add(XRHandJointID.Wrist, transform);

			// for all fingers: generate the control points
			foreach (BoneEntry boneEntry in _boneMapping)
			{
				GameObject constraintTarget =
					drawJointTargets ?
					GameObject.CreatePrimitive(PrimitiveType.Cube)
					: new GameObject();
				constraintTarget.transform.localScale = 0.01f * Vector3.one;
				constraintTarget.name = HandSide + boneEntry.HandJointID.ToString() + "_target";
				constraintTarget.transform.parent = transform;
				constraintTarget.transform.position = boneEntry.Bone.position;
				constraintTarget.transform.rotation = boneEntry.Bone.rotation;
				MultiParentConstraint mpConstraint = constraintTarget.AddComponent<MultiParentConstraint>();
				mpConstraint.Reset();
				mpConstraint.data.constrainedObject = boneEntry.Bone;
				WeightedTransformArray sources = new WeightedTransformArray(0)
			{
				new WeightedTransform(constraintTarget.transform, 1)
			};
				mpConstraint.data.sourceObjects = sources;

				if (!constraintTargets.ContainsKey(boneEntry.HandJointID))
				{
					constraintTargets.Add(boneEntry.HandJointID, constraintTarget.transform);
				}
				else
				{
					Debug.LogWarning($"The hand configuration contains multiple entries for the {HandSide.ToString()} hand: {boneEntry.HandJointID}. Only the first entry has an effect.");
				}
			}

			_rigBuilder.Build();
		}

		public void ApplyPoseToJoint(XRHandJointID jointId, Pose pose)
		{
			if (constraintTargets.ContainsKey(jointId))
			{
				constraintTargets[jointId].transform.position = pose.position;
				constraintTargets[jointId].transform.rotation = pose.rotation * Quaternion.Euler(90, 0, 0);
			}
		}
	}

	[Serializable]
	public class BoneEntry : ISerializationCallbackReceiver
	{
		// we use a name for the entry so that they get labels in the array of the inspector
		[HideInInspector]
		public string name;
		public Transform Bone;
		public XRHandJointID HandJointID;

		// update the name before and after serializing / deserializing so make sure that it is displayed correctly
		public void OnAfterDeserialize()
		{
			name = HandJointID.ToString();
		}

		public void OnBeforeSerialize()
		{
			name = HandJointID.ToString();
		}
	}
}