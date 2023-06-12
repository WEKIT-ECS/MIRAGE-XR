using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MirageXR
{
    public static class ConfigParser
    {
        public static TextAsset ConfigFile;

        public static string MoodleUrl;
        public static string XApiUrl;

        public static string PrimaryColor;
        public static string SecondaryColor;
        public static string TextColor;
        public static string IconColor;
        public static string TaskStationColor;
        public static string UIPathColor;
        public static string NextPathColor;

        public static ConfigEditor Editor = new ();

        /// <summary>
        /// Initializes static members of the <see cref="ConfigParser"/> class.
        /// Parse the configuration settings from the config file
        /// </summary>
        static ConfigParser()
        {
            ConfigFile = Resources.Load<TextAsset>(Editor.configFileName);

            if (ConfigFile == null)
            {
                return;
            }

            var configItems = ConfigFile.text.Split(new[] { '\r', '\n' }).ToList();

            MoodleUrl = configItems.FirstOrDefault(t => t.StartsWith("moodleUrl"))?.Split(":").LastOrDefault();
            XApiUrl = configItems.FirstOrDefault(t => t.StartsWith("xApiUrl"))?.Split(":").LastOrDefault();
            PrimaryColor = configItems.FirstOrDefault(t => t.StartsWith("primaryColor"))?.Split(":").LastOrDefault();
            SecondaryColor = configItems.FirstOrDefault(t => t.StartsWith("secondaryColor"))?.Split(":").LastOrDefault();
            TextColor = configItems.FirstOrDefault(t => t.StartsWith("textColor"))?.Split(":").LastOrDefault();
            IconColor = configItems.FirstOrDefault(t => t.StartsWith("iconColor"))?.Split(":").LastOrDefault();
            TaskStationColor = configItems.FirstOrDefault(t => t.StartsWith("taskStationColor"))?.Split(":").LastOrDefault();
            UIPathColor = configItems.FirstOrDefault(t => t.StartsWith("pathColor"))?.Split(":").LastOrDefault();
            NextPathColor = configItems.FirstOrDefault(t => t.StartsWith("nextPathColor"))?.Split(":").LastOrDefault();

            // Add "https://" if it doesn't start with "https://" or "http://"
            AddProtocols();
        }


        private static void AddProtocols()
        {
            // Add "https://" to MoodleUrl
            if (!string.IsNullOrEmpty(MoodleUrl))
            {
                if (!MoodleUrl.StartsWith("https://") && !MoodleUrl.StartsWith("http://"))
                {
                    MoodleUrl = "https://" + MoodleUrl;
                }
            }

            // Add "https://" to xApiUrl
            if (!string.IsNullOrEmpty(XApiUrl))
            {
                if (!XApiUrl.StartsWith("https://") && !XApiUrl.StartsWith("http://"))
                {
                    XApiUrl = "https://" + XApiUrl;
                }
            }

            if (!IsValidUrl(MoodleUrl))
            {
                Debug.LogError("Moodle Server URL is not valid.");
            }

            if (!IsValidUrl(XApiUrl))
            {
                Debug.LogError("xAPI URL is not valid.");
            }
        }


        private static bool IsValidUrl(string urlString)
        {
            const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";

            if (string.IsNullOrEmpty(urlString))
            {
                return false;
            }

            var regex = new Regex(regexExpression);
            return regex.IsMatch(urlString);
        }
    }
}
