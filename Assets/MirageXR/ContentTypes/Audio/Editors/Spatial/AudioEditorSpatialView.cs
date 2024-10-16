using LearningExperienceEngine;
using DG.Tweening;
using MirageXR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

public class AudioEditorSpatialView : PopupBase
{
    public class IntHolder : ObjectHolder<int> { }

    private const float DEFAULT_RANGE = 3.0f;
    private const float MIN_RANGE = 0.0f;
    private const float MAX_RANGE = 10.0f;
    private const float REWIND_VALUE = 10f;
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    private const string AUDIO_FILE_EXTENSION_WAV = "wav";
    private const string AUDIO_FILE_EXTENSION_MP3 = "mp3";

    private float _currentRangeValue;

    public ContentType Type => ContentType.Audio;

    [SerializeField] private Button _btnApply;

    [SerializeField] private Button _btnAudioSettings;
    [SerializeField] private Button _btnMicRecording;
    [SerializeField] private Button _btnMicReRecording;
    [SerializeField] private Button _btnDeviceFolder;
    [SerializeField] private Button _btnDeviceFolderPlayAudioPanel;
    [SerializeField] private Button _btnRecord;
    [SerializeField] private Button _btnStop;
    [SerializeField] private Button _btnPlay;
    [SerializeField] private Button _btnPause;
    [SerializeField] private Button _btnRewindBack;
    [SerializeField] private Button _btnRewindForward;

    [SerializeField] private Toggle _toggle3D;
    //[SerializeField] private Toggle _toggle2D;
    [SerializeField] private Toggle _toggleLoop;
    [SerializeField] private Button _btnIncreaseRange;
    [SerializeField] private Button _btnDecreaseRange;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private GameObject _objJumpToStep;

    [SerializeField] private TMP_Text _txtSliderRangeValue;
    [SerializeField] private GameObject _panelRange;
    [SerializeField] private ClampedScrollRect _clampedScrollJumpToStep;
    [SerializeField] private GameObject _templatePrefab;
    [Space]
    [SerializeField] private TMP_Text _txtTimer;
    [SerializeField] private Slider _sliderPlayer;
    [SerializeField] private Image _imgRecordingIcon;
    [SerializeField] private CanvasGroup _groupPlayControls;
    [Space]
    [SerializeField] private TMP_Text _txtTimerFrom;
    [SerializeField] private TMP_Text _txtTimerTo;
    [Space]
    [Header("Panels:")]
    [SerializeField] private GameObject _panelRecordControls;
    [SerializeField] private GameObject _panelAudioSettings;
    [SerializeField] private GameObject _panelBottomButtons;
    [Space]
    [SerializeField] private GameObject _topContainer;
    [SerializeField] private GameObject _topContainerPlayAudio;
    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private AudioSource _audioSource;

    private AudioClip _audioClip;
    private string _fileName;
    private Coroutine _updateSliderPlayerCoroutine;
    private Coroutine _updateRecordTimerCoroutine;
    private float _recordStartTime;
    private int _scrollRectStep;
    private string[] _audioFileType;
    private Content<AudioContentData> _content;

    private string _inputTriggerStepNumber = string.Empty;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);

        _toggle3D.isOn = false;
        _toggleLoop.isOn = false;
        _currentRangeValue = DEFAULT_RANGE;
        _txtSliderRangeValue.text = DEFAULT_RANGE.ToString("0");

        _panelRange.SetActive(false);
        _topContainer.SetActive(true);
        _topContainerPlayAudio.SetActive(false); 
        _panelRecordControls.SetActive(false);
        _panelBottomButtons.SetActive(false);
        _panelAudioSettings.SetActive(true);

        _btnApply.onClick.AddListener(OnAccept);
        _btnAudioSettings.onClick.AddListener(OnOpenAudioSettings);
        _btnMicRecording.onClick.AddListener(OnOpenRecordControlsPanel);
        _btnMicReRecording.onClick.AddListener(OnOpenRecordControlsPanel);
        _btnDeviceFolder.onClick.AddListener(OnOpenDeviceFolder);
        _btnDeviceFolderPlayAudioPanel.onClick.AddListener(OnOpenDeviceFolder);

        _btnRecord.onClick.AddListener(OnRecordStarted);
        _btnStop.onClick.AddListener(OnRecordStopped);
        _btnPlay.onClick.AddListener(OnPlayingStarted);
        _btnPause.onClick.AddListener(OnPlayingPaused);
        _btnRewindBack.onClick.AddListener(OnRewindBack);
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        _btnRewindForward.onClick.AddListener(OnRewindForward);
        _btnIncreaseRange.onClick.AddListener(OnIncreaseRange);
        _btnDecreaseRange.onClick.AddListener(OnDecreaseRange);
        _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);

        _sliderPlayer.minValue = 0;
        _sliderPlayer.maxValue = 1f;
        _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);
        _clampedScrollJumpToStep.onItemChanged.AddListener(OnItemJumpToStepChanged);

        _toggle3D.onValueChanged.AddListener(On3DSelected);
        
        _audioFileType = new[] { NativeFilePicker.ConvertExtensionToFileType(AUDIO_FILE_EXTENSION_WAV),
                                 NativeFilePicker.ConvertExtensionToFileType(AUDIO_FILE_EXTENSION_MP3) };

        /*var steps = activityManager.ActionsOfTypeAction;
        var stepsCount = steps.Count;
        InitClampedScrollRect(_clampedScrollJumpToStep, _templatePrefab, stepsCount, stepsCount.ToString());*/

        if (_content != null)
        {
            _topContainer.SetActive(false);
            _topContainerPlayAudio.SetActive(true);
            LoadContent();
            _groupPlayControls.interactable = true;
            //var trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            /*if (trigger != null)
            {
                _inputTriggerStepNumber = trigger.value;
                _scrollRectStep = int.Parse(_inputTriggerStepNumber) - 1;
                _toggleTrigger.isOn = true;
            }*/
            OnClickRecordComplete();
        }
        else
        {
            _fileName = $"MirageXR_Audio_{DateTime.Now.ToFileTimeUtc()}.wav";
            _groupPlayControls.interactable = false;

            _topContainer.SetActive(true);
            _topContainerPlayAudio.SetActive(false);
        }

        SetPlayerActive(true);
        UpdateSliderPlayerAndTimer();
        RootView_v2.Instance.HideBaseView();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        if (args is { Length: 1 } && args[0] is Content<AudioContentData> obj)
        {
            _content = obj;
        }

        return true;
    }

    /*private void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount, string text)
    {
        var currentActionId = activityManager.ActiveAction.id;
        var steps = activityManager.ActionsOfTypeAction;

        for (int i = 1; i <= maxCount; i++)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = i;
            obj.GetComponentInChildren<TMP_Text>().text = $"   {i}/{text}     {steps[i - 1].instruction.title}";

            if (steps[i - 1].id == currentActionId)
            {
                _scrollRectStep = i - 1;
            }
        }
    }*/

    private void OnDestroy()
    {
        if (_audioClip)
        {
            Destroy(_audioClip);
            _audioClip = null;
        }
        RootView_v2.Instance.ShowBaseView();
    }

    private void LoadContent()
    {
        var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
        var folderPath = RootObject.Instance.LEE.AssetsManager.GetFolderPath(activityId, _content.Id, _content.ContentData.Audio.Id);
        var filePath = Path.Combine(folderPath, "audio.wav");
        _audioClip = SaveLoadAudioUtilities.LoadAudioFile(filePath);

        _toggle3D.isOn = _content.ContentData.Is3dSound;
        //_toggle2D.isOn = !_toggle3D.isOn;
        _panelRange.SetActive(_toggle3D.isOn);
        _toggleLoop.isOn = _content.ContentData.IsLooped;
        _txtSliderRangeValue.text = _content.ContentData.SoundRange.ToString("00");
        _currentRangeValue = _content.ContentData.SoundRange;
    }

    private void OnPlayingStarted()
    {
        if (!_audioClip) return;

        if (_audioSource.clip && _audioSource.time > 0)
        {
            _audioSource.UnPause();
        }
        else
        {
            _audioSource.clip = _audioClip;
            _audioSource.Play();
        }

        StartUpdateSliderPlayerAndTimer();
    }

    private static string GetFileName(LearningExperienceEngine.ToggleObject content)
    {
        const string httpPrefix = "http://";
        var fileName = content.url.StartsWith(httpPrefix) ? content.url.Remove(0, httpPrefix.Length) : content.url;
        return fileName;
    }

    private void OnPlayingPaused()
    {
        _audioSource.Pause();
    }

    private void OnRewindBack()
    {
        _audioSource.time = Mathf.Max(_audioSource.time - REWIND_VALUE, 0);
        UpdateSliderPlayerAndTimer();
    }

    private void OnRewindForward()
    {
        _audioSource.time = Mathf.Min(_audioSource.time + REWIND_VALUE, _audioClip.length - 0.1f);
        UpdateSliderPlayerAndTimer();
    }

    private void OnRecordStarted()
    {
        _audioSource.clip = null;
        if (_audioClip)
        {
            Destroy(_audioClip);
            _audioClip = null;
        }

        SetPlayerActive(false);

        _recordStartTime = Time.unscaledTime;
        AudioRecorder.Start(_fileName);
        StartUpdateRecordTimer();
    }

    private void OnRecordStopped()
    {
        _recordStartTime = 0;
        SetPlayerActive(true);
        _audioClip = AudioRecorder.Stop();
        _groupPlayControls.interactable = true;
        StopCoroutine(_updateRecordTimerCoroutine);
        
        OnClickRecordComplete();
        _topContainer.SetActive(false);
        _topContainerPlayAudio.SetActive(true);
    }

    private void OnClickRecordComplete()
    {
        _panelRecordControls.SetActive(false);
        _panelAudioSettings.SetActive(false);
        _txtTimerTo.text = ToTimeFormatMinutes(_audioClip.length);
        OnOpenAudioSettings();
    }

    private void StartUpdateSliderPlayerAndTimer()
    {
        _updateSliderPlayerCoroutine = StartCoroutine(UpdateSliderPlayerAndTimerIEnumerator());
    }

    private IEnumerator UpdateSliderPlayerAndTimerIEnumerator()
    {
        while (_audioSource.isPlaying)
        {
            UpdateSliderPlayerAndTimer();
            yield return null;
        }
        UpdateSliderPlayerAndTimer();
    }

    private void UpdateSliderPlayerAndTimer()
    {
        var clip = _audioSource.clip;
        if (clip)
        {
            var percent = _audioSource.time / clip.length;
            _sliderPlayer.onValueChanged.RemoveListener(OnSliderPlayerValueChanged);
            _sliderPlayer.value = percent;
            _txtTimerFrom.text = ToTimeFormatMinutes(_audioSource.time);
            _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);

            if (Math.Abs((int)_audioSource.time - (int)clip.length) <= float.Epsilon)
            {
                OnPlayingFinished();
            }
        }
        else
        {
            ResetPlayerTimer();
        }
    }

    private void OnPlayingFinished()
    {
        _audioSource.Stop();
        _audioSource.time = 0;
        _audioSource.timeSamples = 0;
        _audioSource.clip = null;
        _btnPlay.gameObject.SetActive(true);
        _btnPause.gameObject.SetActive(false);
        _txtTimerFrom.text = ToTimeFormatMinutes(0);
        ResetPlayerTimer();
        if (_updateSliderPlayerCoroutine != null)
        {
            StopCoroutine(_updateSliderPlayerCoroutine);
        }
    }

    private void ResetPlayerTimer()
    {
        _sliderPlayer.onValueChanged.RemoveListener(OnSliderPlayerValueChanged);
        _sliderPlayer.value = 0;
        _txtTimer.text = ToTimeFormat(0);
        _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);
    }

    private void StartUpdateRecordTimer()
    {
        _updateRecordTimerCoroutine = StartCoroutine(UpdateRecordTimerIEnumerator());
    }

    private IEnumerator UpdateRecordTimerIEnumerator()
    {
        const float second = 1.0f;

        while (AudioRecorder.IsRecording)
        {
            UpdateRecordTimer();
            yield return new WaitForSeconds(second);
        }
        UpdateRecordTimer();
    }

    private void UpdateRecordTimer()
    {
        _txtTimer.text = ToTimeFormat(Time.unscaledTime - _recordStartTime);
    }

    private static string ToTimeFormat(float time)
    {
        var hours = (int)time / 3600;
        var minutes = ((int)time / 60) % 60;
        var seconds = (int)time % 60;
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    private static string ToTimeFormatMinutes(float time)
    {
        var minutes = ((int)time / 60) % 60;
        var seconds = (int)time % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    private void SetPlayerActive(bool value)
    {
        _imgRecordingIcon.gameObject.SetActive(!value);
    }

    private void On3DSelected(bool value)
    {
        _panelRange.SetActive(value);
    }

    private void OnSliderRangeValueChanged(float value)
    {
        _txtSliderRangeValue.text = value.ToString("0");
    }

    private void OnIncreaseRange()
    {
        if (_currentRangeValue < MAX_RANGE)
        {
            _currentRangeValue++;
        }

        _txtSliderRangeValue.text = _currentRangeValue.ToString("0");
    }

    private void OnDecreaseRange()
    {
        if (_currentRangeValue > MIN_RANGE)
        {
            _currentRangeValue--;
        }
        _txtSliderRangeValue.text = _currentRangeValue.ToString("0");
    }

    private void OnSliderPlayerValueChanged(float value)
    {
        _audioSource.time = _audioClip.length * value;
    }

    private void OnItemJumpToStepChanged(Component item)
    {
        _inputTriggerStepNumber = item.GetComponent<ObjectHolder<int>>().item.ToString();
    }

    private void OnOpenAudioSettings()
    {
        _panelBottomButtons.SetActive(false);
        _panelAudioSettings.SetActive(true);

        _objJumpToStep.SetActive(true);
        _clampedScrollJumpToStep.currentItemIndex = _scrollRectStep;
        _objJumpToStep.SetActive(_toggleTrigger.isOn);

    }

    private void OnOpenRecordControlsPanel()
    {
        _panelBottomButtons.SetActive(false);
        _panelAudioSettings.SetActive(false);
        _panelRecordControls.SetActive(true);
        _txtTimer.text = ToTimeFormat(0);
    }

    private void OnOpenDeviceFolder()
    {
        if (NativeFilePicker.IsFilePickerBusy())
        {
            return;
        }
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile(
            (path) =>
            {
                if (path == null)
                {
                    Debug.Log("Operation cancelled");
                }
                else
                {
                    Debug.Log("Picked file: " + path);
                    StartCoroutine(LoadAudioClip(path));
                }
            }, _audioFileType );
        Debug.Log("Permission result: " + permission);
    }
    
    private  IEnumerator LoadAudioClip(string path)
    {
        var correctedPath = "file://" + path;
        using (WWW www = new WWW(correctedPath))
        {
            yield return www;

            if (www.error != null)
            {
                Debug.LogError("Failed to load audio: " + www.error);
            }
            else
            {
                AudioType myAudioType = AudioType.WAV;
                if (Path.GetExtension(path).ToLower() == ".mp3")
                {
                    myAudioType = AudioType.MPEG;
                }
                Debug.Log("File format: " + myAudioType);
                _audioClip = www.GetAudioClip(false, false, myAudioType);
                
                _recordStartTime = 0;
                SetPlayerActive(true);
                _groupPlayControls.interactable = true;
                OnClickRecordComplete();
                _topContainer.SetActive(false);
                _topContainerPlayAudio.SetActive(true);
            }
        }
    }

    private void OnAccept()
    {
        OnAcceptAsync().Forget();
    }

    private async UniTask OnAcceptAsync()
    {
        if (!_audioClip)
        {
            //Toast.Instance.Show("The audio has not been recorded");
            AppLog.LogWarning("The audio has not been recorded");
            return;
        }

        var step = RootObject.Instance.LEE.StepManager.CurrentStep;
        var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
        var fileId = _content?.ContentData?.Audio?.Id ?? Guid.NewGuid();

        _content ??= new Content<AudioContentData>
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            IsVisible = true,
            Steps = new List<Guid> { step.Id },
            Type = ContentType.Image,
            Version = Application.version,
            ContentData = new AudioContentData
            {
                Triggers = null,
                AvailableTriggers = null,
                Audio = null,
                IsLooped = false,
                Is3dSound = false,
                SoundRange = 0
            },
            Location = Location.GetIdentityLocation()
        };

        _content.ContentData.Is3dSound = _toggle3D.isOn;
        _content.ContentData.IsLooped = _toggleLoop.isOn;
        _content.ContentData.SoundRange = _sliderPlayer.value;

        SaveAudio(activityId, _content.Id, fileId);
        _content.ContentData.Audio = await RootObject.Instance.LEE.AssetsManager.CreateFileAsync(activityId, _content.Id, fileId);
        RootObject.Instance.LEE.ContentManager.AddContent(_content);
        RootObject.Instance.LEE.AssetsManager.UploadFileAsync(activityId, _content.Id, fileId);

        Close();
    }

    private void SaveAudio(Guid activityId, Guid contentId, Guid fileId)
    {
        if (_audioClip == null || _content == null)
        {
            return;
        }

        var folder = RootObject.Instance.LEE.AssetsManager.GetFolderPath(activityId, contentId, fileId);
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "audio.wav");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        SaveLoadAudioUtilities.Save(filePath, _audioClip);
    }

    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }

    private void OnToggleTriggerValueChanged(bool value)
    {
        _objJumpToStep.SetActive(value);
    }
}
