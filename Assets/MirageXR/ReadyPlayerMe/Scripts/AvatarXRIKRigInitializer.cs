using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class AvatarXRIKRigInitializer : AvatarInitializer
	{
		[SerializeField] private bool drawHandJointTargets = false;

		[SerializeField] private GameObject _headTargetInstance;
		[SerializeField] private GameObject _leftHandInstance;
		[SerializeField] private GameObject _rightHandInstance;

		public override int Priority => 0;

		public override void InitializeAvatar(GameObject avatar)
		{
			RigReferences rigRefs = avatar.AddComponent<RigReferences>();

			RigBuilder rigBuilder = avatar.AddComponent<RigBuilder>();
			rigRefs.RigBuilder = rigBuilder;

			rigRefs.FindArmature(avatar.transform);

			rigRefs.Bones.CollectBones(avatar);

			BoneRenderer boneRenderer = rigRefs.Armature.gameObject.AddComponent<BoneRenderer>();
			boneRenderer.transforms = rigRefs.Bones.ToArray();

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

		private void SetupIKTargets(Rig rig, RigReferences rigRefs)
		{
			rigRefs.IK.HipsConstraint = AddMuliparentTarget(rig.transform, rigRefs.Bones.Hips);
			rigRefs.IK.HeadConstraint = AddMuliparentTarget(rig.transform, rigRefs.Bones.Head, false, _headTargetInstance);
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

		private MultiParentConstraint AddMuliparentTarget(Transform parent, Transform bone, bool drawJoint = false, GameObject existingTarget = null)
		{
			GameObject ikTarget;
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

		public void GenerateHandBoneConstraintTargets(bool isLeft, RigReferences rigRefs)
		{
			// for all fingers: generate the control points
			for (int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
			{
				XRHandJointID jointId = XRHandJointIDUtility.FromIndex(i);

				Transform handBone = rigRefs.Bones.GetSide(isLeft).Arm.Hand.GetBoneByJointID(jointId);
				if (handBone == null)
				{
					continue;
				}

				HandIKData handIkData = rigRefs.IK.GetSide(isLeft).Hand;

				if (jointId != XRHandJointID.Wrist)
				{
					MultiParentConstraint jointIKTarget = AddMuliparentTarget(handIkData.Target, handBone, drawHandJointTargets);
					handIkData.AddHandBoneIKTarget(jointId, jointIKTarget.transform);
				}
				else
				{
					// the wrist is a special case since we already have another constraint for it
					handIkData.AddHandBoneIKTarget(jointId, handIkData.Target);
				}
			}
		}

		public override void CleanupAvatar(GameObject avatar)
		{
			base.CleanupAvatar(avatar);
			if (_headTargetInstance != null)
			{
				MultiParentConstraint mpc = _headTargetInstance.GetComponent<MultiParentConstraint>();
				DestroyImmediate(mpc);
				_headTargetInstance.transform.parent = transform;
			}

			RigReferences rigReferences = avatar.GetComponent<RigReferences>();

			if (_leftHandInstance != null)
			{
				CleanupHand(_leftHandInstance, true, rigReferences);
			}
			if (_rightHandInstance != null)
			{
				CleanupHand(_rightHandInstance, false, rigReferences);
			}
		}

		private void CleanupHand(GameObject handInstance, bool left, RigReferences rigReferences)
		{
			DestroyImmediate(handInstance.GetComponent<TwoBoneIKConstraint>());
			rigReferences.IK.GetSide(left).Hand.ClearHandBoneIKTargets();
			handInstance.transform.parent = transform;
		}
	}
}
