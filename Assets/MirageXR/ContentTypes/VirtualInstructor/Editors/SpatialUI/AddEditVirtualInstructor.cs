using System;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using AIModel = LearningExperienceEngine.DataModel.AIModel;

public class AddEditVirtualInstructor : EditorSpatialView
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
    [SerializeField] private GameObject setPromptVI;
    [SerializeField] private GameObject setVoicesVI; 
    [SerializeField] private GameObject setModelVI; 
    [SerializeField] private GameObject setLanguageVI; 
    
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
    
    private Content<InstructorContentData> _instructorContentData;

	private string _triggers = String.Empty; // todo
    private string _availableTriggers = String.Empty; // todo
    
    private string _animationClip = "Idle";
    private string _characterName = "Hanna";
    private bool _useReadyPlayerMe = false;
    private string _characterModelUrl = "";
    private string _pathSetting = "No Path"; // todo
    private string _prompt = "Provide a concise answer to the question. Use the context.";
    private string _languageModelEndpointName = "llm/"; 
    private string _languageModelApiName = "ISS-RAG"; 
    private string _languageModelDescription = "ISS-RAG"; 
    private string _languageModelName = "ISS-RAG"; 
    
    private string _speechToTextModelEndpointName = "stt/"; 
    private string _speechToTextModellApiName = "English"; 
    private string _speechToTextModelDescription = "English";  
    private string _speechToTextModelName = "English"; 
    
    private string _textToSpeechModelEndpointName = "tts/"; 
    private string _textToSpeechModelModellApiName = "alloy";
    private string _textToSpeechModelDescription = "Female human voice"; 
    private string _textToSpeechModelName = "Alloy"; 

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

        animationSettingBtnA.onClick.AddListener(OpenAnimationSettingPanel);
        animationSettingBtnB.onClick.AddListener(OpenAnimationSettingPanel);
        pathSettingBtn.onClick.AddListener(OpenPathSettingPanel);
        promptVI.onClick.AddListener(OpenPromptPanel);
        voicesVI.onClick.AddListener(OpenVoicePanel);
        modelVI.onClick.AddListener(OpenModelPanel);
        languageVI.onClick.AddListener(OpenLanguagePanel);
        interactionSettingToggleOpen.onValueChanged.AddListener(SetInteractionSettingsActive);
        interactionSettingToggleClosed.onValueChanged.AddListener(SetInteractionSettingsActive);
        avatarModelSettingPanel.CharacterModelSelected += OnAvatarModelSelected;

        if (IsContentUpdate && _instructorContentData != null)
        {
            UpdatePrompt(_instructorContentData.ContentData.Prompt);
            SetAIModel(_instructorContentData.ContentData.TextToSpeechModel, "TextToSpeechModel");
            SetAIModel(_instructorContentData.ContentData.SpeechToTextModel, "SpeechToTextModel");
            SetAIModel(_instructorContentData.ContentData.LanguageModel, "LanguageModel");
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

        _instructorContentData.Location = new Location
        {
            Position = new Vector3(-0.4f, -1.2f, 0),
            Rotation = new Vector3(0, 180, 0),
            Scale = Vector3.one
        };
        _instructorContentData.ContentData.AnimationClip = _animationClip;
        _instructorContentData.ContentData.CharacterName = _characterName;
        _instructorContentData.ContentData.UseReadyPlayerMe = _useReadyPlayerMe;
        _instructorContentData.ContentData.CharacterModelUrl = _characterModelUrl;
        _instructorContentData.ContentData.Prompt = _prompt;
        _instructorContentData.ContentData.LanguageModel = new AIModel
        {
            EndpointName = _languageModelEndpointName,
            ApiName = _languageModelApiName,
            Description = _languageModelDescription,
            Name = _languageModelName
        };
        _instructorContentData.ContentData.SpeechToTextModel = new AIModel
        {
            EndpointName = _speechToTextModelEndpointName,
            ApiName = _speechToTextModellApiName,
            Description = _speechToTextModelDescription,
            Name = _speechToTextModelName
        };
        _instructorContentData.ContentData.TextToSpeechModel = new AIModel
        {
            EndpointName = _textToSpeechModelEndpointName,
            ApiName = _textToSpeechModelModellApiName,
            Description = _textToSpeechModelDescription,
            Name = _textToSpeechModelName
        };

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

    private void OpenLanguagePanel()
    {
        ResetPanel(); 
        setLanguageVI.SetActive(true);
    }

    private void OpenModelPanel()
    { 
        ResetPanel(); 
        setModelVI.SetActive(true);
    }

    private void OpenVoicePanel()
    {
        ResetPanel(); 
        setVoicesVI.SetActive(true);
    }

    private void OpenPromptPanel()
    {
        ResetPanel(); 
        setPromptVI.SetActive(true);
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
        characterModelSelectionElement.Thumbnail.CharacterModelUrl = _characterModelUrl;
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
         setPromptVI.SetActive(false);
         setVoicesVI.SetActive(false);
         setModelVI.SetActive(false);
         setLanguageVI.SetActive(false);
    }

    private void SetInteractionSettingsActive(bool active)
    {
        interactionSettingOpen.SetActive(active);
        interactionSettingClose.SetActive(!active); 
        interactionSettingToggleOpen.isOn = active;
        interactionSettingToggleClosed.isOn = active;
    }

    public void UpdateName(string name)
    {
        _characterName = name;
    }

    public void UpdatePrompt(string prompt)
    {
        _prompt = prompt;
        aiPrompt.text =  prompt.Length > 14 ? prompt.Substring(0, 14) : prompt; 
    }

    public void SetAIModel(AIModel model, string type)
    {
        switch (type)
        {
            case "LanguageModel":
                _languageModelEndpointName = model.EndpointName;
                _languageModelApiName = model.ApiName; 
                _languageModelDescription = model.Description;
                _languageModelName = aiModel.text = model.Name;
                break;
                
            case "SpeechToTextModel":
                _speechToTextModelEndpointName = model.EndpointName;
                _speechToTextModellApiName = model.ApiName; 
                _speechToTextModelDescription = model.Description;
                _speechToTextModelName = aiLanguage.text = model.Name; 
                break;
          
                
            case "TextToSpeechModel":
                _textToSpeechModelEndpointName = model.EndpointName;
                _textToSpeechModelModellApiName = model.ApiName; 
                _textToSpeechModelDescription = model.Description;
                _textToSpeechModelName = aiVoice.text =  model.Name; 
                break;
        }
        
    }
}
