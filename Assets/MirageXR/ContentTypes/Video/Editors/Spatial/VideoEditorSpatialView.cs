using System;
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
        [Space]
        [SerializeField] private GameObject _AugmentationSettingsPanel;
        
        [Space]
        [SerializeField] private RenderTexture renderTex;
        [SerializeField] private RawImage _videoDisplay;
        [SerializeField] private VideoPlayer _videoPlayer;
  

        private string _text;
        private Texture2D _capturedImage;
        private Content<VideoContentData> _videoContent;
        
        protected static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.activityManagerOld;
        private bool _videoWasRecorded;
        private string _newFileName;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            _showBackground = false;
            base.Initialization(onClose, args);
            
            _videoContent = _content as Content<VideoContentData>;
            
            UpdateView();
            _btnCaptureVideo.onClick.AddListener(OnStartRecordingVideo);
            _btnOpenGallery.onClick.AddListener(OpenGallery);
            _btnGenerateCaption.onClick.AddListener(GenerateCaption);
            _btnSettings.onClick.AddListener(OpenSettings);
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
            if (_capturedImage) Destroy(_capturedImage);
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

            /*var step = RootObject.Instance.LEE.StepManager.CurrentStep;
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

            _videoContent.Location.Scale = CalculateScale(_capturedImage.width, _capturedImage.height);

            //await SaveImageAsync(activityId, _videoContent.Id, fileId);
            _videoContent.ContentData.Video = await RootObject.Instance.LEE.AssetsManager.CreateFileAsync(activityId, _videoContent.Id, fileId);

            RootObject.Instance.LEE.ContentManager.AddContent(_videoContent);
            RootObject.Instance.LEE.AssetsManager.UploadFileAsync(activityId, _videoContent.Id, fileId);
*/
            Close();
        }

        private Vector3 CalculateScale(int textureWidth, int textureHeight)
        {
            if (textureWidth == textureHeight)
            {
                return Vector3.one;
            }

            return textureWidth > textureHeight
                ? new Vector3(textureWidth / (float)textureHeight, 1, 0.05f)
                : new Vector3(1, textureHeight / (float)textureWidth, 0.05f);
        }
        
        private async UniTask SaveImageAsync(Guid activityId, Guid contentId, Guid fileId)
        {
            if (_capturedImage == null)
            {
                return;
            }

            var bytes = _capturedImage.EncodeToJPG();
            var folder = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, contentId, fileId);
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, "image.jpg");  //TODO: move to AssetsManager
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await File.WriteAllBytesAsync(filePath, bytes);
        }

        private void UpdateView()
        {
            if (_videoContent != null)
            {
                var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                var folder = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, _videoContent.Id, _videoContent.ContentData.Video.Id);
                var imagePath = Path.Combine(folder, "image.jpg");  //TODO: move to AssetsManager
                if (!File.Exists(imagePath))
                {
                    return;
                }

                var texture2D = MirageXR.Utilities.LoadTexture(imagePath);
                SetPreview(texture2D);
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
                SetPreview(NativeCameraController.GetVideoThumbnail(filePath));
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
                    _videoWasRecorded = true;
                    // TODO 
                    
                    _videoPlayer.targetTexture = renderTex;
                    _videoDisplay.texture = renderTex;
                    _videoPlayer.url = path;
                    _videoPlayer.Play();
                    Debug.Log("Playing video: " + path);
                }
            });
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
            var width = (float)texture2D.width / texture2D.height * IMAGE_HEIGHT;
            rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, IMAGE_HEIGHT);
            rtImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }
}
