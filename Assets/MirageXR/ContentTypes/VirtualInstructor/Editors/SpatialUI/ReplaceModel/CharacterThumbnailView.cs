using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class CharacterThumbnailView : MonoBehaviour
	{
		[SerializeField] private Image _thumbnailImage;

		public Texture2D DisplayedThumbnail
		{
			get => _thumbnailImage.sprite.texture;
			set
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
	}
}
