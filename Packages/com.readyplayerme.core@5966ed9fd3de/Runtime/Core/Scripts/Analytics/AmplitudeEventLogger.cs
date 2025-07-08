using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ReadyPlayerMe.Core.Analytics
{
    public static class AmplitudeEventLogger
    {
        private const string ENDPOINT = "https://analytics-sdk.readyplayer.me/";
        private const string NO_INTERNET_CONNECTION = "No internet connection.";
#if RPM_DEVELOPMENT
        private const  string TARGET_ENVIRONMENT = "unity-dev";
#else
        private const string TARGET_ENVIRONMENT = "unity";
#endif

        private static long sessionId;

        private static bool HasInternetConnection => Application.internetReachability != NetworkReachability.NotReachable;

        public static void SetSessionId(long id)
        {
            sessionId = id;
        }

        public static bool IsSessionIdSet()
        {
            return sessionId != 0;
        }

        public static async void LogEvent(string eventName, Dictionary<string, object> eventProperties = null, Dictionary<string, object> userProperties = null)
        {
            var eventData = new Dictionary<string, object>
            {
                { AmplitudeKeys.DEVICE_ID, SystemInfo.deviceUniqueIdentifier },
                { AmplitudeKeys.EVENT_TYPE, eventName },
                { AmplitudeKeys.PLATFORM, ApplicationData.GetData().UnityPlatform },
                { AmplitudeKeys.SESSION_ID, sessionId },
                { AmplitudeKeys.APP_VERSION, ApplicationData.GetData().SDKVersion },
                { AmplitudeKeys.OPERATING_SYSTEM, SystemInfo.operatingSystem }
            };

            if (userProperties != null)
            {
                eventData.Add(AmplitudeKeys.USER_PROPERTIES, userProperties);
            }

            if (eventProperties != null)
            {
                eventData.Add(AmplitudeKeys.EVENT_PROPERTIES, eventProperties);
            }

            var payload = new
            {
                target = TARGET_ENVIRONMENT,
                events = new[]
                {
                    eventData
                }
            };

            var json = JsonConvert.SerializeObject(payload);

            try
            {
                await Dispatch(ENDPOINT, json);
            }
            catch (Exception exception)
            {
                SDKLogger.Log(nameof(AmplitudeEventLogger), exception);
            }
        }

        private static async Task Dispatch(string url, string payload)
        {
            if (!HasInternetConnection)
            {
                throw new Exception(NO_INTERNET_CONNECTION);
            }

            var webRequestDispatcher = new WebRequestDispatcher();
            var response = await webRequestDispatcher.SendRequest<ResponseText>(url, HttpMethod.POST, CommonHeaders.GetHeadersWithAppId(), payload);

            if (!response.IsSuccess)
            {
                throw new Exception(response.Error);
            }
        }
    }
}
