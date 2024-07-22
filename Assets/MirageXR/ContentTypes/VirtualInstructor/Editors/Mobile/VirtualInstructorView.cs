using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using MirageXR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using CharacterController = MirageXR.CharacterController;

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
    
    private string _aiPromptData = "Enter text";
    private AIModel _tts;
    private AIModel _llm;
    private AIModel _stt;

    [Header("Settings panel")] [SerializeField]
    private Button _btnNoSpeech;

    private string _prefabName;
    public override ContentType editorForType => ContentType.VIRTUALINSTRUCTOR; 
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
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
            EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        _content.predicate = editorForType.GetPredicate();

        var data = new VirtualInstructorDataModel
        {
            AnimationClip = "Idle",
            CharacterName = _prefabName,
            TextToSpeechModel = _tts,
            Prompt = _aiPromptData,
            LanguageModel = _llm,
            SpeechToTextModel = _stt 
        };
        
        _content.option = JsonConvert.SerializeObject(data);
        EventManager.ActivateObject(_content);

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

    public void SetPrompt(String str)
    {
        _aiPromptData = str; 
    }
    
    public void SetTTS(AIModel tTSModel)
    {
        _tts = tTSModel; 
    }
    public void SetLLM(AIModel lLMModel)
    {
        _llm = lLMModel; 
    }
    
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

}
