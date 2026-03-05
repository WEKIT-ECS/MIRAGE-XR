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
        [SerializeField] private Button _deleteButton;

        private string _characterModelId;

        public delegate void CharacterModelEventHandler(string characterModelId);
        public event CharacterModelEventHandler CharacterModelSelected;

        public event CharacterModelEventHandler CharacterModelIdChanged;

        public event CharacterModelEventHandler CharacterModelDeleted;

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

        public bool Deleteable { get; set; } = false;

        public async void UpdateView()
        {
            _deleteButton.gameObject.SetActive(Deleteable);
            DisplayedThumbnail = null;
            if (!string.IsNullOrWhiteSpace(_characterModelId))
            {
                _waitSpinner.SetActive(true);
                Texture2D thumbnail = await RootObject.Instance.AvatarLoadManager.GetThumbnailAsync(_characterModelId);
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
            if (Deleteable)
            {
                CharacterModelDeleted?.Invoke(_characterModelId);
            }
        }
    }
}