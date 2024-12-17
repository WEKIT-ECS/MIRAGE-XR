using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepsMediaListItemView : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform imageContainer;
        [SerializeField] private Button buttonDelete;

        private FileModel _fileModel;
        private Texture2D _texture2D;
        private Sprite _sprite;
        private Guid _stepId;

        
        public bool Interactable
        {
            get => buttonDelete.gameObject.activeSelf;
            set => buttonDelete.gameObject.SetActive(value);
        }

        public void Initialize(FileModel fileModel, Texture2D texture2D, Guid stepId, UnityAction<Guid, FileModel> onDeleteClick)
        {
            _stepId = stepId;
            _fileModel = fileModel;
            _texture2D = texture2D;
            buttonDelete.onClick.AddListener(() => onDeleteClick(_stepId, _fileModel));
            UpdateImageAsync().Forget();
        }

        private void OnEnable()
        {
            if (layoutElement.preferredWidth == 0)
            {
                UpdateImageAsync().Forget();
            }
        }

#if VISION_OS  //TODO: temp
        private Image _image;
        private async UniTask UpdateImageAsync()
        {
            if (_image == null)
            {
                var obj = rawImage.gameObject;
                DestroyImmediate(rawImage);
                _image = obj.AddComponent<Image>();
            }

            _image.gameObject.SetActive(true);
            var sprite = Utilities.TextureToSprite(_texture2D);
            _image.sprite = sprite;
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            var size = LearningExperienceEngine.Utilities.FitRectByWidth(imageContainer.rect.width, new Vector2(_texture2D.width, _texture2D.height));
            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
#else
        private async UniTask UpdateImageAsync()
        {
            rawImage.texture = _texture2D;
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            var size = LearningExperienceEngine.Utilities.FitRectByWidth(imageContainer.rect.width, new Vector2(_texture2D.width, _texture2D.height));
            rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
#endif
        }

        private void OnDestroy()
        {
            Destroy(_texture2D);
            //Destroy(_sprite);
        }
    }
}