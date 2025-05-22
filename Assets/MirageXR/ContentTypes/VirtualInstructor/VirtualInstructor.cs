using System;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Represents a virtual instructor that provides language-based assistance.
    /// </summary>
    public class VirtualInstructor : MirageXRPrefab, IVirtualInstructor
    {
        private const float CharacterHeight = 1.8f;
        /// <summary>
        /// Represents the format for displaying the history of a conversation.
        /// </summary>
        private static readonly string HistoryFormat = "This is the History of the conversation so fare: Question :{0} Given answer: {1}";

        public Vector3 Position => transform.position;
        
        /// <summary>
        /// Represents the data model for a virtual instructor in the MirageXR application.
        /// </summary>
        /// <remarks>
        /// The InstructorData class stores information about the language models and prompts used by the virtual instructor.
        /// </remarks>
        private InstructorContentData InstructorData { get; set; }

        [SerializeField] private LearningExperienceEngine.ToggleObject _toggleObject;
        private Animator _animator;

        private bool _isModerator = true; // temp

        /// <summary>
        /// Represents the history of a conversation with the VirtualInstructor.
        /// This variable keeps track of the conversation history between the user and the VirtualInstructor.
        /// It is a string that stores the questions and answers exchanged during the conversation.
        /// </summary>
        private string _history;

        /// <summary>
        /// Initializes the virtual instructor with the given toggle object, setting up necessary components like the
        /// animator, collider, and parent. If initialization fails (e.g., invalid JSON data or failure to set parent),
        /// the method returns <c>false</c>.
        /// </summary>
        /// <param name="toggleObject">The toggle object containing options and settings for initialization.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the initialization was successful.
        /// Returns <c>false</c> if initialization fails.
        /// </returns>
        public override bool Init(LearningExperienceEngine.ToggleObject toggleObject)
        {
            // todo add isModerator. 
            _animator = GetComponentInChildren<Animator>();
            _toggleObject = toggleObject;
            try
            {
                InstructorData = JsonConvert.DeserializeObject<InstructorContentData>(toggleObject.option);
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
            
            RootObject.Instance.VirtualInstructorOrchestrator.AddInstructor(this);

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

        public void PlayAudio(AudioClip clip)
        {
            GetComponent<AvatarAudioController>().PlayAudio(clip);
        }
        
        /// <summary>
        /// Retrieves the AI model for text-to-speech functionality.
        /// </summary>
        /// <returns>The AI model for text-to-speech functionality.</returns>
        public AIModel GetTextToSpeechModel()
        {
            return InstructorData.TextToSpeechModel;
        }

        /// <summary>
        /// Asks the virtual instructor a question.
        /// </summary>
        /// <param name="inputAudio">The input audio clip representing the question of the user.</param>
        /// <param name="messageQueue">A string containing a set of events </param>
        /// <returns>A clip containing the response from the virtual instructor.</returns>

        public async Task<AudioClip> AskVirtualInstructorAudio(AudioClip inputAudio, string messageQueue="")
        {
            string context = CreateContext(messageQueue);
            var question = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertSpeechToTextAsync(inputAudio, InstructorData.SpeechToTextModel.ApiName);
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(InstructorData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, InstructorData.TextToSpeechModel.ApiName);
            UpdateHistory(question, response);
            return clip;
        }


        /// <summary>
        /// Sends a message to the virtual instructor and returns the corresponding speech audio clip.
        /// </summary>
        /// <param name="message">Message to be sent along with the message queue.</param>
        /// <param name="messageQueue">the message queue is a set of event that should be included in the answer</param>
        /// <returns>The speech audio clip generated by the virtual instructor in response to the message.</returns>
        public async Task<AudioClip> AskInstructorWithStringQuestion(string message, string messageQueue = "")
        {
            if (messageQueue == null)
            {
                throw new ArgumentNullException(nameof(messageQueue), "MessageQueue cannot be null");
            }

            try
            {
                string final; 
                if (!string.IsNullOrEmpty(messageQueue))
                {
                    final = message + " " + messageQueue;
                }
                else
                {
                    final = messageQueue;
                }
                var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync
                    (InstructorData.LanguageModel.ApiName, final, _history);
                var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(
                    response, InstructorData.TextToSpeechModel.ApiName);
                UpdateHistory(message, response);
                return clip;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("An error occurred: " + ex.Message);
                return null; 
            }
        }

        /// <summary>
        /// Converts a text message to speech using the specified model.
        /// </summary>
        /// <param name="message">The text message to be converted to speech.</param>
        /// <returns>An async task that represents the asynchronous operation. The task result contains the audio clip representing the converted speech.</returns>
        public async Task<AudioClip> ConvertTextToSpeech(string message)
        {
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, InstructorData.TextToSpeechModel.ApiName);
            return clip;
        }

        /// <summary>
        /// CreateContext method is responsible for concatenation the history  and the prompt to a String.
        /// </summary>
        private string CreateContext(string messageQueue = "") => !string.IsNullOrEmpty(_history) ? InstructorData.Prompt + _history + messageQueue : InstructorData.Prompt;

        /// <summary>
        /// Updates the conversation history with the question and response.
        /// </summary>
        private void UpdateHistory(string question, string response) => _history = string.Format(HistoryFormat, question, response);

        private void OnDestroy()
        {
            RootObject.Instance.VirtualInstructorOrchestrator.RemoveInstructor(this);
        }
        
        /// <summary>
        /// Asynchronously sends a question to the virtual instructor and retrieves the response as an
        /// <see cref="AudioClip"/>. The question is processed through an AI model, and converted to speech.
        /// </summary>
        /// <param name="question">The question to be asked, provided as a string.</param>
        /// <param name="queue">A string representing the message queue or context in which the question is being asked.</param>
        /// <returns>
        /// A <see cref="Task{AudioClip}"/> that represents an <see cref="AudioClip"/> containing the spoken response
        /// from the virtual instructor.
        /// </returns>
        public async Task<AudioClip> AskVirtualInstructorString(string question, string queue)
        {
            string context = CreateContext();
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(InstructorData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, InstructorData.TextToSpeechModel.ApiName);
            UpdateHistory(question, response);
            return clip;
        }
        
        /// <summary>
        /// Returns the current moderator status of the virtual instructor.
        /// </summary>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the virtual instructor is a moderator.
        /// </returns>
        public bool ModeratorStatus()
        {
            return _isModerator;
        }
    }
}
