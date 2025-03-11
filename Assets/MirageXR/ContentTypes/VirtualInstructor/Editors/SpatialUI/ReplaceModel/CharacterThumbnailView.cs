using ReadyPlayerMe.Core;
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

		private string _modelUrl;

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

		public string ModelUrl
		{
			get => _modelUrl;
			set
			{
				if (_modelUrl != value)
				{
					_modelUrl = value;
					UpdateView();
				}
			}
		}

		public async void UpdateView()
		{
			_waitSpinner.SetActive(true);
			DisplayedThumbnail = null;
			Texture2D thumbnail = await RootObject.Instance.AvatarLibraryManager.GetThumbnailAsync(_modelUrl);
			DisplayedThumbnail = thumbnail;
			_waitSpinner.SetActive(false);
		}
	}
}