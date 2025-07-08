using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Scripting;

namespace ReadyPlayerMe.AvatarCreator
{
    [Preserve]
    public class CategoryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AssetType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException("Expected string value");
            }

            if (!CategoryHelper.AssetTypeByValue.ContainsKey(token.ToString()))
            {
                return AssetType.None;
            }

            return CategoryHelper.AssetTypeByValue[token.ToString()];
        }
    }
}
