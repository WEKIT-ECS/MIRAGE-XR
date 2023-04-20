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

            MoodleUrl = configItems.Find(ci => ci == "moodleUrl");
            XApiUrl = configItems.Find(ci => ci == "xApiUrl");
            PrimaryColor = configItems.Find(ci => ci == "primaryColor");
            SecondaryColor = configItems.Find(ci => ci == "secondaryColor");
            TextColor = configItems.Find(ci => ci == "textColor");
            IconColor = configItems.Find(ci => ci == "iconColor");
            TaskStationColor = configItems.Find(ci => ci == "taskStationColor");
            UIPathColor = configItems.Find(ci => ci == "pathColor");
            NextPathColor = configItems.Find(ci => ci == "nextPathColor");

            // Add "https://" if it doesn't start with "https://" or "http://"
            AddProtocols();
        }


        private static void AddProtocols()
        {
            // Add "https://" to MoodleUrl
            if (!MoodleUrl.StartsWith("https://") && !MoodleUrl.StartsWith("http://"))
            {
                MoodleUrl = "https://" + MoodleUrl;
            }

            // Add "https://" to xApiUrl
            if (!XApiUrl.StartsWith("https://") && !XApiUrl.StartsWith("http://"))
            {
                XApiUrl = "https://" + XApiUrl;
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
            var regex = new Regex(regexExpression);
            return regex.IsMatch(urlString);
        }
    }
}
