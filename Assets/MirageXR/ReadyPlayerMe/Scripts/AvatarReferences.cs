using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Xml.Serialization.Advanced;
using UnityEngine;

namespace MirageXR
{
	public class AvatarReferences : MonoBehaviour
	{
		private AvatarLoader2 _avatarLoader;
		public AvatarLoader2 Loader { get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarLoader); }
		public RigReferences Rig { get; set; }
		public BodyController BodyController { get; set; }
		public SidedReferences Left { get; set; } = new SidedReferences(true);
		public SidedReferences Right { get; set; } = new SidedReferences(false);
		public AvatarVisibilityController2 VisibilityController { get; set; }

		public SidedReferences GetSide(bool left)
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

		public SidedReferences GetSide(int side)
		{
			if (side != 0 && side != 1)
			{
				Debug.LogError($"side should be 0 for left or 1 for right but was {side}. Cannot select side.");
				return null;
			}
			return GetSide(side == 0);
		}
	}

	public class SidedReferences
	{
		public bool IsLeft { get; set; }
		public HandController HandController { get; set; }
		public HandJointsController HandJointsController { get; set; }
		public FootController FootController { get; set; }

		public SidedReferences(bool isLeft)
		{
			IsLeft = isLeft;
		}
	}
}
