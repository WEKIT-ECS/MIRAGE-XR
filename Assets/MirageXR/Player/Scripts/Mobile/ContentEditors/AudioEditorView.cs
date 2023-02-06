using System;
using System.Collections;
using System.IO;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioEditorView : PopupEditorBase
{
    private const float DEFAULT_RANGE = 3.0f;
    private const float MIN_RANGE = 0.0f;
    private const float MAX_RANGE = 10.0f;
    private const float REWIND_VALUE = 10f;

    public override ContentType editorForType => ContentType.AUDIO;

    [SerializeField] private Button _btnRecord;
    [SerializeField] private Button _btnStop;
    [SerializeField] private Button _btnPlay;
    [SerializeField] private Button _btnPause;
    [SerializeField] private Button _btnRewindBack;
    [SerializeField] private Button _btnRewindForward;

    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Toggle _toggle3D;
    [SerializeField] private Toggle _toggle2D;
    [SerializeField] private Toggle _toggleLoop;
    [SerializeField] private Slider _sliderRange;
    [SerializeField] private TMP_Text _txtSliderRangeValue;
    [SerializeField] private GameObject _panelRange;
    [SerializeField] private TMP_InputField _inputTriggerStepNumber;

    [SerializeField] private TMP_Text _txtTimer;
    [SerializeField] private Slider _sliderPlayer;
    [SerializeField] private Image _imgRecordingIcon;
    [SerializeField] private CanvasGroup _groupPlayControls;
    [SerializeField] private AudioSource _audioSource;

    private AudioClip _audioClip;
    private string _fileName;
    private Coroutine _updateSliderPlayerCoroutine;
    private Coroutine _updateRecordTimerCoroutine;
    private float _recordStartTime;

    private bool _isRecording;
    private bool _isPlaying;


    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _toggle3D.isOn = false;
        _toggleLoop.isOn = false;
        _sliderRange.minValue = MIN_RANGE;
        _sliderRange.maxValue = MAX_RANGE;
        _sliderRange.value = DEFAULT_RANGE;
        _txtSliderRangeValue.text = DEFAULT_RANGE.ToString("0");
        _panelRange.SetActive(false);

        _btnRecord.onClick.AddListener(OnRecordStarted);
        _btnStop.onClick.AddListener(OnRecordStopped);
        _btnPlay.onClick.AddListener(OnPlayingStarted);
        _btnPause.onClick.AddListener(OnPlayingPaused);
        _btnRewindBack.onClick.AddListener(OnRewindBack);
        _btnRewindForward.onClick.AddListener(OnRewindForward);

        _sliderPlayer.minValue = 0;
        _sliderPlayer.maxValue = 1f;
        _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);

        _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);
        _toggle3D.onValueChanged.AddListener(On3DSelected);
        _sliderRange.onValueChanged.AddListener(OnSliderRangeValueChanged);

        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            LoadContent();
            _groupPlayControls.interactable = true;
            var trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            if (trigger != null)
            {
                _toggleTrigger.isOn = true;
                _inputTriggerStepNumber.text = trigger.value;
            }
        }
        else
        {
            _fileName = $"MirageXR_Audio_{DateTime.Now.ToFileTimeUtc()}.wav";
            _groupPlayControls.interactable = false;
        }

        SetPlayerActive(true);
        UpdateSliderPlayerAndTimer();
    }

    private void OnDestroy()
    {
        if (_audioClip)
        {
            Destroy(_audioClip);
            _audioClip = null;
        }
    }

    private void LoadContent()
    {
        _fileName = GetFileName(_content);
        var filePath = Path.Combine(activityManager.ActivityPath, _fileName);
        _audioClip = SaveLoadAudioUtilities.LoadAudioFile(filePath);

        var parameters = _content.option.Split('#');
        if (parameters.Length == 3)
        {
            _toggle3D.isOn = parameters[0] == "3d";
            _toggle2D.isOn = !_toggle3D.isOn;
            _panelRange.SetActive(_toggle3D.isOn);
            _toggleLoop.isOn = parameters[1] == "1";
            if (int.TryParse(parameters[2], out var value))
            {
                _txtSliderRangeValue.text = parameters[2];
                _sliderRange.value = value;
            }
        }
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

    private static string GetFileName(ToggleObject content)
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
        UpdateSliderPlayerAndTimer();
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
            _txtTimer.text = ToTimeFormat(_audioSource.time);
            _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);

            if (Math.Abs(_audioSource.time - clip.length) <= float.Epsilon)
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
        var minutes = time / 60;
        var seconds = time % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    private void SetPlayerActive(bool value)
    {
        _imgRecordingIcon.gameObject.SetActive(!value);
        _groupPlayControls.gameObject.SetActive(value);
        _sliderPlayer.gameObject.SetActive(value);
    }

    private void On3DSelected(bool value)
    {
        _panelRange.SetActive(value);
    }

    private void OnSliderRangeValueChanged(float value)
    {
        _txtSliderRangeValue.text = value.ToString("0");
    }

    private void OnSliderPlayerValueChanged(float value)
    {
        _audioSource.time = _audioClip.length * value;
    }

    private void OnToggleTriggerValueChanged(bool value)
    {
        if (value && activityManager.IsLastAction(_step))
        {
            Toast.Instance.Show("This is the last step. The trigger is disabled!\n Add a new step and try again.");
            _toggleTrigger.onValueChanged.RemoveListener(OnToggleTriggerValueChanged);
            _toggleTrigger.isOn = false;
            _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);
            return;
        }

        if (value)
        {
            _toggleLoop.isOn = false;
            _inputTriggerStepNumber.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            _inputTriggerStepNumber.transform.parent.gameObject.SetActive(false);
        }
        _toggleLoop.interactable = _toggle3D.isOn && !value;
    }

    protected override void OnAccept()
    {
        if (!_audioClip)
        {
            Toast.Instance.Show("The audio has not been recorded");
            return;
        }

        var filePath = Path.Combine(activityManager.ActivityPath, _fileName);
        if (_content != null)
        {
            _fileName = GetFileName(_content);

            if (File.Exists(filePath) && _audioClip != null)
            {
                EventManager.DeactivateObject(_content);
                File.Delete(filePath);
            }
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }

        _content.option = _toggle3D.isOn ? "3d" : "2d";
        _content.option += _toggleLoop.isOn ? "#1" : "#0";
        _content.option += $"#{_txtSliderRangeValue.text}";
        _content.scale = 0.5f;
        _content.url = $"http://{_fileName}";

        if (_inputTriggerStepNumber.text == "" && _toggleTrigger.isOn)
        {
            Toast.Instance.Show("Input field is empty.");
            return;
        }

        if (_toggleTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(TriggerMode.Audio, ActionType.Audio, _content.poi, _audioClip.length, _inputTriggerStepNumber.text);
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        SaveLoadAudioUtilities.Save(filePath, _audioClip);

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);

        Close();
    }
}
