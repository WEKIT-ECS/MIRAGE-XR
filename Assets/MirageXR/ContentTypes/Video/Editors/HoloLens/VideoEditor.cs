using System.IO;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class VideoEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopRecordingButton;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private Toggle stepTrigger;

    const string httpPrefix = "http://";

    private Action action;
    private ToggleObject annotationToEdit;

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
        RootObject.Instance.imageTargetManager.enabled = true;
        action = null;
        annotationToEdit = null;
        newFileName = string.Empty;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(Action action, ToggleObject annotation)
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
        Debug.Log("Video: Accept called");
        if (IsRecording)
        {
            StopRecording();
        }

        /*
#if UNITY_EDITOR
        if (annotationToEdit == null)
        {
            // create dummy video clip so that the augmentation can be created in Unity (debugging only)
            newFileName = "videoTest_MP4.mp4";
            string targetPath = Path.Combine(activityManager.ActivityPath, newFileName);

            try
            {
                if (!File.Exists(targetPath))
                {
                    File.Copy($"{Application.dataPath}/MirageXR/Player/videoTest_MP4.mp4", targetPath);
                    File.Create(newFileName);
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError(e);
            }

            videoWasRecorded = true;
        }
        else
        {
            Debug.LogError("In editor you are only able to edit the trigger setting. Video recording is not possible in the editor.");
            SaveTriggerValue();
            Close();
            return;
        }
#endif
        */
        Debug.Log("videowasrecorded = " + videoWasRecorded);
        if (!videoWasRecorded)
        {
            Debug.Log("just closing, no content was recorded.");
            // just close if no content was recorded
            SaveTriggerValue();
            Close();

            return;
        }

        if (annotationToEdit != null)
        {
            EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var startPointTr = annotationStartingPoint.transform;
            var offset = MirageXR.Utilities.CalculateOffset(startPointTr.position, startPointTr.rotation, originT.transform.position, originT.transform.rotation);

            annotationToEdit = RootObject.Instance.augmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "video";
        }

        // saving of the movie file has already happened since it has been written to file while recording
        annotationToEdit.url = httpPrefix + newFileName;

        EventManager.ActivateObject(annotationToEdit);
        EventManager.NotifyActionModified(action);

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
            Debug.Log("Is already recording");
            return;
        }

        Debug.Log("Record Video");

        RootObject.Instance.imageTargetManager.enabled = false;
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
        Debug.Log("recording was stopped");
        videoWasRecorded = result;
        IsRecording = false;

        RootObject.Instance.imageTargetManager.enabled = true;

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

            action.AddArlemTrigger(TriggerMode.Video, ActionType.Video, annotationToEdit.poi);
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
