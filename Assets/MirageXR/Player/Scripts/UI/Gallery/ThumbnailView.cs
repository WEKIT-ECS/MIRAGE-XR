using Cysharp.Threading.Tasks;
using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ThumbnailView : MonoBehaviour
    {
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private GameObject _waitSpinner;
        [SerializeField] private GameObject _errorDisplay;
        [SerializeField] private Button _deleteButton;

        private string _elementId;

        public delegate void ThumbnailEventHandler(string elementId);
        public event ThumbnailEventHandler ElementSelected;

        public event ThumbnailEventHandler ElementIdChanged;

        public event ThumbnailEventHandler ElementDeleted;

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

        public IThumbnailProvider ThumbnailProvider
        {
            get; set;
        }

        public string ElementId
        {
            get => _elementId;
            set
            {
                if (_elementId != value)
                {
                    _elementId = value;
                    ElementIdChanged?.Invoke(value);
                    UpdateView();
                }
            }
        }

        public bool Deleteable { get; set; } = false;

        public async void UpdateView()
        {
            _deleteButton.gameObject.SetActive(Deleteable);
            DisplayedThumbnail = null;
            if (!string.IsNullOrWhiteSpace(_elementId))
            {
                _waitSpinner.SetActive(true);
                Texture2D thumbnail = null;
                if (ThumbnailProvider != null)
                {
                    try
                    {
                        thumbnail = await ThumbnailProvider.GetThumbnailAsync(_elementId, destroyCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Thumbnail provider is not set up. Ensure that it is set before trying to load an element.", this);
                }
                if (destroyCancellationToken.IsCancellationRequested)
                {
                    return;
                }
                _errorDisplay.SetActive(thumbnail == null);
                DisplayedThumbnail = thumbnail;
                _waitSpinner.SetActive(false);
            }
        }

        public void ThumbnailSelected()
        {
            Debug.LogTrace($"Thumbnail of element with Id {_elementId} selected.");
            ElementSelected?.Invoke(_elementId);
        }

        public void Delete()
        {
            if (Deleteable)
            {
                ElementDeleted?.Invoke(_elementId);
            }
        }
    }
}