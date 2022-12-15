﻿using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;
using Step = MirageXR.Action;
using System.IO;

public class StepsListItem_v2 : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private TMP_Text _txtNumber;
    [SerializeField] private TMP_Text _txtStepName;
    [SerializeField] private TMP_Text _txtStepDescription;
    [SerializeField] private Button _btnStep;
    [SerializeField] private Button _btnEditButton;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private Button _btnImageMarkerPopup;
    [SerializeField] private GameObject _stepStatus;
    [SerializeField] private GameObject _stepDoneImage;
    [SerializeField] private GameObject _stepCurrentImage;
    
    [SerializeField] private ImageMarkerPopup _imageMarkerPopup;

    private MirageXR.Action _step;
    private int _number;
    private Action<Step> _onStepClick;
    private Action<Step> _onEditClick;
    private Action<Step, Action> _onDeleteClick;
    private string _imageMarkerUrl;

    public void Init(Action<Step> onStepClick, Action<Step> onEditClick, Action<Step, Action> onDeleteClick)
    {
        _onStepClick = onStepClick;
        _onEditClick = onEditClick;
        _onDeleteClick = onDeleteClick;
        _btnStep.onClick.AddListener(OnStepClick);
        _btnDelete.onClick.AddListener(OnDeleteClick);
        _btnEditButton.onClick.AddListener(OnEditClick);
        _btnImageMarkerPopup.onClick.AddListener(OnImageMarkerButtonClick);
        OnEditModeChanged(activityManager.EditModeActive);

        EventManager.OnEditModeChanged += OnEditModeChanged;
        EventManager.OnActionModified += OnActionModified;
    }

    public void UpdateView(Step step, int number)
    {
        _step = step;
        _number = number;

        _txtStepName.text = _step.instruction.title;
        _txtNumber.text = (_number + 1).ToString("00");
        var isCurrent = _step.id == RootObject.Instance.activityManager.ActiveActionId;
        _stepCurrentImage.SetActive(isCurrent);
        _stepDoneImage.SetActive(_step.isCompleted && !isCurrent);

        _btnImageMarkerPopup.gameObject.SetActive(ImageMarkerCheck());
    }

    private void OnActionModified(Step step)
    {
        if (step == _step)
        {
            _txtStepName.text = step.instruction.title;
            _txtStepDescription.text = step.instruction.description;
        }
    }

    public void OnEditModeChanged(bool value)
    {
        _btnDelete.gameObject.SetActive(value);
        _stepStatus.SetActive(!value);
        _btnEditButton.gameObject.SetActive(value);
    }

    private void OnStepClick()
    {
        _onStepClick(_step);
    }

    private void OnDeleteClick()
    {
        _onDeleteClick(_step, null);
    }

    public void OnEditClick()
    {
        _onEditClick(_step);
        EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActionInfo);
    }

    public void OnImageMarkerButtonClick()
    {
        var popup = (ImageMarkerPopup)PopupsViewer.Instance.Show(_imageMarkerPopup);
        popup.SetImage(_imageMarkerUrl);
    }

    private bool ImageMarkerCheck()
    {
        bool imageMarker = false;

        var augmentations = _step.enter.activates;

        foreach (var augmentation in augmentations)
        {
            if (augmentation.predicate == "imagemarker")
            {
                imageMarker = true;
                _imageMarkerUrl = augmentation.url;
            }
        }

        return imageMarker;
    }
}
