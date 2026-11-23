using System;
using System.Linq;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using AIModel = LearningExperienceEngine.DataModel.AIModel;

public class AddEditVirtualInstructor : EditorSpatialView   //TODO: rename to VirtualInstructorViewSpatial
{
    // Btn
    [FormerlySerializedAs("ClosePanelBtn")]
    [Header("Buttons")]
    [SerializeField] private Button closePanelBtn;
    [SerializeField] private Button settingsBtn; 
    [SerializeField] private GallerySelectionElement characterModelSelectionElement;
    [SerializeField] private Button replaceCharacterModelBtn;
    [SerializeField] private Button[] communicationSettingBtns;
    [FormerlySerializedAs("animationSettingBtn")]
    [SerializeField] private Button animationSettingBtnA;
    [SerializeField] private Button animationSettingBtnB;
    [SerializeField] private Button pathSettingBtn;
    [SerializeField] private Button promptVI;
    [SerializeField] private Button voicesVI;
    [SerializeField] private Button modelVI;
    [SerializeField] private Button languageVI;

    // Panels
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private ReplaceModel avatarModelSettingPanel; 
    [SerializeField] private GameObject communicationSettingPanel;
    [SerializeField] private GameObject animationSettingPanel;
    [SerializeField] private GameObject pathSettingPanel;
    [SerializeField] private ListItemBoxPicker[] _listItemBoxPickers;
    
    [Header("Background and Shadow resizing")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform shadow;
    [SerializeField] private RectTransform content;

    [Header("Communication Settings ")] 
    [SerializeField] private GameObject ai; 
    [SerializeField] private GameObject audioRecoding;
    [SerializeField] private GameObject noSpeech;
    
    [Header ("Path Settings")]
    [SerializeField] private TMP_Text pathSettingText;
    
    [Header ("Animation Setting")]
    [SerializeField] private TMP_Text animationSettingTextWithOutImage;
    [SerializeField] private TMP_Text animationSettingTextWithImage;
    [SerializeField] private GameObject animationWithImage;
    [SerializeField] private GameObject animationWithOutImage;
    
    [Header ("Interaction Settings")]
    [SerializeField] private Toggle interactionSettingToggleClosed;
    [SerializeField] private Toggle interactionSettingToggleOpen;
    [SerializeField] private GameObject interactionSettingOpen;
    [SerializeField] private GameObject interactionSettingClose;

    [Header("AI Settings")] 
    [SerializeField] private TMP_Text aiPrompt;
    [SerializeField] private TMP_Text aiVoice;
    [SerializeField] private TMP_Text aiModel;
    [SerializeField] private TMP_Text aiLanguage;

    [Header("Prefabs")] 
    [SerializeField] private ContextPrompt contextPromptPrefab;
    [SerializeField] private SpatialAILanguageModel languageModelPrefab;
    [SerializeField] private SpatialUiAiSpeechToTextModel speechToTextModelPrefab;
    [SerializeField] private SpatialUiAiTextToSpeechModel textToSpeechModelPrefab;

    [Header("Configuration Checkboxes")]
    [SerializeField] private Toggle _togglePersistentThread;
    [SerializeField] private Toggle _toggleStartConversation;
    [SerializeField] private Toggle _toggleFollowUser;
    [SerializeField] private Toggle _toggleEyesFollowUser;
    [SerializeField] private Toggle _toggleStandOnFloorPlane;

    private Content<InstructorContentData> _instructorContentData;

	private string _triggers = String.Empty; // todo
    private string _availableTriggers = String.Empty; // todo

    private string _animationClip = "Idle";
    private string _characterName = "Hanna";
    private bool _useReadyPlayerMe = false;
    private string _characterModelUrl = "";
    private string _pathSetting = "No Path"; // todo
    private string _prompt = "Provide a concise answer to the question.";
    private string _voiceInstruction = string.Empty;

    private AIModel _languageModel;
    private AIModel _speechToTextModel;
    private AIModel _textToSpeechModel;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {   
        base.Initialization(onClose, args);
        _instructorContentData = Content as Content<InstructorContentData>;

        settingsBtn.onClick.AddListener(OpenSettingsPanel);
        characterModelSelectionElement.GalleryElementSelectionStarted += OpenCharacterModelSettingPanel;
        replaceCharacterModelBtn.onClick.AddListener(OpenCharacterModelSettingPanel);

        characterModelSelectionElement.Thumbnail.ThumbnailProvider = RootObject.Instance.AvatarLoadManager;

        foreach (var communicationSettingBtn in communicationSettingBtns)
        {
            communicationSettingBtn.onClick.AddListener(OpenCommunicationSettingPanel);
        }

        _languageModel = RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels()?.FirstOrDefault();
        _speechToTextModel = RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels()?.FirstOrDefault();
        _textToSpeechModel = RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels()?.FirstOrDefault();

        animationSettingBtnA.onClick.AddListener(OpenAnimationSettingPanel);
        animationSettingBtnB.onClick.AddListener(OpenAnimationSettingPanel);
        pathSettingBtn.onClick.AddListener(OpenPathSettingPanel);
        promptVI.onClick.AddListener(OpenPromptPanel);
        voicesVI.onClick.AddListener(OpenTextToSpeechPanel);
        modelVI.onClick.AddListener(OpenLanguageModelPanel);
        languageVI.onClick.AddListener(OpenSpeechToTextModelPanel);
        interactionSettingToggleOpen.onValueChanged.AddListener(SetInteractionSettingsActive);
        interactionSettingToggleClosed.onValueChanged.AddListener(SetInteractionSettingsActive);
        avatarModelSettingPanel.CharacterModelSelected += OnAvatarModelSelected;

        EnsureCheckboxes();

        if (IsContentUpdate && _instructorContentData?.ContentData != null)
        {
            var data = _instructorContentData.ContentData;
            _voiceInstruction = data.VoiceInstruction ?? string.Empty; 
            UpdatePrompt(data.Prompt);
            SetAITextToSpeechModel(data.TextToSpeechModel, _voiceInstruction);
            SetAISpeechToTextModel(data.SpeechToTextModel);
            SetAILanguageModel(data.LanguageModel);
            UpdateName(data.CharacterName);
            UpdateAnimationSetting(data.AnimationClip);

            if (_togglePersistentThread != null) _togglePersistentThread.isOn = data.PersistentThread == true;
            if (_toggleStartConversation != null) _toggleStartConversation.isOn = data.StartConversation == true;
            if (_toggleFollowUser != null) _toggleFollowUser.isOn = data.FollowUser;
            if (_toggleEyesFollowUser != null) _toggleEyesFollowUser.isOn = data.EyesFollowUser;
            if (_toggleStandOnFloorPlane != null) _toggleStandOnFloorPlane.isOn = data.StandOnFloorPlane;

            if (data.UseReadyPlayerMe && !string.IsNullOrEmpty(data.CharacterModelUrl))
            {
                string avatarId = AvatarLoadUtils.GetId(data.CharacterModelUrl);
                if (!string.IsNullOrEmpty(avatarId) && !avatarId.Equals("DefaultAvatar", StringComparison.OrdinalIgnoreCase))
                {
                    if (RootObject.Instance?.AvatarLibraryManager != null && !RootObject.Instance.AvatarLibraryManager.Contains(avatarId))
                    {
                        RootObject.Instance.AvatarLibraryManager.Add(avatarId);
                    }
                }
                OnAvatarModelSelected(avatarId);
            }
        }
        else
        {
            if (_toggleStandOnFloorPlane != null) _toggleStandOnFloorPlane.isOn = true;
        }
    }

    private void EnsureCheckboxes()
    {
        if (_togglePersistentThread == null)
            _togglePersistentThread = transform.Find("SettingsPanel/Toggle_PersistentThread")?.GetComponent<Toggle>()
                ?? transform.Find("InteractionSettingOpen/Toggle_PersistentThread")?.GetComponent<Toggle>()
                ?? transform.Find("Toggle_PersistentThread")?.GetComponent<Toggle>();

        if (_toggleStartConversation == null)
            _toggleStartConversation = transform.Find("SettingsPanel/Toggle_StartConversation")?.GetComponent<Toggle>()
                ?? transform.Find("InteractionSettingOpen/Toggle_StartConversation")?.GetComponent<Toggle>()
                ?? transform.Find("Toggle_StartConversation")?.GetComponent<Toggle>();

        if (_toggleFollowUser == null)
            _toggleFollowUser = transform.Find("SettingsPanel/Toggle_FollowUser")?.GetComponent<Toggle>()
                ?? transform.Find("InteractionSettingOpen/Toggle_FollowUser")?.GetComponent<Toggle>()
                ?? transform.Find("Toggle_FollowUser")?.GetComponent<Toggle>();

        if (_toggleEyesFollowUser == null)
            _toggleEyesFollowUser = transform.Find("SettingsPanel/Toggle_EyesFollowUser")?.GetComponent<Toggle>()
                ?? transform.Find("InteractionSettingOpen/Toggle_EyesFollowUser")?.GetComponent<Toggle>()
                ?? transform.Find("Toggle_EyesFollowUser")?.GetComponent<Toggle>();

        if (_toggleStandOnFloorPlane == null)
            _toggleStandOnFloorPlane = transform.Find("SettingsPanel/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>()
                ?? transform.Find("InteractionSettingOpen/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>()
                ?? transform.Find("Toggle_StandOnFloorPlane")?.GetComponent<Toggle>();

        if (_togglePersistentThread == null || _toggleStartConversation == null || _toggleFollowUser == null || _toggleEyesFollowUser == null || _toggleStandOnFloorPlane == null)
        {
            var targetParent = (interactionSettingOpen != null ? interactionSettingOpen.transform : null)
                ?? (settingsPanel != null ? settingsPanel.transform : null)
                ?? transform;

            var containerTransform = targetParent.Find("CheckboxesContainer");
            if (containerTransform == null)
            {
                var containerGo = new GameObject("CheckboxesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                containerGo.transform.SetParent(targetParent, false);
                var vlg = containerGo.GetComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 10f;
                containerTransform = containerGo.transform;
            }

            if (_togglePersistentThread == null)
                _togglePersistentThread = CreateCheckbox(containerTransform, "Toggle_PersistentThread", "One persistent, shared conversation for all users", false);
            if (_toggleStartConversation == null)
                _toggleStartConversation = CreateCheckbox(containerTransform, "Toggle_StartConversation", "Start conversation when step is loaded", false);
            if (_toggleFollowUser == null)
                _toggleFollowUser = CreateCheckbox(containerTransform, "Toggle_FollowUser", "Walk to follow the user", false);
            if (_toggleEyesFollowUser == null)
                _toggleEyesFollowUser = CreateCheckbox(containerTransform, "Toggle_EyesFollowUser", "Seek eye contact with the user", false);
            if (_toggleStandOnFloorPlane == null)
                _toggleStandOnFloorPlane = CreateCheckbox(containerTransform, "Toggle_StandOnFloorPlane", "Jump to floor plane", true);
        }
    }

    private Toggle CreateCheckbox(Transform parent, string name, string labelText, bool defaultValue = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 32);

        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGo.transform.SetParent(go.transform, false);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.5f);
        bgRt.anchorMax = new Vector2(0, 0.5f);
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = new Vector2(14, 0);
        bgRt.sizeDelta = new Vector2(24, 24);
        var bgImg = bgGo.GetComponent<Image>();
        bgImg.color = new Color(0.22f, 0.24f, 0.28f, 1f);

        var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        checkGo.transform.SetParent(bgGo.transform, false);
        var checkRt = checkGo.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.5f, 0.5f);
        checkRt.anchorMax = new Vector2(0.5f, 0.5f);
        checkRt.pivot = new Vector2(0.5f, 0.5f);
        checkRt.anchoredPosition = Vector2.zero;
        checkRt.sizeDelta = new Vector2(16, 16);
        var checkImg = checkGo.GetComponent<Image>();
        checkImg.color = new Color(0.38f, 0.65f, 1f, 1f);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0);
        labelRt.anchorMax = new Vector2(1, 1);
        labelRt.pivot = new Vector2(0, 0.5f);
        labelRt.offsetMin = new Vector2(36, 0);
        labelRt.offsetMax = new Vector2(0, 0);
        var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
        labelTmp.text = labelText;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (aiPrompt != null && aiPrompt.font != null)
        {
            labelTmp.font = aiPrompt.font;
        }

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        toggle.isOn = defaultValue;

        return toggle;
    }

    protected override void OnAccept()
    {
        SaveAndCloseAsync().Forget();
    }

    private async UniTaskVoid SaveAndCloseAsync()
    {
        EnsureCheckboxes();

        string threadId = _instructorContentData?.ContentData?.ThreadId;
        string assistantId = _instructorContentData?.ContentData?.AssistantId;
        bool isPersistent = _togglePersistentThread != null && _togglePersistentThread.isOn;

        if (isPersistent && (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(assistantId)))
        {
            try
            {
                var initResp = await RootObject.Instance.LEE.ArtificialIntelligenceManager.InitializeInstructorAsync(
                    _characterName,
                    _prompt,
                    _languageModel?.ApiName);
                if (initResp != null)
                {
                    threadId = initResp.ThreadId;
                    assistantId = initResp.AssistantId;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddEditVirtualInstructor] Failed to pre-initialize persistent thread: {ex.Message}");
            }
        }

        _instructorContentData = CreateContent<InstructorContentData>(ContentType.Instructor);
        _instructorContentData.Location = new Location { Position = new Vector3(-0.4f, -1.2f, 0), Rotation = new Vector3(0, 180, 0), Scale = Vector3.one };
        _instructorContentData.ContentData.AnimationClip = _animationClip;
        _instructorContentData.ContentData.CharacterName = _characterName;
        _instructorContentData.ContentData.UseReadyPlayerMe = _useReadyPlayerMe;
        _instructorContentData.ContentData.CharacterModelUrl = _characterModelUrl;
        _instructorContentData.ContentData.Prompt = _prompt;
        _instructorContentData.ContentData.LanguageModel = _languageModel;
        _instructorContentData.ContentData.SpeechToTextModel = _speechToTextModel;
        _instructorContentData.ContentData.TextToSpeechModel = _textToSpeechModel;
        _instructorContentData.ContentData.VoiceInstruction = _voiceInstruction;

        _instructorContentData.ContentData.PersistentThread = isPersistent;
        _instructorContentData.ContentData.ThreadId = threadId;
        _instructorContentData.ContentData.AssistantId = assistantId;
        _instructorContentData.ContentData.StartConversation = _toggleStartConversation != null && _toggleStartConversation.isOn;
        _instructorContentData.ContentData.FollowUser = _toggleFollowUser != null && _toggleFollowUser.isOn;
        _instructorContentData.ContentData.EyesFollowUser = _toggleEyesFollowUser != null && _toggleEyesFollowUser.isOn;
        _instructorContentData.ContentData.StandOnFloorPlane = _toggleStandOnFloorPlane == null || _toggleStandOnFloorPlane.isOn;

        if (IsContentUpdate)
        {
            RootObject.Instance.LEE.ContentManager.UpdateContent(_instructorContentData);
        }
        else
        {
            RootObject.Instance.LEE.ContentManager.AddContent(_instructorContentData);
        }

        Close();
    }

    private void OpenLanguageModelPanel()
    {
        ResetPanel(); 
        PopupsViewer.Instance.Show(languageModelPrefab, GetWorldPosition(), _languageModel, (Action<AIModel>)SetAILanguageModel);
    }

    private void OpenSpeechToTextModelPanel()
    {
        ResetPanel(); 
        PopupsViewer.Instance.Show(speechToTextModelPrefab, GetWorldPosition(), _speechToTextModel, (Action<AIModel>)SetAISpeechToTextModel);
    }

    private void OpenTextToSpeechPanel()
    {
        ResetPanel(); 
        PopupsViewer.Instance.Show(textToSpeechModelPrefab, GetWorldPosition(), _textToSpeechModel, (Action<AIModel, string>)SetAITextToSpeechModel, _voiceInstruction);
    }

    private void OpenPromptPanel()
    {
        ResetPanel();
        PopupsViewer.Instance.Show(contextPromptPrefab, GetWorldPosition(), _prompt, (Action<string>)UpdatePrompt);
    }

    private void OnEnable()
    {
        foreach (var listItemBoxPicker in _listItemBoxPickers)
        {
            listItemBoxPicker.OnToggleSelected += HandleToggleSelection;
        }
    }

    private void OnDisable()
    {
        foreach (var listItemBoxPicker in _listItemBoxPickers)
        {
            listItemBoxPicker.OnToggleSelected -= HandleToggleSelection;
        }
    }

    private void HandleToggleSelection(Toggle toggle, ListItemBoxPicker sender)
    {
        switch (sender.gameObject.name)
        {
            case "PathSetting":
                UpdatePathSetting(toggle.name);
                break;
            case "Animation":
                UpdateAnimationSetting(toggle.name);
                break;
            case "CommunicationSettings":
                UpdateCommunicationSettings(toggle.name);
                break;
        }
    }

    private void UpdateCommunicationSettings(string modeName)
    {
        switch (modeName)
        {
            case "AI":
                ai.SetActive(true);
                audioRecoding.SetActive(false);
                noSpeech.SetActive(false);
                break;
            case "AudioRecording":
                ai.SetActive(false);
                audioRecoding.SetActive(true);
                noSpeech.SetActive(false);
                break;
            case "NoSpeech":
                ai.SetActive(false);
                audioRecoding.SetActive(false);
                noSpeech.SetActive(true);
                break;
        }
    }

    private void UpdateAnimationSetting(string animationName)
    {
        switch (animationName)
        {
             case "Idle": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 _animationClip = "Idle";
                 animationSettingTextWithOutImage.text = "Idle";
                 break;
             case "Point": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Point";
                 break;
             case "Walk": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Walk";
                 break;
             case "Hello": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Hello";
                 break;
             case "Bye": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Bye";
                 break;
             case "Sitting": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Sitting";
                 break;
             case "ThumbUp": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Thumb Up";
                 break;
             case "ThumbDown": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Hello";
                 break;
             case "Writing": 
                 animationWithOutImage.SetActive(true);
                 animationWithImage.SetActive(false);
                 animationSettingTextWithOutImage.text = _animationClip = "Writing";
                 break;
             case "ImageDisplay":
                 animationWithOutImage.SetActive(false);
                 animationWithImage.SetActive(true);
                 animationSettingTextWithImage.text = _animationClip = "Image display";
                 break;
             case "ImagePresentation": 
                 animationWithOutImage.SetActive(false);
                 animationWithImage.SetActive(true);
                 animationSettingTextWithImage.text = _animationClip = "Image presentation";
                 break;
             }
    }

    private void UpdatePathSetting(string modeName)
    {
        switch (modeName)
        {
          case "NoPath":
              pathSettingText.text = _pathSetting = "No Path";
              break; 
        }
    }

    private void OpenCharacterModelSettingPanel()
    {
        ResetPanel();
        avatarModelSettingPanel.gameObject.SetActive(true);
    }

	private void OnAvatarModelSelected(string characterModelId)
	{
        if (string.IsNullOrEmpty(characterModelId)) return;
        _useReadyPlayerMe = true;
        _characterModelUrl = AvatarLoadUtils.IdToModelUrl(characterModelId);
        if (characterModelSelectionElement != null)
        {
            var thumbnail = characterModelSelectionElement.Thumbnail ?? characterModelSelectionElement.GetComponent<ThumbnailView>();
            if (thumbnail != null)
            {
                if (thumbnail.ThumbnailProvider == null && RootObject.Instance != null)
                {
                    thumbnail.ThumbnailProvider = RootObject.Instance.AvatarLoadManager;
                }
                thumbnail.ElementId = characterModelId;
            }
        }
	}

	private void OpenCommunicationSettingPanel()
    {
        ResetPanel();
        communicationSettingPanel.SetActive(true);
    }
    private void OpenSettingsPanel()
    {
        ResetPanel();
        settingsPanel.SetActive(true);
    }
    private void OpenAnimationSettingPanel()
    {
        ResetPanel();
        animationSettingPanel.SetActive(true);

    }

    private void OpenPathSettingPanel()
    {
        ResetPanel();
        pathSettingPanel.SetActive(true);
    }

    private void ResetPanel()
    {
         settingsPanel.SetActive(false);
         avatarModelSettingPanel.gameObject.SetActive(false); 
         communicationSettingPanel.SetActive(false);
         animationSettingPanel.SetActive(false);
         pathSettingPanel.SetActive(false);
    }

    private void SetInteractionSettingsActive(bool active)
    {
        interactionSettingOpen.SetActive(active);
        interactionSettingClose.SetActive(!active); 
        interactionSettingToggleOpen.isOn = active;
        interactionSettingToggleClosed.isOn = active;
    }

    public void UpdateName(string characterName)
    {
        _characterName = characterName;
    }

    private void UpdatePrompt(string prompt)
    {
        _prompt = prompt;
        aiPrompt.text =  prompt.Length > 14 ? prompt.Substring(0, 14) : prompt; 
    }

    private void SetAILanguageModel(AIModel model)
    {
        _languageModel = model;
        aiModel.text = model.Name;
    }

    private void SetAISpeechToTextModel(AIModel model)
    {
        _speechToTextModel = model;
        aiLanguage.text = model.Name; 
    }

    private void SetAITextToSpeechModel(AIModel model, string voiceInstruction)
    {
        _textToSpeechModel = model;
        _voiceInstruction = voiceInstruction;
        aiVoice.text = model.Name;
    }
}
