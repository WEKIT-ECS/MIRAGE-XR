using System;
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

        public void Update()
        {
            if (!RootObject.Instance.virtualInstructorOrchestrator.IsVirtualInstructorInList())
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        private void Hide()
        {
            _Record.gameObject.SetActive(false);
            _SendRecord.gameObject.SetActive(false);
        }
        private void Show()
        {
            _Record.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets up the virtual instructor  and the recorder buttons.
        /// </summary>
        public void Awake()
        {
            if (RootObject.Instance.virtualInstructorOrchestrator.IsVirtualInstructorInList()) Show();
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
            responseClip.clip =  await RootObject.Instance.virtualInstructorOrchestrator.AskClosestInstructor(questionClip); 
            responseClip.Play();
            StartCoroutine(WaitForAudioEnd());
        }

        private IEnumerator WaitForAudioEnd()
        {
            yield return new WaitUntil(() => !responseClip.isPlaying);
            OnAudioClipEnd();
        }

        private void OnAudioClipEnd()
        {
            _Loading.SetActive(false);
        }
    }
}
