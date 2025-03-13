#if FUSION2
using Fusion;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class UserListItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text _userNameLabel;
		[SerializeField] private Toggle _avatarToggle;

#if FUSION2
		public PlayerRef PlayerRef { get; private set; }
		public NetworkedUserData UserData { get; private set; }

		private NetworkedAvatarReferences _avatarRefs;

		private void Awake()
		{
			_avatarToggle.onValueChanged.AddListener(OnAvatarToggleChanged);
		}

		public void Initialize(PlayerRef playerRef, NetworkedUserData userData)
		{
			//if (UserData != null)
			//{
			//	UserData.AvatarReferences.OfflineReferences.VisibilityController.VisibilityChanged -= OnVisibilityChanged;
			//}
			PlayerRef = playerRef;
			UserData = userData;
			_avatarRefs = UserData.AvatarReferences;
			_avatarRefs.OfflineReferences.VisibilityController.VisibilityChanged += OnVisibilityChanged;
			UpdateView();
		}

		private void UpdateView()
		{
			var text = UserData.UserName;
			if (PlayerRef == RootObject.Instance.CollaborationManager.LocalPlayer)
			{
				text += " (you)";
			}
			_avatarToggle.SetIsOnWithoutNotify(_avatarRefs.OfflineReferences.VisibilityController.Visible);
			_userNameLabel.text = text;
		}
#endif

		private void OnAvatarToggleChanged(bool value)
		{
#if FUSION2
			_avatarRefs.OfflineReferences.VisibilityController.Visible = value;
#endif
		}

#if FUSION2
		private void OnVisibilityChanged(bool value)
		{
			_avatarToggle.SetIsOnWithoutNotify(value);
		}
#endif
	}
}
