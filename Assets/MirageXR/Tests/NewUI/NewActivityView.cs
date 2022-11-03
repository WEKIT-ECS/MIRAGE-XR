using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using MirageXR;
using Action = MirageXR.Action;

public class NewActivityView : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject newActivityTabs;
    [SerializeField] private GameObject StepsTabs;

    [SerializeField] private Toggle _steps;
    [SerializeField] private Toggle _info;
    [SerializeField] private Toggle _calibration;

    [Space]
    [SerializeField] private GameObject _stepsTab;
    [SerializeField] private GameObject _infoTab;
    [SerializeField] private GameObject _calibrationTab;

    [SerializeField] private GameObject _augmentationsTab;
    [SerializeField] private GameObject _infoStepsTab;
    [SerializeField] private GameObject _MarkerTab;

    [SerializeField] private Button _btnSetPicture;
    [SerializeField] private PopupBase _addPreviewImage;

    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    
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

    [SerializeField] private InfoStepsTab infoStepsTab;
    private int infoStepNumber;



    private void Start()
    {
        _btnSetPicture.onClick.AddListener(OnSetPictureClick);
        _btnStart.onClick.AddListener(OnStartCalibrationClick);
        _btnAddImage.onClick.AddListener(OnAddImageClick);
        _inputFieldName.onValueChanged.AddListener(OnNameChange);
        _btnBackToActivity.GetComponent<Button>().onClick.AddListener(ShowStepsTab);

        _btnArrow.onClick.AddListener(ArrowBtnPressed);
        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

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


    private void OnNameChange(string name)
    {
        _Name.text = name;   
    }

    private void OnBackToActivityClick()
    {
        ShowNewActivityScreen();
    }

    public void ShowStepsTab()
    {
        ShowNewActivityTabs(true);
        ShowStepsTabs(false);

        _stepsTab.SetActive(true);
        _infoTab.SetActive(false);
        _calibrationTab.SetActive(false);
    }

    public void ShowInfoTab()
    {
        ShowNewActivityTabs(true);
        ShowStepsTabs(false);

        _stepsTab.SetActive(false);
        _infoTab.SetActive(true);
        _calibrationTab.SetActive(false);
    }

    public void ShowCalibrationTab()
    {
        ShowNewActivityTabs(true);
        ShowStepsTabs(false);

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
        ShowNewActivityTabs(false);
        ShowStepsTabs(true);

        _stepsTab.SetActive(false);
        _augmentationsTab.SetActive(true);
        _infoStepsTab.SetActive(false);
        _MarkerTab.SetActive(false);
    }

    public void ShowInfoStepsTab()
    {
        ShowNewActivityTabs(false);
        ShowStepsTabs(true);

        _augmentationsTab.SetActive(false);
        _MarkerTab.SetActive(false);
        _infoStepsTab.SetActive(true);

        infoStepsTab.Init(infoStepNumber);
    }

    public void ChangeInfoStepNumber(int stepNumber)
    {
        infoStepNumber = stepNumber;       
    }

    public void ShowMarkerTab()
    {
        ShowNewActivityTabs(false);
        ShowStepsTabs(true);

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

    private void ShowNewActivityTabs(bool show)
    {
        newActivityTabs.SetActive(show);
        _toggles.SetActive(show);
        _btnBackToHome.SetActive(show);
    }

    private void ShowStepsTabs(bool show)
    {
        StepsTabs.SetActive(show);
        _toggles_steps.SetActive(show);
        _btnBackToActivity.SetActive(show);
    }

    public void ArrowBtnPressed()
    {
        if (_arrowDown.activeSelf)
        {
            _panel.DOAnchorPos(new Vector2(0, -1100), 0.25f);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPos(new Vector2(0, -60), 0.25f);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }
}
