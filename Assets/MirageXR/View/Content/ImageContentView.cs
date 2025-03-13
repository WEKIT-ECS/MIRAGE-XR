using System;
using System.IO;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;

namespace MirageXR.View
{
    public class ImageContentView : ContentView
    {
        private const string ImageFileName = "image.jpg";
        private const float ScaleZ = 0.05f;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text text;

        private Texture2D _texture;
        private Sprite _sprite;
        private Camera _camera;
        private bool _isBillboarded;

        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);

            _camera = RootObject.Instance.BaseCamera;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, ScaleZ);

            if (content is Content<ImageContentData> imageContent)
            {
                Initialized = await InitializeContentAsync(imageContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        protected override void InitializeBoxCollider()
        {
            base.InitializeBoxCollider();

            //BoundsControl.BoundsOverride = BoxCollider;
            BoxCollider.center = Vector3.zero;
            BoxCollider.size = Vector3.one;
        }

        /*protected override void OnScaleStopped()
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, ScaleZ);
            base.OnScaleStopped();
        }*/

        protected override async UniTask OnContentUpdatedAsync(Content content)
        {
            if (content is not Content<ImageContentData> newImageContent || Content is not Content<ImageContentData> oldImageContent)
            {
                return;
            }

            if (newImageContent.ContentData.Image.Id != oldImageContent.ContentData.Image.Id)
            {
                Initialized = false;
                Initialized = await InitializeImageAsync(newImageContent);
            }

            InitializeText(newImageContent);
            InitializeBillboard(newImageContent);
            await base.OnContentUpdatedAsync(content);
        }

        private async UniTask<bool> InitializeContentAsync(Content<ImageContentData> content)
        {
            var result = await InitializeImageAsync(content);
            InitializeText(content);
            InitializeBillboard(content);

            return result;
        }

        private async UniTask<bool> InitializeImageAsync(Content<ImageContentData> content)
        {
            var activityId = RootObject.Instance.ViewManager.ActivityView.ActivityId;
            var folderPath = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, content.Id, content.ContentData.Image.Id);
            var imagePath = Path.Combine(folderPath, ImageFileName);

           if (!File.Exists(imagePath))
            {
                var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
                var result = await RootObject.Instance.LEE.AssetsManager.TryDownloadAssetUntilSuccessAsync(activityId, content.Id, content.ContentData.Image.Id, cancellationToken);

                if (!result)
                {
                    Debug.LogError($"Image file {imagePath} does not exist");
                    return false;
                }
            }

            _texture = await LearningExperienceEngine.Utilities.LoadTextureAsync(imagePath);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));

            spriteRenderer.sprite = _sprite;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = Vector2.one;
            //CalculateSize(_texture.width, _texture.height);
            return true;
        }

        private void InitializeText(Content<ImageContentData> content)
        {
            if (!string.IsNullOrEmpty(content.ContentData.Text))
            {
                text.text = content.ContentData.Text;
                text.transform.localPosition = new Vector3(0, transform.localScale.y * -0.5f, 0);
            }
            else
            {
                text.gameObject.SetActive(false);
            }
        }

        private void InitializeBillboard(Content<ImageContentData> content)
        {
            _isBillboarded = content.ContentData.IsBillboarded;
        }

        private void LateUpdate()
        {
            if (_isBillboarded)
            {
                DoBillboarding();
            }
        }

        private void DoBillboarding()
        {
            var newRotation = _camera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            transform.eulerAngles = newRotation;
        }

        private void OnDestroy()
        {
            Destroy(_sprite);
            Destroy(_texture);
        }
    }
}