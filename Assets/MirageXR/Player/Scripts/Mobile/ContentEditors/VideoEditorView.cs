using System;
using System.IO;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class VideoEditorView : PopupEditorBase
{
    private const string HTTP_PREFIX = "http://";
    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";

    public override ContentType editorForType => ContentType.VIDEO;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;      // TODO: replace image preview with a video
    [SerializeField] private Button _btnCaptureVideo;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Toggle _toggleOrientation;

    private string _newFileName;
    private bool _videoWasRecorded;
    private bool _orientation;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _btnCaptureVideo.onClick.AddListener(OnStartRecordingVideo);
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
        VuforiaBehaviour.Instance.enabled = false;

        _newFileName = $"MirageXR_Video_{DateTime.Now.ToFileTimeUtc()}.mp4";
        var filepath = Path.Combine(activityManager.ActivityPath, _newFileName);

        VuforiaBehaviour.Instance.enabled = false;
        NativeCameraController.StartRecordingVideo(filepath, StopRecordingVideo);
    }

    private void StopRecordingVideo(bool result, string filePath)
    {
        _videoWasRecorded = result;
        VuforiaBehaviour.Instance.enabled = true;

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
        if (!value || !activityManager.IsLastAction(_step)) return;

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
}
