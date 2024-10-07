using System.IO;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class ImageContentView : ContentView
    {
        private const string ImageFileName = "image.png";

        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private Texture2D _texture;
        private Sprite _sprite;
        private Camera _camera;
        private bool _isBillboarded;
        
        public override async UniTask InitializeAsync(Content content)
        {
            base.InitializeAsync(content);

            _camera = RootObject.Instance.BaseCamera;
            
            if (content is Content<ImageContentData> imageContent)
            {
                await InitializeContentAsync(imageContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        private async UniTask InitializeContentAsync(Content<ImageContentData> content)
        {
            var folderPath = RootObject.Instance.AssetsManager.GetFolderPath(content.Id, content.ContentData.Image.Id);
            var imagePath = Path.Combine(folderPath, ImageFileName);

            if (!File.Exists(imagePath))
            {
                Debug.LogError($"Image file {imagePath} does not exist");
            }

            var bytes = await File.ReadAllBytesAsync(imagePath);
            _texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            _texture.LoadImage(bytes);

            _sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.zero);

            spriteRenderer.sprite = _sprite;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = Vector2.one;

            _isBillboarded = content.ContentData.IsBillboarded;

            CalculateSize(_texture.width, _texture.height);
        }

        private void CalculateSize(int textureWidth, int textureHeight)
        {
            if (textureWidth == textureHeight)
            {
                transform.localScale = Vector3.one;
                return;
            } 

            var scale = transform.localScale;
            transform.localScale = textureWidth > textureHeight
                ? new Vector3(textureWidth / (float)textureHeight, scale.y, scale.z)
                : new Vector3(scale.x, textureHeight / (float)textureWidth, scale.z);
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