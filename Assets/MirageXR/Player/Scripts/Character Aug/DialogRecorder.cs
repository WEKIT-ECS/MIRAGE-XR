using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class DialogRecorder : MonoBehaviour
    {
        [SerializeField] private GameObject recordButton;
        [SerializeField] private GameObject stopButton;
        [SerializeField] private GameObject playButton;
        [SerializeField] private GameObject closeButton;
        [SerializeField] private GameObject openButton;
        
        public GameObject RecordButton { get { return recordButton; } }
        public GameObject PlayButton { get { return playButton; } }
        public GameObject CloseButton { get { return closeButton; } }
        public GameObject OpenButton { get { return openButton; } }

        public Toggle LoopToggle;

        private AudioEditor _audioEditor;
        private AudioSource _audioSource;

        public float DialogLength()
        {
            if (!_audioSource || !_audioSource.clip) return 0f;
            return _audioSource.clip.length;
        }

        public string DialogSaveName {
            get; set;
        }

        private string _clipPath;

        bool isPlaying;
        public bool isRecording { get; private set; }

        private void Start()
        {
            Init();
            SetEditorState(ActivityManager.Instance.EditModeActive);
        }

        public CharacterController MyCharacter
        {
            get; set;
        }

        private async void Init()
        {
            DialogSaveName = $"characterinfo/{ActivityManager.Instance.ActiveActionId}_{MyCharacter.MyToggleObject().poi}.wav";

            _audioSource = MyCharacter.GetComponentInChildren<ThreeLSControl>().GetComponent<AudioSource>();

            MyCharacter.GetComponent<CharacterController>().AudioEditorCheck();
            _audioEditor = MyCharacter.MyAudioEditor;
            _audioEditor.DialogRecorderPanel = this;
            _audioSource.loop = LoopToggle.isOn;

            _clipPath =  Path.Combine(ActivityManager.Instance.Path, DialogSaveName);

            if (File.Exists(_clipPath))
            {
                _audioSource.clip = await _audioEditor.LoadClipFromExistingFile(_clipPath);
                _audioEditor.GetPlayerAudioSource().clip = _audioSource.clip;
            }
        }

        private void OnEnable()
        {
            EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= SetEditorState;
        }

        private void SetEditorState(bool editModeActive)
        {
            //Dialog recorder is closed manually dont open it 
            if (openButton.activeInHierarchy) return;

            if (_audioSource.clip == null)
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
            if(_audioSource)
                _audioSource.loop = LoopToggle.isOn;
        }

        public void OpenDialogRecorder()
        {
            if (_audioSource == null) return;

            //if a dialog is already recorded
            if(_audioSource.clip == null)
            {
                recordButton.SetActive(ActivityManager.Instance.EditModeActive);
                stopButton.SetActive(!ActivityManager.Instance.EditModeActive);
            }
            else
            {
                recordButton.SetActive(false);
                stopButton.SetActive(true);
            }

            playButton.SetActive(true);
            openButton.SetActive(false);
            closeButton.SetActive(true);
            LoopToggle.gameObject.SetActive(ActivityManager.Instance.EditModeActive);
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

            if(_audioEditor)
                _audioEditor.Close();
        }


        private bool AllowToRecord()
        {
            //Check AI or recording is active in this scene, if so send a nofication and disable recording
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
                /// give the info and close
                DialogWindow.Instance.Show("Info!",
                "For recordeing an audio, you need to deactivate AI on all characters in this step.",
                new DialogButtonContent("Ok"));
                return false;
            }

            if (RecordingIsActiveInThisScene)
            {
                /// give the info and close
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
            else if(isRecording)
            {
                _audioEditor.StopRecording();
                _audioEditor.OnAccept();
                isRecording = false;
                PlayDialog();
            }
            playButton.SetActive(true);
            closeButton.SetActive(true);

        }

        public async void PlayDialog()
        {
            if (!File.Exists(_clipPath) || MyCharacter.AIActivated) return;

            MyCharacter.AudioEditorCheck();
            var dialogClip = await _audioEditor.LoadClipFromExistingFile(_clipPath);
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
            if(_audioSource)
                _audioSource.Stop();
        }
    }
}
