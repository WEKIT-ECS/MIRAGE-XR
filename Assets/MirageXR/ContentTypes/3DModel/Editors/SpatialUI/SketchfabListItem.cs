using System.IO;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public enum SketchfabListItemStatus
    {
        Unknown,
        Local,
        Cloud,
        Downloading,
        Error
    }

    public class SketchfabListItem : MonoBehaviour
    {
        private static ISketchfabManager sketchfabManager => RootObject.Instance.LEE.SketchfabManager;

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text size;
        [SerializeField] private Image imageThumbnail;
        [SerializeField] private Image imageLoading;
        [SerializeField] private Image imageStatus;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonEdit;
        [SerializeField] private Sprite spriteLocal;
        [SerializeField] private Sprite spriteCloud;
        [SerializeField] private Sprite spriteDownloading;
        [SerializeField] private Sprite spriteError;

        public bool Interactable
        {
            get => button.interactable;
            set
            {
                button.interactable = value;
                buttonEdit.interactable = value;
            }
        }

        private SketchfabModel _sketchfabModel;
        private Texture2D _texture;
        private Sprite _sprite;
        private UnityAction<SketchfabModel, SketchfabListItem> _onClick;
        private SketchfabListItemStatus _status = SketchfabListItemStatus.Unknown;

        public async UniTask InitializeAsync(SketchfabModel sketchfabModel, UnityAction<SketchfabModel, SketchfabListItem> onClick)
        {
            _sketchfabModel = sketchfabModel;
            _onClick = onClick;

            button.onClick.AddListener(OnClick);

            title.text = _sketchfabModel.Name;
            size.text = (_sketchfabModel.Archives.Gltf.Size / 1024f / 1024f).ToString("F") + " MB";

            SetStatus(sketchfabManager.IsModelCached(_sketchfabModel.Uid) ? SketchfabListItemStatus.Local : SketchfabListItemStatus.Cloud);

            await LoadThumbnailAsync();
        }

        private void OnClick()
        {
            _onClick?.Invoke(_sketchfabModel, this);
        }

        private async UniTask LoadThumbnailAsync()
        {
            imageThumbnail.gameObject.SetActive(false);
            imageLoading.gameObject.SetActive(true);

            var cancellationToken = gameObject.GetCancellationTokenOnDestroy();

            var thumbnailPath = RootObject.Instance.LEE.AssetsManager.GetModelThumbnailPath(_sketchfabModel.Uid);
            if (!File.Exists(thumbnailPath))
            {
                var response = await sketchfabManager.DownloadThumbnailAsync(_sketchfabModel, cancellationToken: cancellationToken);
                if (!response.IsSuccess)
                {
                    return;
                }
            }

            _texture = Utilities.LoadTexture(thumbnailPath);
            if (_texture == null)
            {
                return;    
            }

            _sprite = Utilities.TextureToSprite(_texture);
            imageThumbnail.sprite = _sprite;

            imageThumbnail.gameObject.SetActive(true);
            imageLoading.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(_sprite);
            Destroy(_texture);
        }

        public void SetStatus(SketchfabListItemStatus status)
        {
            Sprite sprite;
            _status = status;

            switch (status)
            {
                case SketchfabListItemStatus.Local:
                    sprite = spriteLocal;
                    break;
                case SketchfabListItemStatus.Cloud:
                    sprite = spriteCloud;
                    break;
                case SketchfabListItemStatus.Downloading:
                    sprite = spriteDownloading;
                    break;
                case SketchfabListItemStatus.Unknown:
                case SketchfabListItemStatus.Error:
                default:
                    sprite = spriteError;
                    break;
            }

            imageStatus.sprite = sprite;
        }
    }
}
