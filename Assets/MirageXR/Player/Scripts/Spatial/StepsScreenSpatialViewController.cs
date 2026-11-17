using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel.DTOs;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MirageXR
{
    public class StepsScreenSpatialViewController : ScreenViewController<StepsScreenSpatialViewController, StepsScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.StepsScreen;

        private ActivityStep _step;
        private List<Content> _contents;
        private readonly List<ContetItemView> _contentsItemViews = new();
        private readonly List<StepsMediaListItemView> _mediaListItemViews = new();
        private readonly List<StepsToolsListItemView> _toolsListItemViews = new();
        private readonly List<DetectMediaListItemView> _detectMediaListItemViews = new();
        private Toggle _toggleAllUsers;
        private GameObject _toggleAllUsersContainer;
        private GameObject _runtimeFullImageOverlay;
        private RawImage _runtimeFullImageDisplay;
        private GameObject _runtimeLoadingSpinner;
        private CancellationTokenSource _fullImageCts;
        private Dictionary<string, GameObject> hyperlinkPrefabs = new(); 
        private bool descriptionContainsLinks = false;
        private Dictionary<string, Vector3> hyperlinkPositions = new();

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonCloseClick(OnButtonCloseClicked);
            View.SetActionOnButtonAddAugmentationClick(OnButtonAddAugmentationClicked);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetActionOnButtonMediaAddNewFileClick(OnButtonMediaAddNewFileClick);
            View.SetActionOnButtonToolsAddNewToolClick(OnButtonToolsAddNewToolClick);
            View.SetActionOnButtonNextStepClick(OnButtonNextStepClicked);
            View.SetActionOnButtonPreviousStepClick(OnButtonPreviousStepClicked);
            View.SetActionOnTitleInputEndEdit(OnTitleInputEndEdit);
            View.SetActionOnDescriptionInputEndEdit(OnDescriptionInputEndEdit);
            View.SetActionOnDescriptionInputStartEdit(OnDescriptionInputStartEdit);
            View.SetActionOnButtonConfirmHyperlinkPositionClick(OnButtonConfirmHyperlinkPositionClicked);

            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += ActivityManagerOnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnActivityClosed += OnActivityClosed;
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
            RootObject.Instance.LEE.ContentManager.OnContentUpdated += ContentManagerOnContentUpdated;
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (_toggleAllUsers != null)
            {
                _toggleAllUsers.onValueChanged.RemoveListener(OnToggleAllUsersValueChanged);
            }
            if (_runtimeFullImageOverlay != null)
            {
                Destroy(_runtimeFullImageOverlay);
                _runtimeFullImageOverlay = null;
            }
            _fullImageCts?.Cancel();
            _fullImageCts?.Dispose();
            _fullImageCts = null;

            if (RootObject.Instance?.LEE != null)
            {
                if (RootObject.Instance.LEE.ActivityManager != null)
                {
                    RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
                    RootObject.Instance.LEE.ActivityManager.OnActivityUpdated -= ActivityManagerOnActivityUpdated;
                    RootObject.Instance.LEE.ActivityManager.OnActivityClosed -= OnActivityClosed;
                }
                if (RootObject.Instance.LEE.StepManager != null)
                {
                    RootObject.Instance.LEE.StepManager.OnStepChanged -= StepManagerOnStepChanged;
                }
                if (RootObject.Instance.LEE.ContentManager != null)
                {
                    RootObject.Instance.LEE.ContentManager.OnContentActivated -= ContentManagerOnContentActivated;
                    RootObject.Instance.LEE.ContentManager.OnContentUpdated -= ContentManagerOnContentUpdated;
                }
            }
        }

        private void ContentManagerOnContentUpdated(List<Content> list)
        {
            foreach (var item in _contentsItemViews)
            {
                foreach (var content in list)
                {
                    if (item.ContentID == content.Id)
                    {
                        item.Initialize(content, OnStepItemClick, OnStepItemDeleteClick);
                    }
                }
            }
        }

        private void OnTitleInputEndEdit(string text)
        {
            RootObject.Instance.LEE.StepManager.SetStepName(_step.Id, text);
        }

        private void OnDescriptionInputEndEdit(string displayText)
        {
            var fullText = CombineTextWithPositions(displayText);
            RootObject.Instance.LEE.StepManager.SetStepDescription(_step.Id, fullText);
            View.SetHyperlinkDialogActive(descriptionContainsLinks);
        }

        private void OnDescriptionInputStartEdit(string displayText)
        {
        }

        private string CombineTextWithPositions(string displayText)
        {
            var result = displayText;
            var pattern = @"<color=[^>]+>\[([^\[\]]+)\]</color>";
            var matches = Regex.Matches(displayText, pattern);
            var links = new Dictionary<string, string>(); 

            foreach (Match match in matches)
            {
                var linkId = match.Groups[1].Value;
                links[linkId] = match.Groups[0].Value; 
            }

            foreach (var linkId in links.Keys)
            {
                if (hyperlinkPositions.TryGetValue(linkId, out var position))
                {
                    var posTag = $"<pos={position.x:F2},{position.y:F2},{position.z:F2}>";
                    result = Regex.Replace(result, $@"<color=[^>]+>\[{Regex.Escape(linkId)}\]</color>", $"{links[linkId]}{posTag}", (RegexOptions)1);
                }
            }
            return result;
        }
        
        private void ActivityManagerOnActivityUpdated(Activity activity)
        {
            if (_step != null)
            {
                UpdateInfoViewAsync().Forget();
            }

            if (!RootObject.Instance.LEE.ActivityManager.IsEditorMode)
            {
                View.SwitchToInfoTab();
            }
        }

        private void OnButtonMediaAddNewFileClick()
        {
            var prefab = MenuManager.Instance.GetImageSelectPopupViewPrefab();
            PopupsViewer.Instance.Show(prefab, (Action<Texture2D>)OnMediaFileSelected);
        }

        private void OnButtonToolsAddNewToolClick()
        {
            
        }
        
        private void OnButtonPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        }

        private void OnButtonNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnMediaFileSelected(Texture2D texture)
        {
            SaveMediaFileAsync(texture).Forget();
        }

        private async UniTask SaveMediaFileAsync(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            var file = new FileModel
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                FileHash = string.Empty
            };

            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            await RootObject.Instance.LEE.MediaManager.SaveToMediaFileAsync(texture, activityId, file.Id);
            RootObject.Instance.LEE.StepManager.AddAttachment(_step.Id, file);
            RootObject.Instance.LEE.AssetsManager.UploadMediaFileAsync(activityId, file.Id).Forget();
        }

        private void OnToggleEditorValueChanged(bool value)
        {
            RootObject.Instance.LEE.ActivityManager.IsEditorMode = !value;
        }

        private void OnEditorModeChanged(bool value)
        {
            if (value)
            {
                View.SwitchToStepsTab();
            }
            else
            {
                View.SwitchToInfoTab();
            }

            View.SetPanelAddNewStepActive(value);
            View.RemoveActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetIsToggleEditorOn(!value);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetTitleInputInteractable(value);
            View.SetDescriptionInputInteractable(value);

            foreach (var itemView in _contentsItemViews)
            {
                itemView.Interactable = value;
            }

            foreach (var itemView in _mediaListItemViews)
            {
                itemView.Interactable = value;
            }
            View.SetActiveContainerMediaAddNewFile(value);

            foreach (var itemView in _toolsListItemViews)
            {
                itemView.Interactable = value;
            }
            View.SetActiveContainerToolsAddNewToolClick(value);
        }

        private void OnButtonConfirmHyperlinkPositionClicked()
        {
            SaveHyperlinkPosition();
            View.SetHyperlinkDialogActive(false);
        }

        private void ContentManagerOnContentActivated(List<Content> contents)
        {
            _contents = contents;
            UpdateAugmentationsView();
        }

        private void StepManagerOnStepChanged(ActivityStep step)
        {
            _step = step;
            UpdateInfoViewAsync().Forget();
        }

        private async UniTask UpdateInfoViewAsync()
        {
            View.SetTitleInputText(_step.Name);
            var data = HyperlinkPositionData.SplitPositionsFromText(_step.Description);
            View.SetDescriptionInputText(AddColorToBrackets(data.DisplayText));
            hyperlinkPositions = data.Positions; 
            descriptionContainsLinks = Regex.IsMatch(data.DisplayText, @"\[([^\[\]]+)\]");
            UpdateHyperlinkPrefabs(); 
            await UpdateInfoMediaViewAsync();
            UpdateInfoToolsView();
            FindObjectOfType<InfoScreenSpatialView>()?.UpdateSplines(); // not happy with this
        }

        private void UpdateHyperlinkPrefabs()   //TODO: move to class
        {
            if (View == null)
            {
                return;
            }

            var inputField = View.GetDescriptionInputField();
            if (inputField == null)
            {
                return;
            }

            var inputText = inputField.text;
            if (string.IsNullOrEmpty(inputText))
            {
                foreach (var kvp in hyperlinkPrefabs)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                }
                hyperlinkPrefabs.Clear();
                hyperlinkPositions.Clear();
                descriptionContainsLinks = false;
                return;
            }
        
            var cleanText = Regex.Replace(inputText, @"<color=[^>]+>(.*?)</color>", "$1");
            var pattern = @"\[(.*?)\]";
            //var matches = Regex.Matches(cleanText, pattern);
            var newLinks = new HashSet<string>();
            var validLinksWithPositions = new List<(string link, int startIndex, int length)>();

            var openBrackets = 0;
            var startIndex = -1;
            string currentLink = null;

            for (var i = 0; i < cleanText.Length; i++)
            {
                if (cleanText[i] == '[')
                {
                    if (openBrackets == 0)
                    {
                        startIndex = i;
                        currentLink = "";
                    }
                    openBrackets++;
                }
                else if (cleanText[i] == ']')
                {
                    openBrackets--;
                    if (openBrackets == 0 && startIndex != -1)
                    {
                        currentLink = cleanText.Substring(startIndex + 1, i - startIndex - 1);
                        if (!string.IsNullOrEmpty(currentLink) && !currentLink.Contains("[") && !currentLink.Contains("]"))
                        {
                            newLinks.Add(currentLink);
                            validLinksWithPositions.Add((currentLink, startIndex, i - startIndex + 1));
                        }
                    }
                    else if (openBrackets < 0)
                    {
                        openBrackets = 0;
                    }
                }
            }

            descriptionContainsLinks = newLinks.Count > 0;
        
            var linksToRemove = new List<string>();
            foreach (var link in hyperlinkPrefabs.Keys)
            {
                if (!newLinks.Contains(link))
                {
                    var prefabToRemove = hyperlinkPrefabs[link];
                    if (prefabToRemove != null)
                    {
                        Destroy(prefabToRemove);
                    }
                    linksToRemove.Add(link);
                }
            }
            foreach (var link in linksToRemove)
            {
                hyperlinkPrefabs.Remove(link);
                if (hyperlinkPositions.ContainsKey(link))
                {
                    hyperlinkPositions.Remove(link);
                }
            }
        
            foreach (var content in newLinks)
            {
                if (!hyperlinkPrefabs.ContainsKey(content))
                {
                    Vector3 linkPosition;
                    if (hyperlinkPositions.TryGetValue(content, out var savedPosition))
                    {
                        linkPosition = savedPosition;
                    }
                    else
                    {
                        linkPosition = gameObject.transform.position;
                        var randomOffsetX = Random.Range(-0.2f, 0.2f);
                        var randomOffsetY = Random.Range(-0.2f, 0.2f);
                        var randomOffsetZ = Random.Range(-0.2f, 0.2f);
                        linkPosition += new Vector3(randomOffsetX, randomOffsetY, randomOffsetZ) + Vector3.up / 2f;
                        hyperlinkPositions[content] = linkPosition;
                    }

                    var hyperlinkInstance = View.CreateHyperlinkPrefab(linkPosition, content);
                    hyperlinkPrefabs[content] = hyperlinkInstance;
                }
                else
                {
                    if (hyperlinkPositions.TryGetValue(content, out var position) && hyperlinkPrefabs[content] != null)
                    {
                        hyperlinkPrefabs[content].transform.position = position;
                    }
                }
            }
        }
        private string AddColorToBrackets(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                descriptionContainsLinks = false;
                return inputText;
            }
            
            var cleanText = Regex.Replace(inputText, @"<color=[^>]+>(.*?)</color>", "$1");
            
            var pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(cleanText, pattern);
            var newLinks = new HashSet<string>();
            var validLinksWithPositions = new List<(string link, int startIndex, int length)>();
            
            var openBrackets = 0;
            var startIndex = -1;
            string currentLink = null;

            for (int i = 0; i < cleanText.Length; i++)
            {
                if (cleanText[i] == '[')
                {
                    if (openBrackets == 0)
                    {
                        startIndex = i;
                        currentLink = "";
                    }
                    openBrackets++;
                }
                else if (cleanText[i] == ']')
                {
                    openBrackets--;
                    if (openBrackets == 0 && startIndex != -1)
                    {
                        currentLink = cleanText.Substring(startIndex + 1, i - startIndex - 1);
                        if (!string.IsNullOrEmpty(currentLink) && !currentLink.Contains("[") && !currentLink.Contains("]"))
                        {
                            newLinks.Add(currentLink);
                            validLinksWithPositions.Add((currentLink, startIndex, i - startIndex + 1));
                        }
                    }
                    else if (openBrackets < 0)
                    {
                        openBrackets = 0;
                    }
                }
            }

            descriptionContainsLinks = newLinks.Count > 0;
            
            var linksToRemove = new List<string>();
            foreach (var link in hyperlinkPrefabs.Keys)
            {
                if (!newLinks.Contains(link))
                {
                    var prefabToRemove = hyperlinkPrefabs[link];
                    if (prefabToRemove != null)
                    {
                        Destroy(prefabToRemove);
                    }
                    linksToRemove.Add(link);
                }
            }
            foreach (var link in linksToRemove)
            {
                hyperlinkPrefabs.Remove(link);
                if (hyperlinkPositions.ContainsKey(link))
                {
                    hyperlinkPositions.Remove(link);
                }
            }
            
            foreach (var content in newLinks)
            {
                if (!hyperlinkPrefabs.ContainsKey(content))
                {
                    Vector3 linkPosition;
                    if (hyperlinkPositions.TryGetValue(content, out var savedPosition))
                    {
                        linkPosition = savedPosition;
                    }
                    else
                    {
                        linkPosition = gameObject.transform.position;
                        var randomOffsetX = Random.Range(-0.2f, 0.2f);
                        var randomOffsetY = Random.Range(-0.2f, 0.2f);
                        var randomOffsetZ = Random.Range(-0.2f, 0.2f);
                        linkPosition += new Vector3(randomOffsetX, randomOffsetY, randomOffsetZ) + Vector3.up / 2f;
                        hyperlinkPositions[content] = linkPosition;
                    }
            
                    var hyperlinkInstance = View.CreateHyperlinkPrefab(linkPosition, content);
                    hyperlinkPrefabs[content] = hyperlinkInstance;
                }
            }
            
            var result = cleanText;
            foreach (var linkData in validLinksWithPositions.OrderByDescending(x => x.startIndex))
            {
                var link = linkData.link;
                var start = linkData.startIndex;
                var length = linkData.length;
                result = result.Substring(0, start) + $"<color=#8F9CFF>[{link}]</color>" + result.Substring(start + length);
            }
             
            return result;
        }

        private async UniTask UpdateInfoMediaViewAsync()
        {
            var isEditorMode = RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            var container = View.GetContainerMedia();
            var prefab = View.GetStepsMediaListItemViewPrefab();

            foreach (var item in _mediaListItemViews)
            {
                Destroy(item.gameObject);
            }
            _mediaListItemViews.Clear();

            if (_step?.Attachment != null)
            {
                foreach (var file in _step.Attachment)
                {
                    var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                    var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, file.Id);

                    if (texture != null)
                    {
                        var item = Instantiate(prefab, container);
                        item.Initialize(file, texture, _step.Id, (stepId, fileModel) =>
                        {
                            RootObject.Instance.LEE.StepManager.RemoveAttachment(stepId, fileModel.Id);
                        });
                        item.Interactable = isEditorMode;
                        _mediaListItemViews.Add(item);
                    }
                }
            }

            foreach (var item in _detectMediaListItemViews)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _detectMediaListItemViews.Clear();

            EnsureToggleAllUsers();

            bool isManagerOrAdmin = CheckIsManagerOrAdmin();
            if (_toggleAllUsersContainer != null)
            {
                _toggleAllUsersContainer.SetActive(isManagerOrAdmin);
            }
            else if (_toggleAllUsers != null)
            {
                _toggleAllUsers.gameObject.SetActive(isManagerOrAdmin);
            }

            if (_toggleAllUsers != null && !isManagerOrAdmin)
            {
                _toggleAllUsers.SetIsOnWithoutNotify(false);
                var switchToggle = _toggleAllUsers.GetComponent<SwitchToggle>();
                if (switchToggle != null) switchToggle.ForceUpdateView();
            }

            LoadDetectImagesAsync(_step).Forget();
        }

        private void EnsureToggleAllUsers()
        {
            if (_toggleAllUsers != null) return;

            if (View != null && View.GetToggleAllUsers() != null)
            {
                _toggleAllUsers = View.GetToggleAllUsers();
                _toggleAllUsersContainer = View.GetToggleAllUsersContainer();
                _toggleAllUsers.onValueChanged.AddListener(OnToggleAllUsersValueChanged);
                return;
            }

            var container = View?.GetContainerMedia();
            if (container == null) return;

            var barObj = new GameObject("ToggleAllUsersContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            barObj.transform.SetParent(container, false);
            barObj.transform.SetAsFirstSibling();

            var rect = barObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 36);

            var hlg = barObj.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 4, 4);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var toggleObj = new GameObject("ToggleAllUsers", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(barObj.transform, false);
            var toggleRect = toggleObj.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(24, 24);

            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(toggleObj.transform, false);
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkObj.transform.SetParent(bgObj.transform, false);
            var checkImg = checkObj.GetComponent<Image>();
            checkImg.color = new Color(0.6f, 0.5f, 0.95f, 1f);
            var checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.sizeDelta = Vector2.zero;

            var toggle = toggleObj.GetComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;

            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            labelObj.transform.SetParent(barObj.transform, false);
            var labelTmp = labelObj.GetComponent<TMPro.TextMeshProUGUI>();
            labelTmp.text = "All users";
            labelTmp.fontSize = 14;
            labelTmp.color = Color.white;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(100, 24);

            _toggleAllUsers = toggle;
            _toggleAllUsersContainer = barObj;
            _toggleAllUsers.onValueChanged.AddListener(OnToggleAllUsersValueChanged);
        }

        private async UniTask LoadDetectImagesAsync(ActivityStep step)
        {
            foreach (var item in _detectMediaListItemViews)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _detectMediaListItemViews.Clear();

            if (step == null) return;

            var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
            if (aiManager == null) return;

            var activityManager = RootObject.Instance?.LEE?.ActivityManager;
            if (activityManager == null) return;

            string activityId = activityManager.ActivityId.ToString();
            string stepId = step.Id.ToString();

            var auth = RootObject.Instance?.LEE?.AuthorizationManager;
            string targetUserId = (_toggleAllUsers != null && _toggleAllUsers.isOn) ? null : auth?.UserName;

            var images = await aiManager.GetConversationImagesAsync(
                activityId: activityId,
                stepId: stepId,
                userId: targetUserId);

            var container = View?.GetContainerMedia();
            var prefab = View?.GetDetectMediaListItemViewPrefab();

            if (images != null && container != null && prefab != null)
            {
                bool isManagerOrAdmin = CheckIsManagerOrAdmin();
                string currentUserName = auth?.UserName;

                foreach (var imgData in images)
                {
                    bool isOwner = !string.IsNullOrEmpty(currentUserName) &&
                                   string.Equals(currentUserName, imgData.UserId, StringComparison.OrdinalIgnoreCase);
                    bool canDelete = isOwner || isManagerOrAdmin;

                    var item = Instantiate(prefab, container);
                    item.Initialize(imgData, null, ShowFullImagePreview, OnDeleteImageClicked, canDelete);
                    _detectMediaListItemViews.Add(item);
                }
            }
        }

        private void OnDeleteImageClicked(ConversationImageMetadata image, DetectMediaListItemView itemView)
        {
            if (image == null) return;

            Action deleteAction = () =>
            {
                DeleteImageAsync(image, itemView).Forget();
            };

            if (DialogWindow.Instance != null)
            {
                DialogWindow.Instance.Show(
                    "Delete Image",
                    "Are you sure you want to delete this capture?",
                    new DialogButtonContent("Delete", deleteAction, true),
                    new DialogButtonContent("Cancel", () => { }));
            }
            else if (MenuManager.Instance?.Dialog != null)
            {
                MenuManager.Instance.Dialog.ShowMiddle(
                    "Delete Image",
                    "Are you sure you want to delete this capture?",
                    "Delete",
                    deleteAction);
            }
            else
            {
                deleteAction.Invoke();
            }
        }

        private async UniTask DeleteImageAsync(ConversationImageMetadata image, DetectMediaListItemView itemView)
        {
            var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
            if (aiManager == null) return;

            bool success = await aiManager.DeleteConversationImageAsync(image.Id);
            if (success)
            {
                if (itemView != null)
                {
                    _detectMediaListItemViews.Remove(itemView);
                    Destroy(itemView.gameObject);
                }
            }
            else
            {
                var dialog = MenuManager.Instance?.Dialog;
                if (dialog != null)
                {
                    dialog.ShowMiddle("Delete Failed", "Failed to delete the capture from server.", "OK", () => { });
                }
            }
        }

        private void EnsureFullImageOverlay(out RawImage display, out Button closeBtn, out GameObject spinner)
        {
            if (View != null && View.GetFullImageOverlay() != null)
            {
                display = View.GetFullImageDisplay();
                closeBtn = View.GetButtonCloseFullImage();
                spinner = View.GetFullImageLoadingSpinner();
                return;
            }

            if (_runtimeFullImageOverlay == null)
            {
                var canvas = View?.GetComponentInParent<Canvas>();
                Transform parentTransform = canvas != null ? canvas.transform : View?.transform;

                var overlayObj = new GameObject("FullImageOverlay", typeof(RectTransform), typeof(Image));
                if (parentTransform != null)
                {
                    overlayObj.transform.SetParent(parentTransform, false);
                }
                overlayObj.transform.SetAsLastSibling();

                var overlayRect = overlayObj.GetComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero;

                var overlayImg = overlayObj.GetComponent<Image>();
                overlayImg.color = new Color(0f, 0f, 0f, 0.92f);

                var imgObj = new GameObject("FullImageDisplay", typeof(RectTransform), typeof(RawImage));
                imgObj.transform.SetParent(overlayObj.transform, false);
                var imgRect = imgObj.GetComponent<RectTransform>();
                imgRect.anchorMin = new Vector2(0.05f, 0.05f);
                imgRect.anchorMax = new Vector2(0.95f, 0.95f);
                imgRect.sizeDelta = Vector2.zero;
                _runtimeFullImageDisplay = imgObj.GetComponent<RawImage>();

                var btnObj = new GameObject("ButtonClose", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(overlayObj.transform, false);
                var btnRect = btnObj.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(1, 1);
                btnRect.anchorMax = new Vector2(1, 1);
                btnRect.anchoredPosition = new Vector2(-40, -40);
                btnRect.sizeDelta = new Vector2(50, 50);

                var btnImg = btnObj.GetComponent<Image>();
                btnImg.color = new Color(0.3f, 0.3f, 0.35f, 0.9f);

                var closeTextObj = new GameObject("X", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                closeTextObj.transform.SetParent(btnObj.transform, false);
                var closeTmp = closeTextObj.GetComponent<TMPro.TextMeshProUGUI>();
                closeTmp.text = "✕";
                closeTmp.alignment = TMPro.TextAlignmentOptions.Center;
                closeTmp.fontSize = 28;
                closeTmp.color = Color.white;
                var closeTextRect = closeTextObj.GetComponent<RectTransform>();
                closeTextRect.anchorMin = Vector2.zero;
                closeTextRect.anchorMax = Vector2.one;
                closeTextRect.sizeDelta = Vector2.zero;

                var btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(CloseFullImagePreview);

                _runtimeFullImageOverlay = overlayObj;
            }

            _runtimeFullImageOverlay.SetActive(false);
            display = _runtimeFullImageDisplay;
            closeBtn = null;
            spinner = _runtimeLoadingSpinner;
        }

        public void ShowFullImagePreview(ConversationImageMetadata image)
        {
            if (image == null) return;

            EnsureFullImageOverlay(out var display, out var closeBtn, out var spinner);
            var overlay = View?.GetFullImageOverlay() ?? _runtimeFullImageOverlay;
            if (overlay == null || display == null) return;

            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();

            _fullImageCts?.Cancel();
            _fullImageCts = new CancellationTokenSource();

            LoadFullImageAsync(image, display, spinner, _fullImageCts.Token).Forget();
        }

        private async UniTask LoadFullImageAsync(ConversationImageMetadata image, RawImage display, GameObject spinner, CancellationToken ct)
        {
            if (spinner != null) spinner.SetActive(true);

            var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
            if (aiManager != null)
            {
                var fullTex = await aiManager.GetConversationImageTextureAsync(image.Id, isThumbnail: false);
                if (!ct.IsCancellationRequested && fullTex != null)
                {
                    if (display != null)
                    {
                        display.texture = fullTex;
                    }
                }
            }

            if (spinner != null) spinner.SetActive(false);
        }

        public void CloseFullImagePreview()
        {
            _fullImageCts?.Cancel();
            var overlay = View?.GetFullImageOverlay() ?? _runtimeFullImageOverlay;
            if (overlay != null)
            {
                overlay.SetActive(false);
            }
            var display = View?.GetFullImageDisplay() ?? _runtimeFullImageDisplay;
            if (display != null)
            {
                display.texture = null;
            }
        }

        private bool CheckIsManagerOrAdmin()
        {
            var auth = RootObject.Instance?.LEE?.AuthorizationManager;
            if (auth == null) return false;

            if (auth.HasScopePrivilege("*", "admin")) return true;
            if (auth.UserPrivileges != null && auth.UserPrivileges.Any(p =>
                string.Equals(p, "admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p, "manager", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p, "managers", StringComparison.OrdinalIgnoreCase))) return true;

            var activity = RootObject.Instance?.LEE?.ActivityManager?.Activity;
            string scope = string.IsNullOrWhiteSpace(activity?.CustomerScope) ? "wekit" : activity.CustomerScope;
            return auth.HasScopePrivilege(scope, "admin") ||
                   auth.HasScopePrivilege(scope, "managers") ||
                   auth.HasScopePrivilege(scope, "manager");
        }

        private void OnToggleAllUsersValueChanged(bool isOn)
        {
            LoadDetectImagesAsync(_step).Forget();
        }

        private void UpdateInfoToolsView()
        {
            var isEditorMode = RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            var container = View.GetContainerTools();
            var prefab = View.GetStepsToolsListItemViewPrefab();

            foreach (var item in _toolsListItemViews)
            {
                Destroy(item.gameObject);
            }
            _toolsListItemViews.Clear();

            if (_step?.RequiredToolsPartsMaterials != null)
            {
                foreach (var tool in _step.RequiredToolsPartsMaterials)
                {
                    var item = Instantiate(prefab, container);
                    item.Initialize(tool, _step.Id, (stepId, requiredToolsPartsMaterials) =>
                    {
                        RootObject.Instance.LEE.StepManager.RemoveToolPartMaterial(stepId, requiredToolsPartsMaterials.Id);
                    });
                    item.Interactable = isEditorMode;
                    _toolsListItemViews.Add(item);
                }
            }
        }

        private void UpdateAugmentationsView()
        {
            var container = View.GetStepContainer();
            var prefab = View.GetContetItemViewPrefab();

            foreach (var item in _contentsItemViews)
            {
                Destroy(item.gameObject);
            }
            _contentsItemViews.Clear();

            foreach (var content in _contents)
            {
                var item = Instantiate(prefab, container);
                item.Initialize(content, OnStepItemClick, OnStepItemDeleteClick);
                _contentsItemViews.Add(item);
            }
        }

        private void OnStepItemDeleteClick(Content content)
        {
            RootObject.Instance.LEE.ContentManager.RemoveContent(content.Id);
        }

        private void OnStepItemClick(Content content)
        {
            var prefab = MenuManager.Instance.GetEditorPrefab(content.Type);
            PopupsViewer.Instance.Show(prefab, GetWorldPosition(), content);
        }

        private Vector3 GetWorldPosition()
        {
            var rectTransform = (RectTransform)transform;
            var worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            return (worldCorners[0] + worldCorners[2]) / 2f;
        }

        private void OnButtonAddAugmentationClicked()
        {
            MenuManager.Instance.ShowSelectAugmentationScreenSpatialView();
        }

        private void OnButtonBackClicked()
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
        }

        private void OnButtonCloseClicked()
        {
            LearningExperienceEngine.EventManager.ClearAll();
            RootObject.Instance.RoomTwinManager.SetRoomTwinVisibility(false);
            RootObject.Instance.LEE.ActivityManager.CloseActivity();
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }

        private void OnActivityClosed()
        {
            foreach (var kvp in hyperlinkPrefabs)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            hyperlinkPrefabs.Clear();
            hyperlinkPositions.Clear();
            descriptionContainsLinks = false;

            foreach (var item in _mediaListItemViews)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _mediaListItemViews.Clear();

            foreach (var item in _toolsListItemViews)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _toolsListItemViews.Clear();

            foreach (var item in _contentsItemViews)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _contentsItemViews.Clear();

            _step = null;
            _contents = null;

            if (View != null)
            {
                View.SetTitleInputText(string.Empty);
                View.SetDescriptionInputText(string.Empty);
                View.SetHyperlinkDialogActive(false);
            }
        }

        private void SaveHyperlinkPosition()
        {
            foreach (var kvp in hyperlinkPrefabs)
            {
                var linkId = kvp.Key;
                var prefab = kvp.Value;
                if (prefab != null)
                {
                    var newPosition = prefab.transform.position;
                    hyperlinkPositions[linkId] = newPosition;
                }
            }
            var displayText = View.GetDescriptionInputField().text;
            View.GetDescriptionInputField().text = displayText; 
            var fullText = CombineTextWithPositions(displayText);
            RootObject.Instance.LEE.StepManager.SetStepDescription(_step.Id, fullText);
        }
    }
}
