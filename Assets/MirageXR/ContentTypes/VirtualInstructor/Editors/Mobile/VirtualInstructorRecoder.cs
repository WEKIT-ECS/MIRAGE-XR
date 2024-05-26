using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Represents a virtual instructor recorder in the MirageXR project. This class is responsible for recording the
    /// user if a question get ask the virtual instructor.
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


          
        private AIModel speechToTextModel; // @todo remove when done 

        private AIModel textToSpeechModel; // @todo remove when done 

        private AIModel languageLanguageModel; // @todo remove when done 

        private VirtualInstructor demo; // @todo remove when done 

  
        void Awake()
        {
            /// Demo for Testing
            speechToTextModel = new AIModel("listen/", "f", "f", "f");  // @todo remove when done 
            textToSpeechModel = new AIModel("speak/", "Alloy", "Female human voice", "alloy");  // @todo remove when done 
            languageLanguageModel = new AIModel("think/", "gpt-3.5-turbo", "gpt-3.5-turbo", "gpt-3.5-turbo");  // @todo remove when done 
            demo = new VirtualInstructor(_Record, languageLanguageModel, textToSpeechModel, speechToTextModel, "This is a Test, Test!");// @todo remove when done 
            /// Demo for Testing
            _btnRecord.onClick.AddListener(StartRecording);
            _btnSendRecord.onClick.AddListener(SendRecording);
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
        private void StartRecording()
        {
            recoding = true;
            if (Microphone.devices.Length > 0) questionClip = Microphone.Start(null, false, _maxRecordTime, sampleRate);
            else
            {
                UnityEngine.Debug.LogError("No microphone detected!");
            }
            StartCoroutine(CountdownCoroutine()); 
            _SendRecord.gameObject.SetActive(true);
            _Record.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sends the recording to the virtual instructor and plays back the response.
        /// </summary>
        private async void SendRecording()
        {
            Microphone.End(null);
            recoding = false;
            _SendRecord.gameObject.SetActive(false);
            _Record.gameObject.SetActive(true);
            responseClip.clip =  await demo.AskVirtualInstructor(questionClip); 
            responseClip.Play();
             
            
        }
    }
}
