using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// System-wide caption controller responsible for rendering captions across MirageXR.
    /// Supports a hybrid display mode:
    /// - Screen space HUD overlay on Mobile (anchored to bottom-center of the viewport)
    /// - Smooth head-following (tag-along) World Space canvas on Spatial / XR headsets (VisionOS, HoloLens, OpenXR)
    /// 
    /// Any component in the system can trigger captions via:
    /// - CaptionController.Instance.ShowCaption(text, duration, speaker)
    /// - EventManager.ShowCaption(text, duration, speaker)
    /// </summary>
    public class CaptionController : MonoBehaviour
    {
        private const string PrefabResourcePath = "Prefabs/CaptionController";

        public static CaptionController Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panelRectTransform;
        [SerializeField] private TextMeshProUGUI _captionText;
        [SerializeField] private Image _backgroundImage;

        [Header("Spatial Head-Following Settings")]
        [SerializeField] private float _spatialDistance = 1.8f;
        [SerializeField] private float _spatialVerticalOffset = 0.25f;
        [SerializeField] private float _followSpeed = 4.0f;

        [Header("Display Settings")]
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private bool _autoAttachToCamera = true;

        private Transform _cameraTransform;
        private bool _isSpatial;
        private Coroutine _displayCoroutine;
        private Coroutine _fadeCoroutine;

        /// <summary>
        /// Ensures a CaptionController instance exists in the scene, instantiating from Resources if necessary.
        /// </summary>
        public static CaptionController EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var existing = FindObjectOfType<CaptionController>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var prefab = Resources.Load<GameObject>(PrefabResourcePath);
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                go.name = "CaptionController";
                Instance = go.GetComponent<CaptionController>();
                return Instance;
            }

            // Fallback: Create dynamically
            var root = new GameObject("CaptionController");
            Instance = root.AddComponent<CaptionController>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            EnsureUIHierarchy();

            EventManager.OnShowCaption += ShowCaption;
            EventManager.OnHideCaption += HideCaption;
            LearningExperienceEngine.UserSettings.CaptionsSettingChanged += OnCaptionsSettingChanged;
        }

        private void Start()
        {
            InitializePlatformMode();
            HideImmediately();
        }

        private void OnDestroy()
        {
            EventManager.OnShowCaption -= ShowCaption;
            EventManager.OnHideCaption -= HideCaption;
            LearningExperienceEngine.UserSettings.CaptionsSettingChanged -= OnCaptionsSettingChanged;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializePlatformMode()
        {
            _isSpatial = false;

            if (RootObject.Instance?.PlatformManager != null)
            {
                _isSpatial = RootObject.Instance.PlatformManager.WorldSpaceUi || 
                             RootObject.Instance.PlatformManager.GetUiType() == UiType.Spatial;
            }

#if VISION_OS
            _isSpatial = true;
#endif

            UpdateActiveCamera();

            if (_canvas == null)
            {
                return;
            }

            if (_isSpatial)
            {
                _canvas.renderMode = RenderMode.WorldSpace;
                _canvas.transform.localScale = Vector3.one * 0.001f;

                // On spatial, unparent from camera to allow smooth damping follow without micro-jitter
                transform.SetParent(null);

                if (_cameraTransform != null)
                {
                    transform.position = _cameraTransform.position + _cameraTransform.forward * _spatialDistance - _cameraTransform.up * _spatialVerticalOffset;
                    transform.rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);
                }
            }
            else
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                if (_autoAttachToCamera && _cameraTransform != null)
                {
                    transform.SetParent(_cameraTransform);
                }
            }
        }

        private void LateUpdate()
        {
            if (!_isSpatial)
            {
                return;
            }

            UpdateActiveCamera();

            if (_cameraTransform == null)
            {
                return;
            }

            Vector3 targetPosition = _cameraTransform.position + 
                                     _cameraTransform.forward * _spatialDistance - 
                                     _cameraTransform.up * _spatialVerticalOffset;

            Vector3 toCamera = transform.position - _cameraTransform.position;
            Quaternion targetRotation = toCamera.sqrMagnitude > 0.001f 
                ? Quaternion.LookRotation(toCamera.normalized, Vector3.up) 
                : transform.rotation;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _followSpeed);
        }

        private void UpdateActiveCamera()
        {
            if (_cameraTransform == null || !_cameraTransform.gameObject.activeInHierarchy)
            {
                _cameraTransform = RootObject.Instance?.BaseCamera?.transform ?? Camera.main?.transform;
            }
        }

        /// <summary>
        /// Displays a caption with optional speaker attribution for a specified duration in seconds.
        /// If captions are disabled in UserSettings, this call is ignored.
        /// </summary>
        /// <param name="text">The text content to display.</param>
        /// <param name="duration">Duration in seconds to keep the caption visible.</param>
        /// <param name="speaker">Optional speaker name to prefix.</param>
        public void ShowCaption(string text, float duration = 5f, string speaker = null)
        {
            if (!LearningExperienceEngine.UserSettings.showCaptions)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            EnsureUIHierarchy();

            string formattedText = !string.IsNullOrEmpty(speaker) 
                ? $"<b>{speaker}:</b> {text.Trim()}" 
                : text.Trim();

            _captionText.text = formattedText;

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }

            _displayCoroutine = StartCoroutine(DisplayCaptionRoutine(Mathf.Max(1.0f, duration)));
        }

        /// <summary>
        /// Hides the active caption with a smooth fade.
        /// </summary>
        public void HideCaption()
        {
            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
                _displayCoroutine = null;
            }

            FadeTo(0f);
        }

        /// <summary>
        /// Clears text and immediately hides caption.
        /// </summary>
        public void Clear()
        {
            HideImmediately();
            if (_captionText != null)
            {
                _captionText.text = string.Empty;
            }
        }

        private void OnCaptionsSettingChanged(bool enabled)
        {
            if (!enabled)
            {
                HideImmediately();
            }
        }

        private IEnumerator DisplayCaptionRoutine(float duration)
        {
            FadeTo(1f);
            yield return new WaitForSeconds(duration);
            FadeTo(0f);
            _displayCoroutine = null;
        }

        private void FadeTo(float targetAlpha)
        {
            if (_canvasGroup == null) return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, _fadeDuration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            if (duration <= 0.001f)
            {
                _canvasGroup.alpha = targetAlpha;
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _fadeCoroutine = null;
        }

        private void HideImmediately()
        {
            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
                _displayCoroutine = null;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        private void EnsureUIHierarchy()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
                if (_canvas == null)
                {
                    _canvas = gameObject.AddComponent<Canvas>();
                }
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_panelRectTransform == null)
            {
                var panelObj = transform.Find("CaptionPanel")?.gameObject;
                if (panelObj == null)
                {
                    panelObj = new GameObject("CaptionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    panelObj.transform.SetParent(transform, false);
                }

                _panelRectTransform = panelObj.GetComponent<RectTransform>();
                _backgroundImage = panelObj.GetComponent<Image>();
                if (_backgroundImage != null)
                {
                    _backgroundImage.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);
                }
            }

            // Setup panel constraints & anchors for bottom-center reading
            _panelRectTransform.anchorMin = new Vector2(0.5f, 0f);
            _panelRectTransform.anchorMax = new Vector2(0.5f, 0f);
            _panelRectTransform.pivot = new Vector2(0.5f, 0f);
            _panelRectTransform.anchoredPosition = new Vector2(0f, 80f);
            _panelRectTransform.sizeDelta = new Vector2(800f, 140f);

            if (_captionText == null)
            {
                var textObj = _panelRectTransform.Find("CaptionText")?.gameObject;
                if (textObj == null)
                {
                    textObj = new GameObject("CaptionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                    textObj.transform.SetParent(_panelRectTransform, false);
                }

                _captionText = textObj.GetComponent<TextMeshProUGUI>();
                _captionText.alignment = TextAlignmentOptions.Center;
                _captionText.color = Color.white;
                _captionText.fontSize = 24f;
                _captionText.enableWordWrapping = true;
                _captionText.overflowMode = TextOverflowModes.Ellipsis;

                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.offsetMin = new Vector2(24f, 16f);
                textRect.offsetMax = new Vector2(-24f, -16f);
            }
        }
    }
}
