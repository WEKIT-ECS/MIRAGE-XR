using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DTOs;
using LearningExperienceEngine.NewDataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListItem_v2 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        [SerializeField] private TMP_Text _txtLabel;
        [SerializeField] private TMP_Text _txtDeadline;
        [SerializeField] private TMP_Text _txtAuthor;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _defaultThumbnail;
        [SerializeField] private Button _btnMain;
        [SerializeField] private Button _btnDelete;
        [SerializeField] private Sprite _deleteIcon;

        private const float SWIPE_LIMIT = 150f;
        private const float SWIPE_DURATION = 0.2f;

        private bool _interactable = true;
        private Activity _activity;
        private UnityAction<Activity> _onClick;
        private UnityAction<Activity> _onDelete;

        private RectTransform _contentTransform;
        private ScrollRect _scrollRect;
        private bool _isSwiping;
        private bool _isOpen;
        private bool _dragStarted;
        private Coroutine _snapCoroutine;

        private static ActivityListItem_v2 _currentOpenItem;

        public string activityName => _activity.Name;

        public string activityAuthor => _activity.Creator.Name;

        public bool userIsAuthor => false; //TODO: _activity.Creator.Id == RootObject.Instance.LEE.AuthorizationManager.UserId;

        public bool userIsEnroled => false;//_container.hasDeadline;

        public Button BtnMain => _btnMain;

        private void Awake()
        {
            if (_btnMain != null)
            {
                _contentTransform = _btnMain.GetComponent<RectTransform>();
            }
            _scrollRect = GetComponentInParent<ScrollRect>();
            SetupDeleteButton();
        }

        private void OnDisable()
        {
            ResetSwipe(immediate: true);
        }

        private void OnDestroy()
        {
            if (_currentOpenItem == this)
            {
                _currentOpenItem = null;
            }
        }

        private void SetupDeleteButton()
        {
            if (_deleteIcon == null)
            {
#if UNITY_EDITOR
                _deleteIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MirageXR/Design/UI_Elements/SVG_Icons/delete.svg");
#endif
                if (_deleteIcon == null)
                {
                    var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                    foreach (var s in allSprites)
                    {
                        if (s != null && (s.name == "delete" || s.name == "Icon_delete"))
                        {
                            _deleteIcon = s;
                            break;
                        }
                    }
                }
            }

            if (_btnDelete == null)
            {
                var existingDelete = transform.Find("DeleteButton");
                if (existingDelete != null)
                {
                    _btnDelete = existingDelete.GetComponent<Button>();
                }
            }

            if (_btnDelete == null)
            {
                CreateDeleteButton();
            }
            else
            {
                _btnDelete.onClick.RemoveAllListeners();
                _btnDelete.onClick.AddListener(OnTrashButtonClicked);
            }
        }

        private void CreateDeleteButton()
        {
            var deleteObj = new GameObject("DeleteButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            deleteObj.transform.SetParent(transform, false);
            deleteObj.transform.SetSiblingIndex(0);

            var rect = deleteObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-20f, 0f);
            rect.sizeDelta = new Vector2(SWIPE_LIMIT, 0f);

            var img = deleteObj.GetComponent<Image>();
            if (_btnMain != null && _btnMain.TryGetComponent<Image>(out var mainImg))
            {
                img.sprite = mainImg.sprite;
                img.type = mainImg.type;
            }
            img.color = new Color(0.92f, 0.26f, 0.21f, 1f);

            _btnDelete = deleteObj.GetComponent<Button>();
            var colors = _btnDelete.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            _btnDelete.colors = colors;
            _btnDelete.onClick.AddListener(OnTrashButtonClicked);

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(deleteObj.transform, false);

            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(50f, 50f);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = _deleteIcon;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
        }

        public void Init(Activity activity, UnityAction<Activity> onClick, UnityAction<Activity> onDelete)
        {
            _onClick = onClick;
            _onDelete = onDelete;
            _activity = activity;
            _btnMain.onClick.RemoveAllListeners();
            _btnMain.onClick.AddListener(OnBtnMain);

            ResetSwipe(immediate: true);
            UpdateView();
        }

        private void UpdateView()
        {
            _txtLabel.text = _activity.Name;
            //_txtDeadline.text = _container.deadline;
            _txtAuthor.text = _activity.Creator?.Name;
            LoadThumbnailAsync().Forget();
        }

        private async UniTaskVoid LoadThumbnailAsync()
        {
            if (_activity == null || string.IsNullOrEmpty(_activity.ThumbnailLink))
            {
                if (_image != null)
                {
                    _image.sprite = _defaultThumbnail;
                }
                return;
            }

            if (TryToGetGuids(_activity.ThumbnailLink, out var activityId, out var fileId))
            {
                var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, fileId);
                if (texture == null)
                {
                    await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(activityId, fileId);
                    texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, fileId);
                }

                if (this == null)
                {
                    return;
                }

                if (texture != null && _image != null)
                {
                    _image.sprite = Utilities.TextureToSprite(texture);
                    return;
                }
            }

            if (_image != null)
            {
                _image.sprite = _defaultThumbnail;
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

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponentInParent<ScrollRect>();
            }
            _scrollRect?.OnInitializePotentialDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponentInParent<ScrollRect>();
            }

            Vector2 totalDelta = eventData.position - eventData.pressPosition;
            float deltaX = Mathf.Abs(totalDelta.x);
            float deltaY = Mathf.Abs(totalDelta.y);

            if (deltaX > deltaY)
            {
                _isSwiping = true;
                _dragStarted = true;
                if (_currentOpenItem != null && _currentOpenItem != this)
                {
                    _currentOpenItem.ResetSwipe();
                }
            }
            else
            {
                _isSwiping = false;
                if (_isOpen)
                {
                    ResetSwipe();
                }
                _scrollRect?.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isSwiping)
            {
                if (_contentTransform == null)
                {
                    return;
                }

                float newX = _contentTransform.anchoredPosition.x + eventData.delta.x;
                newX = Mathf.Clamp(newX, -SWIPE_LIMIT, 0f);
                _contentTransform.anchoredPosition = new Vector2(newX, _contentTransform.anchoredPosition.y);
            }
            else
            {
                _scrollRect?.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isSwiping)
            {
                _isSwiping = false;
                if (_contentTransform == null)
                {
                    return;
                }

                bool snapOpen;
                if (eventData.delta.x < -3f)
                {
                    snapOpen = true;
                }
                else if (eventData.delta.x > 3f)
                {
                    snapOpen = false;
                }
                else
                {
                    snapOpen = _contentTransform.anchoredPosition.x < -SWIPE_LIMIT * 0.4f;
                }

                if (snapOpen)
                {
                    SnapTo(-SWIPE_LIMIT);
                    _isOpen = true;
                    _currentOpenItem = this;
                }
                else
                {
                    SnapTo(0f);
                    _isOpen = false;
                    if (_currentOpenItem == this)
                    {
                        _currentOpenItem = null;
                    }
                }

                StartCoroutine(ClearDragStarted());
            }
            else
            {
                _scrollRect?.OnEndDrag(eventData);
            }
        }

        private IEnumerator ClearDragStarted()
        {
            yield return null;
            _dragStarted = false;
        }

        public void ResetSwipe(bool immediate = false)
        {
            _isOpen = false;
            if (_currentOpenItem == this)
            {
                _currentOpenItem = null;
            }
            SnapTo(0f, immediate);
        }

        private void SnapTo(float targetX, bool immediate = false)
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
                _snapCoroutine = null;
            }

            if (_contentTransform == null)
            {
                return;
            }

            if (immediate || !gameObject.activeInHierarchy)
            {
                _contentTransform.anchoredPosition = new Vector2(targetX, _contentTransform.anchoredPosition.y);
                return;
            }

            _snapCoroutine = StartCoroutine(AnimateSnap(targetX));
        }

        private IEnumerator AnimateSnap(float targetX)
        {
            float startX = _contentTransform.anchoredPosition.x;
            float elapsed = 0f;

            while (elapsed < SWIPE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / SWIPE_DURATION);
                t = Mathf.SmoothStep(0f, 1f, t);
                float currentX = Mathf.Lerp(startX, targetX, t);
                _contentTransform.anchoredPosition = new Vector2(currentX, _contentTransform.anchoredPosition.y);
                yield return null;
            }

            _contentTransform.anchoredPosition = new Vector2(targetX, _contentTransform.anchoredPosition.y);
            _snapCoroutine = null;
        }

        private void OnTrashButtonClicked()
        {
            var title = _activity?.Name ?? string.Empty;
            var message = $"Do you really want to delete activity '{title}'?";

            if (RootView_v2.Instance?.dialog != null)
            {
                RootView_v2.Instance.dialog.ShowMiddle(
                    "Delete Activity",
                    message,
                    "Yes",
                    () =>
                    {
                        ResetSwipe(immediate: true);
                        OnBtnDelete();
                    },
                    "No",
                    () =>
                    {
                        ResetSwipe();
                    });
            }
            else if (DialogWindow.Instance != null)
            {
                DialogWindow.Instance.Show(
                    "Delete Activity",
                    message,
                    new DialogButtonContent("Yes", () =>
                    {
                        ResetSwipe(immediate: true);
                        OnBtnDelete();
                    }),
                    new DialogButtonContent("No", () =>
                    {
                        ResetSwipe();
                    }));
            }
            else if (MenuManager.Instance?.Dialog != null)
            {
                MenuManager.Instance.Dialog.ShowMiddle(
                    "Delete Activity",
                    message,
                    "Yes",
                    () =>
                    {
                        ResetSwipe(immediate: true);
                        OnBtnDelete();
                    },
                    "No",
                    () =>
                    {
                        ResetSwipe();
                    });
            }
            else
            {
                ResetSwipe(immediate: true);
                OnBtnDelete();
            }
        }

        private void OnBtnDelete()
        {
            if (_onDelete != null)
            {
                _onDelete.Invoke(_activity);
            }
            else
            {
                DeleteFromServer();
            }
        }

        private void DeleteFromServer()
        {
            DeleteFromServerAsync().Forget();
        }

        private async UniTask DeleteFromServerAsync()
        {
            await RootObject.Instance.LEE.ActivityManager.DeleteActivityAsync(_activity.Id);
            RootView.Instance.activityListView.UpdateListView();
        }

        private void OnBtnMain()
        {
            if (_dragStarted)
            {
                _dragStarted = false;
                return;
            }

            if (_isOpen)
            {
                ResetSwipe();
                return;
            }

            if (_currentOpenItem != null)
            {
                _currentOpenItem.ResetSwipe();
                return;
            }

            _onClick?.Invoke(_activity);
        }

        /*private void ShowPopup()
        {
            RootView_v2.Instance.dialog.ShowMiddleMultiline("Open Activity", true,
                ("Open to edit", () => OpenActivity(true), false),
                ("Open to view", () => OpenActivity(false), false),
                ("Cancel", null, true));
        }*/
/*
        public async void OpenActivity(bool value)
        {
            await PlayActivityAsync();
            RootView_v2.Instance.activityView.SetSessionInfo(_container);
            activityManager.EditModeActive = value;
            if (!value)
            {
                RootView_v2.Instance.activityView.stepsListView.SetCalibrationToggle(true);
            }
        }
*/
/*
        private async Task PlayActivityAsync()
        {
            LoadView.Instance.Show();
            var activityJsonFileName = LearningExperienceEngine.LocalFiles.GetActivityJsonFilename(_container.FileIdentifier);
            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.UpdateViewsOfActivity(_container.ItemID, _container.ExistsRemotely);
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity(activityJsonFileName);
            LoadView.Instance.Hide();
        }
*/
/*
        private async Task DownloadActivityAsync()
        {
            _container.HasError = false;
            _container.IsDownloading = true;
            UpdateView();

            LoadView.Instance.Show();
            var (result, activity) = await LearningExperienceEngine.MoodleManager.DownloadActivity(_container.Session);
            LoadView.Instance.Hide();

            _container.HasError = !result;
            _container.Activity = activity;
            _container.IsDownloading = false;
            string message = result ? "<color=#00000000>space</color>Downloaded.<color=#00000000>space</color>" : "Something went wrong.";
            Toast.Instance.Show(message, result);
            UpdateView();
        }*/
    }
}
