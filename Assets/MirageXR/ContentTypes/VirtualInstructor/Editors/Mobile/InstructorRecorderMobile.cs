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
        private async UniTask SendRecordingAsync(AudioClip clip)
        {
            _Loading.SetActive(true);
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
                    Debug.LogWarning("[InstructorRecorderMobile] No instructor response received.");
                }
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
