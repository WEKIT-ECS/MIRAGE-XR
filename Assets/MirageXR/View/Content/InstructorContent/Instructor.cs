using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirageXR.View
{
    /// <summary>
    /// Represents a virtual instructor entity within the scene that can process user input,
    /// generate AI-driven responses, and return spoken output via audio clips.
    /// 
    /// Instructors are dynamically added and removed at runtime and are managed by the
    /// <see cref="VirtualInstructorOrchestrator"/>. This class serves as a bridge between user input,
    /// AI interaction, and audio output.
    /// </summary>

    public class Instructor : MonoBehaviour, IVirtualInstructor
    {
        private static IArtificialIntelligenceManager ArtificialIntelligenceManager => RootObject.Instance.LEE.ArtificialIntelligenceManager;

        public event Action<AudioClip> OnInstructorResponseAvailable;
        public event Action<string, float> OnInstructorSpokenText;
        
        private const float CharacterHeight = 1.8f;

        private Animator _animator;
        private Content<InstructorContentData> _instructorContent;
        private string _assistantId;
        private string _threadId;
        private string _history;
        private string _lastSpokenText;

        [SerializeField] private GameObject _loadingIndicatorPrefab;
        private GameObject _currentLoadingIndicator;

        [SerializeField] private GameObject _controlPanelPrefab;
        private GameObject _controlPanelInstance;

        public Vector3 Position => transform.position;
        
        public async UniTask InitializeAsync(Content<InstructorContentData> content)
        {
            _animator = GetComponentInChildren<Animator>();
            _instructorContent = content;

            var boxCollider = GetComponentInChildren<BoxCollider>();
            if (boxCollider != null)
            {
                var size = boxCollider.size;
                boxCollider.size = new Vector3(size.x, CharacterHeight, size.z);
                var center = boxCollider.center;
                boxCollider.center = new Vector3(center.x, CharacterHeight * 0.5f, center.z);
            }

            gameObject.name = content.ContentData.CharacterName;
            PlayAnimationClip(content.ContentData.AnimationClip);

            RootObject.Instance.VirtualInstructorOrchestrator.AddInstructor(this);
            var response = await ArtificialIntelligenceManager.InitializeInstructorAsync(
                _instructorContent.ContentData.CharacterName, 
                _instructorContent.ContentData.Prompt, 
                _instructorContent.ContentData.LanguageModel.ApiName);

            if (response == null)
            {
                Debug.LogError("Failed to initialize instructor");
                return;
            }
            _threadId = response.ThreadId;
            _assistantId = response.AssistantId;

            try 
            {
                var handle = Addressables.LoadAssetAsync<GameObject>("LoadingIndicator");
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadingIndicatorPrefab = handle.Result;
                    _currentLoadingIndicator = Instantiate(_loadingIndicatorPrefab, transform);
                    _currentLoadingIndicator.transform.localPosition = Vector3.zero + Vector3.up * 1.9f;
                    _currentLoadingIndicator.transform.localRotation = Quaternion.identity;
                    _currentLoadingIndicator.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Failed to load LoadingIndicator prefab via Addressables. Check if 'LoadingIndicator' address is correct.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading LoadingIndicator: {e.Message}");
            }

            try
            {
                SetupSpatialControlPanel();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting up spatial VI Control Panel: {e.Message}");
            }
        }

        private void PlayAnimationClip(string clipName)     //temp
        {
            if (_animator != null)
            {
                foreach (var param in _animator.parameters)
                {
                    _animator.SetBool(param.name, param.name == clipName);
                }
            }
        }

        /// <summary>
        /// Triggers playback of the instructor's response audio.
        /// If an <see cref="AvatarAudioController"/> component is attached to the GameObject,
        /// the audio clip is played directly.  Otherwise, the <see cref="OnInstructorResponseAvailable"/>
        /// event is invoked, allowing external systems (e.g. UI components) to handle playback.
        /// </summary>
        /// <param name="clip">The <see cref="AudioClip"/> containing the instructor's spoken response.</param>
        public void PlayAudio(AudioClip clip)
        {
            var audioController = GetComponent<AvatarAudioController>();
            if (audioController != null)
            {
                audioController.PlayAudio(clip);
            }
            else
            {
                OnInstructorResponseAvailable?.Invoke(clip);
            }

            if (!string.IsNullOrEmpty(_lastSpokenText))
            {
                float duration = clip != null ? clip.length : 5f;
                string speaker = _instructorContent?.ContentData?.Name ?? "Virtual Instructor";
                CaptionController.Instance?.ShowCaption(_lastSpokenText, duration, speaker);
                EventManager.ShowCaption(_lastSpokenText, duration, speaker);
                OnInstructorSpokenText?.Invoke(_lastSpokenText, duration);
            }
        }

        private void SetWaitingState(bool isWaiting)
        {
            if (_currentLoadingIndicator != null)
            {
                _currentLoadingIndicator.SetActive(isWaiting);
            }
        }

		/// <summary>
		/// Retrieves the AI model for text-to-speech functionality.
		/// </summary>
		/// <returns>The AI model for text-to-speech functionality.</returns>
		public AIModel GetTextToSpeechModel()
        {
            return _instructorContent.ContentData.TextToSpeechModel;
        }

        /// <summary>
        /// Asks the virtual instructor a question.
        /// </summary>
        /// <param name="inputAudio">The input audio clip representing the question of the user.</param>
        /// <returns>A clip containing the response from the virtual instructor.</returns>
        public async UniTask<AudioClip> AskVirtualInstructorAudio(AudioClip inputAudio)
        {
            if (inputAudio == null)
            {
                AppLog.LogWarning("[Instructor] AskVirtualInstructorAudio received null audio clip.");
                return null;
            }

            SetWaitingState(true);
            try
            {
                var message = await ArtificialIntelligenceManager.ConvertSpeechToTextAsync(inputAudio, _instructorContent.ContentData.SpeechToTextModel.ApiName);
                if (string.IsNullOrWhiteSpace(message))
                {
                    AppLog.LogWarning("[Instructor] Speech to text returned empty message.");
                    return null;
                }
                AppLog.LogDebug($"AI question: '{message}'");
                var response = await ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, message, _instructorContent.ContentData.Prompt, _assistantId, _threadId);
                if (string.IsNullOrEmpty(response))
                {
                    AppLog.LogWarning("[Instructor] Received null or empty response from assistant.");
                    return null;
                }
                AppLog.LogDebug($"AI response: '{response}'");
                _lastSpokenText = response;
                var clip = await ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName, GetVoiceInstruction());
                return clip;
            }
            finally
            {
                SetWaitingState(false);
            }
        }

        /// <summary>
        /// Sends a message to the virtual instructor and returns the corresponding speech audio clip.
        /// </summary>
        /// <param name="message">Message to be sent along with the message queue.</param>
        /// <param name="messageQueue">the message queue is a set of event that should be included in the answer</param>
        /// <returns>The speech audio clip generated by the virtual instructor in response to the message.</returns>
        public async UniTask<AudioClip> AskInstructorWithStringQuestion(string message)
        {
            SetWaitingState(true);
            try
            {
                var response = await ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, message, _instructorContent.ContentData.Prompt, _assistantId, _threadId);
                if (string.IsNullOrEmpty(response))
                {
                    AppLog.LogWarning("[Instructor] Received null or empty response from assistant.");
                    return null;
                }
                _lastSpokenText = response;
                var clip = await ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName, GetVoiceInstruction());

                return clip;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("An error occurred: " + ex.Message);
                return null; 
            }
            finally
            {
                SetWaitingState(false);
            }
        }

        /// <summary>
        /// Converts a text message to speech using the specified model.
        /// </summary>
        /// <param name="message">The text message to be converted to speech.</param>
        /// <returns>An async task that represents the asynchronous operation. The task result contains the audio clip representing the converted speech.</returns>
        public async UniTask<AudioClip> ConvertTextToSpeech(string message)
        {
            _lastSpokenText = message;
            var clip = await ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, _instructorContent.ContentData.TextToSpeechModel.ApiName, GetVoiceInstruction());
            return clip;
        }

        private string GetVoiceInstruction()
        {
            return !string.IsNullOrEmpty(_instructorContent.ContentData.VoiceInstruction) ? _instructorContent.ContentData.VoiceInstruction : "neutral";
        }

        /// <summary>
        /// CreateContext method is responsible for concatenation the history  and the prompt to a String.
        /// </summary>
        private string CreateContext(string messageQueue = "")
        {
            return !string.IsNullOrEmpty(_history)
                ? _instructorContent.ContentData.Prompt + _history + messageQueue
                : _instructorContent.ContentData.Prompt;
        }

        private void SetupSpatialControlPanel()
        {
            var platformManager = RootObject.Instance?.PlatformManager;
            bool isSpatial = platformManager != null && 
                (platformManager.WorldSpaceUi || platformManager.GetUiType() == UiType.Spatial);

            if (!isSpatial)
            {
                return;
            }

            if (_controlPanelPrefab == null)
            {
                _controlPanelPrefab = Resources.Load<GameObject>("VI_Control_Panel");
#if UNITY_EDITOR
                if (_controlPanelPrefab == null)
                {
                    _controlPanelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/MirageXR/ContentTypes/VirtualInstructor/Resources/VI_Control_Panel.prefab");
                }
#endif
            }

            if (_controlPanelPrefab == null)
            {
                Debug.LogWarning("[Instructor] VI_Control_Panel prefab could not be loaded.");
                return;
            }

            var avatarRoot = _animator != null ? _animator.transform : transform;
            _controlPanelInstance = Instantiate(_controlPanelPrefab, avatarRoot);
            _controlPanelInstance.name = "VI_Control_Panel";
            _controlPanelInstance.transform.localScale = Vector3.one * 0.001f;

            UpdateControlPanelTransform();
        }

        private void LateUpdate()
        {
            UpdateControlPanelTransform();
        }

        private void UpdateControlPanelTransform()
        {
            if (_controlPanelInstance == null) return;

            var avatarRoot = _animator != null ? _animator.transform : transform;
            Transform chestBone = null;
            if (_animator != null && _animator.isHuman)
            {
                chestBone = _animator.GetBoneTransform(HumanBodyBones.Chest) 
                         ?? _animator.GetBoneTransform(HumanBodyBones.UpperChest)
                         ?? _animator.GetBoneTransform(HumanBodyBones.Spine);
            }

            Vector3 chestWorldPos = chestBone != null 
                ? chestBone.position 
                : avatarRoot.position + avatarRoot.up * 1.3f;

            Vector3 avatarForward = avatarRoot.forward;
            Vector3 targetPos = chestWorldPos + avatarForward * 0.22f;

            _controlPanelInstance.transform.position = targetPos;

            if (Camera.main != null)
            {
                Vector3 dirToCam = Camera.main.transform.position - targetPos;
                dirToCam.y = 0;
                if (dirToCam.sqrMagnitude > 0.001f)
                {
                    _controlPanelInstance.transform.rotation = Quaternion.LookRotation(-dirToCam.normalized, Vector3.up);
                }
                else
                {
                    _controlPanelInstance.transform.rotation = Quaternion.LookRotation(-avatarForward, Vector3.up);
                }
            }
            else
            {
                _controlPanelInstance.transform.rotation = Quaternion.LookRotation(-avatarForward, Vector3.up);
            }
        }

        private void OnDestroy()
        {
            if (_controlPanelInstance != null)
            {
                Destroy(_controlPanelInstance);
            }
            RootObject.Instance?.VirtualInstructorOrchestrator?.RemoveInstructor(this);
        }

        /// <summary>
        /// Asynchronously sends a question to the virtual instructor and retrieves the response as an
        /// <see cref="AudioClip"/>. The question is processed through an AI model, and converted to speech.
        /// </summary>
        /// <param name="message">The question to be asked, provided as a string.</param>
        /// <returns>
        /// A <see cref="Task{AudioClip}"/> that represents an <see cref="AudioClip"/> containing the spoken response
        /// from the virtual instructor.
        /// </returns>
        public async UniTask<AudioClip> AskVirtualInstructorString(string message)
        {
            SetWaitingState(true);
            try
            {
                var response = await ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, message, _instructorContent.ContentData.Prompt, _assistantId, _threadId);
                if (string.IsNullOrEmpty(response))
                {
                    AppLog.LogWarning("[Instructor] Received null or empty response from assistant.");
                    return null;
                }
                _lastSpokenText = response;
                var clip = await ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName, GetVoiceInstruction());

                return clip;
            }
            finally
            {
                SetWaitingState(false);
            }
        }
    }
}