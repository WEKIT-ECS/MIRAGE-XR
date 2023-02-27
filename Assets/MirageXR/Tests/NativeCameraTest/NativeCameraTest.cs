using i5.Toolkit.Core.VerboseLogging;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class NativeCameraTest : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private TMP_Text _pathTxt;
    [SerializeField] private Button _photoBtn;
    [SerializeField] private Button _videoBtn;
    [SerializeField] private Button _playBtn;
    [SerializeField] private Button _pauseBtn;
    [SerializeField] private Button _stopBtn;

    private RectTransform _imageRectTransform;
        
    private void Start()
    {
        _photoBtn.onClick.AddListener(TakePhoto);
        _videoBtn.onClick.AddListener(TakeVideo);
        _playBtn.onClick.AddListener(Play);
        _pauseBtn.onClick.AddListener(_videoPlayer.Pause);
        _stopBtn.onClick.AddListener(_videoPlayer.Stop);
        _imageRectTransform = (RectTransform)_rawImage.transform;
    }

    private void TakePhoto()
    {
        NativeCameraController.TakePicture(OnPictureTaken);
        _videoPlayer.Stop();
    }

    private void TakeVideo()
    {
        var path = Path.Combine(Application.persistentDataPath, DateTime.Now.Ticks + ".mp4");
        NativeCameraController.StartRecordingVideo(path, OnVideoRecorded);
    }

    private void Play()
    {
        _rawImage.texture = _renderTexture;
        _videoPlayer.Play();
    }

    private void OnPictureTaken(bool result, Texture2D texture2D)
    {
        _rawImage.texture = texture2D;
        var width = texture2D.width / (float)texture2D.height * _imageRectTransform.rect.height;
        _imageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void OnVideoRecorded(bool result, string path)
    {
        AppLog.LogDebug("Video record path: " + path);
        _rawImage.texture = _renderTexture;
        _pathTxt.text = path;
        _videoPlayer.url = path;
        var width = _renderTexture.width / (float)_renderTexture.height * _imageRectTransform.rect.height;
        _imageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}