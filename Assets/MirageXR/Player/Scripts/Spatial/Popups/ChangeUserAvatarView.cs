using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
	public class ChangeUserAvatarView : PopupBase
	{
		[Header("References")]
		[SerializeField] private RectTransform _confirmationPanel;
		[SerializeField] private Button _closeWindowBtn;
		[SerializeField] private Button _openEditorBtn;
		[SerializeField] private Button _applyAvatarBtn;
		[SerializeField] private TMP_InputField _avatarUrlField;

		protected override bool TryToGetArguments(params object[] args)
		{
			return true;
		}

		public override void Initialization(Action<PopupBase> onClose, params object[] args)
		{
			base.Initialization(onClose, args);

			_closeWindowBtn.onClick.AddListener(Close);
			_openEditorBtn.onClick.AddListener(OpenEditor);
			_applyAvatarBtn.onClick.AddListener(ApplyAvatarUrl);

			_avatarUrlField.text = UserSettings.AvatarUrl;
		}

		private void OpenEditor()
		{
			Application.OpenURL(UserSettings.READYPLAYERME_EDITOR_URL);
		}

		private void ApplyAvatarUrl()
		{
			if (!string.IsNullOrEmpty(_avatarUrlField.text))
			{
				UserSettings.AvatarUrl = _avatarUrlField.text;
				Debug.LogDebug("Changed avatar url to " + UserSettings.AvatarUrl);

				StartCoroutine(ShowConfirmationPanel());
			}
		}

		private IEnumerator ShowConfirmationPanel()
		{
			_confirmationPanel.gameObject.SetActive(true);
			yield return new WaitForSeconds(3f);
			_confirmationPanel.gameObject.SetActive(false);
		}
	}
}
