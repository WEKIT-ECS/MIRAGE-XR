using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class AuthDataConverter
    {
        private const string DATA = "data";

        public static string CreatePayload(Dictionary<string, string> data)
        {
            return JObject.FromObject(data).ToString();
        }

        public static string CreateDataPayload(Dictionary<string, string> data)
        {
            return new JObject(
                new JProperty(DATA, JObject.FromObject(data))
            ).ToString();
        }

        public static JToken ParseResponse(string response)
        {
            var json = JObject.Parse(response);

            if (json == null)
            {
                throw new Exception("No data received");
            }

            return json;
        }

        public static JToken ParseDataResponse(string response)
        {
            var json = JObject.Parse(response);
            var data = json.GetValue(DATA);

            if (data == null)
            {
                throw new Exception("No data received");
            }

            return data;
        }
    }
}
