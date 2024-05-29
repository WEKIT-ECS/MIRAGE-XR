namespace MirageXR
{
    /// <summary>
    /// Represents the data model for the VirtualInstructor class.
    /// </summary>
    public class VirtualInstructorDataModel
    {
        /// <summary>
        /// Represents the language model associated with the virtual instructor.
        /// </summary>
        /// <value>The language model.</value>
        public AIModel LanguageModel { get; set; }

        /// Represents a model for text-to-speech functionality.
        /// /
        public AIModel TextToSpeechModel { get; set; }

        /// <summary>
        /// Represents a speech-to-text model for the virtual instructor.
        /// </summary>
        public AIModel SpeechToTextModel { get; set; }

        /// <summary>
        /// Represents a virtual instructor that provides language-based assistance.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Represents the data model for the VirtualInstructor in the MirageXR application.
        /// </summary>
        public VirtualInstructorDataModel(AIModel languageModel, AIModel textToSpeechModel, AIModel speechToTextModel, string prompt)
        {
            LanguageModel = languageModel;
            TextToSpeechModel = textToSpeechModel;
            SpeechToTextModel = speechToTextModel;
            Prompt = prompt;
        }
    }
}