using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using StringReader = System.IO.StringReader;

namespace MirageXR
{
    public class AiServices
    {
        private AiServicesConfig _config;
        private TokenResponse _token;
        public OptionsResponse _options;

        public async Task<string> Listen(AudioClip audioClip, string model) //@todo!
        {
            var apiURL = _config.ApiURL + "listen/";
            // retun ist eine ""string!
            throw new NotImplementedException();
        }

        public async Task<string> Think(string model, string message, string context) //done
        {
            var apiURL = _config.ApiURL + "think/";
            var requestBody = new
            {
                message = message,
                context = context,
                model = model,
            };
            var jsonBody = JsonUtility.ToJson(requestBody);

            using var webRequest = UnityWebRequest.Post(apiURL, string.Empty);
            webRequest.SetRequestHeader("Authorization", _config.Token);
            var jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the result of the Think endpoint: {webRequest.error}");
            }
            return webRequest.downloadHandler.text;
        }

        public async Task<bool> GetOptions() // done
        {
            var apiURL = _config.ApiURL + "options/";
            using var webRequest = UnityWebRequest.Get(apiURL);
            webRequest.SetRequestHeader("Authorization", _config.Token);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException("todo");
            }
            _options = new OptionsResponse(webRequest.downloadHandler.text);
            return true;
        }

        public async Task<AudioClip> speak(string text, string voice) // @todo!
        {
            var apiURL = _config.ApiURL + "speak/";
            throw new NotImplementedException();
        }

        private static async Task<AiServicesConfig> ReadConfig()
        {
            const string configFileName = "AiServicesConfig";
            const string apiURLKey = "API_URL";
            const string usernameKey = "USERNAME";
            const string passwordKey = "PASSWORD";

            var config = Resources.Load<TextAsset>(configFileName); // Read the file name.
            string apiURL = null;
            string username = null;
            string password = null;
            string token = null;

            if (config != null)
            {
                using var sr = new StringReader(config.text);
                while (await sr.ReadLineAsync() is { } line)
                {
                    var parts = line.Split('=', ':');
                    if (parts.Length == 2)
                    {
                        switch (parts[0].ToUpper())
                        {
                            case apiURLKey:
                                apiURL = parts[1].Trim();
                                break;
                            case usernameKey:
                                username = parts[1].Trim();
                                break;
                            case passwordKey:
                                password = parts[1].Trim();
                                break;
                        }
                    }
                }

                token = await AuthenticateUser(apiURL, username, password);
            }

            if (apiURL == null || username == null || password == null || token == null)
            {
                throw new InvalidOperationException(
                    $"Can't find a parameter for the AI Serves configuration -> " +
                    $"Path = {configFileName}, " +
                    $"API_URL = {apiURL}" +
                    $"Username = {username}" +
                    $"Password = {password}" +
                    $"Token = {token}");
            }

            return new AiServicesConfig(apiURL, username, password, token);
        }

        private static async Task<string> AuthenticateUser(string apiURL, string user, string pass)
        {
            var requestBody = new
            {
                username = user,
                password = pass,
            };
            var jsonBody = JsonUtility.ToJson(requestBody);

            using var webRequest = UnityWebRequest.Post(apiURL, string.Empty);
            var jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {webRequest.error}");
            }
            var response = JsonUtility.FromJson<TokenResponse>(webRequest.downloadHandler.text);
            return response.Token;
        }


        public struct OptionsResponse
        {
            public List<string> Listen { get; set; }

            public List<string> Speak{ get; set; }

            public List<string> Think { get; set; }

            public OptionsResponse(string json)
            {
                Listen = new List<string>();
                Speak = new List<string>();
                Think = new List<string>();
                var deserialized = JsonSerializer.Deserialize<OptionsResponse>(json);
                Listen = deserialized.Listen ?? new List<string>();
                Speak = deserialized.Speak ?? new List<string>();
                Think = deserialized.Think ?? new List<string>();
            }
        }

        private readonly struct TokenResponse
        {
            public string Token { get; }

            public TokenResponse(string token)
            {
                Token = token;
            }
        }

        private struct AiServicesConfig
        {
            public string ApiURL;
            public string Username;
            public string Password;
            public string Token;

            public AiServicesConfig(string apiURL, string username, string password, string token)
            {
                ApiURL = apiURL;
                Username = username;
                Password = password;
                Token = token;
            }
        }
    }
}
