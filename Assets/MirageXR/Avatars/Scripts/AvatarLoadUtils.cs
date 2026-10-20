using i5.Toolkit.Core.Utilities;
using LearningExperienceEngine.NewDataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public static class AvatarLoadUtils
    {
        private const string AVATAR_API = "avatar/get";

        private static IConfigManager _configManager;
        private static string _serverAddress;
        private static string _serverPort;

        /// <summary>
        /// Initializes the utility with the ConfigManager to dynamically receive server address and port updates.
        /// </summary>
        public static void Initialize(IConfigManager configManager)
        {
            if (configManager == null) return;

            if (_configManager != null)
            {
                _configManager.OnNetworkServerAddressChanged -= OnServerAddressChanged;
                _configManager.OnNetworkServerPortChanged -= OnServerPortChanged;
            }

            _configManager = configManager;
            _configManager.OnNetworkServerAddressChanged += OnServerAddressChanged;
            _configManager.OnNetworkServerPortChanged += OnServerPortChanged;

            _serverAddress = configManager.GetNetworkServerAddress();
            _serverPort = configManager.GetNetworkServerPort();
        }

        private static void OnServerAddressChanged(string address)
        {
            _serverAddress = address;
        }

        private static void OnServerPortChanged(string port)
        {
            _serverPort = port;
        }

        private static void EnsureConfigSubscribed()
        {
            if (_configManager != null) return;

            var configManager = RootObject.Instance?.LEE?.ConfigManager;
            if (configManager != null)
            {
                Initialize(configManager);
            }
        }

        /// <summary>
        /// Resets the internal state and subscriptions. Primarily used for unit testing.
        /// </summary>
        public static void ResetForTesting()
        {
            if (_configManager != null)
            {
                _configManager.OnNetworkServerAddressChanged -= OnServerAddressChanged;
                _configManager.OnNetworkServerPortChanged -= OnServerPortChanged;
                _configManager = null;
            }
            _serverAddress = null;
            _serverPort = null;
        }

        public static string ExtractIdFromUrl(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(avatarUrl, UriKind.Absolute, out Uri uri))
            {
                Dictionary<string, string> queryParams = UriUtils.GetUriParameters(uri);
                if (queryParams != null && queryParams.TryGetValue("avatar_name", out string name) && !string.IsNullOrEmpty(name))
                {
                    return name;
                }

                var filename = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                if (!string.IsNullOrEmpty(filename))
                {
                    var cleanId = Regex.Replace(filename, "[^a-zA-Z0-9]", "");
                    if (!string.IsNullOrEmpty(cleanId))
                    {
                        return cleanId;
                    }
                }
                return "";
            }
            else
            {
                var match = Regex.Match(avatarUrl, @"[?&]avatar_name=([^&]+)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                Debug.LogWarning(avatarUrl + " is not a URL, returning it as is.");
                return avatarUrl;
            }
        }

        public static string GetId(string urlOrId)
        {
            if (string.IsNullOrEmpty(urlOrId))
            {
                return string.Empty;
            }

            if (urlOrId.Contains("avatar_name=") || Uri.TryCreate(urlOrId, UriKind.Absolute, out _))
            {
                string extracted = ExtractIdFromUrl(urlOrId);
                if (!string.IsNullOrEmpty(extracted))
                {
                    return extracted;
                }
            }

            return Regex.Replace(urlOrId, "[^a-zA-Z0-9]", "");
        }

        /// <summary>
        /// Generates the model URL for a given avatar ID.
        /// Builds the URL dynamically from the configured server address and port.
        /// </summary>
        public static string IdToModelUrl(string id)
        {
            EnsureConfigSubscribed();

            // make sure that the ID is alpha numerical to avoid string injection attacks
            id = Regex.Replace(id, "[^a-zA-Z0-9]", "");

            var baseAddress = _serverAddress?.TrimEnd('/');
            var portPart = !string.IsNullOrEmpty(_serverPort) ? $":{_serverPort}" : string.Empty;

            return $"{baseAddress}{portPart}/{AVATAR_API}?avatar_name={id}";
        }
    }
}
