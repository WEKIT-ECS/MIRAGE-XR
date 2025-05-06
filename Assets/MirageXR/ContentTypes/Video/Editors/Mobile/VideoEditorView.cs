using LearningExperienceEngine;
using System;
using System.IO;
using DG.Tweening;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class VideoEditorView : PopupEditorBase
{
    private const string HTTP_PREFIX = "http://";
    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    private const float IMAGE_HEIGHT = 630f;

    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Video;

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

    private string _newFileName;
    private bool _videoWasRecorded;
    private bool _orientation;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
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

        _toggleTrigger.interactable = !activityManager.IsLastAction(_step);
        
        RootView_v2.Instance.HideBaseView();
        UpdateView();
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

            Debug.Log("OriginalFilePath = " + originalFilePath);
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
        RootObject.Instance.ImageTargetManager.enabled = false;

        _newFileName = $"MirageXR_Video_{DateTime.Now.ToFileTimeUtc()}.mp4";
        var filepath = Path.Combine(activityManager.ActivityPath, _newFileName);

        NativeCameraController.StartRecordingVideo(filepath, StopRecordingVideo);
    }

    private void StopRecordingVideo(bool result, string filePath)
    {
        _videoWasRecorded = result;
        RootObject.Instance.ImageTargetManager.enabled = true;

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
            Toast.Instance.Show("No video recorded.");
            if (_content == null || string.IsNullOrEmpty(_content.url))
            {
                return;
            }
        }

        if (_content != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(_content);

            var originalFileName = Path.GetFileName(_content.url.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);
            Debug.Log("Will now delete file at originalFilePath = " + originalFilePath);
            Debug.Log("New file = " + _newFileName);
            if (File.Exists(originalFilePath) && !string.IsNullOrEmpty(_newFileName) && _videoWasRecorded)
            {
                Debug.Log("Deleting old video recording file, as replacing with new one.");
                if (Path.Combine(activityManager.ActivityPath, _newFileName) != originalFilePath)
                {
                    File.Delete(originalFilePath);
                    Debug.Log("deleted");
                }
            }
            else
            {
                Debug.Log("Old video recording file did not exist or was the same as the new one.");
            }
            if (string.IsNullOrEmpty(_newFileName) || !_videoWasRecorded)
            {
                _newFileName = originalFileName;
            }
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            //_content.predicate = editorForType.GetName().ToLower();//TODO obsolete
        }

        if (!LearningExperienceEngine.UserSettings.dontShowNewAugmentationHint)
        {
            PopupsViewer.Instance.Show(_hintPrefab);
        }

        // saving of the movie file has already happened since it has been written to file while recording
        _content.url = HTTP_PREFIX + _newFileName;
        _content.key = _orientation ? LANDSCAPE : PORTRAIT;

        if (_toggleTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(LearningExperienceEngine.TriggerMode.Video, LearningExperienceEngine.ActionType.Video, _content.poi, 0, string.Empty);
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();

        Close();
    }

    private void SetPreview(Texture2D texture2D)
    {
        if (!texture2D) return;

        if (_image.sprite && _image.sprite.texture)
        {
            Destroy(_image.sprite.texture);
        }

        var sprite = MirageXR.Utilities.TextureToSprite(texture2D);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var width = (float)texture2D.width / texture2D.height * IMAGE_HEIGHT;
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, IMAGE_HEIGHT);
        rtImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
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

                // TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.VIDEO_SELECTED_FROM_GALLERY); seemingly unnecessary as it is covered by FINISHED_QUEUE
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
}
