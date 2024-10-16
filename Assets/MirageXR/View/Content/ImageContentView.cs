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
        private const string ImageFileName = "image.png";
        private const float ScaleZ = 0.05f;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text text;
        [SerializeField] private BoxCollider boxCollider;

        private Texture2D _texture;
        private Sprite _sprite;
        private Camera _camera;
        private bool _isBillboarded;

        public override async UniTask InitializeAsync(Content content)
        {
            base.InitializeAsync(content);

            _camera = RootObject.Instance.BaseCamera;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, ScaleZ);

            if (content is Content<ImageContentData> imageContent)
            {
                await InitializeContentAsync(imageContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        protected override void InitializeBoundsControl()
        {
            base.InitializeBoundsControl();

            BoundsControl.BoundsOverride = boxCollider;
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;
        }

        protected override void OnScaleStopped()
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, ScaleZ);
            base.OnScaleStopped();
        }

        private async UniTask InitializeContentAsync(Content<ImageContentData> content)
        {
            await InitializeImageAsync(content);
            InitializeText(content);
            InitializeBillboard(content);
        }

        private async UniTask InitializeImageAsync(Content<ImageContentData> content)
        {
            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            var folderPath = RootObject.Instance.LEE.AssetsManager.GetFolderPath(activityId, content.Id, content.ContentData.Image.Id);
            var imagePath = Path.Combine(folderPath, ImageFileName);

            if (File.Exists(imagePath))
            {
                var bytes = await File.ReadAllBytesAsync(imagePath);
                _texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                _texture.LoadImage(bytes);

                _sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));

                spriteRenderer.sprite = _sprite;
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = Vector2.one;
                //CalculateSize(_texture.width, _texture.height);
            }
            else
            {
                Debug.LogError($"Image file {imagePath} does not exist");
            }
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