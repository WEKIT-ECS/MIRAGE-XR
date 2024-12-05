using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ImageSelectPopupView : PopupBase
    {
        private const int MaxPictureSize = 1024;

        [SerializeField] private RectTransform imageHolder;
        [SerializeField] private RawImage image;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonGallery;
        [SerializeField] private Button buttonCamera;
        [SerializeField] private Button buttonApply;

        private Texture2D _texture;
        private Texture2D _capturedImage;
        private Action<Texture2D> _onApply;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            buttonClose.onClick.AddListener(OnButtonCloseClicked);
            buttonGallery.onClick.AddListener(OnButtonGalleryClicked);
            buttonCamera.onClick.AddListener(OnButtonCameraClicked);
            buttonApply.onClick.AddListener(OnButtonApplyClicked);

            base.Initialization(onClose, args);

            if (_texture)
            {
                SetPreviewAsync(_texture).Forget();
            }
        }

        private void OnButtonApplyClicked()
        {
            _onApply?.Invoke(_capturedImage);
            Close();
        }

        private void OnButtonCameraClicked()
        {
            CaptureImage();
        }

        private void OnButtonGalleryClicked()
        {
            PickImage(MaxPictureSize);
        }

        private void OnButtonCloseClicked()
        {
            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }

            if (args is { Length: > 0 })
            {
                _onApply = args[0] as Action<Texture2D>;
            }

            if (args is { Length: > 1 })
            {
                _texture = args[1] as Texture2D;
            }

            return true;
        }

        private void PickImage(int maxSize)
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path == null)
                {
                    return;
                }

                var texture2D = NativeGallery.LoadImageAtPath(path, maxSize, false);

                if (texture2D == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                if (_capturedImage)
                {
                    Destroy(_capturedImage);
                }
                _capturedImage = texture2D;

                SetPreviewAsync(texture2D).Forget();
            });
        }

        private void CaptureImage()
        {
            RootObject.Instance.ImageTargetManager.enabled = false;
            NativeCameraController.TakePicture(OnPictureTaken);
        }

        private void OnPictureTaken(bool result, Texture2D texture2D)
        {
            RootObject.Instance.ImageTargetManager.enabled = true;
            if (!result)
            {
                return;
            }

            if (_capturedImage)
            {
                Destroy(_capturedImage);
            }
            _capturedImage = texture2D;

            SetPreviewAsync(texture2D).Forget();
        }
 
        private async UniTask SetPreviewAsync(Texture2D texture2D)
        {
            image.gameObject.SetActive(true);
            image.texture = texture2D;
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            var size = LearningExperienceEngine.Utilities.FitRectToRect(imageHolder.rect.size, new Vector2(texture2D.width, texture2D.height));
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        }
    }
}
