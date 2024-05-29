using System;
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

        /// Represents the data model for a virtual instructor in the MirageXR application.
        /// </summary>
        /// <remarks>
        /// The InstructorData class stores information about the language models and prompts used by the virtual instructor.
        /// </remarks>
        private VirtualInstructorDataModel InstructorData { get; set; }
        
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
            InstructorData =
                new VirtualInstructorDataModel(languageLanguageModel, textToSpeechModel, speechToTextModel, prompt);
            RootObject.Instance.virtualInstructorManager.AddInstrutor(this);
        }

        /// <summary>
        /// Gets the language model associated with the virtual instructor.
        /// </summary>
        /// <returns>The language model.</returns>
        public AIModel getLanguageLanguageModel()
        {
            return InstructorData.LanguageModel;
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
        /// Retrieves the SpeechToTextModel associated with the virtual instructor.
        /// </summary>
        /// <returns>The SpeechToTextModel for the virtual instructor.</returns>
        public AIModel getSpeechToTextModel()
        {
            return InstructorData.SpeechToTextModel;
        }

        /// <summary>
        /// Gets the prompt associated with the virtual instructor.
        /// </summary>
        /// <returns>The prompt string.</returns>
        public string getPromt()
        {
            return InstructorData.Prompt;
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
        private string CreateContext() => _history != "" ? InstructorData.Prompt + _history : InstructorData.Prompt;

        /// <summary>
        /// Updates the conversation history with the question and response.
        /// </summary>
        private void UpdateHistory(string question, string response) => _history = string.Format(HistoryFormat, question, response);

        private void OnDestroy()
        {
            RootObject.Instance.virtualInstructorManager.RemoveInstrutor(this);
        }
    }
