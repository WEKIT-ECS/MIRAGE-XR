using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR.AIManagerDataModel;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// The AIManager class is responsible for managing AI services such as speech recognition, natural language processing, and text-to-speech conversion.
    /// </summary>
    public class AIManager
    {
        /// <summary>
        /// The URL of the AI API server.
        /// </summary>
        private string _url;

        /// <summary>
        /// The username used for authentication in the AI manager.
        /// </summary>
        private string _username;
        private string _password;

        /// <summary>
        /// Represents a token used for authentication in the AIManager class.
        /// </summary>
        private string _token; 

        /// <summary>
        /// Represents the list of LLM and RAG models available for the AI Manager.
        /// </summary>
        private List<AIModel> _llmModels = new();
        /// <summary>
        /// Represents the list of speech-to-text models available for the AIManager.
        /// </summary>
        private List<AIModel> _sttModels = new();

        /// <summary>
        /// Represents the list of text-to-speech (TTS) models available for conversion.
        /// </summary>
        private List<AIModel> _ttsModels = new();


        /// <summary>
        /// Initializes the AIManager by reading configuration settings, authenticating the user, and setting available models.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                await ReadConfig();
                _token = await AiServices.AuthenticateUserAsync(_url, _username, _password);
                SetModels(await AiServices.GetAvailableModelsAsync(_url, _token));
            }
            catch (Exception e)
            {
                AppLog.LogWarning(e.ToString());
            }
        }

        /// <summary>
        /// Returns a list of LLM models available in the AIManager.
        /// </summary>
        /// <returns>A list of AIModel objects representing the LLM models.</returns>
        public List<AIModel> GetLlmModels()
        {
            return _llmModels;
        }

        /// <summary>
        /// Returns a list of STT models available for use.
        /// </summary>
        /// <returns>A list of AIModel objects representing the available STT models</returns>
        public List<AIModel> GetSttModels()
        {
            return _sttModels;
        }

        /// <summary>
        /// Retrieves the list of TTS models.
        /// </summary>
        /// <returns>The list of TTS models available in the AI manager.</returns>
        public List<AIModel> GetTtsModels()
        {
            return _ttsModels;
        }

        /// <summary>
        /// Sets the AI models based on the provided list of endpoint models.
        /// </summary>
        /// <param name="endpointModels">The list of endpoint models.</param>
        private void SetModels(List<AIModel> endpointModels)
        {
            foreach (var model in endpointModels)
            {
                AddModelBasedOnEndpointName(model);
            }
        }

        /// <summary>
        /// Adds the given AIModel to the appropriate list based on the endpoint name.
        /// </summary>
        /// <param name="model">The AIModel to be added.</param>
        private void AddModelBasedOnEndpointName(AIModel model)
        {
            if (model.EndpointName == "listen/")
            {
                _sttModels.Add(model);
            }
            else if (model.EndpointName == "speak/")
            {
                _ttsModels.Add(model);
            }
            else if (model.EndpointName == "think/")
            {
                _llmModels.Add(model);
            }
        }

        /// <summary>
        /// Reads the configuration file and initializes the AIManager.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ReadConfig()
        {
            const string fileName = "AI_Server";
            const string apiURLKey = "AI_API_URL";
            const string usernameKey = "AI_USERNAME";
            const string passwordKey = "AI_PASSWORD";

            string url = null;
            string username = null;
            string password = null;

            var filepath = Resources.Load(fileName) as TextAsset;
            if (filepath == null)
            {
                throw new Exception($"Failed to load config file: {fileName}");
            }

            using var sr = new StringReader(filepath.text);
            while (await sr.ReadLineAsync() is { } line)
            {
                var parts = line.Split('=');

                switch (parts[0].ToUpper())
                {
                    case usernameKey:
                        username = parts[1].Trim();
                        break;
                    case passwordKey:
                        password = parts[1].Trim();
                        break;
                    case apiURLKey:
                        url = parts[1].Trim();
                        break;
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("can't read url");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new Exception("can't read username");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("can't read password");
            }

            _url = url;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Converts speech to text using the specified audio clip and model.
        /// </summary>
        /// <param name="audioClip">The audio clip that should be transcribed.</param>
        /// <param name="model">The model that should transcribe the audio.</param>
        /// <returns>A string with the transcribed text or null if an error occurs.</returns>
        public async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, string model)
        {
            try
            {
                return await AiServices.ConvertSpeechToTextAsync(audioClip, model, _url, _token);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Sends a message to the AI assistant for processing.
        /// </summary>
        /// <param name="model">The target model for processing the message.</param>
        /// <param name="message">The message provided by the user.</param>
        /// <param name="context">The context provided by the instructor.</param>
        /// <returns>A task representing the asynchronous operation. The result is a string with the result of the operation.</returns>
        public async Task<string> SendMessageToAssistantAsync(string model, string message, string context)
        {
            try
            {
                return await AiServices.SendMessageToAssistantAsync(model, message, context, _url, _token);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Converts a text message to speech using the specified model.
        /// </summary>
        /// <param name="message">The text message to be converted to speech.</param>
        /// <param name="model">The model to be used for text-to-speech conversion.</param>
        /// <returns>An async task that represents the asynchronous operation. The task result contains the audio clip representing the converted speech.</returns>
        public async Task<AudioClip> ConvertTextToSpeechAsync(string message, string model)
        {
            try
            {
                return await AiServices.ConvertTextToSpeechAsync(message, model, _url, _token);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }
    }
}