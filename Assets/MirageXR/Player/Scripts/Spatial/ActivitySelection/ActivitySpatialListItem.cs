using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LearningExperienceEngine.NewDataModel;
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
        [SerializeField] private Transform tagsContainer;
        [SerializeField] private Button tagChipPrefab;

        private Activity _activity;
        private bool _isSelected;
        private UnityAction<Activity> _onItemClicked;
        private UnityAction<Activity> _onItemDeleteClicked;
        private UnityAction<string> _onTagClicked;
        private ButtonLongPress _deleteButtonLongPress;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked, UnityAction<Activity> onItemDeleteClicked, bool isSelected, UnityAction<string> onTagClicked = null)
        {
            _isSelected = isSelected;
            _activity = activity;
            _onItemClicked = onItemClicked;
            _onItemDeleteClicked = onItemDeleteClicked;
            _onTagClicked = onTagClicked;
            button.onClick.AddListener(OnItemClicked);

            bool canDelete = _activity.EffectivePermissions == null || _activity.EffectivePermissions.Delete;
            buttonDelete.gameObject.SetActive(canDelete);
            if (canDelete)
            {
                buttonDelete.onClick.AddListener(OnItemDeleteClicked);
                buttonDeleteConfirmation.onClick.AddListener(DeleteActivity);
                buttonDeleteCancel.onClick.AddListener(HideDeleteConfirmation);
                deleteConfirmation.SetActive(false);
                InitializeDeleteLongPress();
            }
            else
            {
                deleteConfirmation.SetActive(false);
            }

            UpdateView();
            UpdateTagsView();
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

        private void UpdateTagsView()
        {
            if (tagsContainer == null)
            {
                var existingContainer = transform.Find("TagsContainer");
                if (existingContainer != null)
                {
                    tagsContainer = existingContainer;
                }
                else
                {
                    var go = new GameObject("TagsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                    go.transform.SetParent(transform, false);
                    var hlg = go.GetComponent<HorizontalLayoutGroup>();
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = false;
                    hlg.spacing = 8f;
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    rt.anchoredPosition = new Vector2(150f, 15f);
                    rt.sizeDelta = new Vector2(-160f, 30f);
                    tagsContainer = go.transform;
                }
            }

            foreach (Transform child in tagsContainer)
            {
                Destroy(child.gameObject);
            }

            if (_activity?.Tags == null || _activity.Tags.Count == 0)
            {
                return;
            }

            var tagsToShow = _activity.Tags.Take(4);
            foreach (var tag in tagsToShow)
            {
                var chipObj = new GameObject($"TagChip_{tag}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                chipObj.transform.SetParent(tagsContainer, false);
                var img = chipObj.GetComponent<Image>();
                img.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);
                var btn = chipObj.GetComponent<Button>();
                var currentTag = tag;
                btn.onClick.AddListener(() => _onTagClicked?.Invoke(currentTag));

                var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(chipObj.transform, false);
                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.text = tag;
                tmp.fontSize = 14;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                var textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                var chipRt = chipObj.GetComponent<RectTransform>();
                chipRt.sizeDelta = new Vector2(Mathf.Max(60f, tag.Length * 10f + 20f), 26f);
            }
        }

        private void SetDefaultThumbnailVisible(bool visible)
        {
            if (containerThumbnail != null)
            {
                var defaultImg = containerThumbnail.Find("Image_Default");
                if (defaultImg != null)
                {
                    defaultImg.gameObject.SetActive(visible);
                }
            }
        }

        private async UniTask UpdateThumbnailViewAsync()
        {
            if (_activity is { ThumbnailLink: not null } && !string.IsNullOrEmpty(_activity.ThumbnailLink))
            {
                if (TryToGetGuids(_activity.ThumbnailLink, out var activityId, out var fileId))
                {
                    var texture2D = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, fileId);
                    if (texture2D == null)
                    {
                        await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(activityId, fileId);
                        texture2D = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, fileId);
                    }

                    if (texture2D != null)
                    {
                        SetDefaultThumbnailVisible(false);
#if VISION_OS  //TODO: temp
                        var obj = imageThumbnail.gameObject;
                        if (imageThumbnail != null)
                        {
                            DestroyImmediate(imageThumbnail);
                        }
                        var image = obj.GetComponent<Image>();
                        if (image == null)
                        {
                            image = obj.AddComponent<Image>();
                        }

                        image.gameObject.SetActive(true);
                        var sprite = Utilities.TextureToSprite(texture2D);
                        image.sprite = sprite;
                        await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
                        var size = LearningExperienceEngine.Utilities.FitRectToRect(containerThumbnail.rect.size, new Vector2(texture2D.width, texture2D.height));
                        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
#else
                        if (imageThumbnail != null)
                        {
                            imageThumbnail.gameObject.SetActive(true);
                            imageThumbnail.texture = texture2D;
                            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
                            var size = LearningExperienceEngine.Utilities.FitRectToRect(containerThumbnail.rect.size, new Vector2(texture2D.width, texture2D.height));
                            imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                            imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                        }
#endif
                        return;
                    }
                }
            }

            SetDefaultThumbnailVisible(true);
            if (imageThumbnail != null)
            {
                imageThumbnail.gameObject.SetActive(false);
            }
        }

        private static bool TryToGetGuids(string url, out Guid activityId, out Guid fileId)
        {
            activityId = Guid.Empty;
            fileId = Guid.Empty;

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            try
            {
                var matches = Regex.Matches(url, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                if (matches.Count >= 2)
                {
                    if (Guid.TryParse(matches[0].Value, out activityId) && Guid.TryParse(matches[1].Value, out fileId))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            return false;
        }
    }
}
