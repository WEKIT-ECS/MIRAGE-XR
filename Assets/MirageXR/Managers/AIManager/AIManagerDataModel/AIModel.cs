
using Newtonsoft.Json;

namespace MirageXR.AIManagerDataModel
{
    public class AIModel
    {
        [JsonProperty("endpointName")] public string EndpointName { get; }
        [JsonProperty("name")] public string Name { get; }
        [JsonProperty("description")] public string? Description { get; }
        [JsonProperty("apiName")] public string? ApiName { get; }


        public AIModel(string endpointName, string name, string description = null, string apiName = null)
        {
            EndpointName = endpointName;
            Name = name;
            Description = description;
            ApiName = apiName;
        }
    }
}