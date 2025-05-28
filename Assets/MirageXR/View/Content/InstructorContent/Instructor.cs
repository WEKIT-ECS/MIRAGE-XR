using System;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using UnityEngine;

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
        public event Action<AudioClip> OnInstructorResponseAvailable;
        
        private const float CharacterHeight = 1.8f;
        /// <summary>
        /// Represents the format for displaying the history of a conversation.
        /// </summary>
        private static readonly string HistoryFormat = "This is the History of the conversation so fare: Question :{0} Given answer: {1}";

        private Animator _animator;
        
        private Content<InstructorContentData> _instructorContent;

        /// <summary>
        /// Represents the history of a conversation with the VirtualInstructor.
        /// This variable keeps track of the conversation history between the user and the VirtualInstructor.
        /// It is a string that stores the questions and answers exchanged during the conversation.
        /// </summary>
        private string _history;

        public Vector3 Position => transform.position;
        
        public void Initialize(Content<InstructorContentData> content)
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
                OnInstructorResponseAvailable.Invoke(clip); 
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
        /// <param name="messageQueue">A string containing a set of events </param>
        /// <returns>A clip containing the response from the virtual instructor.</returns>

        public async Task<AudioClip> AskVirtualInstructorAudio(AudioClip inputAudio, string messageQueue="")
        {
            string context = CreateContext(messageQueue);
            var question = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertSpeechToTextAsync(inputAudio, _instructorContent.ContentData.SpeechToTextModel.ApiName);
            AppLog.LogDebug($"AI question: '{question}'");
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, question, context);
            AppLog.LogDebug($"AI response: '{response}'");
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName);
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
                var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, final, _history);
                var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName);
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
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, _instructorContent.ContentData.TextToSpeechModel.ApiName);
            return clip;
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
            var response = await RootObject.Instance.LEE.ArtificialIntelligenceManager.SendMessageToAssistantAsync(_instructorContent.ContentData.LanguageModel.ApiName, question, context);
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(response, _instructorContent.ContentData.TextToSpeechModel.ApiName);
            UpdateHistory(question, response);
            return clip;
        }
    }
}