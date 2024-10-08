using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AddEditVirtualInstructor : MonoBehaviour
{
    // Btn
    [FormerlySerializedAs("ClosePanelBtn")]
    [Header("Buttons")]
    [SerializeField] private Button closePanelBtn;
    [SerializeField] private Button settingsBtn; 
    [SerializeField] private Button modelSettingBtn;
    [SerializeField] private Button[] communicationSettingBtns;
    [SerializeField] private Button animationSettingBtn;
    [SerializeField] private Button pathSettingBtn;
    [SerializeField] private Button applyBtn;
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
    [SerializeField] private ListItemBoxPicker[] _listItemBoxPickers; // Array für mehrere ListItemBoxPicker
    [SerializeField] private GameObject setPromptVI;
    [SerializeField] private GameObject setVoicesVI; 
    [SerializeField] private GameObject setModelVI; 
    [SerializeField] private GameObject setLanguageVI; 
    

    [Header("Communication Settings ")] 
    [SerializeField] private GameObject ai; 
    [SerializeField] private GameObject audioRecoding;
    [SerializeField] private GameObject noSpeech;
    
    
    


    void Start()
    {   
        closePanelBtn.onClick.AddListener(Apply);
        settingsBtn.onClick.AddListener(OpenSettingsPanel);
        modelSettingBtn.onClick.AddListener(OpenModelSettingPanel);
        foreach (var communicationSettingBtn in communicationSettingBtns)
        {
            communicationSettingBtn.onClick.AddListener(OpenCommunicationSettingPanel);
        }
        animationSettingBtn.onClick.AddListener(OpenAnimationSettingPanel);
        pathSettingBtn.onClick.AddListener(OpenPathSettingPanel);
        applyBtn.onClick.AddListener(Apply); 
        promptVI.onClick.AddListener(OpenPromptPanel);
        voicesVI.onClick.AddListener(OpenVoicePanel);
        modelVI.onClick.AddListener(OpenModelPanel);
        languageVI.onClick.AddListener(OpenLanguagePanel);
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

    void OnEnable()
    {
        foreach (var listItemBoxPicker in _listItemBoxPickers)
        {
            listItemBoxPicker.OnToggleSelected += HandleToggleSelection;
        }
    }

    void OnDisable()
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
            case "Audio recording":
                ai.SetActive(false);
                audioRecoding.SetActive(true);
                noSpeech.SetActive(false);
                break;
            case "No Speech":
                ai.SetActive(false);
                audioRecoding.SetActive(false);
                noSpeech.SetActive(true);
                break;
        }
    }

    private void UpdateAnimationSetting(Toggle toggle)
    {
        UnityEngine.Debug.Log("UpdateAnimationSetting to " + toggle.gameObject.name);
    }

    private void UpdatePathSetting(Toggle toggle)
    {
        UnityEngine.Debug.Log("UpdatePathSetting to " + toggle.gameObject.name);
    }


    private void Apply()
    {
        ResetPanel();
        UnityEngine.Debug.Log("Apply");
        gameObject.SetActive(false);
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
    
    
}
