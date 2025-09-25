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
    [SerializeField] private CharacterModelSelectionElement characterModelSelectionElement;
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
    [SerializeField] private ContextPromt contextPromptPrefab;
    [SerializeField] private SpatialAILanguageModel languageModelPrefab;
    [SerializeField] private SpatialUiAiSpeechToTextModel speechToTextModelPrefab;
    [SerializeField] private SpatialUiAiTextToSpeechModel textToSpeechModelPrefab;

    private Content<InstructorContentData> _instructorContentData;

	private string _triggers = String.Empty; // todo
    private string _availableTriggers = String.Empty; // todo

    private string _animationClip = "Idle";
    private string _characterName = "Hanna";
    private bool _useReadyPlayerMe = false;
    private string _characterModelUrl = "";
    private string _pathSetting = "No Path"; // todo
    private string _prompt = "Provide a concise answer to the question. Use the context.";
    private string _voiceInstruction = string.Empty;

    private AIModel _languageModel;
    private AIModel _speechToTextModel;
    private AIModel _textToSpeechModel;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {   
        base.Initialization(onClose, args);
        _instructorContentData = Content as Content<InstructorContentData>;

        settingsBtn.onClick.AddListener(OpenSettingsPanel);
        characterModelSelectionElement.CharacterModelSelectionStarted += OpenCharacterModelSettingPanel;
        replaceCharacterModelBtn.onClick.AddListener(OpenCharacterModelSettingPanel);

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

        if (IsContentUpdate && _instructorContentData != null)
        {
            _voiceInstruction = _instructorContentData.ContentData.VoiceInstruction; 
            UpdatePrompt(_instructorContentData.ContentData.Prompt);
            SetAITextToSpeechModel(_instructorContentData.ContentData.TextToSpeechModel, _instructorContentData.ContentData.VoiceInstruction);
            SetAISpeechToTextModel(_instructorContentData.ContentData.SpeechToTextModel);
            SetAILanguageModel(_instructorContentData.ContentData.LanguageModel);
            UpdateName(_instructorContentData.ContentData.CharacterName);
            UpdateAnimationSetting(_instructorContentData.ContentData.AnimationClip);
            if (_instructorContentData.ContentData.UseReadyPlayerMe)
            {
                OnAvatarModelSelected(_instructorContentData.ContentData.CharacterModelUrl);
            }
        }
    }

    protected override void OnAccept()
    {
        _instructorContentData = CreateContent<InstructorContentData>(ContentType.Instructor);
        _instructorContentData.Location = new Location { Position = new Vector3(-0.4f, -1.2f, 0), Rotation = new Vector3(0, 180, 0), Scale = Vector3.one };
        _instructorContentData.ContentData.AnimationClip = _animationClip;
        _instructorContentData.ContentData.CharacterName = _characterName;
        _instructorContentData.ContentData.UseReadyPlayerMe = _useReadyPlayerMe;
        _instructorContentData.ContentData.CharacterModelUrl = _characterModelUrl;
        _instructorContentData.ContentData.Prompt = _prompt;
        _instructorContentData.ContentData.LanguageModel =_languageModel;
        _instructorContentData.ContentData.SpeechToTextModel = _speechToTextModel;
        _instructorContentData.ContentData.TextToSpeechModel = _textToSpeechModel;
        _instructorContentData.ContentData.VoiceInstruction = _voiceInstruction;

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

	private void OnAvatarModelSelected(string characterModelUrl)
	{
        _useReadyPlayerMe = true;
        _characterModelUrl = characterModelUrl;
        characterModelSelectionElement.Thumbnail.CharacterModelId = _characterModelUrl;
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
