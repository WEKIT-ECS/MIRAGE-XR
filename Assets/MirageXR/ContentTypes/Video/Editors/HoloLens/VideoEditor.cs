using System.IO;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Action = LearningExperienceEngine.Action;

public class VideoEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopRecordingButton;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private Toggle stepTrigger;

    const string httpPrefix = "http://";

    private LearningExperienceEngine.Action action;
    private LearningExperienceEngine.ToggleObject annotationToEdit;

    private bool isRecording;
    private string newFileName;
    private bool videoWasRecorded;

    private bool IsRecording
    {
        get => isRecording;
        set
        {
            isRecording = value;
            startRecordingButton.gameObject.SetActive(!isRecording);
            stopRecordingButton.gameObject.SetActive(isRecording);
        }
    }

    public void SetAnnotationStartingPoint(Transform startingPoint)
    {
        annotationStartingPoint = startingPoint;
    }

    private void Start()
    {
        stepTrigger.onValueChanged.AddListener(_ => OnTriggerValueChanged());
    }

    public void Close()
    {
        videoWasRecorded = false;
        IsRecording = false;
        RootObject.Instance.ImageTargetManager.enabled = true;
        action = null;
        annotationToEdit = null;
        newFileName = string.Empty;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
    {
        gameObject.SetActive(true);
        this.action = action;
        annotationToEdit = annotation;
        IsRecording = false;
        videoWasRecorded = false;

        if (annotationToEdit != null)
        {
            var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == annotationToEdit.poi) != null;
            stepTrigger.isOn = trigger;
        }
    }

    public void OnAccept()
    {

        if (IsRecording)
        {
            StopRecording();
        }

        if (!videoWasRecorded)
        {
            Debug.Log("[VideoEditor] just closing, no content was recorded.");
            // just close if no content was recorded
            SaveTriggerValue();
            Close();

            return;
        }

        if (annotationToEdit != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
            LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var startPointTr = annotationStartingPoint.transform;
            var offset = MirageXR.Utilities.CalculateOffset(startPointTr.position, startPointTr.rotation, originT.transform.position, originT.transform.rotation);

            annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "video";
        }

        // saving of the movie file has already happened since it has been written to file while recording
        annotationToEdit.url = httpPrefix + newFileName;

        LearningExperienceEngine.EventManager.ActivateObject(annotationToEdit);
        LearningExperienceEngine.EventManager.NotifyActionModified(action);

        SaveTriggerValue();
        Close();
    }

    private async Task<bool> DeleteOldVideoFile()
    {
        // delete the previous video file
        var originalFileName = Path.GetFileName(annotationToEdit.url.Remove(0, httpPrefix.Length));
        var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);
        if (File.Exists(originalFilePath) && annotationToEdit != null)
        {
            File.Delete(originalFilePath);
        }

        await Task.Yield();

        return true;
    }

    public async void StartRecording()
    {
        if (isRecording)
        {
            return;
        }

        RootObject.Instance.ImageTargetManager.enabled = false;
        IsRecording = true;

        if (annotationToEdit != null)
        {
            await DeleteOldVideoFile();
        }

        newFileName = $"MirageXR_Video_{System.DateTime.Now.ToFileTimeUtc()}.mp4";
        var filepath = Path.Combine(activityManager.ActivityPath, newFileName);

        NativeCameraController.StartRecordingVideo(filepath, OnVideoRecordingStopped);
    }

    private void OnVideoRecordingStopped(bool result, string path)
    {
        videoWasRecorded = result;
        IsRecording = false;

        RootObject.Instance.ImageTargetManager.enabled = true;

        if (result)
        {
            if (previewImage.texture)
            {
                Destroy(previewImage.texture);
            }

            previewImage.texture = NativeCameraController.GetVideoThumbnail(path);
        }
    }

    /// <summary>
    /// User has intended to stop recording
    /// </summary>
    public void StopRecording()
    {
        NativeCameraController.StopRecordVideo();
    }

    private void OnTriggerValueChanged()
    {
        if (stepTrigger.isOn && activityManager.IsLastAction(action))
        {
            // give the info and close
            DialogWindow.Instance.Show("Info!",
            "This is the last step. The trigger is disabled!\n Add a new step and try again.",
            new DialogButtonContent("Ok"));

            stepTrigger.isOn = false;
        }
        else
        {
            SaveTriggerValue();
        }
    }

    private void SaveTriggerValue()
    {
        if (stepTrigger.isOn)
        {
            if (annotationToEdit == null)
            {
                return;
            }

            action.AddArlemTrigger(LearningExperienceEngine.TriggerMode.Video, LearningExperienceEngine.ActionType.Video, annotationToEdit.poi);
        }
        else
        {
            if (annotationToEdit == null)
            {
                return;
            }

            action.RemoveArlemTrigger(annotationToEdit);
        }
    }
}
