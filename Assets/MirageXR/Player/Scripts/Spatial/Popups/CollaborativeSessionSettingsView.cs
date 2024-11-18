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
			_invitationCodeLabel.text = CollaborationManager.Instance.InvitationCode;
			_passwordLabel.text = CollaborationManager.Instance.SessionPassword;
			_userNameLabel.text = CollaborationManager.Instance.UserManager.LocalUserData.UserName;
			_micToggle.isOn = CollaborationManager.Instance.VoiceMicrophoneEnabled;
			_micToggle.SafeAddListener(OnMicToggleChanged);
			_audioToggle.isOn = !CollaborationManager.Instance.MuteVoiceChat;
			_audioToggle.SafeAddListener(OnAudioToggleChanged);
			CollaborationManager.Instance.UserManager.LocalUserData.UserNameChanged += OnUserNameChanged;
			CollaborationManager.Instance.UserManager.UserListChanged += OnNetworkedUserDataListChanged;
			CollaborationManager.Instance.UserManager.AnyUserNameChanged += OnAnyUserNameChanged;
			_userListItems = _userListContainer.GetComponentsInChildren<UserListItem>().ToList();
			UpdatePlayerList();
#endif
		}

#if FUSION2
		private void OnDestroy()
		{
			CollaborationManager.Instance.UserManager.UserListChanged -= OnNetworkedUserDataListChanged;
			CollaborationManager.Instance.UserManager.AnyUserNameChanged -= OnAnyUserNameChanged;
		}

		private void OnAnyUserNameChanged()
		{
			UpdatePlayerList();
		}

		private void OnUserNameChanged(string newUserName)
		{
			_userNameLabel.text = CollaborationManager.Instance.UserManager.LocalUserData.UserName;
		}

		private void OnNetworkedUserDataListChanged()
		{
			UpdatePlayerList();
		}

		private void UpdatePlayerList()
		{
			List<PlayerRef> users = CollaborationManager.Instance.UserManager.UserList.ToList();
			// fill every list view item with content
			for (int i = 0; i < users.Count; i++)
			{
				NetworkedUserData networkedUserData = CollaborationManager.Instance.UserManager.GetNetworkedUserDataOrDefault(users[i]);
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
			CollaborationManager.Instance.VoiceMicrophoneEnabled = micEnabled;
#endif
		}

		private void OnAudioToggleChanged(bool voicesEnabled)
		{
#if FUSION2
			CollaborationManager.Instance.MuteVoiceChat = !voicesEnabled;
#endif
		}
	}
}
