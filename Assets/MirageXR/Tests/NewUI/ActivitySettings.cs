using System;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ActivitySettings : PopupBase
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    private static MoodleManager moodleManager => RootObject.Instance.moodleManager;

    [SerializeField] private Toggle _togglePublicUpload;
    [SerializeField] private Toggle _toggleUploadToCloud;
    [SerializeField] private Toggle _toggleLocalSave;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnPreview;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private Button _btnClose;
    [SerializeField] private DeleteActivityView _deleteActivityViewPrefab;

    private SessionContainer _container;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _togglePublicUpload.onValueChanged.AddListener(OnValueChangedPublicUpload);
        _toggleLocalSave.onValueChanged.AddListener(OnValueChangedSaveToggle);
        _toggleUploadToCloud.onValueChanged.AddListener(OnValueChangedCloudToggle);
        _btnPreview.onClick.AddListener(OnPreviewClicked);
        _btnSave.onClick.AddListener(OnClickSaveChanges);
        _btnDelete.onClick.AddListener(OnButtonDeleteClicked);
        _btnClose.onClick.AddListener(OnCloseClicked);

        _btnDelete.interactable = _container != null;

        ValueHasBeenChanged();

        ResetValues();
    }

    private void OnDisable()
    {
        ResetValues();
    }

    private void ResetValues()
    {
        _togglePublicUpload.isOn = DBManager.publicUploadPrivacy;
        _toggleLocalSave.isOn = DBManager.publicLocalSave;
        _toggleUploadToCloud.isOn = DBManager.publicCloudSave;
    }

    private void SetValues()
    {
        DBManager.publicUploadPrivacy = _togglePublicUpload.isOn;
        DBManager.publicLocalSave = _toggleLocalSave.isOn;
        DBManager.publicCloudSave = _toggleUploadToCloud.isOn;
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        if (args.Length == 0 || args[0] == null)
        {
            return true;
        }

        try
        {
            _container = (SessionContainer)args[0];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void OnValueChangedPublicUpload(bool value)
    {
        if(value && _toggleUploadToCloud.isOn && DBManager.publicShowPublicUploadWarning)
        {
            RootView_v2.Instance.dialog.ShowMiddle(
                "Public Upload",
                "You have selected public upload. Once uploaded, this activity will be visable to all users.",
                "Don't show again", () => DontShowPublicUploadWarning(),
                "OK", () => AppLog.LogTrace("Ok!"),
                true);
        }

        ValueHasBeenChanged();
    }

    private void DontShowPublicUploadWarning()
    {
        DBManager.publicShowPublicUploadWarning = false;
    }

    private void OnValueChangedSaveToggle(bool value)
    {
        ValueHasBeenChanged();
    }

    private void OnValueChangedCloudToggle(bool value)
    {
        ValueHasBeenChanged();
    }

    private void ValueHasBeenChanged()
    {
        _btnSave.interactable = _toggleUploadToCloud.isOn || _toggleLocalSave.isOn;

        if (!_toggleUploadToCloud.isOn)
        {
            _togglePublicUpload.isOn = false;
            _togglePublicUpload.interactable = false;
        }
        else
        {
            _togglePublicUpload.interactable = true;
        }
    }

    private void OnPreviewClicked()
    {
        activityManager.EditModeActive = false;
        activityManager.ActivateFirstAction();
        Close();
    }

    private void OnCloseClicked()
    {
        SetValues();
        Close();
    }

    private void OnClickSaveChanges()
    {
        if (_toggleLocalSave.isOn)
        {
            OnSaveToggleOn();
        }

        if (_toggleUploadToCloud.isOn)
        {
            OnUploadToggleOn();
        }

        DBManager.publicUploadPrivacy = _togglePublicUpload.isOn;
        SetValues();
        ResetValues();
        Close();
    }

    private void OnButtonDeleteClicked()
    {
        Close();

        if (!_container.userIsOwner || !_container.ExistsRemotely)
        {
            RootView_v2.Instance.dialog.ShowMiddle("Delete activity?", "Do you really want to remove activity from the device?", "Yes", DeleteLocal, "No", null);
            return;
        }

        PopupsViewer.Instance.Show(_deleteActivityViewPrefab, (Action<bool, bool>)OnDeleteActivity);
    }

    private void OnDeleteActivity(bool fromDevice, bool fromServer)
    {
        if (!fromDevice && !fromServer)
        {
            return;
        }

        if (fromDevice && !fromServer)
        {
            RootView_v2.Instance.dialog.ShowMiddle("Delete activity?", "Do you really want to remove activity from the device?", "Yes", DeleteLocal, "No", null);
            return;
        }

        RootView_v2.Instance.dialog.ShowMiddle(
            "Delete activity?",
            $"You are trying to delete activity \"{_container.Name}\" from the server. Are you sure?",
            "Yes", () => OnDeleteActivityFromServer(fromDevice, fromServer),
            "No", null);
    }

    private void OnDeleteActivityFromServer(bool fromDevice, bool fromServer)
    {
        if (fromDevice)
        {
            DeleteLocal();
        }

        if (fromServer)
        {
            DeleteFromServer();
        }
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

    private async void DeleteFromServer()
    {
        var result = await RootObject.Instance.moodleManager.DeleteArlem(_container.ItemID, _container.FileIdentifier);
        if (result)
        {
            RootView.Instance.activityListView.UpdateListView();
        }
    }

    private void DeleteLocal()
    {
        if (activityManager.Activity == null)
        {
            return;
        }

        LocalFiles.TryDeleteActivity(activityManager.Activity.id);
        RootView_v2.Instance.OnActivityDeleted();
    }
}
