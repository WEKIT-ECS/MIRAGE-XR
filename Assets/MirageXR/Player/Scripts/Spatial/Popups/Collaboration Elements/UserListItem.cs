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

		public PlayerRef PlayerRef { get; private set; }
		public NetworkedUserData UserData { get; private set; }

		private void Awake()
		{
			_avatarToggle.onValueChanged.AddListener(OnAvatarToggleChanged);
		}

		public void Initialize(PlayerRef playerRef, NetworkedUserData userData)
		{
			if (UserData != null)
			{
				UserData.AvatarController.VisibilityController.VisibilityChanged -= OnVisibilityChanged;
			}
			PlayerRef = playerRef;
			UserData = userData;
			UserData.AvatarController.VisibilityController.VisibilityChanged += OnVisibilityChanged;
			UpdateView();
		}

		private void UpdateView()
		{
			string text = UserData.UserName;
			if (PlayerRef == CollaborationManager.Instance.UserManager.LocalUser)
			{
				text += " (you)";
			}
			_avatarToggle.SetIsOnWithoutNotify(UserData.AvatarController.VisibilityController.Visible);
			_userNameLabel.text = text;
		}

		private void OnAvatarToggleChanged(bool value)
		{
			UserData.AvatarController.VisibilityController.Visible = value;
		}

		private void OnVisibilityChanged(bool value)
		{
			_avatarToggle.SetIsOnWithoutNotify(value);
		}
	}
}
