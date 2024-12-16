using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MirageXR
{
    public class VideoEditorSpatialView : EditorSpatialView
    {
        private const float IMAGE_HEIGHT = 270f;

        [SerializeField] private Transform _imageHolder;
        [SerializeField] private Image _image;
        [SerializeField] private Button _btnCaptureVideo;
        [SerializeField] private Button _btnOpenGallery; 
        [SerializeField] private Button _btnGenerateCaption;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [Space]
        [SerializeField] private GameObject _AugmentationSettingsPanel;
        
        [Space]
        [SerializeField] private RenderTexture renderTex;
        [SerializeField] private RawImage _videoDisplay;
        [SerializeField] private VideoPlayer _videoPlayer;
  

        private string _text;
        private Texture2D _capturedImage;
        private Content<VideoContentData> _videoContent;
        private string _videoFilePath;
        
        private bool _videoWasRecorded;
        private string _newFileName;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            Debug.LogError("[111] Initialization");
            _showBackground = false;
            base.Initialization(onClose, args);
            
            _videoContent = _content as Content<VideoContentData>;
            
            _btnCaptureVideo.onClick.AddListener(OnStartRecordingVideo);
            _btnOpenGallery.onClick.AddListener(OpenGallery);
            _btnGenerateCaption.onClick.AddListener(GenerateCaption);
            _btnSettings.onClick.AddListener(OpenSettings);
            _playButton.onClick.AddListener(Play);
            _pauseButton.onClick.AddListener(Pause);
            
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(false);
            _videoDisplay.gameObject.SetActive(false);
            
            UpdateView();
        }

        private void Pause()
        {
            _videoPlayer.Pause();
            _playButton.gameObject.SetActive(true);
            _pauseButton.gameObject.SetActive(false);
        }

        private void Play()
        {
            _videoPlayer.Play();
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(true);
        }

        private void OpenSettings()
        {
            _AugmentationSettingsPanel.SetActive(true);
        }

        private void GenerateCaption()
        {
            // TODO
        }

        private void OnDestroy()
        {
            Debug.Log("[111] OnDestroy");
            ClearVideoPreview();
        }

        protected override void OnAccept()
        {
            OnAcceptAsync().Forget();
        }

        private async UniTask OnAcceptAsync()
        {
            if (!_videoWasRecorded)
            {
                return;
            }

            var step = RootObject.Instance.LEE.StepManager.CurrentStep;
            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            var fileId = _videoContent?.ContentData?.Video?.Id ?? Guid.NewGuid();
            
            _videoContent ??= new Content<VideoContentData>
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                Steps = new List<Guid> { step.Id },
                Type = ContentType.Video,
                Version = Application.version,
                ContentData = new VideoContentData
                {
                    IsLooped = true,
                    Is3dSound = false,
                },
                Location = Location.GetIdentityLocation()
            };

            //_videoContent.Location.Scale = CalculateScale(_capturedImage.width, _capturedImage.height);

            await SaveVideoAsync(activityId, _videoContent.Id, fileId);
            _videoContent.ContentData.Video = await RootObject.Instance.LEE.AssetsManager.CreateFileAsync(activityId, _videoContent.Id, fileId);

            RootObject.Instance.LEE.ContentManager.AddContent(_videoContent);
            RootObject.Instance.LEE.AssetsManager.UploadFileAsync(activityId, _videoContent.Id, fileId);

            Close();
        }
        
        private async UniTask SaveVideoAsync(Guid activityId, Guid contentId, Guid fileId)
        {
            if (string.IsNullOrEmpty(_videoFilePath) || !File.Exists(_videoFilePath))
            {
                return;
            }
            var videoBytes = await File.ReadAllBytesAsync(_videoFilePath);
            var folder = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, contentId, fileId);
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, "video.mp4");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await File.WriteAllBytesAsync(filePath, videoBytes);
        }

        private void UpdateView()
        {
            Debug.LogError("[111] UpdateView 1");
            if (_videoContent != null)
            {
                
                Debug.LogError("[111] UpdateView 2");
                var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                var folder = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, _videoContent.Id, _videoContent.ContentData.Video.Id);
                var videoPath = Path.Combine(folder, "video.mp4");  //TODO: move to AssetsManager
                if (!File.Exists(videoPath))
                {
                    return;
                }
                Debug.LogError("[111] UpdateView 3");
                //UpdateVideoPreview();
                _playButton.gameObject.SetActive(false);
                _pauseButton.gameObject.SetActive(true);
                _videoDisplay.gameObject.SetActive(true);
                //LayoutRebuilder.ForceRebuildLayoutImmediate(_videoDisplay.rectTransform);
                
                _videoPlayer.targetTexture = renderTex;
                _videoDisplay.texture = renderTex;
                _videoPlayer.url = videoPath;
                _videoPlayer.Play();
                
                Debug.Log("Playing video: " + videoPath);
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
            var filepath = Path.Combine(Application.dataPath, _newFileName);

            NativeCameraController.StartRecordingVideo(filepath, StopRecordingVideo);
        }
        
        private void StopRecordingVideo(bool result, string filePath)
        {
            _videoWasRecorded = result;
            RootObject.Instance.ImageTargetManager.enabled = true;

            if (result)
            {
                _videoFilePath = filePath;
            }
        }

        private void OpenGallery()
        {
            PickVideo();
        }
        
        private void PickVideo()
        {
            NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
            {
                if (path != null)
                {
                    _videoFilePath = path;
                    _videoWasRecorded = true;
                    
                    _videoPlayer.targetTexture = renderTex;
                    _videoDisplay.texture = renderTex;
                    _videoPlayer.url = path;
                    _videoPlayer.Play();
                    Debug.Log("Playing video: " + path);
                    
                    _playButton.gameObject.SetActive(false);
                    _pauseButton.gameObject.SetActive(true);
                    _videoDisplay.gameObject.SetActive(true);
                }
            });
        }
        
        private void UpdateVideoPreview()
        {
            if (!File.Exists(_videoFilePath))
            {
                return;
            }
            ClearVideoPreview();
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(true);
            _videoDisplay.gameObject.SetActive(true);
            
            _videoPlayer.targetTexture = renderTex;
            _videoDisplay.texture = renderTex;
            _videoPlayer.url = _videoFilePath;
            //_videoPlayer.Play();
            Pause();
        }

        private void ClearVideoPreview()
        {
            _videoPlayer.Stop();
            _videoPlayer.targetTexture = null;
            _videoDisplay.texture = null;

            RenderTexture.active = renderTex;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
        }
    }
}
