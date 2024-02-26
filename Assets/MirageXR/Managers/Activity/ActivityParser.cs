using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MirageXR
{
    public static class ActivityParser
    {
        private const string RESOURCES_PREFIX = "resources://";
        private const string HTTP_PREFIX = "http";
        private const string JSON_EXCEPTION = ".json";
        private const char SLASH_CHAR = '/';

        public static Activity Parse(string activityId)
        {
            var json = GetActivityJson(activityId);
            return JsonConvert.DeserializeObject<Activity>(json);
        }

        public static string Serialize(Activity activity)
        {
            return JsonConvert.SerializeObject(activity);
        }

        private static string GetActivityJson(string activityId)
        {
            if (activityId.StartsWith(RESOURCES_PREFIX))
            {
                var filePath = activityId.Replace(RESOURCES_PREFIX, string.Empty);
                var textAsset = Resources.Load(filePath) as TextAsset;
                if (textAsset != null)
                {
                    return textAsset ? textAsset.text : null;
                }

                return null;
            }

            if (activityId.StartsWith(HTTP_PREFIX))
            {
                var fullname = activityId.Split(SLASH_CHAR);
                activityId = fullname[fullname.Length - 1];
            }

            if (!activityId.EndsWith(JSON_EXCEPTION))
            {
                activityId += JSON_EXCEPTION;
            }

            var path = Path.Combine(Application.persistentDataPath, activityId);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
    }
}