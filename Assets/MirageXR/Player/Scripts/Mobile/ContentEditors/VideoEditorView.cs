using System;
using System.IO;
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

    public override ContentType editorForType => ContentType.VIDEO;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;      // TODO: replace image preview with a video
    [SerializeField] private Button _btnCaptureVideo;
    [SerializeField] private Button _btnOpenGallery;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Toggle _toggleOrientation;
    
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button playButton;
    [SerializeField] private Button editCaption;
    [SerializeField] private Button doneEditCaption;
    [SerializeField] private GameObject obj1;
    [SerializeField] private GameObject obj2;
    [SerializeField] private GameObject obj3;
    [SerializeField] private GameObject obj4;
    [SerializeField] private GameObject obj5;
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

        
        base.Initialization(onClose, args);
        _btnCaptureVideo.onClick.AddListener(OnStartRecordingVideo);
        
        _btnOpenGallery.onClick.AddListener(OpenGallery);
        _toggleOrientation.onValueChanged.AddListener(OnToggleOrientationValueChanged);
        _toggleTrigger.onValueChanged.AddListener(OnToggleTriggerValueChanged);
        _orientation = true;
        _toggleOrientation.isOn = _orientation;
        UpdateView();

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
    obj1.SetActive(false);
    obj2.SetActive(false);
    obj4.SetActive(true);
    obj5.SetActive(true);
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
    
    obj4.SetActive(false);
    obj5.SetActive(false);
    obj3.SetActive(true);
    string savedText = captionGenerator.AllGeneratedCaptions();
    _captionEdit.text = savedText;
     Debug.Log(savedText);
    }
private void OnDoneButtonClick(){
    obj1.SetActive(true);
    obj2.SetActive(true);
    obj4.SetActive(false);
    obj5.SetActive(false);
    obj3.SetActive(false);
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

            // Removed the PlayVideo(destPath); line from here
        }

    });
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
   

