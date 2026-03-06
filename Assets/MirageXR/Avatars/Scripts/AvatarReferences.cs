using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Xml.Serialization.Advanced;
using UnityEngine;

namespace MirageXR
{
	public class AvatarReferences : MonoBehaviour
	{
		[SerializeField] private GameObject _loadingDisplay;
		public GameObject LoadingDisplay { get { return _loadingDisplay; } }
		private AvatarLoader _avatarLoader;
		public AvatarLoader Loader { get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarLoader); }
		public GameObject Avatar { get => Loader.CurrentAvatar; }
		public bool AvatarInstantiated { get => Loader.CurrentAvatar != null; }
		public RigReferences Rig { get; set; }
		public BodyController BodyController { get; set; }
		public SidedReferences Left { get; set; } = new SidedReferences(true);
		public SidedReferences Right { get; set; } = new SidedReferences(false);
		private AvatarVisibilityController _visibilityController;
		public AvatarVisibilityController VisibilityController { get => ComponentUtilities.GetOrFetchComponent(this, ref _visibilityController); }

		private AudioSource _audioSource;
		public AudioSource AudioSource
		{
			get
			{
				if (_audioSource == null)
				{
					_audioSource = Rig.IK.HeadTarget.GetComponentInChildren<AudioSource>();
				}
				return _audioSource;
			}
			set
			{
				_audioSource = value;
			}
		}

		private AvatarAudioController _audioController;
		public AvatarAudioController AudioController
		{
			get
			{
				if (_audioController == null)
				{
					_audioController = GetComponent<AvatarAudioController>();
				}
				return _audioController;
			}
		}

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
