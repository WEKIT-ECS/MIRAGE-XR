using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    /// <summary>
    /// Provides AI services such as transcription, conversation, and speech synthesis.
    /// </summary>
    public static class AiServices
    {
        /// <summary>
        /// Provides multiple Transcription models.
        /// </summary>
        /// <param name="audioClip"> The audio clip that should be transcribed. </param>
        /// <param name="model"> The model that should transcribe the audio. </param>
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns> A String with the result of the task or an error if a network error appears. </returns>
        public static async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, string model, string url, string token)
        {
            if (audioClip == null)
            {
                throw new ArgumentException("audioClip is null");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("model is null");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null");
            }

            var apiURL = $"{url}/listen/";
            var bytes = SaveLoadAudioUtilities.AudioClipToByteArray(audioClip);
            var fromData = new WWWForm();
            fromData.AddField("model", model);
            fromData.AddBinaryData("message", bytes);

            using var webRequest = UnityWebRequest.Post(apiURL, fromData);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
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
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns>A String with the result of the operation.</returns>
        public static async Task<string> SendMessageToAssistantAsync(string model, string message, string context, string url, string token)
        {
            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("message is null");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("model is null");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null");
            }

            var apiURL = $"{url}/think/";
            var fromData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("model", model),
                new MultipartFormDataSection("message", message),
                new MultipartFormDataSection("context", context),
            };
            using var webRequest = UnityWebRequest.Post(apiURL, fromData);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException(
                    $"Error while receiving the result of the Think endpoint: {webRequest.error}");
            }

            return webRequest.downloadHandler.text;
        }

        /// <summary>
        /// Downloads the available options from the server.
        /// </summary>
        /// <returns>A boolean. True if the operation was successful, false if it wasn't.</returns>
        /// 
        public static async Task<List<OptionsResponse>> GetAvailableModelsAsync(string url, string token)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null");
            }

            var apiURL = $"{url}/options/";
            var request = UnityWebRequest.Get(apiURL);
            request.SetRequestHeader("Authorization", $"Token {token}");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }

            var optionsText = request.downloadHandler.text;
            var myDeserializedClass = JsonConvert.DeserializeObject<List<OptionsResponse>>(optionsText);
            return myDeserializedClass;
        }

        /// <summary>
        /// Get you an audio based on your text
        /// </summary>
        /// <param name="speakOut">The text that you want to turn into an audio</param>
        /// <param name="voice">The voice that you want to use. Check options json for legal parameters </param>
        /// <param name="model">The model that you use. Check options json for legal parameters</param>
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task will return an <see cref="AudioClip"/>.</returns>
        public static async Task<AudioClip> ConvertTextToSpeechAsync(string speakOut, string voice, string model, string url, string token)
        {
            if (string.IsNullOrEmpty(voice))
            {
                throw new ArgumentException("voice is null");
            }

            if (string.IsNullOrEmpty(speakOut))
            {
                throw new ArgumentException("speakOut is null");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("model is null");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null");
            }

            var apiURL = $"{url}/speak/";
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(apiURL, AudioType.MPEG);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
            webRequest.SetRequestHeader("message", speakOut);
            webRequest.SetRequestHeader("submodel", voice);
            webRequest.SetRequestHeader("model", model);
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the result of the Speak endpoint: {webRequest.error} {webRequest.result}");
            }

            var audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            return audioClip;
        }

        /// <summary>
        /// Authenticates the user and returns a token.
        /// </summary>
        /// <param name="apiURL">URL of the server</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <returns>The authentication token</returns>
        public static async Task<string> AuthenticateUserAsync(string apiURL, string user, string password)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("username", user),
                new MultipartFormDataSection("password", password),
            };

            var request = UnityWebRequest.Post($"{apiURL}/authentication/", formData);
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException($"Error while receiving the token: {request.error}!");
            }

            var text = request.downloadHandler.text;
            if (text.Length < 13)
            {
                throw new HttpRequestException($"Error while receiving the token! Got '{text}' and that is too short!");
            }

            text = text.Substring(10, text.Length - 10 - 2);
            return text;
        }
    }

    public class OptionsResponse
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Provides AI services such as listening, thinking, and speaking.
        /// </summary>
        public List<string> Models { get; set; }
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
    }
}