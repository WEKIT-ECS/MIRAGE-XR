using LearningExperienceEngine;
using MirageXR;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor logic for creating a ghost track
/// The ghost track consists of a recording of an expert's movement and audio
/// </summary>
public class GhosttrackEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Button _startRecordingButton;
    [SerializeField] private Button _stopRecordingButton;

    [SerializeField] private GameObject _maleGhostPrefab;
    [SerializeField] private GameObject _femaleGhostPrefab;

    [SerializeField] private Button _maleButton;
    [SerializeField] private Button _femaleButton;

    private bool _isFemaleGender = false;

    private LearningExperienceEngine.Action _action;
    private LearningExperienceEngine.ToggleObject _annotationToEdit;

    private GameObject _ghostPreview;
    private Transform _ghostPreviewTransform;
    private Transform _ghostPreviewHeadTransform;

    private List<LearningExperienceEngine.GhostDataFrame> _ghostFrames;
    private AudioClip _audioClip;

    private Transform _augOrigin;
    private Transform _cameraTransform;

    private Image _maleThumbnailSelectedIcon;
    private Image _femaleThumbnailSelectedIcon;

    private string _ghostFileName;
    private string _audioFileName;

    private readonly GhostRecorder _ghostRecorder = new GhostRecorder();

    private bool _isRecording;

    private bool IsRecording
    {
        get => _isRecording;
        set
        {
            _isRecording = value;
            _startRecordingButton.gameObject.SetActive(!_isRecording);
            _stopRecordingButton.gameObject.SetActive(_isRecording);
        }
    }

    /// <summary>
    /// Closes the ghost track editor
    /// </summary>
    public void Close()
    {
        if (_isRecording)
        {
            StopRecording();
        }
        _action = null;
        _annotationToEdit = null;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
    {
        gameObject.SetActive(true);
        _action = action;
        _annotationToEdit = annotation;
        IsRecording = false;
        Initialize();
    }

    public void OnAccept()
    {
        var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
        Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_action.id));
        var originT = GameObject.Find(detectable.id);
        var offset = MirageXR.Utilities.CalculateOffset(_augOrigin.position, _augOrigin.rotation, originT.transform.position, originT.transform.rotation);

        StopRecording();
        if (_annotationToEdit != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);

            // delete old xml file
            var xmlPath = $"{activityManager.ActivityPath}/MirageXR_Ghost_{_annotationToEdit.poi}.xml";
            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
            }
            // delete old audio annotation before creating a new one
            activityManager.ActionsOfTypeAction.ForEach(a =>
            {
                if (a.enter.activates.Contains(_annotationToEdit) && _annotationToEdit.option.Contains(":"))
                {
                    var myAudioPoi = _annotationToEdit.option.Split(':')[1];
                    var myAudioToggleObject = a.enter.activates.Find(t => t.poi == myAudioPoi);
                    if (myAudioToggleObject != null)
                    {
                        a.enter.activates.Remove(myAudioToggleObject);
                        var audioFilePath = Path.Combine(activityManager.ActivityPath,
                            myAudioToggleObject.url.Replace("http://", ""));
                        File.Delete(audioFilePath);
                    }
                }
            });

            Debug.LogError("[GhosttrackEditor] Some error: _annotationToEdit exists is " + _annotationToEdit == null);
        }
        else
        {
            _annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
            _annotationToEdit.predicate = "ghosttracks";
            _annotationToEdit.scale = 1f;
        }

        _ghostFileName = $"MirageXR_Ghost_{_annotationToEdit.poi}.xml";

        var ghostFilePath = Path.Combine(activityManager.ActivityPath, _ghostFileName);
        GhostRecorder.ExportToFile(ghostFilePath, _ghostFrames);

        var audioFilePath = Path.Combine(activityManager.ActivityPath, _audioFileName);
        SaveLoadAudioUtilities.Save(audioFilePath, _audioClip);

        _annotationToEdit.url = $"http://{_ghostFileName}";
        _annotationToEdit.position = _augOrigin.position.ToString();
        _annotationToEdit.rotation = _augOrigin.rotation.ToString();

        var audioAnnotation = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
        audioAnnotation.predicate = "audio";
        audioAnnotation.scale = 0.5f;
        audioAnnotation.url = $"http://{_audioFileName}";


        // Set the gender option
        if (_annotationToEdit.option.Contains(":"))
        {
            var temp = _annotationToEdit.option.Split(':')[0];
            _annotationToEdit.option = temp;
        }
        else
        {
            _annotationToEdit.option = _isFemaleGender ? "GhosttrackPrefabFemale" : "GhosttrackPrefab"; // TODO:It's not a good idea to send the name of the prefab.
        }

        // then add the audio poi to option after the gender
        _annotationToEdit.option += ":" + audioAnnotation.poi;

        LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
        LearningExperienceEngine.EventManager.ActivateObject(audioAnnotation);
        LearningExperienceEngine.EventManager.NotifyActionModified(_action);
        Close();
    }

    private void Initialize()
    {
        _cameraTransform = Camera.main.transform;

        _maleThumbnailSelectedIcon = _maleButton.transform.GetChild(0).GetComponent<Image>();
        _femaleThumbnailSelectedIcon = _femaleButton.transform.GetChild(0).GetComponent<Image>();

        _maleButton.onClick.AddListener(OnMaleButtonClick);
        _femaleButton.onClick.AddListener(OnFemaleButtonClick);

        Debug.Log("GhostTrackAnnotation: warm up holosensors");
    }

    private void OnMaleButtonClick()
    {
        _isFemaleGender = false;
        _maleThumbnailSelectedIcon.enabled = true;
        _femaleThumbnailSelectedIcon.enabled = false;
    }

    private void OnFemaleButtonClick()
    {
        _isFemaleGender = true;
        _maleThumbnailSelectedIcon.enabled = false;
        _femaleThumbnailSelectedIcon.enabled = true;
    }

    private void Update()
    {
        if (_isRecording)
        {
            var lastGhostFrame = _ghostRecorder.LastFrame;

            _ghostPreviewTransform.position = _cameraTransform.position + (_cameraTransform.forward * 1.0f) + (_cameraTransform.right * 0.2f) + (_cameraTransform.up * 0.1f);
            _ghostPreviewHeadTransform.rotation = lastGhostFrame.head.rotation;
        }
    }

    /// <summary>
    /// Resets the parent transform for the relative recording to the current location.
    /// </summary>
    private void SetPoint()
    {
        if (_augOrigin != null)
        {
            Destroy(GameObject.Find("GhostOrigin"));
            _augOrigin = null;
        }

        _augOrigin = new GameObject("GhostOrigin").transform;
        _augOrigin.position = _cameraTransform.position;
        _augOrigin.rotation = GameObject.Find(_action.id).transform.rotation;
        _augOrigin.localScale = Vector3.one;
    }

    /// <summary>
    /// Stop ongoing recordings, then register for updates from sensors
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording)
        {
            StopRecording();
        }
        else
        {
            // call to setPoint must happen first to allow AugOrigin to be used in the FixedUpdate loop
            SetPoint();

            _augOrigin = GameObject.Find(_action.id).transform;  // TODO: possible NRE. replace with direct ref

            var timeStamp = System.DateTime.Now.ToFileTimeUtc();
            _audioFileName = $"MirageXR_Audio_{timeStamp}.wav";

            Debug.Log("GhostTrackAnnotation: send filename timestamp to audio: " + _ghostFileName);

            var previewPos = _cameraTransform.position + _cameraTransform.forward * 1.0f + _cameraTransform.right * 0.2f + _cameraTransform.up * 0.1f;
            _ghostPreview = Instantiate(_isFemaleGender ? _femaleGhostPrefab : _maleGhostPrefab, previewPos, Quaternion.identity);
            _ghostPreview.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            _ghostPreviewTransform = _ghostPreview.transform;
            _ghostPreviewHeadTransform = _ghostPreviewTransform.Find("Head");

            _ghostRecorder.Start(_augOrigin, _cameraTransform);
            RootObject.Instance.LEE.AudioManager.Start(_audioFileName);

            IsRecording = true;
        }

        Debug.Log("GhostTrackAnnotation.StartRecording done");
    }

    /// <summary>
    /// Stop the Record function.
    /// </summary>
    public void StopRecording()
    {
        if (!_isRecording)
        {
            return;
        }

        IsRecording = false;

        Debug.Log("stopped recording (= deregistered stream listener 'UpdateSpinePosition')");

        if (_ghostPreview != null)
        {
            Destroy(_ghostPreview);
        }

        _ghostFrames = _ghostRecorder.Stop();
        _audioClip = RootObject.Instance.LEE.AudioManager.Stop();

        Debug.Log("Stopped recording ghost track");
    }
}
