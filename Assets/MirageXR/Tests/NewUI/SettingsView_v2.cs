using System;
using MirageXR;
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

    public void Show()
    {
        PopupsViewer.Instance.Show(this);
    }

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);

        _togglePublicUpload.onValueChanged.AddListener(OnValueChangedPublicUpload);
        _toggleLocalSave.onValueChanged.AddListener(OnValueChangedSavetoggle);
        _toggleLocalSave.onValueChanged.AddListener(OnValueChangedCloudtoggle);
        _btnSave.onClick.AddListener(OnClickSaveChanges);

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
            Toast.Instance.Show("You need to log in.");
        }
    }

    private async void Upload()
    {
        activityManager.SaveData();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 0);
        if (response == "Error: File exist, update")
        {
            RootView_v2.Instance.dialog.ShowBottomMultiline("This file is exist! Please select an option:", 
                ("Update", UploadAndUpdate),
                ("Clone", UploadAndCopy),
                ("Cancel", null));
        }

        if (response == "Error: File exist, clone")
        {
            RootView_v2.Instance.dialog.ShowBottomMultiline("You are not the original author of this file! Please select an option:",
                ("Clone", UploadAndCopy),
                ("Cancel", null));
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
