﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR.AIManagerDataModel;
using UnityEngine;

namespace MirageXR
{
    public class AIManager
    {
        private string _url;
        private string _username;
        private string _password;

        private Token _token;

        //private List<OptionsResponse> _models;
        private List<AIModel> _llmModels = new();
        private List<AIModel> _sttModels = new();
        private List<AIModel> _ttsModels = new();


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

        public List<AIModel> GetLLMModels()
        {
            return _llmModels;
        }
        
        public List<AIModel> GetSTTModels()
        {
            return _sttModels;
        }
        
        public List<AIModel> GetTTSModels()
        {
            return _ttsModels;
        }

        private void SetModels(List<AIModel> endpointModels)
        {
            
            foreach (var model in endpointModels)
            {
                AddModelBasedOnEndpointName(model);
                UnityEngine.Debug.Log("Model "+model);
            }
            UnityEngine.Debug.Log("Set models done! LLM:"+_llmModels.Count+" TTS:"+_ttsModels.Count+ "STT:"+_sttModels.Count );

        }

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

        /** Do we need that one?
         *  See GetLLMModels, GetSTTModels and GetTTSModels.
         * public async Task<List<OptionsResponse>> GetModelsAsync()
        
        {
            try
            {
                _models ??= await AiServices.GetAvailableModelsAsync(_url, _token);
                return _models;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }
         */
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