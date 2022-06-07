using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class NewActivityView : MonoBehaviour
{
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

    private void Start()
    {
        _steps.isOn = true;
        _info.isOn = false;
        _calibration.isOn = false;
        _btnSetPicture.onClick.AddListener(OnSetPictureClick);
        _btnStart.onClick.AddListener(OnStartCalibrationClick);
        _btnAddImage.onClick.AddListener(OnAddImageClick);

        _headLabel = _title.GetComponent<TMP_Text>();
        _headLabel.text = "New activity";
        
        _toggles.SetActive(true);
        _toggles_steps.SetActive(false);
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
        _stepsTab.SetActive(false);
        _infoTab.SetActive(false);
        _calibrationTab.SetActive(true);
    }
    
    public void ShowAugmentationsTab()
    {
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

    public void AddNewStepClick()
    {
        _headLabel.text = "Step ... Untittled";
        _toggles.SetActive(false);
        _toggles_steps.SetActive(true);
        _stepsTab.SetActive(false);
        ShowAugmentationsTab();
    }
}
