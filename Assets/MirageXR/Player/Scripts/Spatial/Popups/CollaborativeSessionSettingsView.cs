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

#if FUSION2
			_invitationCodeLabel.text = CollaborationManager.Instance.InvitationCode;
			_passwordLabel.text = CollaborationManager.Instance.SessionPassword;
			_userNameLabel.text = CollaborationManager.Instance.LocalUserData.UserName;
			_micToggle.isOn = CollaborationManager.Instance.VoiceMicrophoneEnabled;
			_micToggle.SafeAddListener(OnMicToggleChanged);
			_audioToggle.isOn = !CollaborationManager.Instance.MuteVoiceChat;
			_audioToggle.SafeAddListener(OnAudioToggleChanged);
#endif
		}

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
