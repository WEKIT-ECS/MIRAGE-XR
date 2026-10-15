using System;
using System.Text.RegularExpressions;
using LearningExperienceEngine.NewDataModel;

namespace MirageXR
{
    /// <summary>
    /// Utility for parsing learning activity identifiers from QR codes and deep links.
    /// Supports:
    /// - Repository redirect links: {serverAddress}:{serverPort}/deeplink/?activity={guid}
    /// - App deep links: wekit://load?activity={guid}
    /// - Generic URLs with activity={guid} query parameter
    /// - Direct GUID strings
    /// </summary>
    public static class QRCodeActivityParser
    {
        private const string DEEPLINK_API = "deeplink/";

        private static IConfigManager _configManager;
        private static string _serverAddress;
        private static string _serverPort;

        private static readonly Regex ActivityParamRegex = new Regex(@"(?:[?&]activity=)([-a-fA-F0-9]{36})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Initializes the parser with the ConfigManager to dynamically receive server address and port updates.
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

        /// <summary>
        /// Attempts to parse an activity GUID from QR code content or a deep link URL.
        /// </summary>
        /// <param name="qrContent">The string content decoded from a QR code or deep link.</param>
        /// <param name="activityId">The parsed activity GUID if successful.</param>
        /// <returns>True if a valid activity GUID was found; otherwise false.</returns>
        public static bool TryParseActivityId(string qrContent, out Guid activityId)
        {
            activityId = Guid.Empty;
            if (string.IsNullOrWhiteSpace(qrContent))
            {
                return false;
            }

            var trimmed = qrContent.Trim();

            // 1. Direct GUID string
            if (Guid.TryParse(trimmed, out activityId))
            {
                return true;
            }

            // 2. Query parameter: ?activity={guid} or &activity={guid}
            var match = ActivityParamRegex.Match(trimmed);
            if (match.Success && match.Groups.Count > 1)
            {
                if (Guid.TryParse(match.Groups[1].Value, out activityId))
                {
                    return true;
                }
            }

            // 3. Fallback: Parse via System.Uri
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                var query = uri.Query;
                if (!string.IsNullOrEmpty(query))
                {
                    var queryParameters = query.TrimStart('?').Split('&');
                    foreach (var param in queryParameters)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2 && parts[0].Equals("activity", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Guid.TryParse(parts[1], out activityId))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Generates the standard repository redirect deep link URL for a given activity GUID.
        /// Builds the URL dynamically from the configured server address and port.
        /// </summary>
        public static string CreateRedirectUrl(Guid activityId, IConfigManager configManager = null)
        {
            if (configManager != null)
            {
                Initialize(configManager);
            }
            else
            {
                EnsureConfigSubscribed();
            }

            var baseAddress = _serverAddress?.TrimEnd('/');
            var portPart = !string.IsNullOrEmpty(_serverPort) ? $":{_serverPort}" : string.Empty;

            return $"{baseAddress}{portPart}/{DEEPLINK_API}?activity={activityId}";
        }
    }
}
