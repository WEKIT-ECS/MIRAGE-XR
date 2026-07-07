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
        [SerializeField] private GameObject deleteConfirmation;
        [SerializeField] private Button buttonDeleteConfirmation;
        [SerializeField] private Button buttonDeleteCancel;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color defaultColor;

        private Activity _activity;
        private bool _isSelected;
        private UnityAction<Activity> _onItemClicked;
        private UnityAction<Activity> _onItemDeleteClicked;
        private ButtonLongPress _deleteButtonLongPress;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked, UnityAction<Activity> onItemDeleteClicked, bool isSelected)
        {
            _isSelected = isSelected;
            _activity = activity;
            _onItemClicked = onItemClicked;
            _onItemDeleteClicked = onItemDeleteClicked;
            button.onClick.AddListener(OnItemClicked);
            buttonDelete.onClick.AddListener(OnItemDeleteClicked);
            buttonDeleteConfirmation.onClick.AddListener(DeleteActivity);
            buttonDeleteCancel.onClick.AddListener(HideDeleteConfirmation);
            deleteConfirmation.SetActive(false);
            InitializeDeleteLongPress();

            UpdateView();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnItemClicked);
            buttonDelete.onClick.RemoveListener(OnItemDeleteClicked);
            buttonDeleteConfirmation.onClick.RemoveListener(DeleteActivity);
            buttonDeleteCancel.onClick.RemoveListener(HideDeleteConfirmation);
            if (_deleteButtonLongPress == null)
            {
                return;
            }

            _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
        }

        private void OnItemClicked()
        {
            _onItemClicked?.Invoke(_activity);
        }

        private void OnItemDeleteClicked()
        {
            _deleteButtonLongPress?.ConsumeLongPress();
        }

        private void InitializeDeleteLongPress()
        {
            _deleteButtonLongPress = buttonDelete.GetComponent<ButtonLongPress>();
            if (_deleteButtonLongPress == null)
            {
                _deleteButtonLongPress = buttonDelete.gameObject.AddComponent<ButtonLongPress>();
            }

            _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
            _deleteButtonLongPress.onHoldProgressChanged.AddListener(OnDeleteHoldProgressChanged);
        }

        private void OnDeleteHoldProgressChanged(float progress)
        {
            if (_deleteButtonLongPress == null)
            {
                return;
            }

            if (deleteConfirmation.activeSelf)
            {
                backgroundImage.color = GetDeleteHoldColor();
                return;
            }

            if (progress >= 1f)
            {
                ShowDeleteConfirmation();
                return;
            }

            backgroundImage.color = Color.Lerp(defaultColor, GetDeleteHoldColor(), progress);
        }

        private void ShowDeleteConfirmation()
        {
            deleteConfirmation.SetActive(true);
            backgroundImage.color = GetDeleteHoldColor();
        }

        private void HideDeleteConfirmation()
        {
            deleteConfirmation.SetActive(false);
            backgroundImage.color = defaultColor;
        }

        private Color GetDeleteHoldColor()
        {
            var color = _deleteButtonLongPress != null ? _deleteButtonLongPress.holdColor : Color.red;
            color.a = defaultColor.a;
            return color;
        }

        private void DeleteActivity()
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
