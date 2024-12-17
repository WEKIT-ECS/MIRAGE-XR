using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivitySpatialListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text textLabel;
        [SerializeField] private TMP_Text deadline;
        [SerializeField] private TMP_Text author;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private RawImage imageThumbnail;
        [SerializeField] private RectTransform containerThumbnail;

        private Activity _activity;
        private UnityAction<Activity> _onItemClicked;
        private UnityAction<Activity> _onItemDeleteClicked;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked, UnityAction<Activity> onItemDeleteClicked)
        {
            _activity = activity;
            _onItemClicked = onItemClicked;
            _onItemDeleteClicked = onItemDeleteClicked;
            button.onClick.AddListener(OnItemClicked);
            buttonDelete.onClick.AddListener(OnItemDeleteClicked);

            UpdateView();
        }

        private void OnItemClicked()
        {
            _onItemClicked?.Invoke(_activity);
        }

        private void OnItemDeleteClicked()
        {
            _onItemDeleteClicked?.Invoke(_activity);
        }

        private void UpdateView()
        {
            if (_activity == null)
            {
                return;
            }

            gameObject.name = _activity.Name;
            textLabel.text = _activity.Name;
            author.text = _activity.Creator?.Name;

            UpdateThumbnailViewAsync().Forget();
        }

        private async UniTask UpdateThumbnailViewAsync()
        {
            if (_activity is { Thumbnail: not null } && _activity.Thumbnail.Id != Guid.Empty)
            {
                await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(_activity.Id, _activity.Thumbnail.Id);
                var texture2D = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, _activity.Thumbnail.Id);
                if (texture2D != null)
                {
#if VISION_OS  //TODO: temp
                    var obj = imageThumbnail.gameObject;
                    DestroyImmediate(imageThumbnail);
                    var image = obj.AddComponent<Image>();

                    image.gameObject.SetActive(true);
                    var sprite = Utilities.TextureToSprite(texture2D);
                    image.sprite = sprite;
                    await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
                    var size = LearningExperienceEngine.Utilities.FitRectToRect(containerThumbnail.rect.size, new Vector2(texture2D.width, texture2D.height));
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
#else
                    imageThumbnail.gameObject.SetActive(true);
                    imageThumbnail.texture = texture2D;
                    await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
                    var size = LearningExperienceEngine.Utilities.FitRectToRect(containerThumbnail.rect.size, new Vector2(texture2D.width, texture2D.height));
                    imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                    imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
#endif
                }
            }
        }
    }
}
