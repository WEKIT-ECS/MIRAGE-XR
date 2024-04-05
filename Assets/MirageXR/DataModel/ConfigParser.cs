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
        public static string calibrationMarker;
        public static string calibrationMarkerPdf;

        public static ConfigEditor Editor = new ();

        /// <summary>
        /// Initializes static members of the <see cref="ConfigParser"/> class.
        /// Parse the configuration settings from the config file
        /// </summary>
        static ConfigParser()
        {
            ConfigFile = Resources.Load<TextAsset>(Editor.configFileName); // TODO: change to simple JSON file.

            if (ConfigFile == null)
            {
                return;
            }

            var configItems = ConfigFile.text.Split(new[] { '\r', '\n' }).ToList();

            MoodleUrl = configItems.FirstOrDefault(t => t.StartsWith("moodleUrl"))?.Substring(10);
            //PlayerPrefs.DeleteKey("MoodleURL"); // uncomment if you ever have to delete in editor for testing
            if (!PlayerPrefs.HasKey("MoodleURL"))
            {
                Debug.LogTrace("[ConfigParser] No Moodle endpoint URL stored in PlayerPrefs, setting Moodle endpoint from brand configuration: " + MoodleUrl);
                DBManager.domain = MoodleUrl;
            } else
            {
                Debug.LogTrace("[ConfigParser] Moodle endpoint already stored in PlayerPrefs, ignoring Moodle endpoint from brand configuration: " + MoodleUrl);
            }
            XApiUrl = configItems.FirstOrDefault(t => t.StartsWith("xApiUrl"))?.Substring(8);
            if (XApiUrl != "https://lrs.wekit-ecs.com/data/xAPI")
            {
                Debug.LogError("BrandConfiguration faulty: currently only https://lrs.wekit-ecs.com/data/xAPI supported as XApiUrl");
            } else
            {
                Debug.LogTrace("[ConfigParser] xAPI endpoint URL from brand configuration: " + XApiUrl + " ignored as already set");
                // would have to set DBManager.publicCurrentLearningRecordStore and send Eventmanager.XAPIChanged?.Invoke(DBManager.publicCurrentLearningRecordStore);
            }
            PrimaryColor = configItems.FirstOrDefault(t => t.StartsWith("primaryColor"))?.Split(":").LastOrDefault();
            SecondaryColor = configItems.FirstOrDefault(t => t.StartsWith("secondaryColor"))?.Split(":").LastOrDefault();
            TextColor = configItems.FirstOrDefault(t => t.StartsWith("textColor"))?.Split(":").LastOrDefault();
            IconColor = configItems.FirstOrDefault(t => t.StartsWith("iconColor"))?.Split(":").LastOrDefault();
            TaskStationColor = configItems.FirstOrDefault(t => t.StartsWith("taskStationColor"))?.Split(":").LastOrDefault();
            UIPathColor = configItems.FirstOrDefault(t => t.StartsWith("pathColor"))?.Split(":").LastOrDefault();
            NextPathColor = configItems.FirstOrDefault(t => t.StartsWith("nextPathColor"))?.Split(":").LastOrDefault();
            calibrationMarker = configItems.FirstOrDefault(t => t.StartsWith("calibrationMarker"))?.Split(":").LastOrDefault();
            calibrationMarkerPdf = configItems.FirstOrDefault(t => t.StartsWith("pdfCalibrationMarker"))?.Split(":").LastOrDefault();

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
