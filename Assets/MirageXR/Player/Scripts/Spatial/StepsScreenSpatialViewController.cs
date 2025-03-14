using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
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
        private Dictionary<string, GameObject> hyperlinkPrefabs = new(); 
        private bool descriptionContainsLinks = false;
        private Dictionary<string, Vector3> hyperlinkPositions = new();

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
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
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
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
                Version = Application.version,
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
        }
        private void UpdateHyperlinkPrefabs()
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
            PopupsViewer.Instance.Show(prefab, content);
        }

        private void OnButtonAddAugmentationClicked()
        {
            MenuManager.Instance.ShowSelectAugmentationScreenSpatialView();
        }

        private void OnButtonBackClicked()
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
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
