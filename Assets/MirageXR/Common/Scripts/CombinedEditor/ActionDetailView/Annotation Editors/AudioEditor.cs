using MirageXR;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class AudioEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopRecordingButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Image timerIcon;
    [SerializeField] private Text timerText;
    [SerializeField] private Text playBackTimerText;
    [SerializeField] private Slider progress;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Toggle audiotype;
    [SerializeField] private InputField radius;
    [SerializeField] private Toggle loop;
    [SerializeField] private Toggle stepTrigger;

    private Action action;
    private ToggleObject annotationToEdit;

    private AudioClip capturedClip;

    private bool isRecording;
    private bool isPaused;
    private int timerSeconds = 0;


    public AudioSource PlayerAudioSource => audioSource;

    public DialogRecorder DialogRecorderPanel
    {
        get; set;
    }

    public string SaveFileName { get; set; }

    public bool IsRecording
    {
        get => isRecording;
        private set
        {
            isRecording = value;
            timerIcon.enabled = isRecording; // recorder circle shows only on recording
            startRecordingButton.interactable = !isRecording;
        }
    }

    public bool IsPlaying
    {
        get; private set;
    }


    public AudioClip CapturedClip()
    {
        return capturedClip;
    }


    private void Start()
    {
        stepTrigger.onValueChanged.AddListener(delegate { OnTriggerValueChanged(); });
    }

    private void Update()
    {
        if (!gameObject) return;

        if (gameObject.activeInHierarchy && !isRecording && IsPlaying)
        {
            progress.value = audioSource.time / audioSource.clip.length;
            UpdatePlayBackTimer();
        }
        else
            timerText.text = SecToTimeFormat(timerSeconds);
    }

    private IEnumerator Timer()
    {
        timerSeconds = 0;

        while (isRecording)
        {
            yield return new WaitForSeconds(1);
            timerSeconds++;
        }
    }

    private void UpdateUI()
    {
        if (!gameObject) return;

        stopRecordingButton.interactable = IsPlaying || isRecording;
        pauseButton.gameObject.SetActive(!isPaused && IsPlaying);
        playButton.interactable = !IsRecording;
        progress.gameObject.SetActive(!isRecording && capturedClip);
        timerText.enabled = !progress.gameObject.activeInHierarchy;
    }

    private void UpdatePlayBackTimer()
    {
        playBackTimerText.text = SecToTimeFormat((int)audioSource.time) + "/" + SecToTimeFormat((int)audioSource.clip.length);
    }

    private static string SecToTimeFormat(int seconds)
    {
        var mins = Mathf.Floor(seconds / 60 % 60);
        var secs = Mathf.Floor(seconds % 60);

        return $"{mins:00}:{secs:00}";
    }


    public void Close()
    {
        // when editor is closed play the spatial audio if it is exist
        if (annotationToEdit != null)
        {
            var audioPlayer = GameObject.Find(annotationToEdit.poi).GetComponentInChildren<AudioPlayer>();
            if (annotationToEdit != null && audioPlayer != null)
            {
                audioPlayer.PlayAudio();
            }
        }


        StopRecording();
        action = null;
        annotationToEdit = null;
        capturedClip = null;
        SaveFileName = string.Empty;
        IsPlaying = false;

        // play the loop audio which were stopped on recording
        PlayAllLoopedVideo();

        // destroy the editor
        foreach (var ae in FindObjectsOfType<AudioEditor>())
            Destroy(ae.gameObject);
    }

    public void Open(Action action, ToggleObject annotation)
    {
        gameObject.SetActive(true);
        this.action = action;
        annotationToEdit = annotation;
        IsRecording = false;
        timerSeconds = 0;
        audiotype.isOn = false;

        foreach (var foundAudioSource in FindObjectsOfType<AudioSource>())
            foundAudioSource.Stop();

        if (audioSource == null)
        {
            audioSource = GameObject.Find(annotation.id).GetComponentInChildren<AudioSource>();
        }

        if (annotationToEdit != null)
        {
            SaveFileName = annotationToEdit.url;

            capturedClip = SaveLoadAudioUtilities.LoadAudioFile(GetExistingAudioFile());

            if (annotationToEdit.option.Contains("3d"))
            {
                audiotype.isOn = true;
                loop.isOn = annotationToEdit.option.Split('#')[1] == "1";
                radius.text = annotationToEdit.option.Split('#')[2];
                OnAudioTypeToggle();
            }

            // check if the trigger for this audio is on
            stepTrigger.isOn = activityManager.ActiveAction.triggers.Find(t => t.id == annotationToEdit.poi) != null;

            // re-recording is not allowed
            startRecordingButton.interactable = false;

            PlayAudio();
        }
        else
        {
            SaveFileName = string.Empty;
        }
    }

    public void PlayAudio()
    {
        IsPlaying = true;
        PlayAudioFileAt(audioSource.time);
    }

    public void PlayAudioFileAt(float slideValue = 0)
    {
        if (isPaused)
        {
            audioSource.Play();
            isPaused = false;
        }
        else
        {
            if (capturedClip != null)
            {
                audioSource.clip = capturedClip;
                audioSource.time = slideValue;
                audioSource.Play();
                IsPlaying = true;
            }
        }

        UpdateUI();
    }

    private void OnTriggerValueChanged()
    {
        loop.interactable = audiotype.isOn && !stepTrigger.isOn;

        // disable loop if trigger is active
        if (stepTrigger.isOn) loop.isOn = false;

        if (stepTrigger.isOn &&
            activityManager.ActionsOfTypeAction.IndexOf(action) == activityManager.ActionsOfTypeAction.Count - 1)
        {
            // give the info and close
            DialogWindow.Instance.Show("Info!",
            "This is the last step. The trigger is disabled!\n Add a new step and try again.",
            new DialogButtonContent("Ok"));

            stepTrigger.isOn = false;
        }
    }

    private void SaveTriggerValue()
    {
        if (stepTrigger.isOn)
        {
            if (annotationToEdit == null) return;
            action.AddOrReplaceArlemTrigger(TriggerMode.Audio, ActionType.Audio, annotationToEdit.poi, audioSource.clip.length, string.Empty);
        }
        else
        {
            action.RemoveArlemTrigger(annotationToEdit);
        }
    }

    public void Initialize(Action action)
    {
        this.action = action;
        annotationToEdit = null;
        IsRecording = false;
        SaveFileName = string.Empty;
    }

    private string GetExistingAudioFile()
    {
        var audioName = annotationToEdit.url;
        const string httpPrefix = "http://";

        string originalFileName = !audioName.StartsWith(httpPrefix) ? Path.Combine(Application.persistentDataPath, audioName)
            : Path.Combine(activityManager.ActivityPath, Path.GetFileName(audioName.Remove(0, httpPrefix.Length)));

        string originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

        // On character dialog recorder, use the custom dialog file path instead of annotationToEdit.url
        // set the correct dialog recorder(correct character) to the audio player
        foreach (var character in FindObjectsOfType<MirageXR.CharacterController>())
        {
            if (character.MyAction == action && character.DialogRecorder.DialogSaveName != string.Empty)
            {
                SaveFileName = character.DialogRecorder.DialogSaveName;
                originalFilePath = Path.Combine(activityManager.ActivityPath, SaveFileName);
                GameObject.Find(annotationToEdit.poi).GetComponentInChildren<AudioPlayer>().DialogRecorderPanel = character.transform.GetChild(0).GetComponentInChildren<DialogRecorder>(); // TODO: Possible NRE
                break;
            }
        }

        return originalFilePath;
    }

    private static void PlayAllLoopedVideo()
    {
        foreach (var audioPlayer in FindObjectsOfType<AudioPlayer>())
        {
            if (audioPlayer.Loop)
            {
                audioPlayer.PlayAudio();
                audioPlayer.IconImage.sprite = audioPlayer.IconSprite;
            }
        }
    }

    public void OnAccept()
    {
        StopRecording();
        if (annotationToEdit != null)
        {
            // delete the previous audio file if a new file is recorded
            var originalFilePath = GetExistingAudioFile();
            if (File.Exists(originalFilePath) && SaveFileName != string.Empty && annotationToEdit.url == null)
            {
                EventManager.DeactivateObject(annotationToEdit);
                File.Delete(originalFilePath);
            }

            // edit audio type , loop and radius as option
            AudioOptionsAdjustment(annotationToEdit);
        }
        else
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            // move the audio player to the spawn point
            var annotationStartingPoint = GameObject.Find("AnnotationSpawnPoint");

            var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            annotationToEdit = RootObject.Instance.augmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "audio";

            // save audio type , loop and radius as option
            AudioOptionsAdjustment(annotationToEdit);

            annotationToEdit.scale = 0.5f;
        }

        if (SaveFileName != string.Empty)
        {
            annotationToEdit.url = $"http://{SaveFileName}";

            EventManager.ActivateObject(annotationToEdit);
            EventManager.NotifyActionModified(action);

            var player = GameObject.Find(annotationToEdit.poi).GetComponentInChildren<AudioPlayer>();
            if (player != null)
            {
                player.DialogRecorderPanel = DialogRecorderPanel;
            }
        }

        SaveTriggerValue();
        Close();
    }


    private void AudioOptionsAdjustment(ToggleObject annotationToEdit)
    {
        // save audio type , loop and radius as option
        if (!audiotype.isOn)
            annotationToEdit.option = "2d";
        else
            annotationToEdit.option = "3d";

        if (loop.isOn)
            annotationToEdit.option += "#1";
        else
            annotationToEdit.option += "#0";

        if (audiotype.isOn)
            annotationToEdit.option += "#" + radius.text;
    }


    public void StartRecording()
    {
        audioSource.Stop();

        IsRecording = true;
        IsPlaying = false;
        if (string.IsNullOrEmpty(SaveFileName))
        {
            SaveFileName = $"MirageXR_Audio_{System.DateTime.Now.ToFileTimeUtc()}.wav";
        }
        AudioRecorder.Start();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Timer());
            UpdateUI();
        }
    }

    public void StopRecording()
    {
        audioSource.time = 0;
        progress.value = 0;

        if (IsPlaying)
        {
            audioSource.Stop();
            IsPlaying = false;
        }

        if (isRecording)
        {
            capturedClip = AudioRecorder.Stop();

            IsRecording = false;

            audioSource.clip = capturedClip;
            UpdatePlayBackTimer();

            SaveRecording();
        }

        ResetSlider();
    }

    private void SaveRecording()
    {
        const string httpPrefix = "http://";
        if (SaveFileName.StartsWith(httpPrefix))
        {
            SaveFileName = SaveFileName.Remove(0, httpPrefix.Length);
        }
        string fullFilePath = Path.Combine(activityManager.ActivityPath, SaveFileName);
        SaveLoadAudioUtilities.Save(fullFilePath, capturedClip);
    }

    public void PauseAudio()
    {
        isPaused = true;
        audioSource.Pause();

        UpdateUI();
    }

    private void ResetSlider()
    {
        IsPlaying = false;
        isPaused = false;
        UpdateUI();
    }

    public void ChangeAudioTime()
    {
        if (!audioSource.clip || isRecording || !gameObject.activeInHierarchy) return;
        if (audioSource.clip.length * progress.value < audioSource.clip.length)
        {
            audioSource.time = audioSource.clip.length * progress.value;
        }
        else
        {
            audioSource.time = audioSource.clip.length - 0.01f;
            ResetSlider();
        }
    }


    public void OnAudioTypeToggle()
    {
        radius.interactable = audiotype.isOn;
        if (!audiotype.isOn) loop.isOn = false;

        loop.interactable = audiotype.isOn && !stepTrigger.isOn;
    }
}
