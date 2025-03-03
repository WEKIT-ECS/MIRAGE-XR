using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Threads;
using UnityEngine;
using Utilities.WebRequestRest;
using Utilities.WebRequestRest.Interfaces;

namespace MirageXR
{
    public class OpenAIManager  //TODO: add wrapper for all ai tools
    {
        private OpenAIClient _aiClient;
        private readonly Dictionary<string, AssistantResponse> _assistants = new Dictionary<string, AssistantResponse>();
        private readonly Dictionary<string, ThreadResponse> _threads = new Dictionary<string, ThreadResponse>();

        private readonly Func<IServerSentEvent, Task> _streamHandler = (eventData) =>
        {
            // Verarbeite das Event hier.
            Debug.Log($"Stream Event: {eventData}");
            return Task.CompletedTask;
        };

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

            return new OpenAIAuthInfo(key, org);
        }

        public async Task InitializeAsync()
        {
            try
            {
                var keys = await ReadOpenIaAuthKeyAsync();
                _aiClient = new OpenAIClient(new OpenAIAuthentication(keys), new OpenAISettings(new OpenAISettingsInfo()));
            }
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return;
                }

                AppLog.LogError(e.ToString());
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
            }
        }

        public async Task<AssistantResponse> CreateAssistantAsync(string name, string instructions,
            OpenAI.Models.Model model = null, CancellationToken cancellationToken = default)
        {
            try
            {
                model ??= OpenAI.Models.Model.GPT3_5_Turbo;

                var request = new CreateAssistantRequest(model, name, null, instructions);
                var assistant = await _aiClient.AssistantsEndpoint.CreateAssistantAsync(request, cancellationToken);
                _assistants.TryAdd(assistant.Id, assistant);

                return assistant;
            }
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return null;
                }

                AppLog.LogError(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public bool IsAssistantExists(string assistantId)
        {
            return assistantId != null && _assistants.ContainsKey(assistantId);
        }

        public async Task<string> SendMessageToAssistant(string assistantId, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_assistants.ContainsKey(assistantId))
                {
                    AppLog.LogError($"assistantId '{assistantId}' doesn't exists");
                    return null;
                }

                if (!_threads.TryGetValue(assistantId, out var thread))
                {
                    thread = await _aiClient.ThreadsEndpoint.CreateThreadAsync(cancellationToken: cancellationToken);
                    _threads.TryAdd(assistantId, thread);
                }
                var messageResponse = await thread.CreateMessageAsync(new OpenAI.Threads.Message(message), cancellationToken: cancellationToken);
                var runResponse = await _aiClient.ThreadsEndpoint.CreateRunAsync(thread.Id, new CreateRunRequest(assistantId), _streamHandler, cancellationToken);
                runResponse = await runResponse.WaitForStatusChangeAsync(cancellationToken: cancellationToken);
                var messages = await thread.ListMessagesAsync(cancellationToken: cancellationToken);
                var response = messages.Items.FirstOrDefault(t => t.Role == Role.Assistant);
                return response?.PrintContent();
            }
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return null;
                }

                AppLog.LogError(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public async Task<string> GetChatCompletionAsync(string message, string instructions, CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = new List<OpenAI.Chat.Message>
                {
                    new (Role.System, instructions),
                    new (Role.User, message),
                };
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT3_5_Turbo);
                var response = await _aiClient.ChatEndpoint.GetCompletionAsync(chatRequest, cancellationToken);
                return response.FirstChoice.Message;
            }
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return null;
                }

                AppLog.LogError(e.ToString());
                return null;
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
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return null;
                }

                AppLog.LogError(e.ToString());
                return null;
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
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return null;
                }

                AppLog.LogError(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return null;
            }
        }

        public async Task DeleteAssistantAsync(string assistantId, CancellationToken cancellationToken = default)
        {
            if (!_assistants.ContainsKey(assistantId))
            {
                return;
            }

            try
            {
                await _aiClient.AssistantsEndpoint.DeleteAssistantAsync(assistantId, cancellationToken);
                _assistants.Remove(assistantId);
                if (_threads.ContainsKey(assistantId))
                {
                    await _aiClient.ThreadsEndpoint.DeleteThreadAsync(_threads[assistantId].Id, cancellationToken);
                    _threads.Remove(assistantId);
                }
            }
            catch (RestException e)
            {
                if (e.Response.Code == 403)
                {
                    AppLog.LogWarning(e.ToString());
                    return;
                }

                AppLog.LogError(e.ToString());
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
            }
        }
    }
}
