using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Activity = LearningExperienceEngine.DTOs.Activity;

namespace MirageXR
{
    public class ActivitySpatialListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text textLabel;
        [SerializeField] private TMP_Text deadline;
        [SerializeField] private TMP_Text author;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RawImage imageThumbnail;
        [SerializeField] private RectTransform containerThumbnail;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color defaultColor;

        private Activity _activity;
        private bool _isSelected;
        private UnityAction<Activity> _onItemClicked;
        private UnityAction<Activity> _onItemDeleteClicked;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked, UnityAction<Activity> onItemDeleteClicked, bool isSelected)
        {
            _isSelected = isSelected;
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

            backgroundImage.color = _isSelected ? selectedColor : defaultColor;
            gameObject.name = _activity.Name;
            textLabel.text = _activity.Name;
            author.text = _activity.Creator?.Name;

            UpdateThumbnailViewAsync().Forget();
        }

        private async UniTask UpdateThumbnailViewAsync()
        {
            if (_activity is { ThumbnailLink: not null } && !string.IsNullOrEmpty(_activity.ThumbnailLink) &&
                TryToGetGuids(_activity.ThumbnailLink, out var activityId, out var fileId))
            {
                await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(activityId, fileId);
                var texture2D = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, fileId);
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

        private static bool TryToGetGuids(string url, out Guid activityId, out Guid fileId) //temp
        {
            const string pattern = @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})";

            var match = Regex.Match(url, pattern);

            if (match.Success)
            {
                try
                {
                    activityId = Guid.Parse(match.Groups[1].Value);
                    fileId = Guid.Parse(match.Groups[2].Value);
                    return true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            activityId = Guid.Empty;
            fileId = Guid.Empty;
            return false;
        }
    }
}
