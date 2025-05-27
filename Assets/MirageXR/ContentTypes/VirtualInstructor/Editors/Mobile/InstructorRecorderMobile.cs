using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Concrete recorder implementation for mobile (or learner) use case:
    /// - Uses existing VI instance in the scene.
    /// - Provides cancel logic and fallback response playback.
    /// - UI interaction is triggered via button hooks.
    /// </summary>
    public sealed class InstructorRecorderMobile : AbstractInstructorRecorder
    {
        [SerializeField] private Button _btnCancelRecording;

        protected override void Awake()
        {
            base.Awake();
            
            
            OnRecordingCancelled += ResetUI;
            if (_btnCancelRecording != null)
                _btnCancelRecording.onClick.AddListener(CancelRecording);
            
        }
        
        protected override void OnDestroy()
        {
            OnRecordingCancelled -= ResetUI;
            base.OnDestroy();
        }

        /// <summary>
        /// Cancels the current recording session:
        /// - Stops AudioManager
        /// - Resets UI
        /// </summary>
        private void CancelRecording()
        {
            CancelRecordingTask();
            UpdateStateUI();
        }
        
        private void ResetUI()
        {
            _Loading.SetActive(false);
            _SendRecord.SetActive(false);
            _Record.SetActive(true);
        }

        
        /// <summary>
        /// Sends the recorded question to the Virtual Instructor and plays the response.
        /// Falls back to a default clip if the instructor does not respond.
        /// </summary>
        /// <param name="clip">The user question audio clip.</param>
        protected override async UniTask SendRecordingAsync(AudioClip clip)
        {
            Debug.Log("SendRecordingAsync");
            _Loading.SetActive(true);
            
            if (responseClip == null)
            {
                Debug.LogError("[InstructorRecorderMobile] responseClip (AudioSource) is not assigned!");
            }
            else if (clip == null)
            {
                Debug.LogError("[InstructorRecorderMobile] AudioClip is null – nothing to play.");
            }
            else
            {
                Debug.Log($"[InstructorRecorderMobile] Playing AudioClip, length = {clip.length}s");
                responseClip.PlayOneShot(clip);
            }

            
            try
            {
                var response = await RootObject.Instance.VirtualInstructorOrchestrator.AskInstructorWithAudioQuestion(clip);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InstructorRecorderMobile] Error during response: {ex.Message}");
            }

            _Loading.SetActive(false);
            _SendRecord.SetActive(false);
            _Record.SetActive(true);
        }
    }
}
