using System;
using System.Text.RegularExpressions;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView_v2 : PopupBase
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private static MoodleManager moodleManager => RootObject.Instance.moodleManager;

    //public RootView rootView => (RootView)_parentView;

    private const string VERSION_FORMAT = "Version {0}";

    [SerializeField] private Toggle _togglePublicUpload;
    [SerializeField] private Toggle _toggleUploadToCloud;
    [SerializeField] private Toggle _toggleLocalSave;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnPreview;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);

        _togglePublicUpload.onValueChanged.AddListener(OnValueChangedPublicUpload);
        _toggleLocalSave.onValueChanged.AddListener(OnValueChangedSavetoggle);
        _toggleLocalSave.onValueChanged.AddListener(OnValueChangedCloudtoggle);
        _btnSave.onClick.AddListener(OnClickSaveChanges);
        _btnPreview.onClick.AddListener(OnClickPreviewActivity);

        ResetValues();
    }

    private void OnDisable()
    {
        ResetValues();
    }

    private void ResetValues()
    {
        _togglePublicUpload.isOn = DBManager.publicUploadPrivacy;
        _toggleLocalSave.isOn = false;
        _btnSave.interactable = false;
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnValueChangedPublicUpload(bool value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedSavetoggle(bool value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedCloudtoggle(bool value)
    {
        ValueHasBeenChanged();
    }

    private void ValueHasBeenChanged()
    {
        _btnSave.interactable = true;
    }

    private void OnClickSaveChanges()
    {
        if (_toggleLocalSave.isOn) 
        {
            OnSaveToggleOn();
        } else if (_toggleUploadToCloud.isOn) 
        {
            OnUploadToggleOn();
        }

        DBManager.publicUploadPrivacy = _togglePublicUpload.isOn;
        ResetValues();
        Close();
    }

    private void OnClickPreviewActivity()
    {
        EventManager.NotifyPreviewActivity(true);

        ResetValues();
        Close();
    }


    public void OnSaveToggleOn()
    {
        activityManager.SaveData();
        Toast.Instance.Show("Activity saved on your device");
    }

    public void OnUploadToggleOn()
    {
        if (DBManager.LoggedIn)
        {
            Upload();
        }
        else
        {
            DialogWindow.Instance.Show("You need to log in.", new DialogButtonContent("Ok", null));
        }
    }

    private async void Upload()
    {
        activityManager.SaveData();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 0);
        if (response == "Error: File exist, update")
        {
            DialogWindow.Instance.Show("This file is exist! Please select an option:",
                new DialogButtonContent("Update", UploadAndUpdate),
                new DialogButtonContent("Clone", UploadAndCopy),
                new DialogButtonContent("Cancel", null));
            return;
        }

        if (response == "Error: File exist, clone")
        {
            DialogWindow.Instance.Show("You are not the original author of this file! Please select an option:",
                new DialogButtonContent("Clone", UploadAndCopy),
                new DialogButtonContent("Cancel", null));
            return;
        }

        if (result) Toast.Instance.Show("upload completed successfully");
    }

    private async void UploadAndUpdate()
    {
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 1);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
    }

    private async void UploadAndCopy()
    {
        activityManager.CloneActivity();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 2);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
    }
}
