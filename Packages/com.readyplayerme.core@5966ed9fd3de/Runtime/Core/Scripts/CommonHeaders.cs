using System.Collections.Generic;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public static class CommonHeaders
    {
        private const string RPM_SOURCE = "RPM-Source";
        private const string RPM_SDK_VERSION = "RPM-SDK-Version";
        private const string UNITY_PREFIX = "unity";
        private const string EDITOR = "editor";
        private const string RUNTIME = "runtime";
        private const string PLAYMODE = "playmode";
        private const string EDITMODE = "editmode";
        private const string SEPARATOR = "-";

        public static KeyValuePair<string, string> GetApplicationJsonHeader()
        {
            return new KeyValuePair<string, string>("Content-Type", "application/json");
        }

        public static KeyValuePair<string, string> GetAppIdHeader()
        {
            return new KeyValuePair<string, string>("X-APP-ID", CoreSettingsHandler.CoreSettings.AppId);
        }

        public static KeyValuePair<string, string> GetAppSource()
        {
            return new KeyValuePair<string, string>("rpm-source", UNITY_PREFIX);
        }

        public static IDictionary<string, string> GetHeadersWithAppId()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(GetApplicationJsonHeader());
            dictionary.Add(GetAppIdHeader());
            dictionary.Add(GetAppSource());
            return dictionary;
        }

        public static Dictionary<string, string> GetAnalyticsHeaders()
        {
            var source = UNITY_PREFIX + SEPARATOR;
            if (Application.isEditor)
            {
                source += EDITOR + SEPARATOR + (Application.isPlaying ? PLAYMODE : EDITMODE);
            }
            else
            {
                source += RUNTIME;
            }
            return new Dictionary<string, string>
            {
                { RPM_SOURCE, source },
                { RPM_SDK_VERSION, ApplicationData.GetData().SDKVersion },
            };
        }
    }
}
