using System;
using Newtonsoft.Json;

namespace MirageXR
{
    /// <summary>
    /// Represents the data model for the VirtualInstructor class.
    /// </summary>
    [Serializable]
    public class VirtualInstructorDataModel
    {
        /// <summary>
        /// Represents the language model associated with the virtual instructor.
        /// </summary>
        /// <value>The language model.</value>
        [JsonProperty] public AIModel LanguageModel { get; set; }

        /// Represents a model for text-to-speech functionality.
        /// /
        [JsonProperty] public AIModel TextToSpeechModel { get; set; }

        /// <summary>
        /// Represents a speech-to-text model for the virtual instructor.
        /// </summary>
        [JsonProperty] public AIModel SpeechToTextModel { get; set; }

        /// <summary>
        /// Represents a virtual instructor that provides language-based assistance.
        /// </summary>
        [JsonProperty] public string Prompt { get; set; }

        [JsonProperty] public string AnimationClip { get; set; }

        [JsonProperty] public string CharacterName { get; set; }
    }
}