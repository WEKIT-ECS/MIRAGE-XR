using Newtonsoft.Json;

namespace MirageXR.AIManagerDataModel
{
    /// <summary>
    /// Represents a token used for authentication in AiServices.
    /// </summary>
    public class Token
    {
        [JsonProperty("token")] public string BackendToken { get; set; }
    }
}
