using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ContentType = LearningExperienceEngine.DataModel.ContentType;


/// <summary>
/// This 
/// </summary>
public class VirtualInstructorView : PopupEditorBase
{
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private Destination _destinationPrefab;
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private CharacterListItem _characterListItemPrefab;
    [SerializeField] private CharacterObject[] _characterObjects;
    [Space] 
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _togglePanel;
    [SerializeField] private GameObject _tabsPanel;
    [Space]
    [SerializeField] private Toggle _toggleMyCharacters;
    [SerializeField] private Toggle _toggleLibrary;
    [Header("Tabs")] 
    [SerializeField] private GameObject _charactersTab;
    [SerializeField] private GameObject _libraryTab;
    [Space] 
    [SerializeField] private Toggle[] _audioToggles;
    
    [SerializeField] private GameObject _AudioSetting;
    [SerializeField] private TMPro.TextMeshProUGUI _AudioMenuText;
    [SerializeField] private Button _AudioMenuBtn;
    [SerializeField] private GameObject _AudioRecodingMenu;
    [SerializeField] private GameObject _AiMenu;
    [SerializeField] private GameObject _NoSpeech;

    private string _ReadyPlayerMeUrl;
    private string _DeafultCarater = "Hanna"; // Fallback Classic instructor
    private string _rpmURL;
    
    
    /// <summary>
    /// Represents the prompt a for the Virtual Instructor.
    /// </summary>
    private string _aiPromptData = "Enter text";

    /// <summary>
    /// Represents the tts model a for the Virtual Instructor.
    /// </summary>
    private AIModel _tts;

    /// <summary>
    /// Represents an LLM/RAG model for the AiServices.
    /// </summary>
    private AIModel _llm;
    /// <summary>
    /// Represents the stt model a for the Virtual Instructor.
    /// </summary>
    private AIModel _stt;

    [Header("Settings panel")] [SerializeField]
    private Button _btnNoSpeech;

    private string _prefabName;
    //public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.VIRTUALINSTRUCTOR;
    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Instructor;
    //public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.VIRTUALINSTRUCTOR; 
    
    
    
    public override async void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        await RootObject.Instance.WaitForInitialization();
        _showBackground = false;
        base.Initialization(onClose, args);

        foreach (var arg in args)
        {
            switch (arg)
            {
                case LearningExperienceEngine.Action step:
                    _step = step;
                    break;
                case Content content:
                    Content = content;
                    IsContentUpdate = true;
                    break;
            }
        }
        
        UpdateView();

        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        //_btnNoSpeech.onClick.AddListener(OnNoSpeechButtonPressed); Was ist das? 
        _toggleMyCharacters.onValueChanged.AddListener(OnToggleMyCharactersValueChanged);
        _toggleLibrary.onValueChanged.AddListener(OnToggleLibrariesValueChanged);

        for (int i = 0; i < _audioToggles.Length; i++)
        {
            int index = i;
            _audioToggles[i].onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    HandleAudioToggleChange(index);
                }
            });
        }
        // visibility 
        _settingsPanel.SetActive(false);
        _togglePanel.SetActive(true);
        _tabsPanel.SetActive(true);
        RootView_v2.Instance.HideBaseView();
    }
    
    private void UpdateView()
    {
        for (int i = _contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = _contentContainer.GetChild(i);
            Destroy(child);
        }

        foreach (var characterObject in _characterObjects)
        {
            var item = Instantiate(_characterListItemPrefab, _contentContainer);
            item.Init(characterObject, OnAccept);
        }
    }
    
    private void OnAccept(string prefabName)
    {
        _prefabName = prefabName;
        OpenSettingsPanel();
    }

    private void OpenSettingsPanel()
    {
        _settingsPanel.SetActive(true);
        _togglePanel.SetActive(false);
        _tabsPanel.SetActive(false);
    }

    protected override void OnAccept()
    {
        if (_content != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        //_content.predicate = editorForType.GetPredicate();//TODO obsolete

        var data = new InstructorContentData
        {
            AnimationClip = "Idle",
            CharacterName = string.IsNullOrEmpty(_prefabName) ? _DeafultCarater : _prefabName,
            TextToSpeechModel = _tts,
            Prompt = _aiPromptData,
            LanguageModel = _llm,
            SpeechToTextModel = _stt,
            UseReadyPlayerMe = !string.IsNullOrEmpty(_rpmURL), 
            CharacterModelUrl = string.IsNullOrEmpty(_rpmURL) ? "" : _rpmURL
        };
        RootObject.Instance.LEE.ContentManager.AddContent(CreateInstructorContent (data, RootObject.Instance.LEE.StepManager.CurrentStep.Id));
        Close();
    }
    private Content<InstructorContentData> CreateInstructorContent(InstructorContentData data, Guid setpID)
    {
        var  content = new Content<InstructorContentData>
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            Steps = new List<Guid> { setpID },
            Type = ContentType.Instructor,
            IsVisible = true,
            Location = Location.GetIdentityLocation(),
            ContentData = data
        };
        return content;
    }
    
    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }

    /// <summary> todo 
    ///  Was zum mgeier ist das? 
    /// </summary>
    // private void OnNoSpeechButtonPressed()
    // {
    //     /*RootView_v2.Instance.dialog.ShowBottomMultilineToggles("Communication settings", ("No speech", () => NoSpeechSelected(), false, true),
    //         ("Audio recording", () => AudioRecordingSelected(), false, false),
    //         ("AI", () => AIhSelected(), false, false));*/
    // }

    /// <summary>
    /// Sets the prompt for the VirtualInstructor.
    /// </summary>
    public void SetPrompt(string prompt) /// todo hier hast du eine jet bareins anlotation benutzt. Das ist böse. sie hat geschaut das der paremter nciht null. 
    {
        _aiPromptData = prompt ?? throw new ArgumentNullException(nameof(prompt)); 
    }

    /// <summary>
    /// Sets the TTS model for the VirtualInstructor.
    /// </summary>
    public void SetTTS(AIModel tTSModel)
    {
        _tts = tTSModel; 
    }

    /// <summary>
    /// Sets the LLM / RAG for the VirtualInstructor.
    /// </summary>
    public void SetLLM(AIModel lLMModel)
    {
        _llm = lLMModel; 
    }

    /// <summary>
    /// Sets the STT AI model for the VirtualInstructor.
    /// </summary>
    public void SetSTT(AIModel sTTModel)
    {
        _stt = sTTModel; 
    }
    
    
    private void OnToggleLibrariesValueChanged(bool value)
    {
        if (!value) return;
        _charactersTab.SetActive(false);
        _libraryTab.SetActive(true);
    }
    
    private void OnToggleMyCharactersValueChanged(bool value)
    {
        if (!value) return;
        _charactersTab.SetActive(true);
        _libraryTab.SetActive(false);
    }
    
    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
        _toggleMyCharacters.onValueChanged.RemoveAllListeners();
        _toggleLibrary.onValueChanged.RemoveAllListeners();
    }

    private void HandleAudioToggleChange(int index)
    {
        switch (index)
        {
            case 0:
                _AiMenu.SetActive(false);
                _AudioRecodingMenu.SetActive(false);
                _NoSpeech.SetActive(true);
                _AudioMenuText.text = "No speech";
                _AudioSetting.SetActive(false);
                break;
            case 1:
                _AudioMenuText.text = "Audio recording";
                _AiMenu.SetActive(false);
                _AudioRecodingMenu.SetActive(true);
                _NoSpeech.SetActive(false);
                _AudioSetting.SetActive(false);
                 break;
            case 2: 
                _AudioMenuText.text = "AI";
                _AiMenu.SetActive(true);
                _AudioRecodingMenu.SetActive(false);
                _NoSpeech.SetActive(false);
                _AudioSetting.SetActive(false);
                break;
            default:
                Debug.LogError("Unknown value in VirtualInstructorView:" + index);
                break;
        }
    }
}
