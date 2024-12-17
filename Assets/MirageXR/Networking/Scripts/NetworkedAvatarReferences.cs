using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedAvatarReferences : MonoBehaviour
	{
		[SerializeField] TMP_Text _nameLabel;
		public TMP_Text NameLabel { get => _nameLabel; }

		private AvatarReferences _avatarReferences;
		public AvatarReferences OfflineReferences { get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarReferences); }

		private NetworkedUserData _userData;
		public NetworkedUserData UserData { get => ComponentUtilities.GetOrFetchComponent(this, ref _userData); }

		private RigSynchronizer _rigSynchronizer;
		public RigSynchronizer RigSynchronizer { get => ComponentUtilities.GetOrFetchComponent(this, ref _rigSynchronizer); }

		private HandsSynchronizer _handsSynchronizer;
		public HandsSynchronizer HandsSynchronizer { get => ComponentUtilities.GetOrFetchComponent(this, ref _handsSynchronizer); }

		public NetworkedAvatarVisibilityController NetworkedVisibilityController { get; set; }
	}
}
