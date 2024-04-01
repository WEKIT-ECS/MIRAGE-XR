using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using UnityEngine;

namespace MirageXR
{
    public class OpenAIManager  //TODO: add wrapper for all ai tools
    {
        private OpenAIClient _aiClient;

        private static async Task<OpenAIAuthInfo> ReadOpenIaAuthKeyAsync()
        {
            const string openaiFileName = "openai";
            const string openaiKey = "OPENAI_KEY";
            const string openaiApiKey = "OPENAI_API_KEY";
            const string openaiOrganizationKey = "OPENAI_ORGANIZATION";

            var openai = Resources.Load(openaiFileName) as TextAsset;
            string key = null;
            string org = null;
            if (openai != null)
            {
                using var sr = new StringReader(openai.text);
                while (await sr.ReadLineAsync() is { } line)
                {
                    var parts = line.Split('=', ':');
                    if (parts.Length == 2)
                    {
                        switch (parts[0].ToUpper())
                        {
                            case openaiKey:
                                key = parts[1].Trim();
                                break;
                            case openaiApiKey:
                                key = parts[1].Trim();
                                break;
                            case openaiOrganizationKey:
                                org = parts[1].Trim();
                                break;
                        }
                    }
                }
            }

            if (key == null || org == null)
            {
                throw new Exception("can't get openAI's api key");
            }

            Debug.Log($"openAI keys: {key}, org = {org}");

            return new OpenAIAuthInfo(key, org);
        }

        public async Task InitializeAsync()
        {
            var keys = await ReadOpenIaAuthKeyAsync();
            _aiClient = new OpenAIClient(new OpenAIAuthentication(keys));
        }

        public async Task<string> SetChatPromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = new List<OpenAI.Chat.Message> { new (Role.System, prompt) };
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT3_5_Turbo);
                var response = await _aiClient.ChatEndpoint.GetCompletionAsync(chatRequest, cancellationToken);
                return response.FirstChoice.Message;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public async Task<string> GetChatCompletionAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = new List<OpenAI.Chat.Message> { new (Role.User, message) };
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT3_5_Turbo);
                var response = await _aiClient.ChatEndpoint.GetCompletionAsync(chatRequest, cancellationToken);
                return response.FirstChoice.Message;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public async Task<AudioClip> ConvertTextToSpeechAsync(string message, SpeechVoice speechVoice = SpeechVoice.Nova, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new SpeechRequest(message, null, speechVoice);
                var (_, clip) = await _aiClient.AudioEndpoint.CreateSpeechAsync(request, cancellationToken); //TODO: delete cashed audio files
                return clip;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public async Task<string> ConvertSpeechToTextAsync(AudioClip audioClip, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new AudioTranscriptionRequest(audioClip);
                return await _aiClient.AudioEndpoint.CreateTranscriptionTextAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }
    }
}
