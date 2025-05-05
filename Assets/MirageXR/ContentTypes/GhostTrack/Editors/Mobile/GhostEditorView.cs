using LearningExperienceEngine;
using System;
using System.Collections.Generic;
using System.IO;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class GhostEditorView : PopupEditorBase
{
    private const string MALE_TYPE = "GhosttrackPrefab";
    private const string FEMALE_TYPE = "GhosttrackPrefabFemale";

    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Ghost;

    [SerializeField] private Toggle _toggleMale;
    [SerializeField] private Toggle _toggleFemale;
    [SerializeField] private Toggle _toggleAudio;
    [SerializeField] private Button _btnStart;
    [SerializeField] private Button _btnStop;

    private readonly GhostRecorder _ghostRecorder = new GhostRecorder();

    private string _ghostFileName;
    private string _audioFileName;
    private List<LearningExperienceEngine.GhostDataFrame> _ghostFrames;
    private AudioClip _audioClip;
    private Transform _anchor;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _btnStart.onClick.AddListener(StartRecording);
        _btnStop.onClick.AddListener(StopRecording);
        _toggleMale.isOn = true;
        _toggleAudio.isOn = true;

        if (_content != null)
        {
            _toggleFemale.isOn = _content.option == FEMALE_TYPE;
        }
    }

    private void StartRecording()
    {
        var timeStamp = DateTime.Now.ToFileTimeUtc();
        _ghostFileName = $"MirageXR_Ghost_{timeStamp}.xml";
        _audioFileName = $"MirageXR_Audio_{timeStamp}.wav";

        _anchor = GameObject.Find(_step.id).transform;  //TODO: possible NRE. replace with direct ref

        _ghostRecorder.Start(_anchor, Camera.main.transform);
        if (_toggleAudio.isOn) RootObject.Instance.LEE.AudioManager.Start(_audioFileName);

        SetTogglesActive(false);
    }

    private void StopRecording()
    {
        _ghostFrames = _ghostRecorder.Stop();
        if (_toggleAudio.isOn) _audioClip = RootObject.Instance.LEE.AudioManager.Stop();

        SetTogglesActive(true);
    }

    private void SetTogglesActive(bool value)
    {
        _toggleAudio.interactable = value;
        _toggleMale.interactable = value;
        _toggleFemale.interactable = value;
    }

    protected override void OnAccept()
    {
        if (_ghostFrames == null || _ghostFrames.Count == 0)
        {
            Toast.Instance.Show("The ghost walk has not been recorded");
            return;
        }

        var offset = GetOffset();

        if (_content != null)
        {
            DeactivateContent(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, offset);
            //_content.predicate = editorForType.GetPredicate();    //TODO obsolete
        }

        _content.option = _toggleMale.isOn ? MALE_TYPE : FEMALE_TYPE;

        var ghostFilePath = Path.Combine(activityManager.ActivityPath, _ghostFileName);
        GhostRecorder.ExportToFile(ghostFilePath, _ghostFrames);

        _content.url = HTTP_PREFIX + _ghostFileName;
        _content.position = _anchor.position.ToString();
        _content.rotation = _anchor.rotation.ToString();
        _content.scale = 1f;

        if (_toggleAudio.isOn && _audioClip)
        {
            var audioContent = SaveAudio(offset);
            _content.option += $":{audioContent.poi}";
            LearningExperienceEngine.EventManager.ActivateObject(audioContent);
        }

        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();
        Close();
    }

    private ToggleObject SaveAudio(Vector3 offset)
    {
        var audioFilePath = Path.Combine(activityManager.ActivityPath, _audioFileName);
        SaveLoadAudioUtilities.Save(audioFilePath, _audioClip);

        var audioContent = augmentationManager.AddAugmentation(_step, offset);
        audioContent.predicate = ContentType.AUDIO.GetPredicate();
        audioContent.scale = 0.5f;
        audioContent.url = HTTP_PREFIX + _audioFileName;
        return audioContent;
    }

    private static void DeactivateContent(ToggleObject content)
    {
        var fileName = content.url.Replace(HTTP_PREFIX, string.Empty);
        var filePath = Path.Combine(activityManager.ActivityPath, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        LearningExperienceEngine.EventManager.DeactivateObject(content);
    }
}
