using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using MirageXR.AIManagerDataModel;
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
        /// <param name="audioClip">The audio clip that should be transcribed.</param>
        /// <param name="model">The model that should transcribe the audio.</param>
        /// <param name="url">The server URL.</param>
        /// <param name="token">The user token.</param>
        /// <returns>A string with the result of the task or an error if a network error appears.</returns>
        public static async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, string model, string url,
            string token)
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
            fromData.AddBinaryData("message", bytes, "audio.wav", "audio/wav");

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
        /// Sends a user message to the assistant for processing in an LLM or RAG.
        /// </summary>
        /// <param name="model">The target model used for processing the message.</param>
        /// <param name="message">The message sent by the user for processing.</param>
        /// <param name="context">The context provided by the instructor.</param>
        /// <param name="url">The server URL where the assistant is hosted.</param>
        /// <param name="token">The token required for authorization.</param>
        /// <returns>A string with the result of the operation.</returns>
        public static async Task<string> SendMessageToAssistantAsync(string model, string message, string context,
            string url, string token)
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
        /// Downloads the Configuration from the server and Deserialize the JSON.
        /// </summary>
        /// <param name="url">The server URL to download the options from.</param>
        /// <param name="token">The user token for authentication.</param>
        /// <returns>A list of AIModel objects representing the available options downloaded from the server.</returns>
        public static async Task<List<AIModel>> GetAvailableModelsAsync(string url, string token)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            var apiURL = $"{url}/options/";
            var request = UnityWebRequest.Get(apiURL);
            request.SetRequestHeader("Authorization", $"Token {token}");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"Error while receiving the token: {request.error}!");
            }

            var optionsText = request.downloadHandler.text;

            try
            {
                var myDeserializedClass = JsonConvert.DeserializeObject<List<AIModel>>(optionsText);
                return myDeserializedClass;
            }
            catch (Exception e)
            {
                throw new HttpRequestException("Error fetching or deserializing data: " + e.Message);
            }
        }

        /// <summary>
        /// Creates an audio file containing spoken text based on an input message.
        /// </summary>
        /// <param name="message">The text that you want to turn into an audio</param>
        /// <param name="model">The model that you use. Check options json for legal parameters</param>
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task will return an <see cref="AudioClip"/>.</returns>
        public static async Task<AudioClip> ConvertTextToSpeechAsync(string message, string model, string url,
            string token)
        {
            if (string.IsNullOrEmpty(message))
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
            
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            string base64Message = Convert.ToBase64String(messageBytes);

            var apiURL = $"{url}/speak/";
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(apiURL, AudioType.MPEG);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
            webRequest.SetRequestHeader("message", base64Message);
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
        /// <param name="apiURL">The URL of the server.</param>
        /// <param name="user">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The authentication token.</returns>
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
            try
            {
                var token = JsonConvert.DeserializeObject<Token>(request.downloadHandler.text);
                return token.BackendToken;
            }
            catch (Exception e)
            {
                throw new HttpRequestException("Error fetching or deserializing data: " + e.Message);
            }
        }
    }
}