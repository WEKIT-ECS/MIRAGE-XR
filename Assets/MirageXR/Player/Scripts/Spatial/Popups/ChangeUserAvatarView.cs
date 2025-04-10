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
		[SerializeField] private CharacterModelSelectionElement _characterModelSelectionElement;

		[Header("Prefabs")]
		[SerializeField] private GameObject _avatarLibraryMenu;

		private ReplaceModel _avatarLibraryMenuInstance;
		private RectTransform _rectTransform;

		protected override bool TryToGetArguments(params object[] args)
		{
			return true;
		}

		public override void Initialization(Action<PopupBase> onClose, params object[] args)
		{
			base.Initialization(onClose, args);
			
			_rectTransform = GetComponent<RectTransform>();

			_closeWindowBtn.onClick.AddListener(Close);
			_openEditorBtn.onClick.AddListener(OpenEditor);
			//_applyAvatarBtn.onClick.AddListener(ApplyAvatarUrl);
			_characterModelSelectionElement.CharacterModelSelectionStarted += OpenAvatarLibrary;

			//_avatarUrlField.text = UserSettings.AvatarUrl;

			// add the current avatar to the library so that it can be loaded
			if (!RootObject.Instance.AvatarLibraryManager.ContainsAvatar(UserSettings.AvatarUrl))
			{
				RootObject.Instance.AvatarLibraryManager.AddAvatar(UserSettings.AvatarUrl);
			}

			UserSettings.AvatarUrlChanged += OnAvatarUrlChanged;

			ApplyAvatarUrl();
		}

		private void OnDestroy()
		{
			_characterModelSelectionElement.CharacterModelSelectionStarted -= OpenAvatarLibrary;
			UserSettings.AvatarUrlChanged -= OnAvatarUrlChanged;
		}

		private void OpenAvatarLibrary()
		{
			if (_avatarLibraryMenuInstance == null)
			{
				_avatarLibraryMenuInstance = Instantiate(_avatarLibraryMenu, transform).GetComponent<ReplaceModel>();
				_avatarLibraryMenuInstance.CharacterModelSelected += OnAvatarSelected;
			}
			RectTransform avatarLibraryMenuRectTransform = _avatarLibraryMenuInstance.GetComponent<RectTransform>();
			avatarLibraryMenuRectTransform.localPosition = new Vector3(
				_rectTransform.sizeDelta.x + avatarLibraryMenuRectTransform.sizeDelta.x / 2f + 20,
				0,
				0);
			_avatarLibraryMenuInstance.gameObject.SetActive(true);
		}

		private void OnAvatarSelected(string modelUrl)
		{
			UserSettings.AvatarUrl = modelUrl;
		}

		private void OnAvatarUrlChanged(string newAvatarUrl)
		{
			//_avatarUrlField.text = UserSettings.AvatarUrl;
			//StartCoroutine(ShowConfirmationPanel());
			ApplyAvatarUrl();
		}

		private void OpenEditor()
		{
			Application.OpenURL(UserSettings.READYPLAYERME_EDITOR_URL);
		}

		private void ApplyAvatarUrl()
		{
			//if (!string.IsNullOrEmpty(_avatarUrlField.text))
			//{
			//	UserSettings.AvatarUrl = _avatarUrlField.text;
			//	Debug.LogDebug("Changed avatar url to " + UserSettings.AvatarUrl);

			//	StartCoroutine(ShowConfirmationPanel());
			//}
			_characterModelSelectionElement.Thumbnail.CharacterModelUrl = UserSettings.AvatarUrl;
		}

		private IEnumerator ShowConfirmationPanel()
		{
			_confirmationPanel.gameObject.SetActive(true);
			yield return new WaitForSeconds(3f);
			_confirmationPanel.gameObject.SetActive(false);
		}
	}
}
