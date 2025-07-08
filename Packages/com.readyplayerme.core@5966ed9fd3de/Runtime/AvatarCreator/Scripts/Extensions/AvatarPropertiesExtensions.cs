using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class AvatarPropertiesExtensions
    {
        public static string ToJson(this AvatarProperties avatarProperties, bool ignoreEmptyFields = false)
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var settings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };

            var data = new Dictionary<string, AvatarProperties>
            {
                { "data", avatarProperties }
            };

            if (ignoreEmptyFields)
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            }

            return JsonConvert.SerializeObject(data, settings);
        }
    }
}
