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
    public class VirtualInstructorRecoder : MonoBehaviour //TODO: change name to VirtualInstructorRecorder
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
        private AudioClip _questionClip;

        private CancellationTokenSource _source;
        private CancellationToken _cancellationToken;

        private void Hide()
        {
            _Record.gameObject.SetActive(false);
            _SendRecord.gameObject.SetActive(false);
        }

        private void Show()
        {
            _Record.gameObject.SetActive(true);
            _SendRecord.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets up the virtual instructor  and the recorder buttons.
        /// </summary>
        public void Awake()
        {
            var instructorOrchestrator = RootObject.Instance.VirtualInstructorOrchestrator;
            instructorOrchestrator.OnVirtualInstructorsAdded.AddListener(OnVirtualInstructorsAdded);
            instructorOrchestrator.OnVirtualInstructorsRemoved.AddListener(OnVirtualInstructorsRemoved);

            _btnSendRecord.onClick.AddListener(SendRecording);
            _btnRecord.onClick.AddListener(StartRecording); 
            
            if (instructorOrchestrator.IsVirtualInstructorInList())
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnDestroy()
        {
            if (RootObject.Instance is null)
            {
                return;
            }

            var instructorOrchestrator = RootObject.Instance.VirtualInstructorOrchestrator;
            instructorOrchestrator.OnVirtualInstructorsAdded.RemoveListener(OnVirtualInstructorsAdded);
            instructorOrchestrator.OnVirtualInstructorsRemoved.RemoveListener(OnVirtualInstructorsRemoved);
            ClearCancellationTokenSource();
        }

        private void OnVirtualInstructorsAdded(List<IVirtualInstructor> instructors)
        {
            if (instructors is { Count: > 0 })
            {
                Show();
            }
        }

        private void OnVirtualInstructorsRemoved(List<IVirtualInstructor> instructors)
        {
            if (instructors == null || instructors.Count == 0)
            {
                Hide();
            }
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
                    //responseClip.PlayOneShot(clip);
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
