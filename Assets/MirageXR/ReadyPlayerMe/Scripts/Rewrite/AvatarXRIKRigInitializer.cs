using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MirageXR
{
	public class AvatarXRIKRigInitializer : AvatarInitializer
	{
		public override int Priority => 0;

		public override void InitializeAvatar(GameObject avatar)
		{
			RigReferences rigReferences = avatar.AddComponent<RigReferences>();

			RigBuilder rigBuilder = avatar.AddComponent<RigBuilder>();
			rigReferences.RigBuilder = rigBuilder;

			Transform armature = avatar.transform.Find("Armature");
			rigReferences.Armature = armature;

			Transform[] bones = armature.GetComponentsInChildren<Transform>();
			Transform[] bonesWithoutArmature = bones.Where(t => t != armature).ToArray();

			rigReferences.Bones.SetBones(bonesWithoutArmature);

			BoneRenderer boneRenderer = armature.gameObject.AddComponent<BoneRenderer>();
			boneRenderer.transforms = bonesWithoutArmature;

			GameObject xrIKRig = new GameObject("XR IK Rig");
			xrIKRig.transform.parent = avatar.transform;
			xrIKRig.transform.position = avatar.transform.position;
			xrIKRig.transform.rotation = avatar.transform.rotation;
			Rig rig = xrIKRig.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(rig));

			rigReferences.IK.RigLayer = rig;

			SetupIKTargets(rig, rigReferences);

			rigBuilder.Build();
		}

		private void SetupIKTargets(Rig rig, RigReferences rigRefs)
		{
			rigRefs.IK.HipsConstraint = AddMuliparentTarget(rig.transform, rigRefs.Bones.Hips);
			rigRefs.IK.HeadConstraint = AddMuliparentTarget(rig.transform, rigRefs.Bones.Head);
			rigRefs.IK.LeftHandConstraint = AddTwoBoneTarget(rig.transform, rigRefs.Bones.Left.Arm.Upper, rigRefs.Bones.Left.Arm.Lower, rigRefs.Bones.Left.Arm.Hand.Wrist);
			rigRefs.IK.RightHandConstraint = AddTwoBoneTarget(rig.transform, rigRefs.Bones.Right.Arm.Upper, rigRefs.Bones.Right.Arm.Lower, rigRefs.Bones.Right.Arm.Hand.Wrist);
			rigRefs.IK.LeftLegConstraint = AddTwoBoneTarget(rig.transform, rigRefs.Bones.Left.Leg.Upper, rigRefs.Bones.Left.Leg.Lower, rigRefs.Bones.Left.Leg.Foot);
			rigRefs.IK.RightLegConstraint = AddTwoBoneTarget(rig.transform, rigRefs.Bones.Right.Leg.Upper, rigRefs.Bones.Right.Leg.Lower, rigRefs.Bones.Right.Leg.Foot);
		}

		private MultiParentConstraint AddMuliparentTarget(Transform parentRig, Transform targetedBone)
		{
			GameObject target = new GameObject(targetedBone.name + "Target");
			target.transform.parent = parentRig;
			target.transform.position = targetedBone.position;
			target.transform.rotation = targetedBone.rotation;

			MultiParentConstraint multiParentConstraint = target.AddComponent<MultiParentConstraint>();
			multiParentConstraint.data.constrainedObject = targetedBone;
			WeightedTransformArray sources = new WeightedTransformArray(0)
			{
				new WeightedTransform(target.transform, 1)
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

		private TwoBoneIKConstraint AddTwoBoneTarget(Transform parentRig, Transform rootBone, Transform midBone, Transform tipBone)
		{
			GameObject ik = new GameObject(tipBone.name + "IK");
			ik.transform.parent = parentRig;
			ik.transform.position = tipBone.position;
			ik.transform.rotation = tipBone.rotation;

			GameObject target = new GameObject(tipBone.name + "IK_target");
			target.transform.parent = ik.transform;
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;

			GameObject hint = new GameObject(tipBone.name + "IK_hint");
			hint.transform.parent = ik.transform;

			TwoBoneIKConstraint ikConstraint = ik.AddComponent<TwoBoneIKConstraint>();
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
	}
}
