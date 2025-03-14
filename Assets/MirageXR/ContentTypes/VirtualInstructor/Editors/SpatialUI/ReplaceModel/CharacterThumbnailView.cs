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

		private string _characterModelUrl;

		public delegate void CharacterModelSelectedHandler(string characterModelUrl);
		public event CharacterModelSelectedHandler CharacterModelSelected;

		public delegate void CharacterModelUrlChangedHandler(string characterModelUrl);
		public event CharacterModelUrlChangedHandler CharacterModelUrlChanged;

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
					_thumbnailImage.sprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), new Vector2(0.5f, 0.5f));
					_thumbnailImage.preserveAspect = true;
					_thumbnailImage.color = new Color(1, 1, 1, 1);
				}
				else
				{
					_thumbnailImage.color = new Color(1, 1, 1, 0);
				}
			}
		}

		public string CharacterModelUrl
		{
			get => _characterModelUrl;
			set
			{
				if (_characterModelUrl != value)
				{
					_characterModelUrl = value;
					CharacterModelUrlChanged?.Invoke(value);
					UpdateView();					
				}
			}
		}

		public async void UpdateView()
		{
			DisplayedThumbnail = null;
			if (!string.IsNullOrWhiteSpace(_characterModelUrl))
			{
				_waitSpinner.SetActive(true);
				Texture2D thumbnail = await RootObject.Instance.AvatarLibraryManager.GetThumbnailAsync(_characterModelUrl);
				_errorDisplay.SetActive(thumbnail == null);
				DisplayedThumbnail = thumbnail;
				_waitSpinner.SetActive(false);
			}
		}

		public void ThumbnailSelected()
		{
			Debug.LogTrace($"Thumbnail with model url {_characterModelUrl} clicked.");
			CharacterModelSelected?.Invoke(_characterModelUrl);
		}

		public void Delete()
		{
			RootObject.Instance.AvatarLibraryManager.RemoveAvatar(_characterModelUrl);
		}
	}
}