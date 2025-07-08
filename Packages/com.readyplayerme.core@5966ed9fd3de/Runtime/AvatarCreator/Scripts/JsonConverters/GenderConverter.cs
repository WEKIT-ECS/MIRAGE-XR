using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadyPlayerMe.Core;
using UnityEngine.Scripting;

namespace ReadyPlayerMe.AvatarCreator
{
    [Preserve]
    public class GenderConverter : JsonConverter
    {
        private const string MALE = "male";
        private const string FEMALE = "female";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OutfitGender);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var newValue = value switch
            {
                OutfitGender.Masculine => MALE,
                OutfitGender.Feminine => FEMALE,
                OutfitGender.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
            serializer.Serialize(writer, newValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                return token.ToString() switch
                {
                    MALE => OutfitGender.Masculine,
                    FEMALE => OutfitGender.Feminine,
                    _ => OutfitGender.None
                };
            }

            throw new JsonSerializationException("Expected string value, instead found: " + token.Type);
        }
    }
}
