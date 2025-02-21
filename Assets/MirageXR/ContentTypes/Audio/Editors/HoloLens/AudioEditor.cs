using LearningExperienceEngine;
using MirageXR;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Action = LearningExperienceEngine.Action;

public class AudioEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
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

    private LearningExperienceEngine.Action _action;
    private LearningExperienceEngine.ToggleObject _annotationToEdit;

    private AudioClip _capturedClip;

    private bool _isRecording;
    private bool _isPaused;
    private int _timerSeconds = 0;


    public AudioSource PlayerAudioSource => audioSource;

    public DialogRecorder DialogRecorderPanel
    {
        get; set;
    }

    public string SaveFileName { get; set; }

    public bool IsRecording
    {
        get => _isRecording;
        private set
        {
            _isRecording = value;
            timerIcon.enabled = _isRecording; // recorder circle shows only on recording
            startRecordingButton.interactable = !_isRecording;
        }
    }

    public bool IsPlaying
    {
        get; private set;
    }


    public AudioClip CapturedClip()
    {
        return _capturedClip;
    }


    private void Start()
    {
        stepTrigger.onValueChanged.AddListener(delegate { OnTriggerValueChanged(); });
    }

    private void Update()
    {
        if (!gameObject) return;

        if (gameObject.activeInHierarchy && !_isRecording && IsPlaying)
        {
            progress.value = audioSource.time / audioSource.clip.length;
            UpdatePlayBackTimer();
        }
        else
            timerText.text = SecToTimeFormat(_timerSeconds);
    }

    private IEnumerator Timer()
    {
        _timerSeconds = 0;

        while (_isRecording)
        {
            yield return new WaitForSeconds(1);
            _timerSeconds++;
        }
    }

    private void UpdateUI()
    {
        if (!gameObject) return;

        stopRecordingButton.interactable = IsPlaying || _isRecording;
        pauseButton.gameObject.SetActive(!_isPaused && IsPlaying);
        playButton.interactable = !IsRecording;
        progress.gameObject.SetActive(!_isRecording && _capturedClip);
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
        if (_annotationToEdit != null)
        {
            var audioPlayer = GameObject.Find(_annotationToEdit.poi).GetComponentInChildren<AudioPlayer>();
            if (_annotationToEdit != null && audioPlayer != null)
            {
                audioPlayer.PlayAudio();
            }
        }


        StopRecording();
        _action = null;
        _annotationToEdit = null;
        _capturedClip = null;
        SaveFileName = string.Empty;
        IsPlaying = false;

        // play the loop audio which were stopped on recording
        PlayAllLoopedVideo();

        // destroy the editor
        foreach (var ae in FindObjectsOfType<AudioEditor>())
            Destroy(ae.gameObject);
    }

    public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
    {
        gameObject.SetActive(true);
        this._action = action;
        _annotationToEdit = annotation;
        IsRecording = false;
        _timerSeconds = 0;
        audiotype.isOn = false;

        foreach (var foundAudioSource in FindObjectsOfType<AudioSource>())
            foundAudioSource.Stop();

        if (audioSource == null)
        {
            audioSource = GameObject.Find(annotation.id).GetComponentInChildren<AudioSource>();
        }

        if (_annotationToEdit != null)
        {
            SaveFileName = _annotationToEdit.url;

            _capturedClip = SaveLoadAudioUtilities.LoadAudioFile(GetExistingAudioFile());

            if (_annotationToEdit.option.Contains("3d"))
            {
                audiotype.isOn = true;
                loop.isOn = _annotationToEdit.option.Split('#')[1] == "1";
                radius.text = _annotationToEdit.option.Split('#')[2];
                OnAudioTypeToggle();
            }

            // check if the trigger for this audio is on
            stepTrigger.isOn = activityManager.ActiveAction.triggers.Find(t => t.id == _annotationToEdit.poi) != null;

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
        if (_isPaused)
        {
            audioSource.Play();
            _isPaused = false;
        }
        else
        {
            if (_capturedClip != null)
            {
                audioSource.clip = _capturedClip;
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
            activityManager.ActionsOfTypeAction.IndexOf(_action) == activityManager.ActionsOfTypeAction.Count - 1)
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
            if (_annotationToEdit == null) return;
            _action.AddOrReplaceArlemTrigger(LearningExperienceEngine.TriggerMode.Audio, LearningExperienceEngine.ActionType.Audio, _annotationToEdit.poi, audioSource.clip.length, string.Empty);
        }
        else
        {
            _action.RemoveArlemTrigger(_annotationToEdit);
        }
    }

    public void Initialize(LearningExperienceEngine.Action action)
    {
        this._action = action;
        _annotationToEdit = null;
        IsRecording = false;
        SaveFileName = string.Empty;
    }

    private string GetExistingAudioFile()
    {
        var audioName = _annotationToEdit.url;
        const string httpPrefix = "http://";

        string originalFileName = !audioName.StartsWith(httpPrefix) ? Path.Combine(Application.persistentDataPath, audioName)
            : Path.Combine(activityManager.ActivityPath, Path.GetFileName(audioName.Remove(0, httpPrefix.Length)));

        string originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

        // On character dialog recorder, use the custom dialog file path instead of annotationToEdit.url
        // set the correct dialog recorder(correct character) to the audio player
        foreach (var character in FindObjectsOfType<MirageXR.CharacterController>())
        {
            if (character.MyAction == _action && character.DialogRecorder.DialogSaveName != string.Empty)
            {
                SaveFileName = character.DialogRecorder.DialogSaveName;
                originalFilePath = Path.Combine(activityManager.ActivityPath, SaveFileName);
                GameObject.Find(_annotationToEdit.poi).GetComponentInChildren<AudioPlayer>().DialogRecorderPanel = character.transform.GetChild(0).GetComponentInChildren<DialogRecorder>(); // TODO: Possible NRE
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
        if (_annotationToEdit != null)
        {
            // delete the previous audio file if a new file is recorded
            var originalFilePath = GetExistingAudioFile();
            if (File.Exists(originalFilePath) && SaveFileName != string.Empty && _annotationToEdit.url == null)
            {
                LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);
                File.Delete(originalFilePath);
            }

            // edit audio type , loop and radius as option
            AudioOptionsAdjustment(_annotationToEdit);
        }
        else
        {
            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
            LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_action.id));
            GameObject originT = GameObject.Find(detectable.id);

            // move the audio player to the spawn point
            var annotationStartingPoint = GameObject.Find("AnnotationSpawnPoint");

            var offset = MirageXR.Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            _annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
            _annotationToEdit.predicate = "audio";

            // save audio type , loop and radius as option
            AudioOptionsAdjustment(_annotationToEdit);

            _annotationToEdit.scale = 0.5f;
        }

        if (SaveFileName != string.Empty)
        {
            _annotationToEdit.url = $"http://{SaveFileName}";

            LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(_action);

            var player = GameObject.Find(_annotationToEdit.poi).GetComponentInChildren<AudioPlayer>();
            if (player != null)
            {
                player.DialogRecorderPanel = DialogRecorderPanel;
            }
        }

        SaveTriggerValue();
        Close();
    }


    private void AudioOptionsAdjustment(LearningExperienceEngine.ToggleObject annotationToEdit)
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
        RootObject.Instance.LEE.AudioManager.Start();

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

        if (_isRecording)
        {
            _capturedClip = RootObject.Instance.LEE.AudioManager.Stop();

            IsRecording = false;

            audioSource.clip = _capturedClip;
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
        SaveLoadAudioUtilities.Save(fullFilePath, _capturedClip);
    }

    public void PauseAudio()
    {
        _isPaused = true;
        audioSource.Pause();

        UpdateUI();
    }

    private void ResetSlider()
    {
        IsPlaying = false;
        _isPaused = false;
        UpdateUI();
    }

    public void ChangeAudioTime()
    {
        if (!audioSource.clip || _isRecording || !gameObject.activeInHierarchy) return;
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
