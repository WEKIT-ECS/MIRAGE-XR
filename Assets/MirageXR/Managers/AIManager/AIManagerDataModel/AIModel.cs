
using Newtonsoft.Json;

namespace MirageXR.AIManagerDataModel
{
    /// <summary>
    /// Represents an AI model for the AiServices.
    /// </summary>
    public class AIModel
    {
        /// <summary>
        ///  Name of the Endpoint that the Model uses. 
        /// </summary>
        [JsonProperty("endpointName")] public string EndpointName { get; }

        /// <summary>
        /// Name of the AIModel
        /// </summary>
        [JsonProperty("name")] public string Name { get; }

        /// <summary>
        /// The description of the Model for the User.
        /// </summary>
        [JsonProperty("description")] public string Description { get; }

        /// <summary>
        /// A string representing the name of the Model on the Server.
        /// </summary>
        [JsonProperty("apiName")] public string ApiName { get; }


        /// <summary>
        /// Represents an AI model.
        /// </summary>
        public AIModel(string endpointName, string name = null, string description = null, string apiName = null)
        { 
            EndpointName = endpointName ?? string.Empty;
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            ApiName = apiName ?? string.Empty;
        }
    }
}