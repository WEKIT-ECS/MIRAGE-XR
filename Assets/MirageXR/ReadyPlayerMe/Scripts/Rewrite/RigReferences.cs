using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR;

namespace MirageXR
{
	public class RigReferences : MonoBehaviour
	{
		public RigBuilder RigBuilder { get; set;}
		public Transform Armature { get; set; }
		public BoneCollection Bones { get; private set; } = new BoneCollection();
		public IKCollection IK { get; private set; } = new IKCollection();
	}

	public class BoneCollection
	{
		public const string headBoneName = "Head";
		public const string hipsBoneName = "Hips";
		public const string leftUpperArmBoneName = "LeftArm";
		public const string leftLowerArmBoneName = "LeftForeArm";
		public const string leftHandBoneName = "LeftHand";
		public const string rightUpperArmBoneName = "RightArm";
		public const string rightLowerArmBoneName = "RightForeArm";
		public const string rightHandBoneName = "RightHand";
		public const string leftUpperLegBoneName = "LeftUpLeg";
		public const string leftLowerLegBoneName = "LeftLeg";
		public const string leftFootBoneName = "LeftFoot";
		public const string rightUpperLegBoneName = "RightUpLeg";
		public const string rightLowerLegBoneName = "RightLeg";
		public const string rightFootBoneName = "RightFoot";

		public Transform Head { get => GetByName(headBoneName); }
		public Transform Hips { get => GetByName(hipsBoneName); }
		public Transform LeftUpperArm { get => GetByName(leftUpperArmBoneName); }
		public Transform LeftLowerArm { get => GetByName(leftLowerArmBoneName); }
		public Transform LeftHand { get => GetByName(leftHandBoneName); }
		public Transform RightUpperArm { get => GetByName(rightUpperArmBoneName); }
		public Transform RightLowerArm { get => GetByName(rightLowerArmBoneName); }
		public Transform RightHand { get => GetByName(rightHandBoneName); }
		public Transform LeftUpperLeg { get => GetByName(leftUpperLegBoneName); }
		public Transform LeftLowerLeg { get => GetByName(leftLowerLegBoneName); }
		public Transform LeftFoot { get => GetByName(leftFootBoneName); }
		public Transform RightUpperLeg { get => GetByName(rightUpperLegBoneName); }
		public Transform RightLowerLeg { get => GetByName(rightLowerLegBoneName); }
		public Transform RightFoot { get => GetByName(rightFootBoneName); }

		private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();

		public void Initialize(Transform[] bones)
		{
			_bones.Clear();
			foreach (Transform bone in bones)
			{
				_bones.Add(bone.name, bone);
			}
		}

		public Transform GetByName(string name)
		{
			if (_bones.ContainsKey(name)) return _bones[name];
			else
			{
				Debug.LogError($"Could not get bone with name {name}. The armature layout was likely changed so tha the configuration needs to be adjusted.");
				return null;
			}
		}
	}

	public class IKCollection
	{
		public Rig RigLayer { get; set; }
		public MultiParentConstraint HipsConstraint { get; set; }
		public Transform HipsTarget { get => HipsConstraint.data.sourceObjects[0].transform; }
		public MultiParentConstraint HeadConstraint { get; set; }
		public Transform HeadTarget { get => HeadConstraint.data.sourceObjects[0].transform; }
		public TwoBoneIKConstraint LeftHandConstraint { get; set; }
		public Transform LeftHandTarget { get => LeftHandConstraint.data.target; }
		public Transform LeftElbowHint { get => LeftHandConstraint.data.hint; }
		public TwoBoneIKConstraint RightHandConstraint { get; set; }
		public Transform RightHandTarget { get => RightHandConstraint.data.target; }
		public Transform RightElbowHint { get => RightHandConstraint.data.hint; }
		public TwoBoneIKConstraint LeftLegConstraint { get; set; }
		public Transform LeftLegTarget { get => LeftLegConstraint.data.target; }
		public Transform LeftKneeHint { get => LeftLegConstraint.data.hint; }
		public TwoBoneIKConstraint RightLegConstraint { get; set; }
		public Transform RightLegTarget { get => RightLegConstraint.data.target; }
		public Transform RightKneeHint { get => RightLegConstraint.data.hint; }
	}
}
