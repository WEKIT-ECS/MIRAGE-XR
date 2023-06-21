using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioEditorView : PopupEditorBase
{
    public class IntHolder : ObjectHolder<int> { }

    private const float DEFAULT_RANGE = 3.0f;
    private const float MIN_RANGE = 0.0f;
    private const float MAX_RANGE = 10.0f;
    private const float REWIND_VALUE = 10f;
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    private float _currentRangeValue;

    public override ContentType editorForType => ContentType.AUDIO;

    [SerializeField] private Button _btnAudioSettings;
    [SerializeField] private Button _btnCancel;
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
    [SerializeField] private GameObject _panelRecordControls;
    [SerializeField] private GameObject _panelPlayRecord;
    [SerializeField] private GameObject _panelAudioSettings;
    [SerializeField] private GameObject _panelRecordComplete;
    [SerializeField] private GameObject _panelBottomButtons;
    [SerializeField] private Button _btnRecordComplete;
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
        _panelRecordComplete.SetActive(false);
        _panelPlayRecord.SetActive(false);
        _panelRecordControls.SetActive(true);
        _panelBottomButtons.SetActive(false);

        _btnAudioSettings.onClick.AddListener(OnOpenAudioSettings);
        _btnCancel.onClick.AddListener(OnClickCancel);

        _btnRecord.onClick.AddListener(OnRecordStarted);
        _btnStop.onClick.AddListener(OnRecordStopped);
        _btnPlay.onClick.AddListener(OnPlayingStarted);
        _btnPause.onClick.AddListener(OnPlayingPaused);
        _btnRewindBack.onClick.AddListener(OnRewindBack);
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        _btnRewindForward.onClick.AddListener(OnRewindForward);
        _btnIncreaseRange.onClick.AddListener(OnIncreaseRange);
        _btnDecreaseRange.onClick.AddListener(OnDecreaseRange);
        _btnRecordComplete.onClick.AddListener(OnClickRecordComplete);

        _sliderPlayer.minValue = 0;
        _sliderPlayer.maxValue = 1f;
        _sliderPlayer.onValueChanged.AddListener(OnSliderPlayerValueChanged);
        _clampedScrollJumpToStep.onItemChanged.AddListener(OnItemJumpToStepChanged);

        _toggle3D.onValueChanged.AddListener(On3DSelected);

        var steps = activityManager.ActionsOfTypeAction;
        var stepsCount = steps.Count;
        InitClampedScrollRect(_clampedScrollJumpToStep, _templatePrefab, stepsCount, stepsCount.ToString());

        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            LoadContent();
            _groupPlayControls.interactable = true;
            var trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            if (trigger != null)
            {
                _inputTriggerStepNumber = trigger.value;
            }
        }
        else
        {
            _fileName = $"MirageXR_Audio_{DateTime.Now.ToFileTimeUtc()}.wav";
            _groupPlayControls.interactable = false;
        }

        SetPlayerActive(true);
        UpdateSliderPlayerAndTimer();
        RootView_v2.Instance.HideBaseView();
    }

    private void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount, string text)
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
                _clampedScrollJumpToStep.currentItemIndex = i;
            }
        }
    }

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
        _fileName = GetFileName(_content);
        var filePath = Path.Combine(activityManager.ActivityPath, _fileName);
        _audioClip = SaveLoadAudioUtilities.LoadAudioFile(filePath);

        var parameters = _content.option.Split('#');
        if (parameters.Length == 3)
        {
            _toggle3D.isOn = parameters[0] == "3d";
            //_toggle2D.isOn = !_toggle3D.isOn;
            _panelRange.SetActive(_toggle3D.isOn);
            _toggleLoop.isOn = parameters[1] == "1";
            if (int.TryParse(parameters[2], out var value))
            {
                _txtSliderRangeValue.text = parameters[2];
                _currentRangeValue = value;
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
        _panelRecordComplete.SetActive(false);
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

        _panelRecordComplete.SetActive(true);
    }

    private void OnClickRecordComplete()
    {
        _panelPlayRecord.SetActive(true);
        _panelRecordControls.SetActive(false);
        _panelBottomButtons.SetActive(true);
        _panelRecordComplete.SetActive(false);
        _txtTimerTo.text = ToTimeFormatMinutes(_audioClip.length);
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
        _panelPlayRecord.SetActive(false);
        _panelBottomButtons.SetActive(false);
        _panelAudioSettings.SetActive(true);
    }

    private void OnClickCancel()
    {
        _panelPlayRecord.SetActive(false);
        _panelBottomButtons.SetActive(false);
        _panelRecordControls.SetActive(true);
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

        _step.AddOrReplaceArlemTrigger(TriggerMode.Audio, ActionType.Audio, _content.poi, _audioClip.length, _inputTriggerStepNumber);

        SaveLoadAudioUtilities.Save(filePath, _audioClip);

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);

        Close();
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
}
