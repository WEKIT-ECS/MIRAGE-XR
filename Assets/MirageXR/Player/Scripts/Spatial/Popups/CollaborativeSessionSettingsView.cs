#if FUSION2
using Fusion;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
	public class CollaborativeSessionSettingsView : PopupBase
	{
		[Header("References")]
		[SerializeField] private Button _btnClose;
		[SerializeField] private TMP_Text _invitationCodeLabel;
		[SerializeField] private TMP_Text _passwordLabel;
		[SerializeField] private TMP_Text _userNameLabel;
		[SerializeField] private Toggle _micToggle;
		[SerializeField] private Toggle _audioToggle;
		[SerializeField] private Transform _userListContainer;
		[Header("Prefabs")]
		[SerializeField] private GameObject _userListItemPrefab;

		private List<UserListItem> _userListItems = new List<UserListItem>();

		protected override bool TryToGetArguments(params object[] args)
		{
			return true;
		}

		public override async void Initialization(Action<PopupBase> onClose, params object[] args)
		{
			base.Initialization(onClose, args);

			_btnClose.SafeSetListener(Close);

#if FUSION2
			var collaborationManager = RootObject.Instance.CollaborationManager;
			_invitationCodeLabel.text = collaborationManager.InvitationCode;
			_passwordLabel.text = collaborationManager.SessionPassword;
			_userNameLabel.text = collaborationManager.UserManager.LocalUserData.UserName;
			_micToggle.isOn = collaborationManager.VoiceMicrophoneEnabled;
			_micToggle.SafeAddListener(OnMicToggleChanged);
			_audioToggle.isOn = !collaborationManager.MuteVoiceChat;
			_audioToggle.SafeAddListener(OnAudioToggleChanged);
			collaborationManager.UserManager.LocalUserData.UserNameChanged += OnUserNameChanged;
			collaborationManager.UserManager.UserListChanged += OnNetworkedUserDataListChanged;
			collaborationManager.UserManager.AnyUserNameChanged += OnAnyUserNameChanged;
			_userListItems = _userListContainer.GetComponentsInChildren<UserListItem>().ToList();
			UpdatePlayerList();
#endif
		}

#if FUSION2
		private void OnDestroy()
		{
			if (RootObject.Instance is null)
			{
				return;
			}

			var collaborationManager = RootObject.Instance.CollaborationManager;
			if (collaborationManager != null && collaborationManager.UserManager != null)
			{
				collaborationManager.UserManager.UserListChanged -= OnNetworkedUserDataListChanged;
				collaborationManager.UserManager.AnyUserNameChanged -= OnAnyUserNameChanged;
			}
		}

		private void OnAnyUserNameChanged()
		{
			UpdatePlayerList();
		}

		private void OnUserNameChanged(string newUserName)
		{
			_userNameLabel.text = RootObject.Instance.CollaborationManager.UserManager.LocalUserData.UserName;
		}

		private void OnNetworkedUserDataListChanged()
		{
			UpdatePlayerList();
		}

		private void UpdatePlayerList()
		{
			List<PlayerRef> users = RootObject.Instance.CollaborationManager.UserManager.UserList.ToList();
			// fill every list view item with content
			for (int i = 0; i < users.Count; i++)
			{
				NetworkedUserData networkedUserData = RootObject.Instance.CollaborationManager.UserManager.GetNetworkedUserDataOrDefault(users[i]);
				UserListItem userListItem;
				if (i < _userListItems.Count)
				{
					userListItem = _userListItems[i];
				}
				else
				{
					GameObject newItem = Instantiate(_userListItemPrefab, _userListContainer);
					userListItem = newItem.GetComponent<UserListItem>();
					_userListItems.Add(userListItem);
				}
				userListItem.gameObject.SetActive(true);
				userListItem.Initialize(users[i], networkedUserData);
			}

			// disable unneeded list items
			for (int i = users.Count; i < _userListItems.Count; i++)
			{
				_userListItems[i].gameObject.SetActive(false);
			}
		}
#endif

		private void OnMicToggleChanged(bool micEnabled)
		{
#if FUSION2
			RootObject.Instance.CollaborationManager.VoiceMicrophoneEnabled = micEnabled;
#endif
		}

		private void OnAudioToggleChanged(bool voicesEnabled)
		{
#if FUSION2
			RootObject.Instance.CollaborationManager.MuteVoiceChat = !voicesEnabled;
#endif
		}
	}
}
