using System;
using System.IO;
using DG.Tweening;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using UnityEngine.Video;
using IBM.Watsson.Examples;
using TMPro;

public class VideoEditorView : PopupEditorBase
{
    private const string HTTP_PREFIX = "http://";
    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";
	
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    private const float IMAGE_HEIGHT = 630f;

    public override ContentType editorForType => ContentType.VIDEO;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;      // TODO: replace image preview with a video
    [SerializeField] private Button _btnCaptureVideo;
    [SerializeField] private Button _btnOpenGallery;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Toggle _toggleOrientation;
	  [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private HintViewWithButtonAndToggle _hintPrefab;
    [SerializeField] private Button _backEditButton;
    [SerializeField] private Button _backPreviewButton; 
    [Space] 
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button playButton;
    [SerializeField] private Button editCaption;
    [SerializeField] private Button doneEditCaption;
    [SerializeField] private GameObject _speechToText;
    [SerializeField] private GameObject Toppanel;
    [SerializeField] private GameObject Bottompanel;
    [SerializeField] private GameObject VideoEditorPrefab;
    [SerializeField] private GameObject VideoPreviePrefab;
    [SerializeField] private GameObject _videoplayer;
    [SerializeField]
    private CaptionGenerator captionGenerator;
    [SerializeField] private TMP_InputField _captionEdit;

    private string _newFileName;
    private bool _videoWasRecorded;
    private bool _orientation;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        //captionGenerator = GetComponent<CaptionGenerator>();
        playButton.onClick.AddListener(OnPlayButtonClick);
        editCaption.onClick.AddListener(OnEditButtonClick);
        doneEditCaption.onClick.AddListener(OnDoneButtonClick);
         _backEditButton.onClick.AddListener(OnBackEditButtonClick);
        _backPreviewButton.onClick.AddListener(OnPreviewButtonClick);

        
        base.Initialization(onClose, args);
        _btnCaptureVideo.onClick.AddListener(OnStartRecordingVideo);
        
        _btnOpenGallery.onClick.AddListener(OpenGallery);
        _toggleOrientation.onValueChanged.AddListener(OnToggleOrientationValueChanged);
        _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);
        _orientation = true;
        _toggleOrientation.isOn = _orientation;
		
		_btnArrow.onClick.AddListener(OnArrowButtonPressed);
        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

        RootView_v2.Instance.HideBaseView();
        UpdateView();

    }
    
    private void OnPreviewButtonClick(){
    Toppanel.SetActive(true);
    Bottompanel.SetActive(true);
    VideoPreviePrefab.SetActive(false);
    }
     private void OnBackEditButtonClick(){
    VideoEditorPrefab.SetActive(false);
    VideoPreviePrefab.SetActive(true);
    }
    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
    }
    private void UpdateView()
    {
        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            var originalFileName = Path.GetFileName(_content.url.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

            if (!File.Exists(originalFilePath)) return;

            var trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            if (trigger != null)
            {
                _toggleTrigger.isOn = true;
            }
            _orientation = _content.key == LANDSCAPE;
            _toggleOrientation.isOn = _orientation;
            SetPreview(NativeCameraController.GetVideoThumbnail(originalFilePath));
        }
    }
   
    private void OnStartRecordingVideo()
    {
        StartRecordingVideo();
    }

    private void StartRecordingVideo()
    {
        RootObject.Instance.imageTargetManager.enabled = false;

        _newFileName = $"MirageXR_Video_{DateTime.Now.ToFileTimeUtc()}.mp4";
        var filepath = Path.Combine(activityManager.ActivityPath, _newFileName);

        NativeCameraController.StartRecordingVideo(filepath, StopRecordingVideo);
    }

    private void StopRecordingVideo(bool result, string filePath)
    {
        _videoWasRecorded = result;
        RootObject.Instance.imageTargetManager.enabled = true;

        if (result)
        {
            SetPreview(NativeCameraController.GetVideoThumbnail(filePath));
        }
    }

    private void OnToggleOrientationValueChanged(bool value)
    {
        _orientation = value;
    }

    private void OnToggleTriggerValueChanged(bool value)
    {
        if (!value || !activityManager.IsLastAction(_step))
        {
            return;
        }

        Toast.Instance.Show("This is the last step. The trigger is disabled!\n Add a new step and try again.");
        _toggleTrigger.onValueChanged.RemoveListener(OnToggleTriggerValueChanged);
        _toggleTrigger.isOn = false;
        _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);
    }

    protected override void OnAccept()
    {
        if (!_videoWasRecorded)
        {
            Toast.Instance.Show("The video has not been recorded");
            return;
        }

        if (_content != null)
        {
            EventManager.DeactivateObject(_content);

            // delete the previous video file
            var originalFileName = Path.GetFileName(_content.url.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);
            if (File.Exists(originalFilePath))
            {
                File.Delete(originalFilePath);
            }
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetName().ToLower();
        }

        // saving of the movie file has already happened since it has been written to file while recording
        _content.url = HTTP_PREFIX + _newFileName;
        _content.key = _orientation ? LANDSCAPE : PORTRAIT;

        if (_toggleTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(TriggerMode.Video, ActionType.Video, _content.poi, 0, string.Empty);
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);
          _speechToText.SetActive(false);
        Close();
    }

    private void SetPreview(Texture2D texture2D)
    {
        if (!texture2D) return;

        if (_image.sprite && _image.sprite.texture)
        {
            Destroy(_image.sprite.texture);
        }

        var sprite = Utilities.TextureToSprite(texture2D);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var height = rtImage.rect.width / texture2D.width * texture2D.height + (rtImage.sizeDelta.y * -1);
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
private void OnPlayButtonClick()
{
    _speechToText.SetActive(true);
    Toppanel.SetActive(false);
    Bottompanel.SetActive(false);
    VideoPreviePrefab.SetActive(true);
    _videoplayer.SetActive(true);
    if (_videoWasRecorded)
    {
        string videoPath = Path.Combine(activityManager.ActivityPath, _newFileName);
        PlayVideo(videoPath);
    }
    else
    {
        Debug.Log("Video not recorded or fetched yet.");
    }
}

public void OnEditButtonClick(){
        _speechToText.SetActive(false);
    VideoPreviePrefab.SetActive(false);
    _videoplayer.SetActive(false);
    VideoEditorPrefab.SetActive(true);
    string savedText = captionGenerator.AllGeneratedCaptions();
    _captionEdit.text = savedText;
     Debug.Log(savedText);
    }
private void OnDoneButtonClick(){
    Toppanel.SetActive(true);
    Bottompanel.SetActive(true);
    VideoPreviePrefab.SetActive(false);
      _speechToText.SetActive(false);
    VideoEditorPrefab.SetActive(false);
        SaveTextToFile();
}

    private void OpenGallery()
    {
        PickVideo();
    }

    private void PickVideo()
{
    NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
    {
        Debug.Log("Video path: " + path);
        if (path != null)
        {
            _videoWasRecorded = true;
            SetPreview(NativeGallery.GetVideoThumbnail(path));

            _newFileName = $"MirageXR_Video_{DateTime.Now.ToFileTimeUtc()}.mp4";
            var newFilePath = Path.Combine(activityManager.ActivityPath, _newFileName);

            var sourcePath = Path.Combine(Application.persistentDataPath, path);
            var destPath = Path.Combine(Application.persistentDataPath, newFilePath);
            File.Move(sourcePath, destPath);

            
        }

    });
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

   private void PlayVideo(string path)
{
    if (videoPlayer == null) return;

    videoPlayer.url = path;
    videoPlayer.Play();
}
public string GetVideoPath()
{
    return Path.Combine(activityManager.ActivityPath);
}
    private void SaveTextToFile()
    {
        string inputText = _captionEdit.text;
        string captionfilePath = Path.Combine(activityManager.ActivityPath, $"MirageXR_Video_{DateTime.Now.ToFileTimeUtc()}.txt");
        // Check if the input text is not empty
        if (!string.IsNullOrEmpty(inputText))
        {
            // Write the input text to the text file
            using (StreamWriter writer = new StreamWriter(captionfilePath, true))
            {
                writer.WriteLine(inputText);
            }

            Debug.Log("Text saved to file: " + captionfilePath);
        }
        else
        {
            Debug.LogWarning("Input text is empty. Cannot save to file.");
        }
    }

}
   

