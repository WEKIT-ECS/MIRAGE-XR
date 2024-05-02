using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public class AIManager
    {
        private string _url;
        private string _username;
        private string _password;
        private string _token;
        private List<OptionsResponse> _models;

        public async Task InitializeAsync()
        {
            try
            {
                await ReadConfig();
                _token = await AiServices.AuthenticateUserAsync(_url, _username, _password);
                _models = await AiServices.GetAvailableModelsAsync(_url, _token);
            }
            catch (Exception e)
            {
                AppLog.LogWarning(e.ToString());
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

        public async Task<List<OptionsResponse>> GetModelsAsync()
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

        public async Task<AudioClip> ConvertTextToSpeechAsync(string speakOut, string voice, string model)
        {
            try
            {
                return await AiServices.ConvertTextToSpeechAsync(speakOut, voice, model, _url, _token);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }
    }
}