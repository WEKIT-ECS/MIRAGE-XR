using System;
using System.Threading.Tasks;
using DG.Tweening;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using MirageXR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

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
    
    
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        for (int i = 0; i < _audioToggles.Length; i++)
        {
            int index = i;
            _audioToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    HandleAudioToggleChange(index);
                }
            });
        }

        
        _showBackground = false;
        base.Initialization(onClose, args);
        //_step = RootObject.Instance.LEE.StepManager.CurrentStep;
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
        _btnNoSpeech.onClick.AddListener(OnNoSpeechButtonPressed);
        _toggleMyCharacters.onValueChanged.AddListener(OnToggleMyCharactersValueChanged);
        _toggleLibrary.onValueChanged.AddListener(OnToggleLibrariesValueChanged);

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
    
    private async void SetupCharacter()
    {
        const string movementType = "followpath";

        var characterObjectName = $"{_content.id}/{_content.poi}/{_content.predicate}";
        var character = GameObject.Find(characterObjectName);   // TODO: possible NRE

        while (character == null)
        {
            character = GameObject.Find(characterObjectName);   // TODO: possible NRE
            await Task.Delay(10);
        }

        /*var characterController = character.GetComponent<CharacterController>();
        characterController.MovementType = movementType;
        characterController.AgentReturnAtTheEnd = false;

        var destinations = new List<GameObject>();
        var taskStationPosition = TaskStationDetailMenu.Instance.ActiveTaskStation.transform.position;
        character.transform.position = taskStationPosition;
        var destination = Instantiate(_destinationPrefab, taskStationPosition - Vector3.up, Quaternion.identity);
        destination.transform.rotation *= Quaternion.Euler(0, 180, 0);
        destination.MyCharacter = characterController;
        destination.transform.SetParent(character.transform.parent);
        destinations.Add(destination.gameObject);

        characterController.Destinations = destinations;
        characterController.AudioEditorCheck();
        characterController.MyAction = _step;*/
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

        _content.predicate = editorForType.GetPredicate();

        var data = new InstructorContentData
        {
            AnimationClip = "Idle",
            CharacterName = _prefabName,
            TextToSpeechModel = _tts,
            Prompt = _aiPromptData,
            LanguageModel = _llm,
            SpeechToTextModel = _stt 
        };
        
        _content.option = JsonConvert.SerializeObject(data);
        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();

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
    public void SetPrompt(String str)
    {
        _aiPromptData = str; 
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
        if (value)
        {
            _charactersTab.SetActive(false);
            _libraryTab.SetActive(true);
        }
    }
    
    private void OnToggleMyCharactersValueChanged(bool value)
    {
        if (value)
        {
            _charactersTab.SetActive(true);
            _libraryTab.SetActive(false);
        }
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
                _AudioMenuBtn.onClick.AddListener(() => _AudioSetting.SetActive(true));
                _AudioMenuText.text = "No speech";
                _AudioSetting.SetActive(false);
                break;
            case 1: 
                _AudioMenuBtn.onClick.AddListener(() => _AudioRecodingMenu.SetActive(true));
                _AudioMenuText.text = "Audio recording";
                _AudioSetting.SetActive(false);
                 break;
            case 2: 
                _AudioMenuBtn.onClick.AddListener(() => _AiMenu.SetActive(true));
                _AudioMenuText.text = "AI";
                _AudioSetting.SetActive(false);
                break;
            default:
                Debug.LogWarning("Unknown value in VirtualInstructorView:" + index);
                break;
        }
    }
}
