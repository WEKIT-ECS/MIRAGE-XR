using LearningExperienceEngine;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MirageXR
{
    public class DialogRecorder : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private GameObject recordButton;
        [SerializeField] private GameObject stopButton;
        [SerializeField] private GameObject playButton;
        [SerializeField] private GameObject closeButton;
        [SerializeField] private GameObject openButton;

        public GameObject RecordButton => recordButton;
        public GameObject PlayButton => playButton;
        public GameObject CloseButton => closeButton;
        public GameObject OpenButton => openButton;

        public Toggle LoopToggle;

        private AudioEditor _audioEditor;
        private AudioSource _audioSource;

        public float DialogLength()
        {
            if (!_audioSource || !_audioSource.clip) return 0f;
            return _audioSource.clip.length;
        }

        public string DialogSaveName
        {
            get; set;
        }

        private string _clipPath;

        bool isPlaying;
        public bool isRecording { get; private set; }

        private IEnumerator Start()
        {
            while (!MyCharacter || !MyCharacter.CharacterParsed)
            {
                yield return null;
            }

            Init();
            SetEditorState(activityManager.EditModeActive);
        }

        public CharacterController MyCharacter
        {
            get; set;
        }



        private void Init()
        {
            DialogSaveName = $"characterinfo/{activityManager.ActiveActionId}_{MyCharacter.ToggleObject.poi}.wav";

            // For character who has lipsync the audio source is added to the object with ThreeLSControl component
            var threeLSControl = MyCharacter.GetComponentInChildren<ThreeLSControl>();
            if (threeLSControl)
            {
                _audioSource = threeLSControl.GetComponent<AudioSource>();
            }
            else
            {
                _audioSource = MyCharacter.GetComponent<AudioSource>();
            }

            MyCharacter.GetComponent<CharacterController>().AudioEditorCheck();
            _audioEditor = MyCharacter.MyAudioEditor;

            if (_audioEditor != null)
            {
                _audioEditor.DialogRecorderPanel = this;
                _audioSource.loop = LoopToggle.isOn;
                _clipPath = Path.Combine(activityManager.ActivityPath, DialogSaveName);

                if (File.Exists(_clipPath))
                {
                    _audioSource.clip = LearningExperienceEngine.SaveLoadAudioUtilities.LoadAudioFile(_clipPath);
                    _audioEditor.PlayerAudioSource.clip = _audioSource.clip;
                }
            }

        }


        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= SetEditorState;
        }

        private void SetEditorState(bool editModeActive)
        {
            // Dialog recorder is closed manually dont open it
            if (openButton.activeInHierarchy) return;

            if (!_audioSource || _audioSource.clip == null)
            {
                recordButton.SetActive(editModeActive);
                stopButton.SetActive(!editModeActive);
            }
            else
            {
                recordButton.SetActive(false);
                stopButton.SetActive(true);
            }

            LoopToggle.gameObject.SetActive(editModeActive);
        }

        public void UpdateLoopStatus()
        {
            if (_audioSource)
                _audioSource.loop = LoopToggle.isOn;
        }

        public void OpenDialogRecorder()
        {
            if (_audioSource == null) return;

            // if a dialog is already recorded
            if (_audioSource.clip == null)
            {
                recordButton.SetActive(activityManager.EditModeActive);
                stopButton.SetActive(!activityManager.EditModeActive);
            }
            else
            {
                recordButton.SetActive(false);
                stopButton.SetActive(true);
            }

            playButton.SetActive(true);
            openButton.SetActive(false);
            closeButton.SetActive(true);
            LoopToggle.gameObject.SetActive(activityManager.EditModeActive);
        }

        public void ResetDialogRecorder()
        {
            _audioSource.clip = null;
            StopDialogRecording();
            OpenDialogRecorder();
        }

        public void CloseDialogRecorder()
        {
            recordButton.SetActive(false);
            stopButton.SetActive(false);
            playButton.SetActive(false);
            openButton.SetActive(true);
            closeButton.SetActive(false);
            LoopToggle.gameObject.SetActive(false);

            if (_audioEditor)
                _audioEditor.Close();
        }


        private bool AllowToRecord()
        {
            // Check AI or recording is active in this scene, if so send a nofication and disable recording
            var AIIsActiveInThisScene = false;
            var RecordingIsActiveInThisScene = false;
            foreach (var character in FindObjectsOfType<CharacterController>())
            {
                if (character.AIActivated)
                {
                    AIIsActiveInThisScene = true;
                    break;
                }
                else if (character.DialogRecorder.isRecording)
                {
                    RecordingIsActiveInThisScene = true;
                    break;
                }
            }

            if (AIIsActiveInThisScene)
            {
                // give the info and close
                DialogWindow.Instance.Show("Info!",
                "For recordeing an audio, you need to deactivate AI on all characters in this step.",
                new DialogButtonContent("Ok"));
                return false;
            }

            if (RecordingIsActiveInThisScene)
            {
                // give the info and close
                DialogWindow.Instance.Show("Info!",
                "You are recording audio using another character. Please stop recording there first.",
                new DialogButtonContent("Ok"));
                return false;
            }

            return true;
        }



        public void StartDialogRecording()
        {

            if (!AllowToRecord()) return;

            Init();

            MyCharacter.AudioEditorCheck(); // makes sure that there is an audio editor available
            _audioEditor.Initialize(MyCharacter.MyAction);
            _audioEditor.SaveFileName = DialogSaveName;

            isRecording = true;

            _audioEditor.StartRecording();
            recordButton.SetActive(false);
            stopButton.SetActive(true);
            playButton.SetActive(false);
            closeButton.SetActive(false);
        }

        public void StopDialogRecording()
        {
            if (isPlaying)
            {
                _audioSource.Stop();
                isPlaying = false;
            }
            else if (isRecording)
            {
                _audioEditor.StopRecording();
                _audioEditor.OnAccept();
                isRecording = false;
                PlayDialog();
            }
            playButton.SetActive(true);
            closeButton.SetActive(true);

        }

        public void PlayDialog()
        {
            if (!File.Exists(_clipPath) || MyCharacter.AIActivated) return;

            MyCharacter.AudioEditorCheck();
            var dialogClip = LearningExperienceEngine.SaveLoadAudioUtilities.LoadAudioFile(_clipPath);
            _audioSource.clip = dialogClip;
            _audioSource.loop = LoopToggle.isOn;
            _audioSource.Play();
            recordButton.SetActive(false);
            stopButton.SetActive(true);
            playButton.SetActive(true);
            closeButton.SetActive(true);
            isPlaying = true;
        }

        public void StopDialog()
        {
            if (_audioSource)
                _audioSource.Stop();
        }
    }
}
