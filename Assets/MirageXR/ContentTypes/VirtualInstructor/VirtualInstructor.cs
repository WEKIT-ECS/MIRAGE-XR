using System;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Runtime representation of a virtual instructor.
    /// Handles AI interaction (STT/LLM/TTS), audio playback, and registration in the system.
    /// </summary>
    public class VirtualInstructor : MirageXRPrefab, IVirtualInstructor
    {
        public event Action<AudioClip> OnInstructorResponseAvailable;
        
        private const float CharacterHeight = 1.8f;
        private static readonly string HistoryFormat = "This is the History of the conversation so far: Question: {0}, Answer: {1}";

        public Vector3 Position => transform.position;

        private InstructorContentData InstructorData { get; set; }
        private string _history;
        private Animator _animator;

        [SerializeField] private LearningExperienceEngine.ToggleObject _toggleObject;

        public override bool Init(LearningExperienceEngine.ToggleObject toggleObject)
        {
            _animator = GetComponentInChildren<Animator>();
            _toggleObject = toggleObject;

            try
            {
                InstructorData = JsonConvert.DeserializeObject<InstructorContentData>(toggleObject.option);
            }
            catch (Exception e)
            {
                Debug.LogError("[VI] Failed to parse InstructorData: " + e);
                return false;
            }

            if (!SetParent(toggleObject))
            {
                Debug.LogWarning("[VI] Failed to set parent transform.");
                return false;
            }

            SetupCollider();
            SetupTransform();
            SetupNameAndAnimation();

            RootObject.Instance.VirtualInstructorOrchestrator.AddInstructor(this);
            return base.Init(toggleObject);
        }

        private void SetupCollider()
        {
            var boxCollider = GetComponentInChildren<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(boxCollider.size.x, CharacterHeight, boxCollider.size.z);
                boxCollider.center = new Vector3(boxCollider.center.x, CharacterHeight * 0.5f, boxCollider.center.z);
            }

            if (transform.parent.TryGetComponent(out PoiEditor poiEditor))
            {
                poiEditor.canRotate = true;
            }
        }

        private void SetupTransform()
        {
            if (_toggleObject.scale > 0)
            {
                transform.localScale = Vector3.one * _toggleObject.scale;
            }
        }

        private void SetupNameAndAnimation()
        {
            gameObject.name = InstructorData.CharacterName;
            PlayAnimationClip(InstructorData.AnimationClip);
        }

        private void PlayAnimationClip(string clipName)
        {
            if (_animator == null || string.IsNullOrEmpty(clipName)) return;

            foreach (var param in _animator.parameters)
            {
                _animator.SetBool(param.name, param.name == clipName);
            }
        }

        public void PlayAudio(AudioClip clip)
        {
            var audioController = GetComponent<AvatarAudioController>();
            if (audioController != null && clip != null)
            {
                 audioController.PlayAudio(clip);
            }
            else if (clip != null)
            {
                OnInstructorResponseAvailable?.Invoke(clip);     
            }
        }

        public async Task<AudioClip> AskVirtualInstructorAudio(AudioClip inputAudio, string messageQueue = "")
        {
            string context = CreateContext(messageQueue);
            var question = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertSpeechToTextAsync(
                inputAudio, InstructorData.SpeechToTextModel.ApiName);
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(
                InstructorData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(
                response, InstructorData.TextToSpeechModel.ApiName);

            UpdateHistory(question, response);
            return clip;
        }

        public async Task<AudioClip> AskVirtualInstructorString(string question, string messageQueue = "")
        {
            string context = CreateContext(messageQueue);
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(
                InstructorData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(
                response, InstructorData.TextToSpeechModel.ApiName);

            UpdateHistory(question, response);
            return clip;
        }

        public async Task<AudioClip> ConvertTextToSpeech(string message)
        {
            return await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(
                message, InstructorData.TextToSpeechModel.ApiName);
        }

        private string CreateContext(string messageQueue = "")
        {
            return !string.IsNullOrEmpty(_history)
                ? $"{InstructorData.Prompt}{_history}{messageQueue}"
                : InstructorData.Prompt;
        }

        private void UpdateHistory(string question, string response)
        {
            _history = string.Format(HistoryFormat, question, response);
        }

        private void OnDestroy()
        {
            RootObject.Instance.VirtualInstructorOrchestrator.RemoveInstructor(this);
        }
    }
}
