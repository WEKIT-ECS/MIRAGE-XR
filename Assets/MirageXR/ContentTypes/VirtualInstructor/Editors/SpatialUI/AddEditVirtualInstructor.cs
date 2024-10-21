using System;
using System.Collections.Generic;
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
    [SerializeField] private Button modelSettingBtnA;
    [SerializeField] private Button modelSettingBtnB;
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
    [SerializeField] private GameObject modelSettingPanel; 
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
    
    
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {   
        base.Initialization(onClose, args);
        _instructorContentData = _content as Content<InstructorContentData>;
            
        settingsBtn.onClick.AddListener(OpenSettingsPanel);
        modelSettingBtnA.onClick.AddListener(OpenModelSettingPanel);
        modelSettingBtnB.onClick.AddListener(OpenModelSettingPanel);
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
    }
    
    
    private string _animationClip;
    private string _characterName;
    private string _pathSetting; // todo
    private string _prompt;
    private string _languageModelEndpointName; 
    private string _languageModelApiName; 
    private string _languageModelDescription; 
    private string _languageModelName; 
    
    private string _speechToTextModelEndpointName; 
    private string _speechToTextModellApiName; 
    private string _speechToTextModelDescription; 
    private string _speechToTextModelName; 
    
    private string _textToSpeechModelEndpointName; 
    private string _textToSpeechModelModellApiName; 
    private string _textToSpeechModelDescription; 
    private string _textToSpeechModelName; 
    
    
    
    protected override void OnAccept()
    {
        var step = RootObject.Instance.LEE.StepManager.CurrentStep;

        _instructorContentData ??= new Content<InstructorContentData>
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            IsVisible = true,
            Steps = new List<Guid> { step.Id },
            Type = ContentType.Instructor,
            Version = Application.version,
            Location = Location.GetIdentityLocation(),
            ContentData = new InstructorContentData //TODO: set up your data here
            {
                Triggers = null,
                AvailableTriggers = null,
                AnimationClip = _animationClip,
                CharacterName = _characterName,
                Prompt = _prompt,
                LanguageModel = new AIModel
                {
                    EndpointName = _languageModelEndpointName,
                    ApiName = _languageModelApiName,
                    Description = _languageModelDescription,
                    Name = _languageModelName
                },
                SpeechToTextModel = new AIModel
                {
                    EndpointName = _speechToTextModelEndpointName,
                    ApiName = _speechToTextModellApiName,
                    Description = _speechToTextModelDescription,
                    Name = _speechToTextModelName
                },
                TextToSpeechModel = new AIModel
                {
                    EndpointName = _textToSpeechModelEndpointName,
                    ApiName = _textToSpeechModelModellApiName,
                    Description = _textToSpeechModelDescription,
                    Name = _textToSpeechModelName
                }
            },
        };

        RootObject.Instance.LEE.ContentManager.AddContent(_instructorContentData);

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
                UpdatePathSetting(toggle);
                break;
            case "Animation":
                UpdateAnimationSetting(toggle);
                break;
            case "CommunicationSettings":
                UpdateCommunicationSettings(toggle);
                break;
        }
    }

    private void UpdateCommunicationSettings(Toggle toggle)
    {
        switch (toggle.name)
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

    private void UpdateAnimationSetting(Toggle toggle)
    {
        switch (toggle.name)
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

    private void UpdatePathSetting(Toggle toggle)
    {
        switch (toggle.name)
        {
          case "NoPath":
              pathSettingText.text = _pathSetting = "No Path";
              break; 
        }
    }

    private void OpenModelSettingPanel()
    {
        ResetPanel();
        modelSettingPanel.SetActive(true);
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
         modelSettingPanel.SetActive(false); 
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
        aiPrompt.text = prompt.Substring(0,14); // hier ArgumentOutOfRangeException: Index and length must refer to a location within the string.
                                                // Parameter name: length pasier wenn es zu kurz ist. 
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
