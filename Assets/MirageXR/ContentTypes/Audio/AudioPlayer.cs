using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public class AudioPlayer : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [Tooltip("Audio file. Only .wav format supported for external sources. Internally, .mp3 are supported as well")]
        [SerializeField] private string audioName = "audio.wav";
        public string AudioName => audioName;

        [Tooltip("Set to false to read from project's 'Resources' folder; set to true to read from applications 'LocalState' folder on HoloLens, or online, if filename starts with 'http'")]
        [SerializeField] private bool useExternalSource = false;

        private AudioEditor audioEditor;
        private float audioLength;
        private bool audio3dMode;
        public bool Loop { get; private set; }


        [SerializeField] private GameObject icon;
        [SerializeField] private Sprite iconSprite;
        public Sprite IconSprite => iconSprite;

        [SerializeField] private Sprite pauseIcon;

        [SerializeField] private SpriteRenderer iconImage;
        public SpriteRenderer IconImage => iconImage;

        public string AudioSpatialType { get; private set; }

        public DialogRecorder DialogRecorderPanel
        {
            get; set;
        }

        private bool isReady = false;
        private bool isPlaying = false;

        private LearningExperienceEngine.ToggleObject _obj;

        public LearningExperienceEngine.ToggleObject MyAnnotation => _obj;

        private GameObject _contentObject;

        private void Awake()
        {
            // Makes no sense to use gaze guide with audio...
            UseGuide = false;

            var actionEditor = FindObjectOfType<ActionEditor>();
            audioEditor = (AudioEditor)actionEditor.CreateEditorView(LearningExperienceEngine.ContentType.AUDIO);
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            // check audio is 2d or 3d
            audio3dMode = obj.option.Split('#')[0] == "3d";

            float radius = 0f;

            Loop = obj.option.Split('#')[1] == "1";

            // 3d
            if (audio3dMode)
            {
                // get the radius if it is 3d audio
                radius = float.Parse(obj.option.Split('#')[2]);
            }
            else
            {
                Destroy(icon);
            }

            // Load audio from resources.
            if (obj.url.StartsWith("resources://"))
            {
                audioName = obj.url.Replace("resources://", "");
                CreateAudioPlayer(false, audio3dMode, radius, Loop);
            }

            // Load audio from server.
            else
            {
                audioName = obj.url;
                CreateAudioPlayer(true, audio3dMode, radius, Loop);
            }

            // If all went well, return true.
            return true;
        }


        private void Update()
        {
            if (audioEditor && (audioEditor.IsRecording || audioEditor.IsPlaying))
            {
                StopAudio();
                if (iconImage)
                    iconImage.sprite = pauseIcon;
                return;
            }

            if (isPlaying == false)
            {
                if (isReady == true)
                {
                    PlayAudio();
                }
            }
            if (Input.GetKeyUp(KeyCode.M))
            {
                MuteAudio();
            }
        }

        /// <summary>
        /// This method starts playback of the audio file, unmuted and on full volume.
        /// If the audio track is already being played, it will be restarted from the beginning.
        /// If audio source doesn't exist, or hasn't finished loading, the method returns without doing anything
        /// </summary>
        public void PlayAudio()
        {
            if (isReady == false)
            {
                return;
            }
            var audioSource = gameObject.GetComponent<AudioSource>();

            if (audioSource != null)
            {
                if (audioSource.isPlaying == true)
                {
                    audioSource.Stop();
                }
                audioSource.mute = false;
                audioSource.volume = 1.0f;
                audioSource.Play();
                isPlaying = true;

                audioLength = audioSource.clip.length;
                var myTrigger = activityManager.ActiveAction.triggers.Find(t => t.id == _obj.poi);
                if (myTrigger != null)
                {
                    StartCoroutine(ActivateTrigger(audioSource, myTrigger));
                }
            }
        }

        private static IEnumerator ActivateTrigger(AudioSource audioSource, LearningExperienceEngine.Trigger trigger)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            if (!activityManager.EditModeActive)
            {
                var triggerDuration = trigger.duration;
                yield return new WaitForSeconds(triggerDuration);


                if (activityManager.ActiveAction != null)
                {
                    activityManager.ActiveAction.isCompleted = true;
                }

                if (int.TryParse(trigger.value, out var stepNumber))
                {
                    activityManager.ActivateActionByIndex(stepNumber - 1);
                }

                TaskStationDetailMenu.Instance.SelectedButton = null;

            }
        }

        /// <summary>
        /// Pause the playback of audio, or if already paused, resume play
        /// </summary>
        public void PauseAudio()
        {
            if (isReady == false)
            {
                return;
            }
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if (audioSource.isPlaying == true)
                {
                    audioSource.Pause();
                    isPlaying = false;
                }
                else
                {
                    audioSource.Play();
                    isPlaying = true;
                }
            }
        }


        /// <summary>
        /// Stop the playback of audio
        /// </summary>
        public void StopAudio()
        {
            if (isReady == false)
            {
                return;
            }
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if (audioSource.isPlaying == true)
                {
                    audioSource.Stop();
                    isPlaying = false;
                }
            }
        }


        /// <summary>
        /// Set the volume of the audio
        /// </summary>
        /// <param name="targetVolume">0.0-1.0</param>
        public void SetAudioVolume(float targetVolume)
        {
            if (isReady == false)
            {
                return;
            }
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.mute = false;
                audioSource.volume = targetVolume;
            }
        }

        /// <summary>
        /// This method mutes the audio, or unmutes it, if it is already muted
        /// </summary>
        public void MuteAudio()
        {
            if (isReady == false)
            {
                return;
            }
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.mute = !audioSource.mute;
            }
        }


        /// <summary>
        /// This method is used to set the audio to loop itself or be a one-shot
        /// </summary>
        /// <param name="setLooping">if true, audio repeats itself</param>
        public void LoopAudio(bool setLooping)
        {
            if (isReady == false)
            {
                return;
            }
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.loop = setLooping;
            }
        }


        /// <summary>
        /// This method creates an audio player
        /// Destroys any already existing audio player in this GameObject.
        /// PlayAudio() must be called to start the audio playback.
        /// </summary>
        /// <param name="useExternalAudioSource">If true, load video from application's LocalState folder, if false, load from project resources.</param>
        public void CreateAudioPlayer(bool useExternalAudioSource = false, bool spatialAudio = false, float radius = 0f, bool loopAduio = false)
        {

            useExternalSource = useExternalAudioSource;
            if (gameObject.GetComponent<AudioSource>() != null)
            {
                Destroy(gameObject.GetComponent<AudioSource>());
            }
            isReady = false;
            isPlaying = false;

            // Create audio source component
            var audioPlayer = gameObject.AddComponent<AudioSource>();

            if (spatialAudio)
            {
                audioPlayer.spatialBlend = 1;
                audioPlayer.rolloffMode = AudioRolloffMode.Linear;
                audioPlayer.minDistance = 0.1f;
                audioPlayer.maxDistance = radius;
            }

            audioPlayer.loop = loopAduio;

            if (useExternalSource == true)
            {
                StartCoroutine(nameof(LoadAudio));
            }
            else
            {
                // If the audio clip name has a suffix, remove it
                if (audioName.EndsWith(".mp3") || audioName.EndsWith(".wav"))
                {
                    audioName = audioName.Substring(0, audioName.Length - 4);
                }
                Debug.LogTrace("Trying to load audio: " + audioName);
                AudioClip audioClip = Resources.Load(audioName, typeof(AudioClip)) as AudioClip;
                audioPlayer.clip = audioClip;
                audioPlayer.playOnAwake = false;
                isReady = true;
            }
            // _contentObject = ActionContentStorageManager.Instance.CreateObject(gameObject, _obj);
        }


        private IEnumerator LoadAudio()
        {
            AudioSource audioPlayer = gameObject.GetComponent<AudioSource>();
            if (audioName.StartsWith("http") == false)
            {
                // Local file
                string dataPath = Application.persistentDataPath;
                string completeAudioName = "file://" + dataPath + "/" + audioName;
                Debug.LogTrace("Trying to load audio: " + completeAudioName);
                using (UnityWebRequest request =
                       UnityWebRequestMultimedia.GetAudioClip(completeAudioName, AudioType.WAV))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[Audio Palyer]: LoadAudio() filed with {request.error}");
                    }
                    else
                    {
                        var audioClip = DownloadHandlerAudioClip.GetContent(request);
                        audioPlayer.clip = audioClip;
                        audioPlayer.playOnAwake = false;
                        isReady = true;
                    }
                }
            }

            else
            {
                // Online file stored locally
                var url = audioName.Split('/');
                var filename = url[url.Length - 1];
                var audioType = GetAudioType(filename);

                var completeAudioName = "file://" + activityManager.ActivityPath + "/" + filename;
                Debug.LogTrace("Trying to load audio: " + completeAudioName);
                if (audioType != AudioType.UNKNOWN)
                {
                    using (UnityWebRequest request =
                           UnityWebRequestMultimedia.GetAudioClip(completeAudioName, audioType))
                    {
                        yield return request.SendWebRequest();
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            AudioClip audioClip  = DownloadHandlerAudioClip.GetContent(request);
                            audioPlayer.clip = audioClip;
                            audioPlayer.playOnAwake = false;
                            //audioPlayer.loop = false;
                            isReady = true;
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning(
                                $"The file: \n {completeAudioName} has an unknown audio type or Error. " +
                                $"Please use .wav or .mp3 or check {request.error}");
                        }
                        
                    }
                }
                else
                {
                    Debug.LogWarning("The file: \n" + completeAudioName + "\n has an unknown audio type. Please use .wav or .mp3");
                }

                // Online file
                /*
                Debug.Log ("Trying to download audio: " + audioName);
                WWW www = new WWW (audioName);
                yield return www;
                AudioClip audioClip = www.GetAudioClip (false, false, AudioType.WAV);
                audioPlayer.clip = audioClip;
                audioPlayer.playOnAwake = false;
                audioPlayer.loop = false;
                isReady = true;
                */
            }
        }

        private AudioType GetAudioType(string filename)
        {

            if (filename.EndsWith("wav"))
            {
                return AudioType.WAV;
            }
            else if (filename.EndsWith("mp3"))
            {
                return AudioType.MPEG;
            }
            else
            {
                return AudioType.UNKNOWN;
            }
        }

        private void OnDestroy()
        {
            if (_contentObject != null)
                Destroy(_contentObject);
        }


        public float getAudioLength()
        {
            return audioLength;
        }


        public float getCurrenttime()
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            return audioSource.time;
        }
    }
}