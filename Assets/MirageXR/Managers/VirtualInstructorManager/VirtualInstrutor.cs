using System.Threading.Tasks;
using UnityEngine;
using MirageXR;

/// <summary>
/// Represents a virtual instructor that provides language-based assistance.
/// </summary>
public class VirtualInstructor : MonoBehaviour
    {
        /// <summary>
        /// Represents the format for displaying the history of a conversation.
        /// </summary>
        private static readonly string HistoryFormat = "This is the History of the conversation so fare: Question :{0} Given answer: {1}";

        /// <summary>
        /// Represents a virtual instructor.
        /// </summary>
        private GameObject Instructor { get; }

        /// <summary>
        /// Represents the LanguageLanguageModel used by the VirtualInstructor to communicate with an AI assistant.
        /// </summary>
        private AIModel LanguageLanguageModel { get; }

        /// <summary>
        /// Represents a text-to-speech AI model.
        /// </summary>
        private AIModel TextToSpeechModel { get; }

        /// <summary>
        /// Represents a speech-to-text AI model for converting speech audio into text.
        /// </summary>
        private AIModel SpeechToTextModel { get; }

        /// <summary>
        /// Represents the prompt given to the VirtualInstructor.
        /// </summary>
        private string Prompt { get; }
        /// <summary>
        /// Represents the history of a conversation with the VirtualInstructor.
        /// This variable keeps track of the conversation history between the user and the VirtualInstructor.
        /// It is a string that stores the questions and answers exchanged during the conversation.
        /// </summary>
        private string _history = "";


        /// <summary>
        /// Constructor for a virtual instructor in the MirageXR application.
        /// </summary>
        public VirtualInstructor(GameObject instructor, AIModel languageLanguageModel, AIModel textToSpeechModel, 
            AIModel speechToTextModel, string prompt)
        {
            Instructor = instructor;
            LanguageLanguageModel = languageLanguageModel;
            TextToSpeechModel = textToSpeechModel;
            SpeechToTextModel = speechToTextModel;
            Prompt = prompt;
        }

        /// <summary>
        /// Asks the virtual instructor a question.
        /// </summary>
        /// <param name="inputAudio">The input audio clip representing the question of the user.</param>
        /// <returns>A clip containing the response from the virtual instructor.</returns>
        public async Task<AudioClip> AskVirtualInstructor(AudioClip inputAudio ) // input fehlt
        {
            string context = CreateContext();
            var question = await RootObject.Instance.aiManager.ConvertSpeechToTextAsync(inputAudio, SpeechToTextModel.ApiName);
            var response = await RootObject.Instance.aiManager.SendMessageToAssistantAsync(LanguageLanguageModel.ApiName, question, context);
            UnityEngine.Debug.Log("Response :"+response);
            var clip = await RootObject.Instance.aiManager.ConvertTextToSpeechAsync(response, TextToSpeechModel.ApiName);
            UpdateHistory(question, response);
            return clip;
        }

        /// <summary>
        /// CreateContext method is responsible for concatinactein the history  and the Promt to a String.
        /// </summary>
        private string CreateContext() => _history != "" ? Prompt + _history : Prompt;

        /// <summary>
        /// Updates the conversation history with the question and response.
        /// </summary>
        private void UpdateHistory(string question, string response) => _history = string.Format(HistoryFormat, question, response);
    }
