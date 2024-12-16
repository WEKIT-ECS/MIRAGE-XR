using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MirageXR
{
    public class VideoEditorSpatialView : EditorSpatialView,  IPointerDownHandler, IPointerUpHandler
    {
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
        [SerializeField] private Slider _slider;
        private bool _slide = false;
        
        private string _text;
        private Texture2D _capturedImage;
        private Content<VideoContentData> _videoContent;
        private string _videoFilePath;
        
        private bool _videoWasRecorded;
        private string _newFileName;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
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
        
        private void Update()
        {
            if (_videoPlayer.isPlaying && !_slide)
            {
                _slider.value = (float)_videoPlayer.frame / (float)_videoPlayer.frameCount;
            }
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
            if (_videoContent != null)
            {
                var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                var folder = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, _videoContent.Id, _videoContent.ContentData.Video.Id);
                var videoPath = Path.Combine(folder, "video.mp4");  //TODO: move to AssetsManager
                if (!File.Exists(videoPath))
                {
                    return;
                }
                SetupVideoPlayer(videoPath);
            }
        }
        
        private void SetupVideoPlayer(string videoPath)
        {
            if (!File.Exists(videoPath))
            {
                Debug.LogError($"Video file not found: {videoPath}");
                return;
            }

            _videoPlayer.url = videoPath;
            _videoPlayer.targetTexture = renderTex;
            _videoDisplay.texture = renderTex;

            _videoDisplay.gameObject.SetActive(true);
            _playButton.gameObject.SetActive(true);
            _pauseButton.gameObject.SetActive(false);
            _slider.value = 0;
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
                SetupVideoPlayer(_videoFilePath);
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
                if (!string.IsNullOrEmpty(path))
                {
                    _videoFilePath = path;
                    _videoWasRecorded = true;
                    SetupVideoPlayer(_videoFilePath);
                }
            });
        }

        private void ClearVideoPreview()
        {
            _videoPlayer.Stop();
            _videoPlayer.targetTexture = null;
            _videoDisplay.texture = null;

            RenderTexture.active = renderTex;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;

            _videoDisplay.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _slide = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_videoPlayer.frameCount > 0)
            {
                var frame = _slider.value * _videoPlayer.frameCount;
                _videoPlayer.frame = (long)frame;
            }
            _slide = false;
        }
    }
}
