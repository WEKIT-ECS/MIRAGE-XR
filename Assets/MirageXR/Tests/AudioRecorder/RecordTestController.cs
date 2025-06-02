using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using TMPro;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
using UnityEngine.Android;
#endif
using UnityEngine.UI;

public class RecordTestController : MonoBehaviour
{
    private const string RECORDING_STATUS_TEXT = "Recording";
    private const string PAUSE_STATUS_TEXT = "Pause";
    private const string DEFAULT_STATUS_TEXT = "Stopped";
    private const string RECORDS_FOLDER = "records";
    private const string WAV_EXTENSION = ".wav";

    [SerializeField] private Canvas _canvas;
    [Header("Recorder")]
    [SerializeField] private TMP_Dropdown _ddownDeviceSelector;
    [SerializeField] private TMP_Text _txtResordStatus;
    [SerializeField] private Color _colorRecord = Color.red;
    [SerializeField] private Color _colorDefault = Color.black;
    [SerializeField] private Color _colorPause = Color.yellow;
    [SerializeField] private Button _btnRecorderStart;
    [SerializeField] private Button _btnRecorderStop;
    [SerializeField] private Button _btnRecorderPause;
    [SerializeField] private Button _btnRecorderResume;
    [Header("Player")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Button _btnPlayerStart;
    [SerializeField] private Button _btnPlayerStop;
    [Header("Saver")]
    [SerializeField] private Button _btnSave;
    [SerializeField] private TMP_Text _txtSavePath;
    private AudioClip _audioClip;

    private void Awake()
    {
        Debug.LogDebug("Number of microphones found: " + Microphone.devices.Length);
    }

    private void Start()
    {
        if (HasPermission())
        {
            Init();
        }
        else
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
            _canvas.enabled = false;
            Permission.RequestUserPermission(Permission.Microphone);
            Invoke(nameof(Start), 10f);
#endif
        }
    }

    private bool HasPermission()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
        return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
        return true;
#endif
    }

    private void Init()
    {
        _canvas.enabled = true;
        var devices = AudioRecorder.GetRecordingDevices();
        if (devices == null || devices.Length == 0)
        {
            var list = new List<string> { "Can't get recording devices" };
            _ddownDeviceSelector.AddOptions(list);
        }
        else
        {
            _ddownDeviceSelector.AddOptions(devices.ToList());
            _ddownDeviceSelector.onValueChanged.AddListener(OnDeviceSelectorValueChanged);
        }
        _ddownDeviceSelector.value = 0;

        _txtResordStatus.color = _colorDefault;
        _txtResordStatus.text = DEFAULT_STATUS_TEXT;

        _btnRecorderStart.onClick.AddListener(OnRecordStart);
        _btnRecorderStop.onClick.AddListener(OnRecordStop);
        _btnRecorderPause.onClick.AddListener(OnRecordPause);
        _btnRecorderResume.onClick.AddListener(OnRecordResume);

        _btnPlayerStart.onClick.AddListener(OnPlayerStart);
        _btnPlayerStop.onClick.AddListener(OnPlayerStop);

        _btnRecorderResume.interactable = false;
        _btnRecorderPause.interactable = false;
        _btnRecorderStop.interactable = false;
        SetPlayerInteractable(false);

        _btnSave.interactable = false;

        _btnSave.onClick.AddListener(OnRecordSave);
        _txtSavePath.text = string.Empty;
    }

    private void SetPlayerInteractable(bool value)
    {
        _audioSource.Stop();
        _btnPlayerStart.interactable = value;
        _btnPlayerStop.interactable = value;
    }

    private void OnDeviceSelectorValueChanged(int value)
    {
        AudioRecorder.SetRecordingDevice(_ddownDeviceSelector.options[value].text);
    }

    private void OnRecordStart()
    {
        _txtResordStatus.color = _colorRecord;
        _txtResordStatus.text = RECORDING_STATUS_TEXT;
        AudioRecorder.Start("test", () => true);
        _btnRecorderStart.interactable = false;
        _btnRecorderPause.interactable = true;
        _btnRecorderStop.interactable = true;
        _ddownDeviceSelector.interactable = false;
        SetPlayerInteractable(false);

        _btnSave.interactable = false;
    }

    private void OnRecordStop()
    {
        _txtResordStatus.color = _colorDefault;
        _txtResordStatus.text = DEFAULT_STATUS_TEXT;
        _audioClip = AudioRecorder.Stop();
        _btnRecorderStart.interactable = true;
        _btnRecorderResume.interactable = false;
        _btnRecorderPause.interactable = false;
        _btnRecorderStop.interactable = false;
        _ddownDeviceSelector.interactable = true;
        _btnPlayerStart.interactable = true;
        _btnSave.interactable = true;
    }

    private void OnRecordPause()
    {
        _txtResordStatus.color = _colorPause;
        _txtResordStatus.text = PAUSE_STATUS_TEXT;
        _btnRecorderResume.interactable = true;
        _btnRecorderPause.interactable = false;
        AudioRecorder.Pause();
    }

    private void OnRecordResume()
    {
        _txtResordStatus.color = _colorRecord;
        _txtResordStatus.text = RECORDING_STATUS_TEXT;
        _btnRecorderResume.interactable = false;
        _btnRecorderPause.interactable = true;
        AudioRecorder.Resume(() => true);
    }

    private void OnPlayerStart()
    {
        _btnPlayerStart.interactable = false;
        _btnPlayerStop.interactable = true;
        _audioSource.PlayOneShot(_audioClip);
        Invoke(nameof(OnPlayerStop), _audioClip.length);
    }

    private void OnPlayerStop()
    {
        _btnPlayerStart.interactable = true;
        _btnPlayerStop.interactable = false;
        _audioSource.Stop();
    }

    private void OnRecordSave()
    {
        var path = Path.Combine(Application.persistentDataPath,
            RECORDS_FOLDER, $"{DateTime.Now:yy-MM-dd_HH-mm-ss}{WAV_EXTENSION}");
        _txtSavePath.text = LearningExperienceEngine.SaveLoadAudioUtilities.Save(path, _audioClip) ? path : "Error";
    }
}
