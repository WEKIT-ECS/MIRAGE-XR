using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Represents a virtual instructor recorder in the MirageXR project. This class is responsible for recording the
    /// user if a question is asked by the virtual instructor.
    /// </summary>
    public class VirtualInstructorRecoder : MonoBehaviour
    {
        /// <summary>
        /// Represents a record button
        /// </summary>
        [SerializeField] private Button _btnRecord;

        /// <summary>
        /// Send button
        /// </summary>
        [SerializeField] private Button _btnSendRecord;

        /// <summary>
        /// Game object of the buttons that can be activated and deactivate. 
        /// </summary>
        [SerializeField] private GameObject _Record;
        [SerializeField] private GameObject _SendRecord;
        [SerializeField] private GameObject _Loading;

        /// <summary>
        /// Maximum record time in seconds.
        /// </summary>
        [SerializeField] private int _maxRecordTime;

        /// <summary>
        /// AudioClip for storing the response from the virtual instructor.
        /// </summary>
        [SerializeField] private AudioSource responseClip;

        /// <summary>
        /// The sample rate used for recording audio.
        /// </summary>
        [SerializeField] private int sampleRate;

        /// <summary>
        /// Represents a recorded audio clip of a question.
        /// </summary>
        private AudioClip questionClip;

        /// <summary>
        /// Represents a microphone used for recording audio inputs.
        /// </summary>
        private string microphone;

        /// <summary>
        /// Flag indicating if the recording is currently being done.
        /// </summary>
        private bool recoding;


        /// <summary>
        /// Represents a reference to the subtitle box GameObject in the VirtualInstructorRecoder class.
        /// The subtitle box is responsible for displaying subtitles in the virtual instructor recorder and is a
        /// temporary solution
        /// </summary>
        [SerializeField] private GameObject subtitleBox; // Temp

        /// <summary>
        /// Represents the subtitle text in the VirtualInstructorRecorder class. This variable is of type `TMP_Text`
        /// from the TMPro namespace and is a temporary solution. 
        /// It is responsible for displaying the subtitle text on the virtual instructor.
        /// </summary>
        [SerializeField] private TMP_Text subtitleText; // Temp

        /// <summary>
        /// decides if a virtual instructor is on the screen and call the function Hide and Show. 
        /// </summary>
        public void Update()
        {
            if (!RootObject.Instance.virtualInstructorManager.IsVirtualInstructorInList())
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        /// <summary>
        /// Hides the UI if no virtual instructor is on the screen. 
        /// </summary>
        private void Hide()
        {
            _Record.gameObject.SetActive(false);
            _SendRecord.gameObject.SetActive(false);
        }
        /// <summary>
        /// Shows the UI if no virtual instructor is on the screen. 
        /// </summary>
        private void Show()
        {
            _Record.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets up the virtual instructor  and the recorder buttons.
        /// </summary>
        public void Awake()
        {
            if (RootObject.Instance.virtualInstructorManager.IsVirtualInstructorInList()) Show();
        }

        /// <summary>
        /// Coroutine for countdown timer before calling SendRecording method.
        /// </summary>
        IEnumerator CountdownCoroutine()
        {
            yield return new WaitForSeconds(_maxRecordTime);
            if(recoding) SendRecording();
        }

        /// <summary>
        /// Starts the recording process.
        /// </summary>
        public void StartRecording()
        {
            recoding = true;
            if (Microphone.devices.Length > 0)
            {
                questionClip = Microphone.Start(null, false, _maxRecordTime, sampleRate);
                StartCoroutine(CountdownCoroutine()); 
            }
            else
            {
                UnityEngine.Debug.LogError("No microphone detected!");
            }
        }

        /// <summary>
        /// Sends the recording to the virtual instructor and plays back the response.
        /// </summary>
        public async void SendRecording()
        {
            _Loading.SetActive(true);
            Microphone.End(null);
            recoding = false;
            (string text, AudioClip clip)= await RootObject.Instance.virtualInstructorManager.AskClosestInstructor(questionClip);
            responseClip.clip = clip;
            ShowSubtitle(text);
            responseClip.Play();
            StartCoroutine(WaitForAudioEnd());
            
        }

        /// <summary>
        /// Waits for the audio playback to end. Temp!
        /// </summary>
        /// <returns>Coroutine object.</returns>
        private IEnumerator WaitForAudioEnd()
        {
            yield return new WaitUntil(() => !responseClip.isPlaying);
            OnAudioClipEnd();
        }

        /// <summary>
        /// Callback method called when the audio clip playback ends. Temp!
        /// </summary>
        private void OnAudioClipEnd()
        {
            _Loading.SetActive(false);
            RemoveSubtitle();

        }

        /// <summary>
        /// Shows a subtitle on the screen. Temp!
        /// </summary>
        private void ShowSubtitle(string text) // Temp
        {
            subtitleText.text = text;
            subtitleBox.gameObject.SetActive(true);
        }

        /// <summary>
        /// Removes the subtitle text from the VirtualInstructorRecorder class. Temp!
        /// </summary>
        private void RemoveSubtitle() // Temp
        {
            subtitleText.text = string.Empty;
            subtitleBox.gameObject.SetActive(false);
        }
        
    }
}
