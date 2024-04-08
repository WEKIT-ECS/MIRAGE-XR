using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using StringReader = System.IO.StringReader;

namespace MirageXR
{
    public class AiServices
    {
        public List<OptionsResponse> Options;
        public AiServicesConfig _config;
        private TokenResponse _token;
        private JsonConfig _jsonConfig;

        /// <summary>
        /// Provides multiple Transcription models.
        /// </summary>
        /// <param name="audioClip"> The audio clip that should be transcribe</param>
        /// <param name="model"> The Model that should transcribe the audio</param>
        /// <returns>A String with the result of task or an error if an network error appears</returns>
        public async Task<string> Listen(AudioClip audioClip, string model)
        {
            UnityEngine.Debug.LogError("Speak");
            var apiURL = _config.AiApiUrl + "/listen/";
            var audioInBase64String = SaveLoadAudioUtilities.AudioClipToByteArray(audioClip);
            var fromData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("model", model),
                new MultipartFormFileSection("audio", audioInBase64String),
            };
            using var webRequest = UnityWebRequest.Post(apiURL, fromData);
            webRequest.SetRequestHeader("Authorization", _config.AiToken);
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException(
                    $"Error while receiving the result of the Listen endpoint: {webRequest.error}");
            }
            return webRequest.downloadHandler.text;
        }

        /// <summary> Done!
        /// Processes user input in an LLM.
        /// </summary>
        /// <param name="model">The target model</param>
        /// <param name="message">The message of the User</param>
        /// <param name="context">The message of the Instructor</param>
        /// <returns>A String with the result of the operation.</returns>
        public async Task<string> Think(string model, string message, string context)
        {
            var apiURL = _config.AiApiUrl + "/think/";
            var fromData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("model", model),
                new MultipartFormDataSection("message", message),
                new MultipartFormDataSection("context", context),
            };
            using var webRequest = UnityWebRequest.Post(apiURL, fromData);
            webRequest.SetRequestHeader("Authorization", "Token " + _config.AiToken);
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
        private async Task<bool> GetOptions()
        {
            UnityEngine.Debug.LogError("GetOptions");
            var apiURL = _config.AiApiUrl + "/options/";
            var request = UnityWebRequest.Get(apiURL);
            request.SetRequestHeader("Authorization", $"Token {_config.AiToken}");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }

            var r = request.downloadHandler.text;
            UnityEngine.Debug.LogError(r);
            var myDeserializedClass = JsonConvert.DeserializeObject<List<OptionsResponse>>(r);
            UnityEngine.Debug.LogError(myDeserializedClass.ToString());
            Options = myDeserializedClass;
            await Think("gpt-3.5-turbo", "Write test", "Write test");
            var audio = await Speak("Hallo andreas", "onyx", "default");
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
        /// <returns><placeholder>A <see cref="Tasks.Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<AudioClip> Speak(string speakOut, string voice, string model)
        {
            UnityEngine.Debug.LogError("Speak");
            var apiURL = _config.AiApiUrl + "/speak/";
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(apiURL, AudioType.UNKNOWN);
            webRequest.SetRequestHeader("Authorization", "Token " + _config.AiToken);
            webRequest.SetRequestHeader("speakOut", speakOut);
            webRequest.SetRequestHeader("voice", voice);
            webRequest.SetRequestHeader("model", model);
            await webRequest.SendWebRequest();
            UnityEngine.Debug.LogError("Send Speak request");
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the result of the Speak endpoint: {webRequest.error} {webRequest.result}");
            }
            AudioClip audioClip;
            audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            return audioClip;
        }

        /// <summary>
        /// Loads the config and set everything up.
        /// </summary>
        /// <returns> A Config object</returns>
        public async Task<AiServicesConfig> ReadConfig()
        {
            const string configFileName = "MirageXRConfig.txt";
            const string apiURLKey = "AI_API_URL";
            const string usernameKey = "AI_USERNAME";
            const string passwordKey = "AI_PASSWORD";
            const string apiPort = ":8000";
            //var filepath = Path.Combine(Application.streamingAssetsPath, "MirageXRConfig.txt");
            var filepath = Path.Combine(Application.streamingAssetsPath, "config.json");
            string apiURL = null;
            string username = null;
            string password = null;
            string token = null;
            if (!File.Exists(filepath))
            {
                UnityEngine.Debug.LogError($"Failed to load config file: {configFileName}");
            }
            //if (File.Exists(filepath))
            //{
            //    var configText = await File.ReadAllTextAsync(filepath);
            //    using var sr = new StringReader(configText);
            //    while (await sr.ReadLineAsync() is { } line)
            //    {
            //        var parts = line.Split('=', ':');
            //        if (parts.Length == 2)
            //        {
            //           switch (parts[0].ToUpper())
            //            {
            //                case apiURLKey:
            //                    apiURL = parts[1].Trim();
            //                    break;
            //                case usernameKey:
            //                    username = parts[1].Trim();
            //                    break;
            //                case passwordKey:
            //                    password = parts[1].Trim();
            //                    break;
            //            }
            //        }
            //    }
            //
            // if (apiURL == null)
            // {
            //    UnityEngine.Debug.LogError("apiURL is null");
            // }
            // }

            if (File.Exists(filepath))
            {
                UnityEngine.Debug.LogError("1");
                var configJson = await File.ReadAllTextAsync(filepath);
                var config = JsonUtility.FromJson<JsonConfig>(configJson);
                apiURL = config.AiApiUrl;
                username = config.AiUsername;
                password = config.AiPassword;
            }
            apiURL = "http://" + apiURL + apiPort; //Need to be updated! @todo...
            token = await AuthenticateUser(apiURL, username, password);
            UnityEngine.Debug.LogError("2");
            if (apiURL == null || username == null || password == null || token == null)
            {
                throw new InvalidOperationException(
                    $"Can't find a parameter for the AI Serves configuration -> " +
                    $"Path = {configFileName}, " +
                    $"API_URL = {apiURL}," +
                    $"Username = {username}," +
                    $"Password = {password}," +
                    $"Token = {token}");
            }
            UnityEngine.Debug.LogError(apiURL+ username+ password);
            var r = new AiServicesConfig(apiURL, username, password);
            r.SetToken(token);
            _config = r;
            UnityEngine.Debug.LogError("Config is done with " + _config.AiToken + _config.AiApiUrl + _config.AiPassword + _config.AiUsername);
            var options = await GetOptions();
            if (options)
            {
                return r;
            }
            UnityEngine.Debug.LogError("Configuration of the AI Services failed! Unable to load the options!");
            throw new InvalidOperationException();
        }

        /// <summary> DONE!
        /// Get the token
        /// </summary>
        /// <param name="apiURL"> URL of the Server</param>
        /// <param name="user"> Username</param>
        /// <param name="pass"> Password</param>
        /// <returns>The token as struct</returns>
        private static async Task<string> AuthenticateUser(string apiURL, string user, string pass)
        {
            apiURL += "/authentication/";
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("username", user),
                new MultipartFormDataSection("password", pass),
            };
            var request = UnityWebRequest.Post(apiURL, formData);
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }
            var r = request.downloadHandler.text;
            if (r.Length < 13)
            {
                throw new HttpRequestException($"Error while receiving the token! Got ' {r} 'and that is too short!");
            }
            r = r.Substring(10, r.Length - 10 - 2);
            return r;
        }

        /// <summary>
        /// Stores the Toke for the API
        /// </summary>
        private readonly struct TokenResponse // still needed? @todo..
        {
            public TokenResponse(string t)
            {
                token = t;
            }

            public string token { get; }
        }

        /// <summary>
        /// Stores the Config
        /// </summary>
        public struct AiServicesConfig
        {
            public string AiApiUrl;
            public string AiUsername;
            public string AiPassword;
            public string AiToken;

            public AiServicesConfig(string apiURL, string username, string password)
            {
                AiApiUrl = apiURL;
                AiUsername = username;
                AiPassword = password;
                AiToken = null;
            }

            public void SetToken(string token)
            {
                AiToken = token;
            }
        }
    }

    public class OptionsResponse
    {
        public string name { get; set; }

        public List<string> models { get; set; }
    }

    public class JsonConfig
    {
        public string CompanyName { get; set; }

        public string ProductName { get; set; }

        public string MoodleUrl { get; set; }

        public string XApiUrl { get; set; }

        public string Version { get; set; }

        public string SplashScreen { get; set; }

        public string Logo { get; set; }

        public string SplashBackgroundColor { get; set; }

        public string PrimaryColor { get; set; }

        public string SecondaryColor { get; set; }

        public string TextColor { get; set; }

        public string IconColor { get; set; }

        public string TaskStationColor { get; set; }

        public string PathColor { get; set; }

        public string NextPathColor { get; set; }

        public string CalibrationMarker { get; set; }

        public string PdfCalibrationMarker { get; set; }

        public string AiApiUrl { get; set; }

        public string AiUsername { get; set; }

        public string AiPassword { get; set; }
    }
}
