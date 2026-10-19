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
        private Transform _originalParent;
        private static GameObject _tabletOverlayCanvas;
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
                _originalParent = _rectTransform.parent;
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
            else
            {
                UpdateButtonLayout(false);
            }
        }

        private void Start()
        {
            if (RootView_v2.Instance?.activityView != null)
            {
                UpdateButtonLayout(RootView_v2.Instance.activityView.IsMenuMinimized);
            }
            else
            {
                UpdateButtonLayout(false);
            }
        }

        private void Update()
        {
            // Sync visibility with original parent when reparented to tablet overlay
            if (_originalParent != null && _tabletOverlayCanvas != null && _rectTransform != null && _rectTransform.parent == _tabletOverlayCanvas.transform)
            {
                if (gameObject.activeSelf != _originalParent.gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(_originalParent.gameObject.activeInHierarchy);
                }
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

            if (_tabletOverlayCanvas != null && _tabletOverlayCanvas.transform.childCount <= 1)
            {
                Destroy(_tabletOverlayCanvas);
                _tabletOverlayCanvas = null;
            }

            base.OnDestroy();
        }

        private void HandleMenuStateChanged(bool isMinimized)
        {
            UpdateButtonLayout(isMinimized);
        }

        private void EnsureTabletOverlayCanvas()
        {
            if (_tabletOverlayCanvas == null)
            {
                _tabletOverlayCanvas = new GameObject("VI_TabletOverlayCanvas");
                DontDestroyOnLoad(_tabletOverlayCanvas);
                var canvas = _tabletOverlayCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
                var scaler = _tabletOverlayCanvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                _tabletOverlayCanvas.AddComponent<GraphicRaycaster>();
            }
        }

        public void UpdateButtonLayout(bool isMinimized)
        {
            if (_rectTransform == null) return;

            bool isTablet = PlatformManager.GetDeviceFormat() == DeviceFormat.Tablet;
            if (isTablet)
            {
                EnsureTabletOverlayCanvas();
                if (_tabletOverlayCanvas != null && _rectTransform.parent != _tabletOverlayCanvas.transform)
                {
                    _rectTransform.SetParent(_tabletOverlayCanvas.transform, false);
                }

                // Tablet: bottom-left of the 2/3 (70%) AR camera view, scaled ~2x
                _rectTransform.anchorMin = new Vector2(0.08f, 0.15f);
                _rectTransform.anchorMax = new Vector2(0.08f, 0.15f);
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _rectTransform.anchoredPosition = Vector2.zero;
                _rectTransform.localScale = _defaultScale * 2f;
                return;
            }

            // Phone: restore to original parent in ActivityView
            if (_originalParent != null && _rectTransform.parent != _originalParent)
            {
                _rectTransform.SetParent(_originalParent, false);
            }

            if (!isMinimized)
            {
                _rectTransform.anchorMin = _defaultAnchorMin;
                _rectTransform.anchorMax = _defaultAnchorMax;
                _rectTransform.pivot = _defaultPivot;
                _rectTransform.anchoredPosition = _defaultAnchoredPosition;
                _rectTransform.localScale = _defaultScale;
                return;
            }

            // Phone: centre of the camera see-through, scaled ~2x
            _rectTransform.anchorMin = new Vector2(0.5f, 0.55f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.55f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = _defaultScale * 2f;
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
