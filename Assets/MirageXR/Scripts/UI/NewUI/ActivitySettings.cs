using System;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using LearningExperienceEngine;

public class ActivitySettings : PopupBase
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    private static LearningExperienceEngine.MoodleManager moodleManager => LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager;

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
        _togglePublicUpload.isOn = UserSettings.publicUploadPrivacy;
        _toggleLocalSave.isOn = UserSettings.publicLocalSave;
        _toggleUploadToCloud.isOn = UserSettings.publicCloudSave;
    }

    private void SetValues()
    {
        UserSettings.publicUploadPrivacy = _togglePublicUpload.isOn;
        UserSettings.publicLocalSave = _toggleLocalSave.isOn;
        UserSettings.publicCloudSave = _toggleUploadToCloud.isOn;
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
        if(value && _toggleUploadToCloud.isOn && UserSettings.publicShowPublicUploadWarning)
        {
            RootView_v2.Instance.dialog.ShowMiddle(
                "Public Upload",
                "You have selected public upload. Once uploaded, this activity will be visable to all users.",
                "Don't show again", () => DontShowPublicUploadWarning(),
                "OK", () => Debug.LogTrace("Ok!"),
                true);
        }

        ValueHasBeenChanged();
    }

    private void DontShowPublicUploadWarning()
    {
        UserSettings.publicShowPublicUploadWarning = false;
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

        UserSettings.publicUploadPrivacy = _togglePublicUpload.isOn;
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
        if (UserSettings.LoggedIn)
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
            RootView_v2.Instance.dialog.ShowBottomMultiline("Activity already exists on the cloud. Please choose:", 
                ("Update", UploadAndUpdate),
                ("Clone", UploadAndCopy),
                ("Cancel", null));
        }

        if (response == "Error: File exist, clone")
        {
            RootView_v2.Instance.dialog.ShowBottomMultiline("Not activity owner! Please choose:",
                ("Clone", UploadAndCopy),
                ("Cancel", null));
        }

        if (result) Toast.Instance.Show("Upload completed successfully");
    }

    private async void UploadAndUpdate()
    {
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 1);
        Toast.Instance.Show(result ? "Upload completed successfully" : response);
    }

    private async void UploadAndCopy()
    {
        activityManager.CloneActivity();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 2);
        Toast.Instance.Show(result ? "Upload completed successfully" : response);
    }

    private async void DeleteFromServer()
    {
        var result = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.DeleteArlem(_container.ItemID, _container.FileIdentifier);
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
