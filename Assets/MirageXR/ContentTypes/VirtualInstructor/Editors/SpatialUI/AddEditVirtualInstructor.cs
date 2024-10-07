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
    [SerializeField] private Button communicationSettingBtn;
    [SerializeField] private Button animationSettingBtn;
    [SerializeField] private Button pathSettingBtn;
    [SerializeField] private Button applyBtn;
    // Panels
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject modelSettingPanel; 
    [SerializeField] private GameObject communicationSettingPanel;
    [SerializeField] private GameObject animationSettingPanel;
    [SerializeField] private GameObject pathSettingPanel;
    
    
    void Start()
    {   
        closePanelBtn.onClick.AddListener(Apply);
        settingsBtn.onClick.AddListener(() => OpenSettingsPanel());
        modelSettingBtn.onClick.AddListener(()=> OpenModelSettingPanel());
        communicationSettingBtn.onClick.AddListener(() =>OpenCommunicationSettingPanel());
        animationSettingBtn.onClick.AddListener(() => OpenAnimationSettingPanel());
        pathSettingBtn.onClick.AddListener(() => OpenPathSettingPanel());
        applyBtn.onClick.AddListener(Apply);
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
    }
    
    
}
