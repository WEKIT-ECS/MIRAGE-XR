using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Base class for instructor voice recording, playback, and UI control.
    /// Handles platform-independent audio recording using the MirageXR AudioManager.
    /// Supports automatic state-driven UI updates (Idle, Recording, Paused).
    /// Extend this class for platform-specific implementations (e.g. Mobile, Spatial).
    /// </summary>
    public abstract class AbstractInstructorRecorder : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] protected Button _btnRecord;
        [SerializeField] protected Button _btnSendRecord;
        [SerializeField] protected GameObject _Record;
        [SerializeField] protected GameObject _SendRecord;
        [SerializeField] protected GameObject _Loading;
        [SerializeField] protected GameObject _ButtonContaner;
        [SerializeField] protected AudioSource responseClip;

        [Header("Optional State Indicators")]
        [SerializeField] private GameObject _stateIdle;
        [SerializeField] private GameObject _stateRecording;
        [SerializeField] private GameObject _stateisBuzy;

        [Header("Recording Settings")]
        [SerializeField] protected int _maxRecordTime = 60;

        protected AudioClip _questionClip;
        private CancellationTokenSource _source;
        private CancellationToken _cancellationToken;
        
        public static event Action OnRecordingCancelled;

        /// <summary>
        /// Initializes event listeners, button bindings, and UI state.
        /// </summary>
        protected virtual void Awake()
        {
            var orchestrator = RootObject.Instance.VirtualInstructorOrchestrator;

            orchestrator.OnVirtualInstructorsAdded.AddListener(OnInstructorsChanged);
            orchestrator.OnVirtualInstructorsRemoved.AddListener(OnInstructorsChanged);

            _btnSendRecord.onClick.AddListener(SendRecording);
            _btnRecord.onClick.AddListener(StartRecording);

            _ButtonContaner.SetActive(orchestrator.IsVirtualInstructorInList());
            UpdateStateUI();

            Debug.Log($"[Recorder] Using audio device: {RootObject.Instance.LEE.AudioManager.AudioDevice}");
        }

        /// <summary>
        /// Cleans up event listeners and async token source.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (RootObject.Instance is null) return;

            var orchestrator = RootObject.Instance.VirtualInstructorOrchestrator;
            orchestrator.OnVirtualInstructorsAdded.RemoveListener(OnInstructorsChanged);
            orchestrator.OnVirtualInstructorsRemoved.RemoveListener(OnInstructorsChanged);

            ClearCancellationTokenSource();
        }

        private void OnInstructorsChanged(List<IVirtualInstructor> instructors)
        {
            _ButtonContaner.SetActive(instructors is { Count: > 0 });
        }

        /// <summary>
        /// Starts audio recording from the default device or Photon source.
        /// </summary>
        public void StartRecording()
        {
            StartRecordingAsync().Forget();
        }

        private async UniTask StartRecordingAsync()
        {
            var devices = RootObject.Instance.LEE.AudioManager.GetRecordingDevices();
            if (devices.Length == 0)
            {
                Debug.LogError("[Recorder] No microphone detected.");
                return;
            }

            ClearCancellationTokenSource();
            _source = new CancellationTokenSource();
            _cancellationToken = _source.Token;

            RootObject.Instance.LEE.AudioManager.Start();
            UpdateStateUI();

            _Record.SetActive(false);
            _SendRecord.SetActive(true);

            await UniTask.WaitForSeconds(_maxRecordTime, cancellationToken: _cancellationToken);

            if (RootObject.Instance.LEE.AudioManager.IsRecording)
            {
                _questionClip = RootObject.Instance.LEE.AudioManager.Stop();
                ClearCancellationTokenSource();
                UpdateStateUI();
                await SendRecordingAsync(_questionClip);
            }
        }

        /// <summary>
        /// Stops recording manually and sends the audio to the instructor.
        /// </summary>
        public void SendRecording()
        {
            _SendRecord.SetActive(false);
            _Loading.SetActive(true);

            _questionClip = RootObject.Instance.LEE.AudioManager.Stop();
            ClearCancellationTokenSource();
            _source?.Cancel();

            UpdateStateUI();
            SendRecordingAsync(_questionClip).Forget();
        }

        /// <summary>
        /// Sends the audio clip to the Virtual Instructor orchestrator and handles response playback.
        /// Can be overridden by subclasses to add fallback or custom behavior.
        /// </summary>
        /// <param name="clip">The recorded user question as an AudioClip.</param>
        protected virtual async UniTask SendRecordingAsync(AudioClip clip)
        {
            try
            {
                var response = await RootObject.Instance.VirtualInstructorOrchestrator.AskInstructorWithAudioQuestion(clip);
                if (response != null)
                {
                    responseClip.PlayOneShot(response);
                    await UniTask.WaitForSeconds(response.length);
                }
                else
                {
                    Debug.LogWarning("[Recorder] No instructor response received.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Recorder] Exception during response: {e.Message}");
            }

            _Loading.SetActive(false);
            _Record.SetActive(true);
            UpdateStateUI();
        }

        /// <summary>
        /// Updates optional UI indicators based on current AudioManager state.
        /// </summary>
        protected void UpdateStateUI()
        {
            var state = RootObject.Instance.LEE.AudioManager.State;

            bool isIdle = state == IAudioManager.RecordingState.Idle;
            bool isRecording = state == IAudioManager.RecordingState.Recording;

            _stateIdle?.SetActive(isIdle);
            _stateRecording?.SetActive(isRecording);

            _Record?.SetActive(isIdle);
            _SendRecord?.SetActive(isRecording);
        }
        
        protected virtual void CancelRecordingTask()
        {
            _source?.Cancel();
            _source?.Dispose();
            _source = null;
            OnRecordingCancelled?.Invoke();
            RootObject.Instance.LEE.AudioManager.Stop();
        }

        private void ClearCancellationTokenSource()
        {
            _source?.Cancel();
            _source?.Dispose();
            _source = null;
        }
    }
}
