using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MirageXR.View;
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

        private RectTransform _rectTransform;
        private Vector2 _defaultAnchorMin;
        private Vector2 _defaultAnchorMax;
        private Vector2 _defaultAnchoredPosition;
        private Vector2 _defaultSizeDelta;
        private Vector2 _defaultPivot;
        private Vector3 _defaultScale = Vector3.one;

        protected override void Awake()
        {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _defaultAnchorMin = _rectTransform.anchorMin;
                _defaultAnchorMax = _rectTransform.anchorMax;
                _defaultAnchoredPosition = _rectTransform.anchoredPosition;
                _defaultSizeDelta = _rectTransform.sizeDelta;
                _defaultPivot = _rectTransform.pivot;
                _defaultScale = _rectTransform.localScale;
            }

            ActivityView_v2.OnMenuStateChanged += HandleMenuStateChanged;

            OnRecordingCancelled += ResetUI;
            RootObject.Instance.VirtualInstructorOrchestrator.OnVirtualInstructorsAdded.AddListener(OnInstructorsUpdated);

            if (_btnCancelRecording != null)
                _btnCancelRecording.onClick.AddListener(CancelRecording);

            if (RootView_v2.Instance?.activityView != null)
            {
                UpdateButtonLayout(RootView_v2.Instance.activityView.IsMenuMinimized);
            }
        }

        protected override void OnDestroy()
        {
            ActivityView_v2.OnMenuStateChanged -= HandleMenuStateChanged;
            OnRecordingCancelled -= ResetUI;
            if (RootObject.Instance != null && 
                RootObject.Instance.VirtualInstructorOrchestrator != null &&
                RootObject.Instance.VirtualInstructorOrchestrator.OnVirtualInstructorsAdded != null)
            {
                RootObject.Instance.VirtualInstructorOrchestrator.OnVirtualInstructorsAdded.RemoveListener(OnInstructorsUpdated);
            }

            base.OnDestroy();
        }

        private void HandleMenuStateChanged(bool isMinimized)
        {
            UpdateButtonLayout(isMinimized);
        }

        public void UpdateButtonLayout(bool isMinimized)
        {
            if (_rectTransform == null) return;

            if (!isMinimized)
            {
                _rectTransform.anchorMin = _defaultAnchorMin;
                _rectTransform.anchorMax = _defaultAnchorMax;
                _rectTransform.pivot = _defaultPivot;
                _rectTransform.anchoredPosition = _defaultAnchoredPosition;
                _rectTransform.localScale = _defaultScale;
                return;
            }

            bool isTablet = PlatformManager.GetDeviceFormat() == DeviceFormat.Tablet;
            if (isTablet)
            {
                // Tablet: bottom-left of the 70% camera see-through area, scaled ~4x (2x width & height)
                _rectTransform.anchorMin = new Vector2(0.08f, 0.15f);
                _rectTransform.anchorMax = new Vector2(0.08f, 0.15f);
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _rectTransform.anchoredPosition = Vector2.zero;
                _rectTransform.localScale = _defaultScale * 2f;
            }
            else
            {
                // Phone: centre of the camera see-through, scaled ~4x (2x width & height)
                _rectTransform.anchorMin = new Vector2(0.5f, 0.55f);
                _rectTransform.anchorMax = new Vector2(0.5f, 0.55f);
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _rectTransform.anchoredPosition = Vector2.zero;
                _rectTransform.localScale = _defaultScale * 2f;
            }
        }


        /// <summary>
        /// Cancels the current recording session:
        /// - Stops MicrophoneManager
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

        private void PlayAudio(AudioClip clip)
        {
            responseClip.PlayOneShot(clip); 
        }
        
        private void OnInstructorsUpdated(List<IVirtualInstructor> instructors)
        {
            foreach (var vi in instructors)
            {
                if (vi is Instructor instructor)
                {

                    instructor.OnInstructorResponseAvailable -= PlayAudio;
                    instructor.OnInstructorResponseAvailable += PlayAudio;

                    Debug.Log($"[InstructorRecorderMobile] Event verbunden mit Instructor: {instructor.name}");
                }
            }
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
