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
        private Sprite _ownedSprite;
        private Texture2D _ownedTexture;

        public delegate void ThumbnailEventHandler(string elementId);
        public event ThumbnailEventHandler ElementSelected;

        public event ThumbnailEventHandler ElementIdChanged;

        public event ThumbnailEventHandler ElementDeleted;

        /// <summary>
        /// The displayed thumbnail. The view owns it and destroys it when it is replaced or the view is destroyed.
        /// </summary>
        public Texture2D DisplayedThumbnail
        {
            get => _ownedTexture;
            private set
            {
                ReleaseThumbnail(value);
                if (value != null)
                {
                    _ownedTexture = value;
                    _ownedSprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), new Vector2(0.5f, 1f));
                    _thumbnailImage.sprite = _ownedSprite;
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
            if (this == null)
            {
                return;
            }

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
                        var requestedElementId = _elementId;
                        var ct = destroyCancellationToken;
                        thumbnail = await ThumbnailProvider.GetThumbnailAsync(requestedElementId, ct);
                        if (ct.IsCancellationRequested || this == null || requestedElementId != _elementId)
                        {
                            // outdated result: the view was destroyed or shows another element by now
                            if (thumbnail != null)
                            {
                                Destroy(thumbnail);
                            }

                            return;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (MissingReferenceException)
                    {
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Thumbnail provider is not set up. Ensure that it is set before trying to load an element.", this);
                }

                if (this == null)
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

        private void OnDestroy()
        {
            ReleaseThumbnail(null);
        }

        private void ReleaseThumbnail(Texture2D replacement)
        {
            if (_ownedSprite != null)
            {
                if (_thumbnailImage != null && _thumbnailImage.sprite == _ownedSprite)
                {
                    _thumbnailImage.sprite = null;
                }

                Destroy(_ownedSprite);
            }

            if (_ownedTexture != null && _ownedTexture != replacement)
            {
                Destroy(_ownedTexture);
            }

            _ownedSprite = null;
            _ownedTexture = null;
        }
    }
}