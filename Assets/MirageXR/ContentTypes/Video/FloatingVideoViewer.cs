using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace MirageXR
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FloatingVideoViewer : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private float _width = 0.32f;
        private float _height = 0.18f;

        [Tooltip("H.264 encoded video file. .mp4 and .mov formats supported")]
        [SerializeField] private string videoName = "video.mp4";
        [Tooltip("Audio file. Only .wav format supported for external sources. Internally, .mp3 are supported as well")]
        [SerializeField] private string audioName = "audio.wav";
        [Tooltip("Set to false to read from project's 'Resources' folder; set to true to read from applications 'LocalState' folder on HoloLens, or online, if filename starts with 'http'")]
        [SerializeField] private bool useExternalSource = false;

        private bool isAudioReady = false;
        private bool isVideoReady = false;
        private bool isPlaying = false;
        private bool isPaused = false;

        [SerializeField] private bool InPanel;
        // private Transform _contentPanel;
        private Vector3 _originalPosition = Vector3.zero;
        private Quaternion _originalRotation = Quaternion.identity;
        private Vector3 _originalScale = Vector3.one;

        private bool _originalGuideState;
        private GameObject _thinLine;

        private LearningExperienceEngine.ToggleObject _obj;

        [SerializeField] private GameObject _landscapePlayerObject;
        [SerializeField] private GameObject _portraitPlayerObject;


        [Tooltip("The video texture component")]
        private UnityEngine.UI.RawImage _renderTexture;
        [SerializeField] private UnityEngine.UI.RawImage _renderTextureLand;
        [SerializeField] private UnityEngine.UI.RawImage _renderTexturePort;

        private VideoPlayer _videoPlayer;
        [SerializeField] private VideoPlayer _videoPlayerLandscape;
        [SerializeField] private VideoPlayer _videoPlayerPortrait;

        private AudioSource _audioSource;
        [SerializeField] private AudioSource _audioSourceLandscape;
        [SerializeField] private AudioSource _audioSourcePortrait;

        public bool VideoClipLoaded => _videoPlayer.clip != null;
        public float VideoDuration => (float)_videoPlayer.length;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="content">Action toggle object.</param>
        /// <returns>Returns true if initialization successful.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject content)
        {
            _obj = content;

            if (_obj.key == "P")
            {
                _landscapePlayerObject.SetActive(false);
                _portraitPlayerObject.SetActive(true);

                _videoPlayer = _videoPlayerPortrait;
                _audioSource = _audioSourcePortrait;
                _renderTexture = _renderTexturePort;
            }
            else
            {
                _landscapePlayerObject.SetActive(true);
                _portraitPlayerObject.SetActive(false);
                _videoPlayer = _videoPlayerLandscape;
                _audioSource = _audioSourceLandscape;
                _renderTexture = _renderTextureLand;
            }

            if (ServiceManager.GetService<VideoAudioTrackGlobalService>().UseAudioTrack)
            {
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                _videoPlayer.EnableAudioTrack(0, true);
                _videoPlayer.SetTargetAudioSource(0, _audioSource);
            }

            // Check that url is not empty.
            if (string.IsNullOrEmpty(content.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(content))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            // Rename with the predicate + id to get unique name.
            name = content.predicate;

            // Load video from resources.
            videoName = content.url.StartsWith("resources://") ? content.url.Replace("resources://", string.Empty) : content.url;

            string videoFilePath;
            // Video stored in the HoloLens's application data's "LocalState" folder, or online
            if (videoName.StartsWith("http"))
            {
                // Online file stored locally
                var url = videoName.Split('/');
                var filename = url[url.Length - 1];

                videoFilePath = Path.Combine(activityManager.ActivityPath, filename);
            }
            else
            {
                // Locally stored file
                videoFilePath = Path.Combine(Application.persistentDataPath, videoName);
            }

            // Adjust the duration of the trigger
            var myTrigger = activityManager.ActiveAction.triggers.Find(t => t.id == _obj.poi);
            if (myTrigger != null)
                activityManager.ActiveAction.triggers.Find(t => t == myTrigger).duration = (float)_videoPlayer.length;

            Debug.Log($"Trying to load video: {videoFilePath}");

#if !UNITY_ANDROID
            videoFilePath = $"file://{videoFilePath}";
#endif

            SetupNewRenderTexture();

            _videoPlayer.url = videoFilePath;
            if (!content.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(content))
                    return false;

                _thinLine = transform.FindDeepChild("ThinLine").gameObject;
            }

            // Set scaling if defined in action configuration.
            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();

            // load scaling
            transform.localScale = _obj.scale != 0 ? new Vector3(_obj.scale, _obj.scale, _obj.scale) : Vector3.one;

            // this ensures objectmanipulator and billboard components are set
            GetComponentInParent<PoiEditor>().UpdateManipulationOptions(gameObject);

            _videoPlayer.Play();
            if (ServiceManager.GetService<VideoAudioTrackGlobalService>().UseAudioTrack)
            {
                _audioSource.Play();
            }

            OnLock(_obj.poi, _obj.positionLock);
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            // Check if trigger is active
            StartCoroutine(ActivateTrigger());
            // If all went well, return true.
            return base.Init(content);
        }

        private void SetupNewRenderTexture()
        {
            RenderTexture rendTex;

            rendTex = new RenderTexture(1920, 1080, 24);

            _renderTexture.name = "Tex_" + _obj.poi;
            _renderTexture.texture = rendTex;
            _videoPlayer.targetTexture = rendTex;
        }

        /// <summary>
        /// This method starts playback of the previously initialized video.
        /// If a video is already being played, it is restarted from the beginning.
        /// </summary>
        public void PlayVideo()
        {
            if (isVideoReady == false || isAudioReady == false)
            {
                return;
            }
            var videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (videoPlayer == null) return;
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            videoPlayer.Play();
            isPlaying = true;
        }

        private void CheckForTrigger()
        {
            if (_obj == null || _videoPlayer == null) return;
        }

        private IEnumerator ActivateTrigger()
        {
            if (activityManager.EditModeActive) yield break;

            while (_obj == null || _videoPlayer == null)
                yield return null;

            var myTrigger = activityManager.ActiveAction.triggers.Find(t => t.id == _obj.poi);
            if (myTrigger != null)
            {
                _videoPlayer.isLooping = false;
                myTrigger.duration = (float)_videoPlayer.length;
            }

            while (_videoPlayer.isPlaying)
                yield return null;

            var triggerDuration = myTrigger.duration;
            yield return new WaitForSeconds(triggerDuration);

            if (!activityManager.IsLastAction(activityManager.ActiveAction))
            {
                if (activityManager.ActiveAction != null)
                {
                    activityManager.ActiveAction.isCompleted = true;
                }

                activityManager.ActivateNextAction();
                TaskStationDetailMenu.Instance.SelectedButton = null;
            }
        }

        /// <summary>
        /// Pause the playback of video, or if already paused, resume play
        /// </summary>
        public void PauseVideo()
        {
            if (isVideoReady == false || isAudioReady == false)
            {
                return;
            }
            var videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Pause();
                    isPlaying = false;
                    isPaused = true;
                }
                else
                {
                    videoPlayer.Play();
                    isPlaying = true;
                    isPaused = false;
                }
            }
        }

        /// <summary>
        /// Stop the playback of video
        /// </summary>
        public void StopVideo()
        {
            if (isVideoReady == false || isAudioReady == false)
            {
                return;
            }
            var videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                    isPlaying = false;
                    isPaused = false;
                }
            }
        }

        /// <summary>
        /// Set the volume of the audio in the video being played
        /// </summary>
        /// <param name="targetVolume">0.0-1.0</param>
        public void SetAudioVolume(float targetVolume)
        {
            if (isVideoReady == false || isAudioReady == false)
            {
                return;
            }
            var videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                if (videoPlayer.audioOutputMode != VideoAudioOutputMode.None)
                {
                    var audioSource = videoPlayer.GetTargetAudioSource(0);
                    if (audioSource != null)
                    {
                        audioSource.mute = false;
                        audioSource.volume = targetVolume;
                    }
                }
            }
        }

        /// <summary>
        /// This method mutes the audio on the video being played, or unmutes it, if it is already muted
        /// </summary>
        public void MuteAudio()
        {
            if (isVideoReady == false || isAudioReady == false)
            {
                return;
            }
            var videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                if (videoPlayer.audioOutputMode != VideoAudioOutputMode.None)
                {
                    var audioSource = videoPlayer.GetTargetAudioSource(0);
                    if (audioSource != null)
                    {
                        audioSource.mute = !audioSource.mute;
                    }
                }
            }
        }

        /// <summary>
        /// This method creates a video player according to the input dimensions.
        /// Set "videoName" and "audioName" variables before calling this method, or default filenames "video" and "audio" are used.
        /// Destroys any already existing video player in this GameObject.
        /// PlayVideo() must be called to start the video playback.
        /// </summary>
        /// <param name="width">Use to set video aspect ratio. If 0 or negative, default 16:9 is used.</param>
        /// <param name="height">Use to set video aspect ratio. If 0 or negative, default 16:9 is used.</param>
        /// <param name="hasAudio">If true, load audio file. Current Unity version (5.6.0) requires audio in a separate file.</param>
        /// <param name="useExternalVideoSource">If true, load video from application's LocalState folder, if false, load from project resources.</param>
        public void CreateVideoPlayer(float width = 0.16f, float height = 0.09f, bool hasAudio = true, bool useExternalVideoSource = false)
        {
            if (width > 0 && height > 0)
            {
                _width = width;
                _height = height;
            }
            else
            {
                _width = 0.32f;
                _height = 0.18f;
            }
            useExternalSource = useExternalVideoSource;
            if (gameObject.GetComponent<VideoPlayer>() != null)
            {
                Destroy(gameObject.GetComponent<VideoPlayer>());
            }
            if (gameObject.GetComponent<AudioSource>() != null)
            {
                Destroy(gameObject.GetComponent<AudioSource>());
            }
            isVideoReady = false;
            isAudioReady = false;
            isPlaying = false;
            isPaused = false;

            // Create video player screen
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = CreatePlaneMesh();
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material.shader = Shader.Find("Unlit/Texture");

            // Create video player component and attach an audio source to it (direct audio will be integrated to Unity video player in later versions)
            var videoPlayer = gameObject.AddComponent<VideoPlayer>();
            var audioPlayer = gameObject.AddComponent<AudioSource>();
            if (useExternalSource)
            {
                string videoFilePath;
                // Video stored in the HoloLens's application data's "LocalState" folder, or online
                if (videoName.StartsWith("http"))
                {
                    // Online file stored locally
                    var url = videoName.Split('/');
                    var filename = url[url.Length - 1];

                    videoFilePath = $"file://{Path.Combine(activityManager.ActivityPath, filename)}";
                }
                else
                {
                    // Locally stored file
                    videoFilePath = $"file://{Path.Combine(Application.persistentDataPath, videoName)}";
                }

                Debug.Log($"Trying to load video: {videoFilePath}");
                videoPlayer.url = videoFilePath;
                videoPlayer.prepareCompleted += VideoPrepared;
                videoPlayer.Prepare();
                if (hasAudio)
                {
                    StartCoroutine(nameof(LoadAudio));
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    videoPlayer.SetTargetAudioSource(0, audioPlayer);
                }
                else
                {
                    Destroy(audioPlayer);
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    isAudioReady = true;
                }
            }
            else
            {
                // Video stored in Unity project's "Resources" folder
                if (videoName.EndsWith(".mp4") || videoName.EndsWith(".mov"))
                {
                    videoName = videoName.Substring(0, videoName.Length - 4);
                }
                Debug.Log("Trying to load video: " + videoName);
                VideoClip videoClip = Resources.Load(videoName, typeof(VideoClip)) as VideoClip;
                videoPlayer.clip = videoClip;
                if (hasAudio)
                {
                    // If the audio clip name has a suffix, remove it
                    if (audioName.EndsWith(".mp3") || audioName.EndsWith(".wav"))
                    {
                        audioName = audioName.Substring(0, audioName.Length - 4);
                    }
                    Debug.Log("Trying to load audio: " + audioName);
                    AudioClip audioClip = Resources.Load(audioName, typeof(AudioClip)) as AudioClip;
                    audioPlayer.clip = audioClip;
                    audioPlayer.playOnAwake = false;
                    audioPlayer.loop = true;
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    videoPlayer.SetTargetAudioSource(0, audioPlayer);
                    isAudioReady = true;
                }
                else
                {
                    Destroy(audioPlayer);
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    isAudioReady = true;
                }
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
                videoPlayer.targetCameraAlpha = 1.0f;
                videoPlayer.frame = 0;
                videoPlayer.isLooping = false;
                videoPlayer.Play();
                isVideoReady = true;
            }
        }

        private void VideoPrepared(VideoPlayer videoPlayer)
        {
            if (isVideoReady == false)
            {
                Debug.Log("Video prepared!");
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
                videoPlayer.targetCameraAlpha = 1.0f;
                videoPlayer.frame = 0;
                videoPlayer.isLooping = true;
                videoPlayer.Play();
                isVideoReady = true;
            }
        }

        private IEnumerator LoadAudio()
        {
            AudioSource audioPlayer = gameObject.GetComponent<AudioSource>();
            if (audioName.StartsWith("http") == false)
            {
                // Local file
                string dataPath = Application.persistentDataPath;
                string completeAudioName = "file://" + dataPath + "/" + audioName;
                Debug.Log("Trying to load audio: " + completeAudioName);
                using var request = UnityWebRequestMultimedia.GetAudioClip(completeAudioName, AudioType.WAV);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[FloatingVideoViewer]: LoadAudio() error => {request.error}");
                }
                else
                {
                    var audioClip = DownloadHandlerAudioClip.GetContent(request);
                    audioPlayer.clip = audioClip;
                    audioPlayer.playOnAwake = false;
                    audioPlayer.loop = false;
                    isAudioReady = true;
                }
            }
            else
            {
                // Online file
                Debug.Log("Trying to download audio: " + audioName);
                using var request = UnityWebRequestMultimedia.GetAudioClip(audioName, AudioType.WAV);
                yield return request.SendWebRequest();
                AudioClip audioClip =  DownloadHandlerAudioClip.GetContent(request);
                audioPlayer.clip = audioClip;
                audioPlayer.playOnAwake = false;
                audioPlayer.loop = false;
                isAudioReady = true;
            }
        }

        /// <summary>
        /// Create a simple 2-triangle rectangle mesh in standing up position
        /// </summary>
        private Mesh CreatePlaneMesh()
        {
            Mesh m = new Mesh
            {
                name = "PlaneMesh",
                vertices = new Vector3[] {
            new Vector3(_width / 2f, -_height / 2f, 0),
            new Vector3(-_width / 2f, -_height / 2f, 0),
            new Vector3(-_width / 2f, _height / 2f, 0),
            new Vector3(_width / 2f, _height / 2f, 0)
        },
                uv = new Vector2[] {
            new Vector2(1, 0),
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        },
                triangles = new int[] { 0, 1, 2, 0, 2, 3 }
            };
            m.RecalculateNormals();

            return m;
        }

        public void ToggleInPanel(bool inPanel)
        {
            if (inPanel)
            {
                _originalPosition = transform.position;
                _originalRotation = transform.rotation;
                _originalScale = transform.localScale;

                if (_thinLine != null)
                {
                    _originalGuideState = _thinLine.activeSelf;
                    _thinLine.SetActive(false);
                }

                _videoPlayer.frame = 0;

                InPanel = true;
            }

            else
            {
                InPanel = false;
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
                transform.localScale = _originalScale;

                if (_thinLine != null)
                    _thinLine.SetActive(_originalGuideState);
            }
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                GetComponent<BoundsControl>().enabled = !_obj.positionLock;

                GetComponentInParent<PoiEditor>().IsLocked(_obj.positionLock);

                if (gameObject.GetComponent<ObjectManipulator>())
                {
                    gameObject.GetComponent<ObjectManipulator>().enabled = !_obj.positionLock;
                }
            }
        }

        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }

        public override void Delete()
        {

        }
    }
}