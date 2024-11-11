using Fusion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class UserListItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text _userNameLabel;
		[SerializeField] private Toggle _avatarToggle;

		public event Action<PlayerRef, bool> AvatarVisbilityChanged;

		public PlayerRef PlayerRef { get; private set; }
		public NetworkedUserData UserData { get; private set; }

		private void Awake()
		{
			_avatarToggle.onValueChanged.AddListener(OnAvatarToggleChanged);
		}

		public void Initialize(PlayerRef playerRef, NetworkedUserData userData)
		{
			PlayerRef = playerRef;
			UserData = userData;
			UpdateView();
		}

		private void UpdateView()
		{
			string text = UserData.UserName;
			if (PlayerRef == CollaborationManager.Instance.UserManager.LocalUser)
			{
				text += " (you)";
			}
			_avatarToggle.SetIsOnWithoutNotify(UserData.AvatarController.Visible);
			_userNameLabel.text = text;
		}

		private void OnAvatarToggleChanged(bool value)
		{
			AvatarVisbilityChanged?.Invoke(PlayerRef, value);
		}
	}
}
