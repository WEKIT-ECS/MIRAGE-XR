using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Represents a virtual instructor recorder in the MirageXR project. This class is responsible for recording the
    /// user if a question get ask the virtual instructor.
    /// </summary>
    public class VirtualInstructorRecorder : MonoBehaviour
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
        /// Game object of the buttons that represented the state. 
        /// </summary>
        [SerializeField] private GameObject _Record;
        [SerializeField] private GameObject _SendRecord;
        [SerializeField] private GameObject _Loading;

        /// <summary>
        /// Maximum record time in seconds.
        /// </summary>
        [SerializeField] private int _maxRecordTime; // raus

        /// <summary>
        /// AudioClip for storing the response from the virtual instructor.
        /// </summary>
        [SerializeField] private AudioSource responseClip;

        /// <summary>
        /// Represents a recorded audio clip of a question.
        /// </summary>
        private AudioClip _questionClip;

        private CancellationTokenSource _source;
        private CancellationToken _cancellationToken;
        private const int FallbackMaxRecordTime = 5;


        /// <summary>
        /// Validating the Editor input in regard to plausibility. 
        /// </summary>
        private void OnValidate()
        {
            if (_maxRecordTime > 0) return;
            UnityEngine.Debug.LogError("Max record time must be greater than 0, Is now set to 5.");
            _maxRecordTime = FallbackMaxRecordTime;
        }
        
        /// <summary>
        /// Sets up the virtual instructor  and the recorder buttons.
        /// </summary>
        public void Awake()
        {
            var instructorOrchestrator = RootObject.Instance.VirtualInstructorOrchestrator;
            instructorOrchestrator.OnVirtualInstructorsAdded.AddListener(OnVirtualInstructorsChanged);
            instructorOrchestrator.OnVirtualInstructorsRemoved.AddListener(OnVirtualInstructorsChanged);

            _btnSendRecord.onClick.AddListener(SendRecording);
            _btnRecord.onClick.AddListener(StartRecording);

            SetVisibility(instructorOrchestrator.IsVirtualInstructorInList()); 
        }
        
        
        /// <summary>
        /// Cleans up event listeners and  cancellation token previously registered
        /// from the <c>VirtualInstructorOrchestrator</c>  If the <c>RootObject</c>
        /// is null (e.g., during application shutdown), cleanup is skipped.
        /// </summary>
        private void OnDestroy()
        {
             if (RootObject.Instance is null)
             {
                 return;
             }
 
             var instructorOrchestrator = RootObject.Instance.VirtualInstructorOrchestrator;
             instructorOrchestrator.OnVirtualInstructorsAdded.RemoveListener(OnVirtualInstructorsChanged);
             instructorOrchestrator.OnVirtualInstructorsRemoved.RemoveListener(OnVirtualInstructorsChanged);
             ClearCancellationTokenSource();
         }
             
        /// <summary>
        /// Shows either the VI UI or removes the UI. 
        /// </summary>
        /// <param name="visible"> true == the UI is there </param>
        private void SetVisibility(bool  visible)
        {
            _Record.gameObject.SetActive(visible);
            _SendRecord.gameObject.SetActive(visible);
        }
        /// <summary>
        /// Checks and displays the Recording UI if a Virtual Instructor is included in the scene.
        /// </summary>
        /// <param name="instructors">List of Virtual Instructor objects</param>
        private void OnVirtualInstructorsChanged(List<IVirtualInstructor> instructors)
        {
            if (instructors == null || instructors.Count == 0)
            {
                SetVisibility(false);
                return;
            }
            SetVisibility(true);
        }

        /// <summary>
        /// Starts the recording process.
        /// </summary>
        public void StartRecording()
        {
            StartRecordingAsync().Forget();
        }

        private async UniTask StartRecordingAsync()
        {
            Debug.Log("StartRecording");
            var recordingDevices = RootObject.Instance.LEE.AudioManager.GetRecordingDevices();
            if (recordingDevices.Length > 0)
            {
                ClearCancellationTokenSource();
                _source = new CancellationTokenSource();
                _cancellationToken = _source.Token;
                RootObject.Instance.LEE.AudioManager.Start();
                _Record.gameObject.SetActive(false);
                await UniTask.WaitForSeconds(_maxRecordTime, cancellationToken: _cancellationToken);
                if (RootObject.Instance.LEE.AudioManager.IsRecording)
                {
                    _Record.gameObject.SetActive(true);
                    _questionClip = RootObject.Instance.LEE.AudioManager.Stop();
                    ClearCancellationTokenSource();
                    await SendRecordingAsync(_questionClip);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("No microphone detected!");
            }
        }

        /// <summary>
        /// Sends the recording to the virtual instructor and plays back the response.
        /// </summary>
        public void SendRecording()
        {
            _Record.gameObject.SetActive(true);
            _questionClip = RootObject.Instance.LEE.AudioManager.Stop();
            ClearCancellationTokenSource();
            _source?.Cancel();
            SendRecordingAsync(_questionClip).Forget();
        }

        private async UniTask SendRecordingAsync(AudioClip audioClip)
        {
            _Loading.SetActive(true);
            try
            {
                var clip = await RootObject.Instance.VirtualInstructorOrchestrator.AskInstructorWithAudioQuestion(audioClip);
                if (clip)
                {
                    responseClip.PlayOneShot(clip);
                    await UniTask.WaitForSeconds(clip.length);
                }
                else
                {
                    AppLog.LogError("Could not send recording to the Virtual Instructor Orchestrator");
                }
            }
            catch (Exception e)
            {
                AppLog.LogException(e);
            }
            _Loading.SetActive(false);
            //_SendRecord.SetActive(false);
        }

        private void ClearCancellationTokenSource()
        {
            if (_source != null)
            {
                _source.Cancel();
                _source.Dispose();
                _source = null;
            }
        }
    }
}
