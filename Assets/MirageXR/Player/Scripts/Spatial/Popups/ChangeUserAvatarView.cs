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
			_characterModelSelectionElement.CharacterModelSelectionStarted += OpenAvatarLibrary;

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

		// opens the avatar library menu (creates the menu instance if it did not already exist)
		private void OpenAvatarLibrary()
		{
			if (_avatarLibraryMenuInstance == null)
			{
				_avatarLibraryMenuInstance = Instantiate(_avatarLibraryMenu, transform).GetComponent<ReplaceModel>();
				_avatarLibraryMenuInstance.CharacterModelSelected += OnAvatarSelected;
			}
			RectTransform avatarLibraryMenuRectTransform = _avatarLibraryMenuInstance.GetComponent<RectTransform>();
			// place next to this menu
			avatarLibraryMenuRectTransform.localPosition = new Vector3(
				_rectTransform.sizeDelta.x + avatarLibraryMenuRectTransform.sizeDelta.x / 2f + 20,
				0,
				0);
			_avatarLibraryMenuInstance.gameObject.SetActive(true);
		}

		// called if the user selected an avatar from the library view
		// applies the selected avatar to the user settings
		// this automatically invokes an event that the avatar was changed and this will update the UI
		private void OnAvatarSelected(string modelUrl)
		{
			UserSettings.AvatarUrl = modelUrl;
		}

		// called if a new avatar was set in the user settings
		private void OnAvatarUrlChanged(string newAvatarUrl)
		{
			ApplyAvatarUrl();
		}

		// opens the ReadyPlayerMe editor website
		private void OpenEditor()
		{
			Application.OpenURL(UserSettings.READYPLAYERME_EDITOR_URL);
		}

		// apply the currently saved AvatarUrl to the UI to reflect the selected avatar
		private void ApplyAvatarUrl()
		{
			_characterModelSelectionElement.Thumbnail.CharacterModelUrl = UserSettings.AvatarUrl;
		}
	}
}
