using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	/// <summary>
	/// Sets up the IK rig of the avatar
	/// </summary>
	public class AvatarXRIKRigInitializer : AvatarInitializer
	{
		[Tooltip("Debug option to draw small cubes on the joints of the hands")]
		[SerializeField] private bool drawHandJointTargets = false;

		[Tooltip("Set this option if you want to reuse an existing GameObject as the head IK target. If left empty, the script will create a new empty GameObject.")]
		[SerializeField] private GameObject _headTargetInstance;
		[Tooltip("Set this option if you want to reuse an existing GameObject as the left hand IK target. If left empty, the script will create a new empty GameObject.")]
		[SerializeField] private GameObject _leftHandInstance;
		[Tooltip("Set this option if you want to reuse an existing GameObject as the right hand IK target. If left empty, the script will create a new empty GameObject.")]
		[SerializeField] private GameObject _rightHandInstance;

		/// <summary>
		/// Runs at the default priority of 0
		/// </summary>
		public override int Priority => 0;

		/// <summary>
		/// Sets up the IK rig for the avatar
		/// </summary>
		/// <param name="avatar">The avatar to initialize</param>
		public override void InitializeAvatar(GameObject avatar)
		{
			// set up the rig reference component
			// which will be vital to access the different bones and IK targets from other scripts
			RigReferences rigRefs = avatar.AddComponent<RigReferences>();

			// a rig builder is the central component to the IK rig
			RigBuilder rigBuilder = avatar.AddComponent<RigBuilder>();
			rigRefs.RigBuilder = rigBuilder;

			rigRefs.FindArmature(avatar.transform);

			rigRefs.Bones.CollectBones(avatar);

#if UNITY_EDITOR
			// set up the bone renderer which shows the skeleton in the editor
			// in the build, the transforms of the bone renderer are not accessible, so this needs to be editor-only
			BoneRenderer boneRenderer = rigRefs.Armature.gameObject.AddComponent<BoneRenderer>();
			boneRenderer.transforms = rigRefs.Bones.ToArray();
#endif

			// the XR IK Rig is the parent object under which we create the IK targets as children
			// it has a rig component which defines all constraints in the children as IK targets of the current IK layer
			GameObject xrIKRig = new GameObject("XR IK Rig");
			xrIKRig.transform.parent = avatar.transform;
			xrIKRig.transform.position = avatar.transform.position;
			xrIKRig.transform.rotation = avatar.transform.rotation;
			Rig rig = xrIKRig.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(rig));

			rigRefs.IK.RigLayer = rig;

			SetupIKTargets(rig, rigRefs);

			rigBuilder.Build();
		}

		// sets up the IK targets for hips, head, hands, hand joints and feet
		private void SetupIKTargets(Rig rig, RigReferences rigRefs)
		{
			rigRefs.IK.HipsConstraint = AddMuliparentTarget(rig.transform, rigRefs.Bones.Hips);
			rigRefs.IK.HeadConstraint = AddMuliRotationTarget(rig.transform, rigRefs.Bones.Head, false, _headTargetInstance);
			for (int i = 0; i < 2; i++)
			{
				SidedIKCollection sidedIks = rigRefs.IK.GetSide(i);
				SidedBonesCollection sidedBones = rigRefs.Bones.GetSide(i);

				bool isLeft = i == 0;

				GameObject existingHandIKTarget = isLeft ? _leftHandInstance : _rightHandInstance;
				sidedIks.Hand.Constraint = AddTwoBoneTarget(rig.transform, sidedBones.Arm.Upper, sidedBones.Arm.Lower, sidedBones.Arm.Hand.Wrist, existingHandIKTarget);
				sidedIks.Foot.Constraint = AddTwoBoneTarget(rig.transform, sidedBones.Leg.Upper, sidedBones.Leg.Lower, sidedBones.Leg.Foot);

				GenerateHandBoneConstraintTargets(isLeft, rigRefs);
			}
		}

		// creates a new GameObject and configures it as multiparent constraint target
		// these are useful for driving certain bone rotations directly via a target control object
		private MultiParentConstraint AddMuliparentTarget(Transform parent, Transform bone, bool drawJoint = false, GameObject existingTarget = null)
		{
			GameObject ikTarget;
			// use an existing target if one is provided, otherwise create a new one, either with visuals or just empty
			if (existingTarget == null)
			{
				ikTarget = drawJoint ?
					GameObject.CreatePrimitive(PrimitiveType.Cube)
					: new GameObject();
				if (drawJoint)
				{
					ikTarget.transform.localScale = 0.01f * Vector3.one;
				}
				ikTarget.name = bone.name + "IK_target";
			}
			else
			{
				ikTarget = existingTarget;
			}
			ikTarget.transform.parent = parent;
			ikTarget.transform.position = bone.position;
			ikTarget.transform.rotation = bone.rotation;

			MultiParentConstraint multiParentConstraint = ikTarget.AddComponent<MultiParentConstraint>();
			multiParentConstraint.data.constrainedObject = bone;
			WeightedTransformArray sources = new WeightedTransformArray(0)
			{
				new WeightedTransform(ikTarget.transform, 1)
			};
			multiParentConstraint.data.sourceObjects = sources;
			multiParentConstraint.data.constrainedPositionXAxis = true;
			multiParentConstraint.data.constrainedPositionYAxis = true;
			multiParentConstraint.data.constrainedPositionZAxis = true;
			multiParentConstraint.data.constrainedRotationXAxis = true;
			multiParentConstraint.data.constrainedRotationYAxis = true;
			multiParentConstraint.data.constrainedRotationZAxis = true;

			return multiParentConstraint;
		}

		// creates a new GameObject and configures it as multirotation constraint target
		// these are useful for driving certain bone rotations directly via a target control object
		private MultiRotationConstraint AddMuliRotationTarget(Transform parent, Transform bone, bool drawJoint = false, GameObject existingTarget = null)
		{
			GameObject ikTarget;
			// use an existing target if one is provided, otherwise create a new one, either with visuals or just empty
			if (existingTarget == null)
			{
				ikTarget = drawJoint ?
					GameObject.CreatePrimitive(PrimitiveType.Cube)
					: new GameObject();
				if (drawJoint)
				{
					ikTarget.transform.localScale = 0.01f * Vector3.one;
				}
				ikTarget.name = bone.name + "IK_target";
			}
			else
			{
				ikTarget = existingTarget;
			}
			ikTarget.transform.parent = parent;
			ikTarget.transform.position = bone.position;
			ikTarget.transform.rotation = bone.rotation;

			MultiRotationConstraint multiRotationConstraint = ikTarget.AddComponent<MultiRotationConstraint>();
			multiRotationConstraint.data.constrainedObject = bone;
			WeightedTransformArray sources = new WeightedTransformArray(0)
			{
				new WeightedTransform(ikTarget.transform, 1)
			};
			multiRotationConstraint.data.sourceObjects = sources;
			multiRotationConstraint.data.constrainedXAxis = true;
			multiRotationConstraint.data.constrainedYAxis = true;
			multiRotationConstraint.data.constrainedZAxis = true;

			return multiRotationConstraint;
		}

		// adds the target and hint objects for a two bone constraints
		// useful for setting up limb end positions such as arms and legs
		private TwoBoneIKConstraint AddTwoBoneTarget(Transform parent, Transform rootBone, Transform midBone, Transform tipBone, GameObject existingTarget = null)
		{
			GameObject ik = new GameObject(tipBone.name + "IK");
			ik.transform.parent = parent;
			ik.transform.position = tipBone.position;
			ik.transform.rotation = tipBone.rotation;

			GameObject target;
			if (existingTarget != null)
			{
				target = existingTarget;
			}
			else
			{
				target = new GameObject(tipBone.name + "IK_target");
			}
			target.transform.parent = ik.transform;
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;

			GameObject hint = new GameObject(tipBone.name + "IK_hint");
			hint.transform.parent = ik.transform;

			TwoBoneIKConstraint ikConstraint = ik.GetComponent<TwoBoneIKConstraint>();
			if (ikConstraint == null)
			{
				ikConstraint = ik.AddComponent<TwoBoneIKConstraint>();
			}
			else
			{
				ikConstraint.Reset();
			}
			ikConstraint.data.root = rootBone;
			ikConstraint.data.mid = midBone;
			ikConstraint.data.tip = tipBone;
			ikConstraint.data.target = target.transform;
			ikConstraint.data.hint = hint.transform;

			ikConstraint.data.targetPositionWeight = 1;
			ikConstraint.data.targetRotationWeight = 1;
			ikConstraint.data.hintWeight = 1;

			return ikConstraint;
		}

		// generates the targets for the various hand bones
		private void GenerateHandBoneConstraintTargets(bool isLeft, RigReferences rigRefs)
		{
			// for all fingers: generate the control points
			for (int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
			{
				XRHandJointID jointId = XRHandJointIDUtility.FromIndex(i);

				// the mapping from an XRHandJointID to a specific bone in our armature is done by the GetBoneByJointID method
				Transform handBone = rigRefs.Bones.GetSide(isLeft).Arm.Hand.GetBoneByJointID(jointId);
				if (handBone == null)
				{
					continue;
				}

				HandIKData handIkData = rigRefs.IK.GetSide(isLeft).Hand;

				if (jointId != XRHandJointID.Wrist)
				{
					// generate new target objects for each joint
					MultiRotationConstraint jointIKTarget = AddMuliRotationTarget(handIkData.Target, handBone, drawHandJointTargets);
					handIkData.AddHandBoneIKTarget(jointId, jointIKTarget.transform);
				}
				else
				{
					// the wrist is a special case since we already have another constraint for it
					// so we don't need to set up the constraint or target object, just add it to the data collection
					handIkData.AddHandBoneIKTarget(jointId, handIkData.Target);
				}
			}
		}

		/// <summary>
		/// Cleans up the rig
		/// If persistent target instances for head or hands are used,
		/// it removes components that were specific to the avatar so that the objects can be reused on the next avatar
		/// </summary>
		/// <param name="avatar">The avatar to clean up</param>
		public override void CleanupAvatar(GameObject avatar)
		{
			base.CleanupAvatar(avatar);
			// for the head, we parent it again to the avatar container to avoid that it gets deleted with the avatar
			// Since we are moving it out of the rig hierarchy, we also need to remove its constraint, otherwise erros happen
			if (_headTargetInstance != null)
			{
				MultiRotationConstraint mrc = _headTargetInstance.GetComponent<MultiRotationConstraint>();
				DestroyImmediate(mrc);
				_headTargetInstance.transform.parent = transform;
			}

			RigReferences rigReferences = avatar.GetComponent<RigReferences>();

			// for the hands, similar re-parenting happens but we also need to clean up the generated joint IKs
			if (_leftHandInstance != null)
			{
				CleanupHand(_leftHandInstance, true, rigReferences);
			}
			if (_rightHandInstance != null)
			{
				CleanupHand(_rightHandInstance, false, rigReferences);
			}
		}

		// removes the constraint on the hand and deletes all IK targets for the finger joints
		// then, it reparents the instance object to the avatar container so that it can be reused
		private void CleanupHand(GameObject handInstance, bool left, RigReferences rigReferences)
		{
			DestroyImmediate(handInstance.GetComponent<TwoBoneIKConstraint>());
			rigReferences.IK.GetSide(left).Hand.ClearHandBoneIKTargets();
			handInstance.transform.parent = transform;
		}
	}
}
