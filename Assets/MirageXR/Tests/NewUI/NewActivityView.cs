using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using MirageXR;
using Action = MirageXR.Action;

public class NewActivityView : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private Toggle _steps;
    [SerializeField] private Toggle _info;
    [SerializeField] private Toggle _calibration;

    [SerializeField] private GameObject _stepsTab;
    [SerializeField] private GameObject _infoTab;
    [SerializeField] private GameObject _calibrationTab;

    [SerializeField] private GameObject _augmentationsTab;
    [SerializeField] private GameObject _infoStepsTab;
    [SerializeField] private GameObject _MarkerTab;

    [SerializeField] private Button _btnSetPicture;
    [SerializeField] private PopupBase _addPreviewImage;

    [SerializeField] private Button _btnStart;
    [SerializeField] private PopupBase _startcalibrationPanel;

    [SerializeField] private GameObject _title;
    private TMP_Text _headLabel;

    [SerializeField] private GameObject _toggles;
    [SerializeField] private GameObject _toggles_steps;

    [SerializeField] private Button _btnAddImage;
    [SerializeField] private PopupBase _addImageMaker;

    [SerializeField] private GameObject _btnActivitySettings;
    [SerializeField] private GameObject _btnStepSettings;

    [SerializeField] private GameObject _btnBackToHome;
    [SerializeField] private GameObject _btnBackToActivity;

    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private TMP_Text _Name;

    [SerializeField] private StepsListView_v2 _stepsListView;

    private void Start()
    {
        _btnSetPicture.onClick.AddListener(OnSetPictureClick);
        _btnStart.onClick.AddListener(OnStartCalibrationClick);
        _btnAddImage.onClick.AddListener(OnAddImageClick);
        _inputFieldName.onValueChanged.AddListener(onNameChange);
        _btnBackToActivity.GetComponent<Button>().onClick.AddListener(OnBackToActivityClick);

        _headLabel = _title.GetComponent<TMP_Text>();

        ShowNewActivityScreen();
    }

    private void ShowNewActivityScreen()
    {
        _steps.isOn = true;
        _info.isOn = false;
        _calibration.isOn = false;
        
        _headLabel.text = "New activity";

        _toggles.SetActive(true);
        _toggles_steps.SetActive(false);
        
        _stepsTab.SetActive(true);
        _btnActivitySettings.SetActive(true);
        _btnStepSettings.SetActive(false);
        _augmentationsTab.SetActive(false);
        _infoStepsTab.SetActive(false);
        _MarkerTab.SetActive(false);
        
        _btnBackToHome.SetActive(true);
        _btnBackToActivity.SetActive(false);
    }


    private void onNameChange(string name) {
        _Name.text = name;   
    }

    private void OnBackToActivityClick()
    {
        ShowNewActivityScreen();
    }

    public void ShowStepsTab()
    {
        _stepsTab.SetActive(true);
        _infoTab.SetActive(false);
        _calibrationTab.SetActive(false);
    }

    public void ShowInfoTab()
    {
        _stepsTab.SetActive(false);
        _infoTab.SetActive(true);
        _calibrationTab.SetActive(false);
    }

    public void ShowCalibrationTab()
    {
        if (_stepsTab.activeInHierarchy)
        {
            _stepsTab.SetActive(false);
        }
        if (_infoTab.activeInHierarchy)
        {
            _infoTab.SetActive(false);
        }
        _calibrationTab.SetActive(true);
    }

    public void ShowAugmentationsTab()
    {
        _stepsTab.SetActive(false);
        _augmentationsTab.SetActive(true);
        _infoStepsTab.SetActive(false);
        _MarkerTab.SetActive(false);
    }

    public void ShowInfoStepsTab()
    {
        _augmentationsTab.SetActive(false);
        _infoStepsTab.SetActive(true);
        _MarkerTab.SetActive(false);
    }

    public void ShowMarkerTab()
    {
        _augmentationsTab.SetActive(false);
        _infoStepsTab.SetActive(false);
        _MarkerTab.SetActive(true);
    }

    private void OnSetPictureClick()
    {
        PopupsViewer.Instance.Show(_addPreviewImage);
    }

    private void OnAddImageClick()
    {
        PopupsViewer.Instance.Show(_addImageMaker);
    }

    private void OnStartCalibrationClick()
    {
        PopupsViewer.Instance.Show(_startcalibrationPanel);
    }

    public async void AddNewStepClick()
    {
        /*
        _headLabel.text = "Step ... Untittled";
        _toggles.SetActive(false);
        _toggles_steps.SetActive(true);
        _stepsTab.SetActive(false);
        _btnActivitySettings.SetActive(false);
        _btnStepSettings.SetActive(true);
        
        _btnBackToHome.SetActive(false);
        _btnBackToActivity.SetActive(true);
        //ShowAugmentationsTab();
        */
        await activityManager.AddAction(Vector3.zero);

        _stepsListView.UpdateView();
    }
}
