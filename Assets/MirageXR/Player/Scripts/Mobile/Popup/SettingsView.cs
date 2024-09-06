using System;
using System.Text.RegularExpressions;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : PopupBase
{
    private const string VERSION_FORMAT = "Version {0}";

    [SerializeField] private TMP_Text _txtVersion;
    [SerializeField] private ExtendedInputField _inputFieldMoodleAddress;
    [SerializeField] private Toggle _togglePublicUpload;
    [SerializeField] private Toggle _toggleUiForKids;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnReset;
    [SerializeField] private Dropdown _learningRecordStoreDropdown;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _txtVersion.text = string.Format(VERSION_FORMAT, Application.version);
        _inputFieldMoodleAddress.SetValidator(IsValidUrl);
        _inputFieldMoodleAddress.inputField.onValueChanged.AddListener(OnValueChangedMoodleAddress);
        _togglePublicUpload.onValueChanged.AddListener(OnValueChangedPublicUpload);
        _toggleUiForKids.onValueChanged.AddListener(OnValueChangedUiForKids);
        _btnSave.onClick.AddListener(OnClickSaveChanges);
        _btnReset.onClick.AddListener(OnClickReset);
        _learningRecordStoreDropdown.onValueChanged.AddListener(OnValueChangedRecordStore);

        ResetValues();
    }

    private void OnDisable()
    {
        ResetValues();
    }

    private void ResetValues()
    {
        _inputFieldMoodleAddress.text = LearningExperienceEngine.UserSettings.domain;
        _inputFieldMoodleAddress.ResetValidation();
        _togglePublicUpload.isOn = LearningExperienceEngine.UserSettings.publicUploadPrivacy;
        _toggleUiForKids.isOn = false;
        _btnSave.interactable = false;

        switch (LearningExperienceEngine.UserSettings.publicCurrentLearningRecordStore)
        {
            case LearningExperienceEngine.UserSettings.LearningRecordStores.WEKIT:
                _learningRecordStoreDropdown.value = 0;
                break;
        }
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnValueChangedRecordStore(int value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedMoodleAddress(string value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedPublicUpload(bool value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedUiForKids(bool value)
    {
        //ValueHasBeenChanged();
    }

    private void ValueHasBeenChanged()
    {
        _btnSave.interactable = true;
    }

    private void OnClickSaveChanges()
    {
        if (!_inputFieldMoodleAddress.Validate()) return;

        if (LearningExperienceEngine.UserSettings.domain != _inputFieldMoodleAddress.text)
        {
            LearningExperienceEngine.UserSettings.domain = _inputFieldMoodleAddress.text;
            LearningExperienceEngine.UserSettings.ClearLoginData();
            RootView.Instance.activityListView.UpdateListView();
        }

        LearningExperienceEngine.UserSettings.publicUploadPrivacy = _togglePublicUpload.isOn;

        var selectedLearningRecordStore = LearningExperienceEngine.UserSettings.publicCurrentLearningRecordStore;

        switch (_learningRecordStoreDropdown.value) 
        {
            case 0:
                selectedLearningRecordStore = LearningExperienceEngine.UserSettings.LearningRecordStores.WEKIT;
                break;
        }

        changeLearningRecordStore(selectedLearningRecordStore);

        ResetValues();

        Close();
    }

    private void changeLearningRecordStore(LearningExperienceEngine.UserSettings.LearningRecordStores selectedLearningRecordStore)
    {
        EventManager.NotifyxAPIChanged(selectedLearningRecordStore);

        LearningExperienceEngine.UserSettings.publicCurrentLearningRecordStore = selectedLearningRecordStore;

    }

    private void OnClickReset()
    {
        _inputFieldMoodleAddress.text = LearningExperienceEngine.UserSettings.WEKIT_URL;
        _togglePublicUpload.isOn = LearningExperienceEngine.UserSettings.PUBLIC_UPLOAD_PRIVACY_DEFAULT;
        _learningRecordStoreDropdown.value = 0;
    }

    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

}
