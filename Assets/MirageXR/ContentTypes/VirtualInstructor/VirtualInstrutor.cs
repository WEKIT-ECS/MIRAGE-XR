using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Represents a virtual instructor that provides language-based assistance.
    /// </summary>
    public class VirtualInstructor : MirageXRPrefab
    {
        private const float CharacterHeight = 1.8f;
        /// <summary>
        /// Represents the format for displaying the history of a conversation.
        /// </summary>
        private static readonly string HistoryFormat = "This is the History of the conversation so fare: Question :{0} Given answer: {1}";

        /// Represents the data model for a virtual instructor in the MirageXR application.
        /// </summary>
        /// <remarks>
        /// The InstructorData class stores information about the language models and prompts used by the virtual instructor.
        /// </remarks>
        private VirtualInstructorDataModel InstructorData { get; set; }

        [SerializeField] private LearningExperienceEngine.ToggleObject _toggleObject;
        private Animator _animator;

        /// <summary>
        /// Represents the history of a conversation with the VirtualInstructor.
        /// This variable keeps track of the conversation history between the user and the VirtualInstructor.
        /// It is a string that stores the questions and answers exchanged during the conversation.
        /// </summary>
        private string _history;

        public override bool Init(LearningExperienceEngine.ToggleObject toggleObject)
        {
            _animator = GetComponentInChildren<Animator>();
            _toggleObject = toggleObject;
            try
            {
                InstructorData = JsonConvert.DeserializeObject<VirtualInstructorDataModel>(toggleObject.option);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            if (!SetParent(toggleObject))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            var poiEditor = transform.parent.GetComponent<PoiEditor>();
            if (poiEditor)
            {
                poiEditor.canRotate = true;
            }
            
            var boxCollider = GetComponentInChildren<BoxCollider>();
            if (boxCollider != null)
            {
                var size = boxCollider.size;
                boxCollider.size = new Vector3(size.x, CharacterHeight, size.z);
                var center = boxCollider.center;
                boxCollider.center = new Vector3(center.x, CharacterHeight * 0.5f, center.z);
            }

            if (!toggleObject.scale.Equals(0))
            {
                transform.localScale = new Vector3(toggleObject.scale, toggleObject.scale, toggleObject.scale);
            }

            gameObject.name = InstructorData.CharacterName;
            PlayAnimationClip(InstructorData.AnimationClip);
            
            RootObject.Instance.virtualInstructorManager.AddInstructor(this);

            return base.Init(toggleObject);
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
        /// Retrieves the AI model for text-to-speech functionality.
        /// </summary>
        /// <returns>The AI model for text-to-speech functionality.</returns>
        public AIModel getTextToSpeechModel()
        {
            return InstructorData.TextToSpeechModel;
        }

        /// <summary>
        /// Asks the virtual instructor a question.
        /// </summary>
        /// <param name="inputAudio">The input audio clip representing the question of the user.</param>
        /// <returns>A clip containing the response from the virtual instructor.</returns>
        public async Task<AudioClip> AskVirtualInstructor(AudioClip inputAudio )
        {
            string context = CreateContext();
            var question = await RootObject.Instance.aiManager.ConvertSpeechToTextAsync(inputAudio, InstructorData.SpeechToTextModel.ApiName);
            var response = await RootObject.Instance.aiManager.SendMessageToAssistantAsync(InstructorData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.aiManager.ConvertTextToSpeechAsync(response, InstructorData.TextToSpeechModel.ApiName);
            UpdateHistory(question, response);
            return clip;
        }

        /// <summary>
        /// CreateContext method is responsible for concatinactein the history  and the Promt to a String.
        /// </summary>
        private string CreateContext() => !string.IsNullOrEmpty(_history) ? InstructorData.Prompt + _history : InstructorData.Prompt;

        /// <summary>
        /// Updates the conversation history with the question and response.
        /// </summary>
        private void UpdateHistory(string question, string response) => _history = string.Format(HistoryFormat, question, response);

        private void OnDestroy()
        {
            RootObject.Instance.virtualInstructorManager.RemoveInstructor(this);
        }
    }
}
