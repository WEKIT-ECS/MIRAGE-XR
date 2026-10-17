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
		[SerializeField] private GallerySelectionElement _characterModelSelectionElement;
		[SerializeField] private Button _replaceModelBtn;

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
			if (_openEditorBtn != null)
			{
				_openEditorBtn.gameObject.SetActive(false);
			}
			_replaceModelBtn.onClick.AddListener(OpenAvatarLibrary);
			_characterModelSelectionElement.GalleryElementSelectionStarted += OpenAvatarLibrary;

			if (_characterModelSelectionElement != null && _characterModelSelectionElement.Thumbnail != null)
			{
				_characterModelSelectionElement.Thumbnail.ThumbnailProvider = RootObject.Instance.AvatarLoadManager;
			}

			string avatarId = AvatarLoadUtils.GetId(UserSettings.AvatarUrl);
			// add the current avatar to the library so that it can be loaded
			if (!string.IsNullOrEmpty(avatarId) && !avatarId.Equals("DefaultAvatar", StringComparison.OrdinalIgnoreCase) && !RootObject.Instance.AvatarLibraryManager.Contains(avatarId))
			{
				RootObject.Instance.AvatarLibraryManager.Add(avatarId);
			}

			UserSettings.AvatarUrlChanged += OnAvatarUrlChanged;

			ApplyAvatarUrl();
		}

		private void OnDestroy()
		{
			_characterModelSelectionElement.GalleryElementSelectionStarted -= OpenAvatarLibrary;
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
		private void OnAvatarSelected(string avatarId)
		{
			UserSettings.AvatarUrl = AvatarLoadUtils.IdToModelUrl(avatarId);
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
			string cleanId = AvatarLoadUtils.GetId(UserSettings.AvatarUrl);
			_characterModelSelectionElement.Thumbnail.ElementId = string.IsNullOrEmpty(cleanId) ? "DefaultAvatar" : cleanId;
		}
	}
}
