using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using StringReader = System.IO.StringReader;

namespace MirageXR
{
    public class AiServices
    {
        public OptionsResponse Options;
        private AiServicesConfig _config;
        private TokenResponse _token;


        /// <summary>
        /// Provides multiple Transcription models.
        /// </summary>
        /// <param name="audioClip"> The audio clip that should be transcribe</param>
        /// <param name="model"> The Model that should transcribe the audio</param>
        /// <returns>A String with the result of task or an error if an network error appears</returns>
        public async Task<string> Listen(AudioClip audioClip, string model)
        {
            var apiURL = _config.ApiURL + "listen/";
            var audioInBase64String = Convert.ToBase64String(SaveLoadAudioUtilities.AudioClipToByteArray(audioClip));
            var jsonBody = "{\"model\":\"" + model + "\",\"audio\":\"" + audioInBase64String + "\"}";
            var jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
            using var webRequest = UnityWebRequest.Post(apiURL, string.Empty);
            webRequest.SetRequestHeader("Authorization", _config.Token);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException(
                    $"Error while receiving the result of the Listen endpoint: {webRequest.error}");
            }
            return webRequest.downloadHandler.text;
        }

        /// <summary>
        /// Processes user input in an LLM.
        /// </summary>
        /// <param name="model">The target model</param>
        /// <param name="message">The message of the User</param>
        /// <param name="context">The message of the Instructor</param>
        /// <returns>A String with the result of the operation.</returns>
        public async Task<string> Think(string model, string message, string context)
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
            var jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
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

        /// <summary>
        /// Downloads the available option from the Server.
        /// </summary>
        /// <returns>A boolean. True if the operations was successful, falls if it wasn't.</returns>
        public async Task<bool> GetOptions()
        {
            var apiURL = _config.ApiURL + "options/";
            using var webRequest = UnityWebRequest.Get(apiURL);
            webRequest.SetRequestHeader("Authorization", _config.Token);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the result of the Options endpoint: {webRequest.error}");
            }
            Options = new OptionsResponse(webRequest.downloadHandler.text);
            return true;
        }

        /// <summary>
        /// Get you a audio based on your text
        /// </summary>
        /// <param name="speakOut"> That is the text that you want to turn in to an audio</param>
        /// <param name="voice">The voice that you wan to use. Check options json for legal parameters </param>
        /// <param name="model">The model that use. Check options json for legal parameters</param>
        /// <param name="onSuccess">Function that get invoke onSuccess</param>
        /// <param name="onError">Well if it dosed work, we do this. </param>
        public async Task Speak(string speakOut, string voice, string model, Action<AudioClip> onSuccess, Action<string> onError)
        {
            var apiURL = _config.ApiURL + "speak/";
            var requestBody = new
            {
                speakOut = speakOut,
                voice = voice,
                model = model,
            };
            var jsonBody = JsonUtility.ToJson(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
            using var webRequest = UnityWebRequest.Post(apiURL, string.Empty);
            webRequest.SetRequestHeader("Authorization", _config.Token);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new HttpRequestException($"Error while receiving the result of the Speak endpoint: {webRequest.error}");
                }
                var audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                onSuccess?.Invoke(audioClip);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Loads the config and set everything up.
        /// </summary>
        /// <returns> A Config object</returns>
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

        /// <summary>
        /// Get the token
        /// </summary>
        /// <param name="apiURL"> URL of the Server</param>
        /// <param name="user"> Username</param>
        /// <param name="pass"> Password</param>
        /// <returns>The token as struct</returns>
        private static async Task<string> AuthenticateUser(string apiURL, string user, string pass)
        {
            var requestBody = new
            {
                username = user,
                password = pass,
            };
            var jsonBody = JsonUtility.ToJson(requestBody);

            using var webRequest = UnityWebRequest.Post(apiURL, string.Empty);
            var jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
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

        /// <summary>
        /// Stores the Options JSON
        /// </summary>
        public struct OptionsResponse
        {
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

            public List<string> Listen { get; set; }

            public List<string> Speak{ get; set; }

            public List<string> Think { get; set; }
        }

        /// <summary>
        /// Stores the Toke for the API
        /// </summary>
        private readonly struct TokenResponse
        {
            public TokenResponse(string token)
            {
                Token = token;
            }

            public string Token { get; }
        }

        /// <summary>
        /// Stores the Config
        /// </summary>
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
