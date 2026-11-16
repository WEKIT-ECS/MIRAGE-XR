using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel.DTOs;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class DetectMediaListItemView : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImageThumbnail;
        [SerializeField] private Button _btnThumbnail;
        [SerializeField] private Button _btnDelete;
        [SerializeField] private TMP_Text _txtSubstep;
        [SerializeField] private TMP_Text _txtInterpretation;
        [SerializeField] private TMP_Text _txtAuthor;
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private LayoutElement _layoutElement;

        private ConversationImageMetadata _data;
        private Texture2D _thumbnailTexture;

        public void Initialize(
            ConversationImageMetadata data, 
            Texture2D cachedThumbnail, 
            Action<ConversationImageMetadata> onThumbnailClicked,
            Action<ConversationImageMetadata, DetectMediaListItemView> onDeleteClicked = null,
            bool canDelete = false)
        {
            _data = data;
            _thumbnailTexture = cachedThumbnail;

            if (_txtSubstep != null)
            {
                _txtSubstep.text = _data.CurrentSubstep.HasValue 
                    ? $"Substep {_data.CurrentSubstep.Value}" 
                    : "Verification Capture";
            }

            if (_txtInterpretation != null)
            {
                _txtInterpretation.text = !string.IsNullOrEmpty(_data.Interpretation) 
                    ? _data.Interpretation 
                    : "No interpretation available.";
            }

            if (_txtAuthor != null)
            {
                if (!string.IsNullOrEmpty(_data.UserId))
                {
                    _txtAuthor.text = $"@{_data.UserId}";
                    _txtAuthor.gameObject.SetActive(true);
                }
                else
                {
                    _txtAuthor.gameObject.SetActive(false);
                }
            }

            if (_btnThumbnail != null)
            {
                _btnThumbnail.onClick.RemoveAllListeners();
                _btnThumbnail.onClick.AddListener(() => onThumbnailClicked?.Invoke(_data));
            }

            if (_btnDelete != null)
            {
                _btnDelete.onClick.RemoveAllListeners();
                if (canDelete && onDeleteClicked != null)
                {
                    _btnDelete.gameObject.SetActive(true);
                    _btnDelete.onClick.AddListener(() => onDeleteClicked.Invoke(_data, this));
                }
                else
                {
                    _btnDelete.gameObject.SetActive(false);
                }
            }

            if (_thumbnailTexture != null)
            {
                ApplyThumbnailTexture(_thumbnailTexture);
            }
            else
            {
                LoadThumbnailAsync().Forget();
            }
        }

        private async UniTask LoadThumbnailAsync()
        {
            if (_loadingSpinner != null) _loadingSpinner.SetActive(true);

            var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
            if (aiManager != null && _data != null && !string.IsNullOrEmpty(_data.Id))
            {
                _thumbnailTexture = await aiManager.GetConversationImageTextureAsync(_data.Id, isThumbnail: true);
                if (_thumbnailTexture != null)
                {
                    ApplyThumbnailTexture(_thumbnailTexture);
                }
            }

            if (_loadingSpinner != null) _loadingSpinner.SetActive(false);
        }

        private void ApplyThumbnailTexture(Texture2D texture)
        {
            if (_rawImageThumbnail != null)
            {
                _rawImageThumbnail.texture = texture;
                _rawImageThumbnail.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (_btnThumbnail != null)
            {
                _btnThumbnail.onClick.RemoveAllListeners();
            }
            if (_btnDelete != null)
            {
                _btnDelete.onClick.RemoveAllListeners();
            }
        }
    }
}
