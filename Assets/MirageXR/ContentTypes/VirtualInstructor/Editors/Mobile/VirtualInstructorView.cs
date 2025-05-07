using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
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
    
    private IContentManager _contentManager = RootObject.Instance.LEE.ContentManager;
    private IStepManager _stepManager =  RootObject.Instance.LEE.StepManager;
    
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
    public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.VIRTUALINSTRUCTOR;
    
    
    
    public override async void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        await RootObject.Instance.WaitForInitialization(); // Warten auf LEE init

        _showBackground = false;
        base.Initialization(onClose, args);

        // 1. Argumente auswerten (Step, Content)
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
        
        // 2. UI-Setup
        UpdateView();

        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        _btnNoSpeech.onClick.AddListener(OnNoSpeechButtonPressed);
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

        // 3. Sichtbarkeit der UI-Panels
        _settingsPanel.SetActive(false);
        _togglePanel.SetActive(true);
        _tabsPanel.SetActive(true);

        // 4. Editor-Modus vorbereiten
        RootView_v2.Instance.HideBaseView();
        
        // for (int i = 0; i < _audioToggles.Length; i++)
        // {
        //     int index = i;
        //     _audioToggles[i].onValueChanged.AddListener((isOn) =>
        //     {
        //         if (isOn)
        //         {
        //             HandleAudioToggleChange(index);
        //         }
        //     });
        // }
        //
        //
        // _showBackground = false;
        // base.Initialization(onClose, args);
        // //_step = RootObject.Instance.LEE.StepManager.CurrentStep;
        // foreach (var arg in args)
        // {
        //     if (arg is LearningExperienceEngine.Action step)
        //     {
        //         _step = step;
        //     }
        //     else if (arg is Content content)
        //     {
        //         Content = content;
        //         IsContentUpdate = true;
        //     }
        // }
        //
        // UpdateView();
        //
        // _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        // _btnNoSpeech.onClick.AddListener(OnNoSpeechButtonPressed);
        // _toggleMyCharacters.onValueChanged.AddListener(OnToggleMyCharactersValueChanged);
        // _toggleLibrary.onValueChanged.AddListener(OnToggleLibrariesValueChanged);
        //
        // _settingsPanel.SetActive(false);
        // _togglePanel.SetActive(true);
        // _tabsPanel.SetActive(true);
        // RootView_v2.Instance.HideBaseView();
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
    
    private async void SetupCharacter()
    {
        var data = GetInstructorData();
        if (data == null)
        {
            Debug.LogError("Cannot set up character: instructor content data missing.");
            return;
        }

        if (data.UseReadyPlayerMe && !string.IsNullOrEmpty(data.CharacterModelUrl))
        {
            await SetupReadyPlayerMeCharacter(data);
        }
        else if (!string.IsNullOrEmpty(data.CharacterName))
        {
            SetupLegacyCharacter(data);
        }
        else
        {
            Debug.LogWarning("No valid character configuration found.");
        }
    }
    
    private async Task SetupReadyPlayerMeCharacter(InstructorContentData data)
    {
        var avatarContainer = new GameObject($"Instructor_{_content.id}_RPM");
        var loader = avatarContainer.AddComponent<AvatarLoader>();
        loader.LoadDefaultAvatarOnStart = false;

        // Eventual fallback, falls du auf Fertigstellung warten willst
        TaskCompletionSource<bool> tcs = new();
        loader.AvatarLoaded += success => tcs.SetResult(success);
        loader.LoadAvatar(data.CharacterModelUrl);

        bool success = await tcs.Task;
        if (!success)
        {
            Debug.LogError("ReadyPlayerMe avatar failed to load.");
        }
    }
    
    private void SetupLegacyCharacter(InstructorContentData data)
    {
        var characterObj = _characterObjects.FirstOrDefault(obj => obj.prefabName == data.CharacterName);
        if (!characterObj)
        {
            Debug.LogError($"CharacterObject for '{data.CharacterName}' not found.");
            return;
        }

        var prefab = Resources.Load<GameObject>($"Characters/{characterObj.prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"Prefab '{characterObj.prefabName}' not found in Resources/Characters.");
            return;
        }

        var character = Instantiate(prefab);
        character.name = $"Instructor_{Content.Id}_Legacy";
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
        var data = new InstructorContentData
        {
            AnimationClip = "Idle",
            CharacterName = string.IsNullOrEmpty(_prefabName) ? _DeafultCarater : _prefabName,
            TextToSpeechModel = _tts,
            Prompt = _aiPromptData,
            LanguageModel = _llm,
            SpeechToTextModel = _stt,
            UseReadyPlayerMe = false, 
            CharacterModelUrl = "" 
        };
        
        if (_ReadyPlayerMeUrl != null)
        {
            data = CreateInstructor(_ReadyPlayerMeUrl, data); 
        }
        _contentManager.AddContent(CreateInstructorContent (data, _stepManager.CurrentStep.Id));
        SetupCharacter();
        Close();
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

    private void OnNoSpeechButtonPressed()
    {
        /*RootView_v2.Instance.dialog.ShowBottomMultilineToggles("Communication settings", ("No speech", () => NoSpeechSelected(), false, true),
            ("Audio recording", () => AudioRecordingSelected(), false, false),
            ("AI", () => AIhSelected(), false, false));*/
    }

    /// <summary>
    /// Sets the prompt for the VirtualInstructor.
    /// </summary>
    public void SetPrompt([NotNull] string prompt)
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
                Debug.LogWarning("Unknown value in VirtualInstructorView:" + index);
                break;
        }
    }
    
    private InstructorContentData CreateInstructor(string characterSource, InstructorContentData data)
    {
        // todo check how to! 
        if (characterSource.StartsWith("http")) // ReadyPlayerMe Avatar URL
        {
            data.UseReadyPlayerMe = true;
            data.CharacterModelUrl = characterSource;
            data.CharacterName = null; // optional, falls überschrieben werden muss
        }
        
        return data;
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
    
    private InstructorContentData? GetInstructorData()
    {
        if (Content is Content<InstructorContentData> instructorContent)
        {
            return instructorContent.ContentData;
        }

        Debug.LogError("Content is not of type Content<InstructorContentData>");
        return null;
    }


}
