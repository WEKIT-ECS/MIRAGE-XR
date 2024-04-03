using System;
using System.Collections;
using System.Linq; 
using TMPro;
using UnityEngine;

namespace MirageXR
{
    public class AudioPlayer : MirageXRPrefab
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
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
        [SerializeField] private TMP_Text _captionText;
        [SerializeField] private GameObject _captionObj;
        public Sprite IconSprite => iconSprite;

        [SerializeField] private Sprite pauseIcon;

        [SerializeField] private SpriteRenderer iconImage;
        public SpriteRenderer IconImage => iconImage;

        public string AudioSpatialType { get; private set; }

        public DialogRecorder DialogRecorderPanel { get; set; }

        private bool isReady = false;
        private bool isPlaying = false;

        private ToggleObject _obj;

        public ToggleObject MyAnnotation => _obj;

        private GameObject _contentObject;

        private void Awake()
        {
            UseGuide = false;

            var actionEditor = FindObjectOfType<ActionEditor>();
            audioEditor = (AudioEditor)actionEditor.CreateEditorView(ContentType.AUDIO);
        }

        public override bool Init(ToggleObject obj)
        {
            _obj = obj;

            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;

            audio3dMode = obj.option.Split('#')[0] == "3d";
            float radius = 0f;
            if (audio3dMode)
            {
                Loop = obj.option.Split('#')[1] == "1";
                if (audio3dMode)
                {
                    radius = float.Parse(obj.option.Split('#')[2]);
                }
            }
            else
            {
                Destroy(icon);
            }

            if (obj.url.StartsWith("resources://"))
            {
                audioName = obj.url.Replace("resources://", "");
                CreateAudioPlayer(false, audio3dMode, radius, Loop);
            }
            else
            {
                audioName = obj.url;
                CreateAudioPlayer(true, audio3dMode, radius, Loop);
            }

            var caption = obj.caption;
            if (caption != string.Empty)
            {
                StartCaptionDisplay(caption);
            }

            return true;
        }

        private void StartCaptionDisplay(string caption)
        {
            StartCoroutine(DisplayCaptionWithDelay(caption));
        }

private IEnumerator DisplayCaptionWithDelay(string fullCaption)
{
    // Split the full caption into words
    string[] words = fullCaption.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

    // Calculate the number of sections
    int numberOfWords = 12;
    int numberOfSections = (int)Math.Ceiling((double)words.Length / numberOfWords);

    for (int i = 0; i < numberOfSections; i++)
    {
        // Get the words for the current section
        string[] sectionWords = words.Skip(i * numberOfWords).Take(numberOfWords).ToArray();

        // Join the words back into a string
        string sectionText = string.Join(" ", sectionWords);

        // Display the text section
        _captionText.text = sectionText.Trim();
        _captionObj.SetActive(true);

        // Wait for 4 seconds before moving to the next section
        yield return new WaitForSeconds(4);
    }

    // Optionally hide or clear the caption object after all sections have been displayed
    _captionObj.SetActive(false); // Or _captionText.text = string.Empty; to clear the text
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
                _captionObj.SetActive(true);
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

        private static IEnumerator ActivateTrigger(AudioSource audioSource, Trigger trigger)
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
                WWW www = new WWW(completeAudioName);
                yield return www;
                AudioClip audioClip = www.GetAudioClip(false, false, AudioType.WAV);
                audioPlayer.clip = audioClip;
                audioPlayer.playOnAwake = false;
                isReady = true;
            }

            else
            {
                // Online file stored locally
                var url = audioName.Split('/');
                var filename = url[url.Length - 1];

                var completeAudioName = "file://" + activityManager.ActivityPath + "/" + filename;
                Debug.LogTrace("Trying to load audio: " + completeAudioName);
                WWW www = new WWW(completeAudioName);
                yield return www;

                var audioType = GetAudioType(filename);

                if (audioType != AudioType.UNKNOWN)
                {
                    AudioClip audioClip = www.GetAudioClip(false, false, audioType);

                    audioPlayer.clip = audioClip;
                    audioPlayer.playOnAwake = false;
                    //audioPlayer.loop = false;
                    isReady = true;
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