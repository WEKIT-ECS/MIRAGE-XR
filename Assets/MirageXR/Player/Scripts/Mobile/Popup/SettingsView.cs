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
    [SerializeField] private ExtendedInputField _inputFieldRecordStore;
    [SerializeField] private ExtendedInputField _inputFieldMoodleAddress;
    [SerializeField] private Toggle _togglePublicUpload;
    [SerializeField] private Toggle _toggleUiForKids;
    [SerializeField] private Button _btnSave;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        _txtVersion.text = string.Format(VERSION_FORMAT, Application.version);
        _inputFieldRecordStore.SetValidator(IsValidUrl);
        _inputFieldRecordStore.inputField.onValueChanged.AddListener(OnValueChangedRecordStore);
        _inputFieldMoodleAddress.SetValidator(IsValidUrl);
        _inputFieldMoodleAddress.inputField.onValueChanged.AddListener(OnValueChangedMoodleAddress);
        _togglePublicUpload.onValueChanged.AddListener(OnValueChangedPublicUpload);
        _toggleUiForKids.onValueChanged.AddListener(OnValueChangedUiForKids);
        _btnSave.onClick.AddListener(OnClickSaveChanges);

        ResetValues();
    }

    private void OnDisable()
    {
        ResetValues();
    }

    private void ResetValues()
    {
        _inputFieldRecordStore.text = string.Empty;
        _inputFieldMoodleAddress.text = DBManager.domain;
        _inputFieldRecordStore.ResetValidation();
        _inputFieldMoodleAddress.ResetValidation();
        _togglePublicUpload.isOn = DBManager.publicUploadPrivacy;
        _toggleUiForKids.isOn = false;
        _btnSave.interactable = false;
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
    
    private void OnValueChangedRecordStore(string value)
    {
        //ValueHasBeenChanged();
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
        if (_inputFieldMoodleAddress.Validate())
        {
            DBManager.domain = _inputFieldMoodleAddress.text;
        }
        else
        {
            return;
        }
        
        DBManager.publicUploadPrivacy = _togglePublicUpload.isOn;
        ResetValues();
        Close();
    }

    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }
}
