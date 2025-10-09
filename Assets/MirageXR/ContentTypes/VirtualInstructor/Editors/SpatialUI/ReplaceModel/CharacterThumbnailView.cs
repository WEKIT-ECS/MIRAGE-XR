using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class CharacterThumbnailView : MonoBehaviour
	{
		[SerializeField] private Image _thumbnailImage;
		[SerializeField] private GameObject _waitSpinner;
		[SerializeField] private GameObject _errorDisplay;

		private string _characterModelId;

		public delegate void CharacterModelSelectedHandler(string characterModelId);
		public event CharacterModelSelectedHandler CharacterModelSelected;

		public delegate void CharacterModelUrlChangedHandler(string characterModelId);
		public event CharacterModelUrlChangedHandler CharacterModelIdChanged;

		public Texture2D DisplayedThumbnail
		{
			get => _thumbnailImage.sprite.texture;
			private set
			{
				if (_thumbnailImage.sprite != null)
				{
					Destroy(_thumbnailImage.sprite);
				}
				if (value != null)
				{
					_thumbnailImage.sprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), new Vector2(0.5f, 1f));
					_thumbnailImage.preserveAspect = true;
					_thumbnailImage.color = new Color(1, 1, 1, 1);
				}
				else
				{
					_thumbnailImage.color = new Color(1, 1, 1, 0);
				}
			}
		}

		public string CharacterModelId
		{
			get => _characterModelId;
			set
			{
				if (_characterModelId != value)
				{
					_characterModelId = value;
					CharacterModelIdChanged?.Invoke(value);
					UpdateView();					
				}
			}
		}

		public async void UpdateView()
		{
			DisplayedThumbnail = null;
			if (!string.IsNullOrWhiteSpace(_characterModelId))
			{
				_waitSpinner.SetActive(true);
				Texture2D thumbnail = await RootObject.Instance.AvatarLibraryManager.GetThumbnailAsync(_characterModelId);
				_errorDisplay.SetActive(thumbnail == null);
				DisplayedThumbnail = thumbnail;
				_waitSpinner.SetActive(false);
			}
		}

		public void ThumbnailSelected()
		{
			Debug.LogTrace($"Thumbnail with character model Id {_characterModelId} clicked.");
			CharacterModelSelected?.Invoke(_characterModelId);
		}

		public void Delete()
		{
			RootObject.Instance.AvatarLibraryManager.RemoveAvatar(_characterModelId);
		}
	}
}