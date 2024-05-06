using System;
using System.Collections.Generic;
using System.Net.Http;
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
        /// <param name="audioClip"> The audio clip that should be transcribed. </param>
        /// <param name="model"> The model that should transcribe the audio. </param>
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns> A String with the result of the task or an error if a network error appears. </returns>
        public static async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, string model, string url, Token token)
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

            if (string.IsNullOrEmpty(token.BackendToken))
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
        public static async Task<string> SendMessageToAssistantAsync(string model, string message, string context, string url, Token token)
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

            if (string.IsNullOrEmpty(token.BackendToken))
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
                UnityEngine.Debug.LogError(
                    $"Error while receiving the result of the Think endpoint: {webRequest.error}");
            }

            return webRequest.downloadHandler.text;
        }

        /// <summary>
        /// Downloads the available options from the server.
        /// </summary>
        /// <returns>A boolean. True if the operation was successful, false if it wasn't.</returns>
        /// 
        public static async Task<List<AIModel>> GetAvailableModelsAsync(string url, Token token)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null");
            }

            var apiURL = $"{url}/options/";
            var request = UnityWebRequest.Get(apiURL);
            request.SetRequestHeader("Authorization", $"Token {token.BackendToken}");
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
                UnityEngine.Debug.LogError("Error fetching or deserializing data: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get you an audio based on your text
        /// </summary>
        /// <param name="message">The text that you want to turn into an audio</param>
        /// <param name="voice">The voice that you want to use. Check options json for legal parameters </param>
        /// <param name="model">The model that you use. Check options json for legal parameters</param>
        /// <param name="url">server url</param>
        /// <param name="token">user token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task will return an <see cref="AudioClip"/>.</returns>
        public static async Task<AudioClip> ConvertTextToSpeechAsync(string message, string model, string url, Token token)
        {

            if (string.IsNullOrEmpty(message))
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

            if (string.IsNullOrEmpty(token.BackendToken))
            {
                throw new ArgumentException("token is null");
            }

            var apiURL = $"{url}/speak/";
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(apiURL, AudioType.MPEG);
            webRequest.SetRequestHeader("Authorization", $"Token {token.BackendToken}");
            webRequest.SetRequestHeader("message", message);
            webRequest.SetRequestHeader("model", model);
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"Error while receiving the result of the Speak endpoint: {webRequest.error} {webRequest.result}");
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
        public static async Task<Token> AuthenticateUserAsync(string apiURL, string user, string password)
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
                UnityEngine.Debug.LogError($"Error while receiving the token: {request.error}!");
            }

            var text = request.downloadHandler.text;
            if (text.Length < 13)
            {
                UnityEngine.Debug.LogError($"Error while receiving the token! Got '{text}' and that is too short!");
            }

            text = text.Substring(10, text.Length - 10 - 2);
            Token token = new Token(text);
            return token;
        }
    }
}