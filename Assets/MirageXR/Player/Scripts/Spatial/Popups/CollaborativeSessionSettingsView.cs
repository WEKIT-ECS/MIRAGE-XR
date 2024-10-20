using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
	public class CollaborativeSessionSettingsView : PopupBase
	{
		[SerializeField] private Button _btnClose;
		[SerializeField] private TMP_Text _invitationCodeLabel;
		[SerializeField] private TMP_Text _passwordLabel;
		[SerializeField] private TMP_Text _userNameLabel;
		[SerializeField] private Toggle _micToggle;
		[SerializeField] private Toggle _audioToggle;

		protected override bool TryToGetArguments(params object[] args)
		{
			return true;
		}

		public override async void Initialization(Action<PopupBase> onClose, params object[] args)
		{
			base.Initialization(onClose, args);

			_btnClose.SafeSetListener(Close);

			_invitationCodeLabel.text = CollaborationManager.Instance.InvitationCode;
			_passwordLabel.text = CollaborationManager.Instance.SessionPassword;
			_userNameLabel.text = CollaborationManager.Instance.LocalUserData.UserName;
			_micToggle.isOn = CollaborationManager.Instance.VoiceMicrophoneEnabled;
			_micToggle.SafeAddListener(OnMicToggleChanged);
			_audioToggle.isOn = !CollaborationManager.Instance.MuteVoiceChat;
			_audioToggle.SafeAddListener(OnAudioToggleChanged);
		}

		private void OnMicToggleChanged(bool micEnabled)
		{
			CollaborationManager.Instance.VoiceMicrophoneEnabled = micEnabled;
		}

		private void OnAudioToggleChanged(bool voicesEnabled)
		{
			CollaborationManager.Instance.MuteVoiceChat = !voicesEnabled;
		}
	}
}
