using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

namespace MirageXR
{
	public class RigReferences : MonoBehaviour
	{
		public RigBuilder RigBuilder { get; set; }
		public Transform Armature { get; private set; }
		public BoneCollection Bones { get; private set; }
		public IKCollection IK { get; private set; } = new IKCollection();

		public RigReferences()
		{
			Bones = new BoneCollection(this);
		}

		public void FindArmature(Transform avatar)
		{
			Armature = avatar.Find("Armature");
		}
	}

	public class BoneCollection
	{
		private const string headBoneName = "Head";
		private const string hipsBoneName = "Hips";

		public Transform Head { get => GetByName(headBoneName); }
		public Transform Hips { get => GetByName(hipsBoneName); }
		public SidedBonesCollection Left { get; private set; }
		public SidedBonesCollection Right { get; private set; }

		private RigReferences _rigReferences;
		private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();

		public BoneCollection(RigReferences rigReferences)
		{
			_rigReferences = rigReferences;
			Left = new SidedBonesCollection(true, this);
			Right = new SidedBonesCollection(false, this);
		}

		/// <summary>
		/// Selects a side based on an integer
		/// Can be used with a for loop to work on both sides of the rig
		/// </summary>
		/// <param name="side">The side to select; 0 for left and 1 for right</param>
		/// <returns>The bone structure on the selected side</returns>
		public SidedBonesCollection GetSide(int side)
		{
			if (side != 0 & side != 1)
			{
				Debug.LogError($"side must be either 0 for left or 1 for right but was {side}.");
				return null;
			}
			return GetSide(side == 0);
		}

		/// <summary>
		/// Selects a side based on a bool
		/// </summary>
		/// <param name="left">The side to select; true for left and false for right</param>
		/// <returns>The bone structure on the selected side</returns>
		public SidedBonesCollection GetSide(bool left)
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

		public SidedBonesCollection GetSide(Handedness handedness)
		{
			if (handedness == Handedness.Invalid)
			{
				Debug.LogError("Received invalid handedness. Cannot select bone side.");
				return null;
			}
			return GetSide(handedness == Handedness.Left);
		}

		public void CollectBones(GameObject avatar)
		{
			Transform[] bones = _rigReferences.Armature.GetComponentsInChildren<Transform>();
			Transform[] bonesWithoutArmature = bones.Where(t => t != _rigReferences.Armature).ToArray();

			_bones.Clear();
			foreach (Transform bone in bonesWithoutArmature)
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

		public Transform[] ToArray()
		{
			return _bones.Values.ToArray();
		}
	}

	public class SidedBonesCollection
	{
		public bool IsLeft { get; private set; }

		public ArmBones Arm { get; private set; }
		public LegBones Leg { get; private set; }

		private BoneCollection _bones;

		public SidedBonesCollection(bool isLeft, BoneCollection boneCollection)
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
			get => _sidedBones.GetByName(_sidedBones.MakeSidedName(upperArmBoneName));
		}
		public Transform Lower
		{
			get => _sidedBones.GetByName(_sidedBones.MakeSidedName(lowerArmBoneName));
		}
		public HandBones Hand { get; private set; }


		private SidedBonesCollection _sidedBones;

		public ArmBones(SidedBonesCollection parent)
		{
			_sidedBones = parent;
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

		public Transform Wrist { get => _sidedBones.GetByName(_sidedBones.MakeSidedName(handBoneName)); }
		public FingerBones Thumb { get; private set; }
		public FingerBones IndexFinger { get; private set; }
		public FingerBones MiddleFinger { get; private set; }
		public FingerBones RingFinger { get; private set; }
		public FingerBones LittleFinger { get; private set; }


		private SidedBonesCollection _sidedBones;

		public HandBones(SidedBonesCollection parent)
		{
			_sidedBones = parent;
			Thumb = new FingerBones(thumbFingerName, parent);
			IndexFinger = new FingerBones(indexFingerName, parent);
			MiddleFinger = new FingerBones(middleFingerName, parent);
			RingFinger = new FingerBones(ringFingerName, parent);
			LittleFinger = new FingerBones(littleFingerName, parent);
		}

		public Transform GetBoneByJointID(XRHandJointID jointId)
		{
			switch (jointId)
			{
				case XRHandJointID.Wrist:
					return Wrist;
				case XRHandJointID.Palm:
					return null;
				case XRHandJointID.ThumbMetacarpal:
					return Thumb.Proximal1;
				case XRHandJointID.ThumbProximal:
					return Thumb.Intermediate2;
				case XRHandJointID.ThumbDistal:
					return Thumb.Distal3;
				case XRHandJointID.ThumbTip:
					return Thumb.Tip4;
				case XRHandJointID.IndexMetacarpal:
					return null;
				case XRHandJointID.IndexProximal:
					return IndexFinger.Proximal1;
				case XRHandJointID.IndexIntermediate:
					return IndexFinger.Intermediate2;
				case XRHandJointID.IndexDistal:
					return IndexFinger.Distal3;
				case XRHandJointID.IndexTip:
					return IndexFinger.Tip4;
				case XRHandJointID.MiddleMetacarpal:
					return null;
				case XRHandJointID.MiddleProximal:
					return MiddleFinger.Proximal1;
				case XRHandJointID.MiddleIntermediate:
					return MiddleFinger.Intermediate2;
				case XRHandJointID.MiddleDistal:
					return MiddleFinger.Distal3;
				case XRHandJointID.MiddleTip:
					return MiddleFinger.Tip4;
				case XRHandJointID.RingMetacarpal:
					return null;
				case XRHandJointID.RingProximal:
					return RingFinger.Proximal1;
				case XRHandJointID.RingIntermediate:
					return RingFinger.Intermediate2;
				case XRHandJointID.RingDistal:
					return RingFinger.Distal3;
				case XRHandJointID.RingTip:
					return RingFinger.Tip4;
				case XRHandJointID.LittleMetacarpal:
					return null;
				case XRHandJointID.LittleProximal:
					return LittleFinger.Proximal1;
				case XRHandJointID.LittleIntermediate:
					return LittleFinger.Intermediate2;
				case XRHandJointID.LittleDistal:
					return LittleFinger.Distal3;
				case XRHandJointID.LittleTip:
					return LittleFinger.Tip4;
				default:
					return null;
			}
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
		public Transform Proximal1 { get => _sidedBones.GetByName(FingerBoneName(proximalName)); }
		public Transform Intermediate2 { get => _sidedBones.GetByName(FingerBoneName(intermediateName)); }
		public Transform Distal3 { get => _sidedBones.GetByName(FingerBoneName(distalName)); }
		public Transform Tip4 { get => _sidedBones.GetByName(FingerBoneName(tipName)); }

		private SidedBonesCollection _sidedBones;
		private string _fingerName;

		public FingerBones(string fingerName, SidedBonesCollection parent)
		{
			_sidedBones = parent;
			_fingerName = fingerName;
		}

		private string FingerBoneName(string fingerSegment)
		{
			return _sidedBones.MakeSidedName(_fingerName + fingerSegment);
		}
	}

	public class LegBones
	{
		public const string upperLegBoneName = "UpLeg";
		public const string lowerLegBoneName = "Leg";
		public const string footBoneName = "Foot";

		public Transform Upper
		{
			get => _sidedBones.GetByName(_sidedBones.MakeSidedName(upperLegBoneName));
		}
		public Transform Lower
		{
			get => _sidedBones.GetByName(_sidedBones.MakeSidedName(lowerLegBoneName));
		}
		public Transform Foot
		{
			get => _sidedBones.GetByName(_sidedBones.MakeSidedName(footBoneName));
		}

		private SidedBonesCollection _sidedBones;

		public LegBones(SidedBonesCollection parent)
		{
			_sidedBones = parent;
		}
	}

	public class IKCollection
	{
		public Rig RigLayer { get; set; }
		public MultiParentConstraint HipsConstraint { get; set; }
		public Transform HipsTarget { get => HipsConstraint.data.sourceObjects[0].transform; }
		public MultiRotationConstraint HeadConstraint { get; set; }
		public Transform HeadTarget { get => HeadConstraint.data.sourceObjects[0].transform; }

		public SidedIKCollection Left { get; private set; } = new SidedIKCollection(true);
		public SidedIKCollection Right { get; private set; } = new SidedIKCollection(false);

		/// <summary>
		/// Selects the side based on an integer
		/// </summary>
		/// <param name="side">The side to select, 0 for left and 1 for right</param>
		/// <returns>Returns the IK data for the selected side</returns>
		public SidedIKCollection GetSide(int side)
		{
			if (side != 0 && side != 1)
			{
				Debug.LogError($"side must be 0 for left or 1 for right but was {side}");
				return null;
			}
			return GetSide(side == 0);
		}

		public SidedIKCollection GetSide(bool left)
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

		public SidedIKCollection GetSide(Handedness handedness)
		{
			if (handedness == Handedness.Invalid)
			{
				Debug.LogError("Received invalid handedness. Cannot select side.");
				return null;
			}
			return GetSide(handedness == Handedness.Left);
		}
	}

	public class SidedIKCollection
	{
		public HandIKData Hand { get; private set; } = new HandIKData();
		public FootIKData Foot { get; private set; } = new FootIKData();

		public bool IsLeft { get; private set; }

		public SidedIKCollection(bool isLeft)
		{
			IsLeft = isLeft;
		}
	}

	public abstract class TwoBoneIKData
	{
		public TwoBoneIKConstraint Constraint { get; set; }
		public Transform Target { get => Constraint.data.target; }
		protected Transform Hint { get => Constraint.data.hint; }
	}

	public class HandIKData : TwoBoneIKData
	{
		public Transform ElbowHint { get => Hint; }
		private Dictionary<XRHandJointID, Transform> _handBoneIKTargets = new Dictionary<XRHandJointID, Transform>();

		public void AddHandBoneIKTarget(XRHandJointID jointId, Transform constraint)
		{
			_handBoneIKTargets.Add(jointId, constraint);
		}

		public Transform GetHandBoneIKTarget(XRHandJointID jointId)
		{
			if (_handBoneIKTargets.ContainsKey(jointId))
			{
				return _handBoneIKTargets[jointId];
			}
			else
			{
				return null;
			}
		}

		public bool HasHandBoneIKTarget(XRHandJointID jointId)
		{
			return _handBoneIKTargets.ContainsKey(jointId);
		}

		public void ClearHandBoneIKTargets()
		{
			for (int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
			{
				XRHandJointID jointId = XRHandJointIDUtility.FromIndex(i);

				if (jointId != XRHandJointID.Wrist)
				{
					DestroyHandBoneIKTarget(jointId);
				}
				else
				{
					_handBoneIKTargets.Remove(jointId);
				}
			}
		}

		public void DestroyHandBoneIKTarget(XRHandJointID jointId)
		{
			if (_handBoneIKTargets.ContainsKey(jointId))
			{
				GameObject.DestroyImmediate(_handBoneIKTargets[jointId].gameObject);
				_handBoneIKTargets.Remove(jointId);
			}
		}
	}

	public class FootIKData : TwoBoneIKData
	{
		public Transform KneeHint { get => Hint; }
	}
}
