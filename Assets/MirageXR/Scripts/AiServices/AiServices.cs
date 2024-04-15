using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using StringReader = System.IO.StringReader;

namespace MirageXR
{
    /// <summary>
    /// Provides AI services such as transcription, conversation, and speech synthesis.
    /// </summary>
    public class AiServices
    {
        /// <summary>
        /// Represents the variable "Options" in the AiServices class.
        /// </summary>
        public List<OptionsResponse> Options;

        /// <summary>
        /// Stores the Config for the AI services.
        /// </summary>
        public AiServicesConfig _config;

        /// <summary>
        /// Stores the token for the API.
        /// </summary>
        private TokenResponse _token;

        /// <summary>
        /// Represents the JSON configuration used by the AiServices class.
        /// </summary>
        private JsonConfig _jsonConfig;

        /// <summary>
        /// Provides multiple Transcription models.
        /// </summary>
        /// <param name="audioClip"> The audio clip that should be transcribed. </param>
        /// <param name="model"> The model that should transcribe the audio. </param>
        /// <returns> A String with the result of the task or an error if a network error appears. </returns>
        public async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, string model)
        {
                if (audioClip == null || string.IsNullOrEmpty(model))
                {
                    UnityEngine.Debug.LogError($"AudioClip or Model is  null! (Model: {model})");
                }

                var apiURL = _config.AiApiUrl + "/listen/";
                var audioBytArray = SaveLoadAudioUtilities.AudioClipToByteArray(audioClip);
                UnityEngine.Debug.LogError(audioBytArray.GetType() == typeof(byte[])
                    ? "audioByteArray ist ein byte-Array"
                    : "audioByteArray ist kein byte-Array");

                WWWForm fromData = new WWWForm();
                fromData.AddField("model", model);
                fromData.AddBinaryData("message", audioBytArray);
                
            
         
                using var webRequest = UnityWebRequest.Post(apiURL, fromData);
                webRequest.SetRequestHeader("Authorization", $"Token {_config.AiToken}");
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
        public async Task<string> SendMessageToLlm(string model, string message, string context)
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
        /// Downloads the available options from the server.
        /// </summary>
        /// <returns>A boolean. True if the operation was successful, false if it wasn't.</returns>
        /// 
        private async Task<bool> GetAvailableModels()
        {
            var apiURL = _config.AiApiUrl + "/options/";
            var request = UnityWebRequest.Get(apiURL);
            request.SetRequestHeader("Authorization", $"Token {_config.AiToken}");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }

            var optionsText = request.downloadHandler.text;
            var myDeserializedClass = JsonConvert.DeserializeObject<List<OptionsResponse>>(optionsText);
            Options = myDeserializedClass;
            
            //await SendMessageToLlm("gpt-3.5-turbo", "Write test", "Write test");
            //var audio = await ConvertTextToSpeechAsync("Hallo andreas", "onyx", "default");
            //var text = await ConvertSpeechToTextAsync(audio, "default");
            //UnityEngine.Debug.LogError(text);
            return true;
        }

        /// <summary>
        /// Get you an audio based on your text
        /// </summary>
        /// <param name="speakOut">The text that you want to turn into an audio</param>
        /// <param name="voice">The voice that you want to use. Check options json for legal parameters </param>
        /// <param name="model">The model that you use. Check options json for legal parameters</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task will return an <see cref="AudioClip"/>.</returns>
        public async Task<AudioClip> ConvertTextToSpeechAsync(string speakOut, string voice, string model)
        {
            var apiURL = _config.AiApiUrl + "/speak/";
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(apiURL, AudioType.MPEG);
            webRequest.SetRequestHeader("Authorization", "Token " + _config.AiToken);
            webRequest.SetRequestHeader("message", speakOut);
            webRequest.SetRequestHeader("submodel", voice);
            webRequest.SetRequestHeader("model", model);
            await webRequest.SendWebRequest();
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. It returns a <see cref="AiServicesConfig"/> object.</returns>
        public async Task<AiServicesConfig> ReadConfig()
        {
            const string miragexrFileName = "MirageXRConfig";
            const string aiServicesFileName = "aiServices";
            const string apiURLKey = "AI_API_URL";
            const string usernameKey = "AI_USERNAME";
            const string passwordKey = "AI_PASSWORD";
            const string apiPort = ":8000";
            var filepath = Resources.Load(miragexrFileName) as TextAsset; //Path.Combine(Application.streamingAssetsPath, "MirageXRConfig.txt");
            //var filepath = Path.Combine(Application.streamingAssetsPath, "config.json");
            string apiURL = null;
            string username = null;
            string password = null;
            string token = null;
            if (filepath == null)
            {
                UnityEngine.Debug.LogError($"Failed to load config file: {miragexrFileName}");
            }
            if (filepath != null)
            {
                using var sr = new StringReader(filepath.text);
                while (await sr.ReadLineAsync() is { } line)
                {
                    var parts = line.Split('=', ':');
                    if (parts.Length == 2)
                    {
                        if (parts[0].ToUpper() == apiURLKey)
                        {
                            apiURL = parts[1].Trim();
                        }
                    }
                }
            }
            filepath = Resources.Load(miragexrFileName) as TextAsset; 
            if (filepath != null)
            {
                using var sr = new StringReader(filepath.text);
                while (await sr.ReadLineAsync() is { } line)
                {
                    var parts = line.Split('=', ':');
                    if (parts.Length == 2)
                    {
                        switch (parts[0].ToUpper())
                        {
                            case usernameKey:
                                username = parts[1].Trim();
                                break;
                            case passwordKey:
                                password = parts[1].Trim();
                                break;
                        }
                    }
                }
            }
            UnityEngine.Debug.LogError(apiURL + username +password);
            if (apiURL == null)
            {
                UnityEngine.Debug.LogError("apiURL is null");
                throw new InvalidOperationException();
            }

            if (apiURL != null)
            {
                apiURL = "http://" + apiURL + apiPort; //@todo Need to be updated!
                token = await AuthenticateUser(apiURL, username, password);
                if (apiURL == null || username == null || password == null || token == null)
                {
                    throw new InvalidOperationException(
                        $"Can't find a parameter for the AI Serves configuration -> " +
                        $"Path = {miragexrFileName}, " +
                        $"API_URL = {apiURL}," +
                        $"Username = {username}," +
                        $"Password = {password}," +
                        $"Token = {token}");
                }
                _config = new AiServicesConfig(apiURL, username, password);
                _config.SetToken(token);
            }
            var options = await GetAvailableModels();
            if (options)
            {
                return _config;
            }
            UnityEngine.Debug.LogError("Configuration of the AI Services failed! Unable to load the options!");
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Authenticates the user and returns a token.
        /// </summary>
        /// <param name="apiURL">URL of the server</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <returns>The authentication token</returns>
        private static async Task<string> AuthenticateUser(string apiURL, string user, string password)
        {
            apiURL += "/authentication/";
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("username", user),
                new MultipartFormDataSection("password", password),
            };
            var request = UnityWebRequest.Post(apiURL, formData);
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }
            var text = request.downloadHandler.text;
            if (text.Length < 13)
            {
                throw new HttpRequestException($"Error while receiving the token! Got ' {text} 'and that is too short!");
            }
            text = text.Substring(10, text.Length - 10 - 2);
            return text;
        }

        /// <summary>
        /// Struct that stores the token for the API.
        /// </summary>
        private readonly struct TokenResponse // still needed? @todo..
        {
            public TokenResponse(string inputToken)
            {
                token = inputToken;
            }

            /// <summary>
            /// Stores the Token for the API.
            /// </summary>
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
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Provides AI services such as listening, thinking, and speaking.
        /// </summary>
        public List<string> models { get; set; }
    }


    public class JsonConfig
    {
        /// <summary>
        /// The name of the property holding the company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Represents the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Represents the URL associated with the Moodle server.
        /// </summary>
        /// <value>
        /// The URL of the Moodle server.
        /// </value>
        public string MoodleUrl { get; set; }

        /// <summary>
        /// Represents the URL for the X API.
        /// </summary>
        public string XApiUrl { get; set; }

        /// <summary>
        /// Provides information about the version of the software.
        /// </summary>
        /// <remarks>
        /// The Version property is a string that represents the version of the software.
        /// It is included in the JsonConfig object, which is used to store the configuration settings for the application.
        /// The value of the Version property can be accessed and modified as needed.
        /// </remarks>
        public string Version { get; set; }

        /// <summary>
        /// Represents the configuration for the splash screen of the application.
        /// </summary>
        public string SplashScreen { get; set; }

        /// *Name**: `Logo`
        /// *Type**: `string`
        /// *Access Modifier**: `public`
        public string Logo { get; set; }

        /// Represents the background color of the splash screen.
        /// This property is a part of the `JsonConfig` class, which is used for configuring various settings of the application.
        /// The `SplashBackgroundColor` property defines the background color of the splash screen displayed when the application starts up.
        /// It can be set to any valid color value.
        /// Example usage:
        /// ```csharp
        /// JsonConfig config = new JsonConfig();
        /// config.SplashBackgroundColor = "#FF0000"; // Set the splash screen background color to red
        /// ```
        /// /
        public string SplashBackgroundColor { get; set; }

        /// <summary>
        /// Represents the primary color property for the application.
        /// </summary>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary color for the application.
        /// </summary>
        /// <value>
        /// The secondary color specified in the JSON configuration file.
        /// </value>
        public string SecondaryColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        public string TextColor { get; set; }

        /// <summary>
        /// The color of the icon.
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// Represents the color configuration for the task station.
        /// </summary>
        public string TaskStationColor { get; set; }

        /// <summary>
        /// The color of the path in the application.
        /// </summary>
        public string PathColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the next path.
        /// </summary>
        /// <value>
        /// The color of the next path.
        /// </value>
        public string NextPathColor { get; set; }

        /// <summary>
        /// Represents a calibration marker for the application.
        /// </summary>
        public string CalibrationMarker { get; set; }

        /// <summary>
        /// Represents a class for working with PDF calibration markers.
        /// </summary>
        public string PdfCalibrationMarker { get; set; }

        /// <summary>
        /// The URL of the AI API.
        /// </summary>
        public string AiApiUrl { get; set; }

        /// <summary>
        /// The username used for authentication with the AI services.
        /// </summary>
        public string AiUsername { get; set; }

        /// <summary>
        /// Represents the password property for the AiServicesConfig class.
        /// </summary>
        public string AiPassword { get; set; }
    }
}
