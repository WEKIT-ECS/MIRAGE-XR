using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR;

namespace MirageXR
{
	public class RigReferences : MonoBehaviour
	{
		public RigBuilder RigBuilder { get; set; }
		public Transform Armature { get; set; }
		public BoneCollection Bones { get; private set; } = new BoneCollection();
		public IKCollection IK { get; private set; } = new IKCollection();
	}

	public class BoneCollection
	{
		private const string headBoneName = "Head";
		private const string hipsBoneName = "Hips";

		public Transform Head { get => GetByName(headBoneName); }
		public Transform Hips { get => GetByName(hipsBoneName); }
		public MirroredBones Left { get; private set; }
		public MirroredBones Right { get; private set; }


		private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();

		public BoneCollection()
		{
			Left = new MirroredBones(true, this);
			Right = new MirroredBones(true, this);
		}

		/// <summary>
		/// Selects a side based on an integer
		/// Can be used with a for loop to work on both sides of the rig
		/// </summary>
		/// <param name="side">The side to select; 0 for left and 1 for right</param>
		/// <returns>The bone structure on the selected side</returns>
		public MirroredBones GetSide(int side)
		{
			switch(side)
			{
				case 0:
					return Left;
				case 1:
					return Right;
				default:
					Debug.LogError($"side must be either 0 for left or 1 for right but was {side}.");
					return null;
			}
		}

		/// <summary>
		/// Selects a side based on a bool
		/// </summary>
		/// <param name="left">The side to select; true for left and false for right</param>
		/// <returns>The bone structure on the selected side</returns>
		public MirroredBones GetSide(bool left)
		{
			if (left)
			{
				return Left;
			}
			else
			{
				return Right;
			}
		}

		public void SetBones(Transform[] bones)
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

	public class MirroredBones
	{
		public bool IsLeft { get; private set; }

		public ArmBones Arm { get; private set; }
		public LegBones Leg { get; private set; }

		private BoneCollection _bones;

		public MirroredBones(bool isLeft, BoneCollection boneCollection)
		{
			_bones = boneCollection;
			IsLeft = isLeft;
			Arm = new ArmBones(this);
			Leg = new LegBones(this);
		}

		public string MakeSidedName(string unsidedName)
		{
			// if the naming scheme of the armature is changed at some point, this method needs to be updated
			return (IsLeft ? "Left" : "Right") + unsidedName;
		}

		public Transform GetByName(string name)
		{
			return _bones.GetByName(name);
		}
	}

	public class ArmBones
	{
		private const string upperArmBoneName = "Arm";
		private const string lowerArmBoneName = "ForeArm";

		public Transform Upper
		{
			get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(upperArmBoneName));
		}
		public Transform Lower
		{
			get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(lowerArmBoneName));
		}
		public HandBones Hand { get; private set; }


		private MirroredBones _mirroredBones;

		public ArmBones(MirroredBones parent)
		{
			_mirroredBones = parent;
			Hand = new HandBones(parent);
		}
	}

	public class HandBones
	{
		private const string handBoneName = "Hand";

		// infix for the finger name
		private const string thumbFingerName = "HandThumb";
		private const string indexFingerName = "HandIndex";
		private const string middleFingerName = "HandMiddle";
		private const string ringFingerName = "HandRing";
		private const string littleFingerName = "HandPinky";

		public Transform Wrist { get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(handBoneName)); }
		public FingerBones Thumb { get; private set; }
		public FingerBones IndexFinger { get; private set; }
		public FingerBones MiddleFinger { get; private set; }
		public FingerBones RingFinger { get; private set; }
		public FingerBones LittleFinger { get; private set; }


		private MirroredBones _mirroredBones;

		public HandBones(MirroredBones parent)
		{
			_mirroredBones = parent;
			Thumb = new FingerBones(thumbFingerName, parent);
			IndexFinger = new FingerBones(indexFingerName, parent);
			MiddleFinger = new FingerBones(middleFingerName, parent);
			RingFinger = new FingerBones(ringFingerName, parent);
			LittleFinger = new FingerBones(littleFingerName, parent);
		}
	}

	public class FingerBones
	{
		// current armature naming convention: number of finger segments as the suffix
		private const string proximalName = "1";
		private const string intermediateName = "2";
		private const string distalName = "3";
		private const string tipName = "4";

		//public Transform Metacarpal;
		public Transform Proximal1 { get => _mirroredBones.GetByName(FingerBoneName(proximalName)); }
		public Transform Intermediate2 { get => _mirroredBones.GetByName(FingerBoneName(intermediateName)); }
		public Transform Distal3 { get => _mirroredBones.GetByName(FingerBoneName(distalName)); }
		public Transform Tip4 { get => _mirroredBones.GetByName(FingerBoneName(tipName)); }

		private MirroredBones _mirroredBones;
		private string _fingerName;

		public FingerBones(string fingerName, MirroredBones parent)
		{
			_mirroredBones = parent;
			_fingerName = fingerName;
		}

		private string FingerBoneName(string fingerSegment)
		{
			return _mirroredBones.MakeSidedName(_fingerName + fingerSegment);
		}
	}

	public class LegBones
	{
		public const string upperLegBoneName = "UpLeg";
		public const string lowerLegBoneName = "Leg";
		public const string footBoneName = "Foot";

		public Transform Upper
		{
			get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(upperLegBoneName));
		}
		public Transform Lower
		{
			get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(lowerLegBoneName));
		}
		public Transform Foot
		{
			get => _mirroredBones.GetByName(_mirroredBones.MakeSidedName(footBoneName));
		}

		private MirroredBones _mirroredBones;

		public LegBones(MirroredBones parent)
		{
			_mirroredBones = parent;
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
