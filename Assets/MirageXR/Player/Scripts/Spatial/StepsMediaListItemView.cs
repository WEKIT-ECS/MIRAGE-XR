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

        private async UniTask UpdateImageAsync()
        {
            rawImage.texture = _texture2D;
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            var size = LearningExperienceEngine.Utilities.FitRectByWidth(imageContainer.rect.width, new Vector2(_texture2D.width, _texture2D.height));
            rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
        }

        private void OnDestroy()
        {
            Destroy(_texture2D);
            //Destroy(_sprite);
        }
    }
}