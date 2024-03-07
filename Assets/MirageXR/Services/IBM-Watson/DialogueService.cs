using i5.Toolkit.Core.VerboseLogging;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using OpenAI_API;
using System.Threading.Tasks;
using MirageXR;
using Model = OpenAI_API.Models.Model;

public enum AIservice
{
    openAI,
    Watson
};

public class DialogueService : MonoBehaviour
{

    [SerializeField] private Text ResponseTextField; // inspector slot for drag & drop of the Canvas > Text gameobject
    private SpeechOutputService dSpeechOutputMgr;
    private SpeechInputService dSpeechInputMgr;

    private UserProfile dUser;
    private ExerciseController dEC;

    private MirageXR.CharacterController _character;
    private static MirageXR.ActivityManager activityManager => MirageXR.RootObject.Instance.activityManager;

    [Space(10)]

    public AIservice AI = AIservice.openAI;

    private OpenAIAPI _openAIinterface;
    private OpenAI_API.Chat.Conversation _chat;
    public string AIprompt = "You are a baker in a small Irish bakery. You will be asked questions about your bakery products. Try and sell them well. You speak only English with a Dublin accent.";

    [Space(10)]

    [Tooltip("The IBM Watson version date with which you would like to use the service in the form YYYY-MM-DD.")]
    [SerializeField]
    private string versionDate = "2019-02-28";//"2021-11-27"
    [Tooltip("The IBM Watson assistant ID to run the example.")]
    [SerializeField]
    private string assistantId = "b392e763-cfde-44c4-b24a-275c92fc4f9b";
    private AssistantService service;

    private DaimonManager dAImgr;
    private string username;

    private bool createSessionTested = false;
    private bool deleteSessionTested = false;
    private string sessionId;

    private APIAuthentication _openIaApiKey; 
    
    public string AssistantID
    {
        get => assistantId;
        set => assistantId = value;
    }

    public void Start()
    {
        LogSystem.InstallDefaultReactors();

        dSpeechOutputMgr = GetComponent<SpeechOutputService>();
        dAImgr = GetComponent<DaimonManager>();
        dUser = GetComponent<UserProfile>();
        dEC = GetComponent<ExerciseController>();

        dSpeechInputMgr = GetComponent<SpeechInputService>();
        dSpeechInputMgr.onInputReceived += OnInputReceived;

        _character = dSpeechOutputMgr.myCharacter.GetComponentInParent<MirageXR.CharacterController>();

        CreateService();
    }

    public void CreateService()
    {
        switch (AI)
        {
            case AIservice.openAI:
                CreateOpenAIServiceAsync().AsAsyncVoid();
                break;
            case AIservice.Watson:
                Runnable.Run(CreateWatsonService());
                break;
            default:
                AppLog.Log($"DialogueService: ERROR: AI service provider {AI} does not exist.", LogLevel.CRITICAL);
                break;
        }
    }

    private static async Task<APIAuthentication> ReadOpenIaAuthKeyAsync()
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

        Debug.Log("openAI keys: " + key + ", org = " + org);

        return new APIAuthentication(key, org);
    }

    private async Task CreateOpenAIServiceAsync()
    {
        try
        {
            _openIaApiKey ??= await ReadOpenIaAuthKeyAsync();
            
            if (!await _openIaApiKey.ValidateAPIKey())
            {
                throw new Exception("can't Validate openAI's api key");
            }

            _openAIinterface = new OpenAIAPI(_openIaApiKey);
            createSessionTested = true;

            AppLog.Log($"DialogueService: connected to openAI with organization ID = '{_openAIinterface.Auth.OpenAIOrganization}", LogLevel.INFO);

            _chat = _openAIinterface.Chat.CreateConversation();
            _chat.Model = Model.ChatGPTTurbo;
            _chat.RequestParameters.Temperature = 0;

            _chat.AppendSystemMessage(AIprompt);
        }
        catch (Exception ex)
        {
            AppLog.Log($"DialogueService: AI provider initialisation failed: {ex.Message}, trace: {ex.StackTrace}", LogLevel.CRITICAL);
            RootView_v2.Instance.dialog.ShowMiddle(
                "Error: connection failed",
                "Could not connect to the AI provider (OpenAI), it seems the API key is missing?",
                "OK", () => AppLog.Log("DialogueService: Connection error acknowledge by user (OK)", LogLevel.INFO));
        }
        
    }

    private IEnumerator CreateWatsonService()
    {
        AppLog.Log("[DialogueService] Switching AI provider to IBM Watson.", LogLevel.INFO);
        service = new AssistantService(versionDate);
        yield return new WaitUntil(service.Authenticator.CanAuthenticate);  //TODO: possible infinite loop
    
        Runnable.Run(WatsonCreateSession());
    }

    private IEnumerator WatsonCreateSession()
    {
        AppLog.Log("DialogueService: Connecting to Watson assistant with id = " + assistantId, LogLevel.INFO);
        service.CreateSession(OnWatsonCreateSession, assistantId);
        while (!createSessionTested)
        {
            yield return null;
        }
    }

    private void OnWatsonDeleteSession(DetailedResponse<object> response, IBMError error)
    {
        deleteSessionTested = true;
    }

    private void OnWatsonCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
    {
        Log.Debug("[DialogueService] OnWatsonCreateSession()", "Session: {0}", response.Result.SessionId);
        sessionId = response.Result.SessionId;
        createSessionTested = true;
    }

    public async Task SendMessageToAssistantAsync(string theText)
    {
        Debug.LogDebug("[DialogueService] Sending transcribed input to " + AI.ToString() + ", text = '" + theText + "'");

        if (createSessionTested)
        {
            Debug.Log("[DialogueService] Existing session available");
            if (AI == AIservice.openAI)
            {
                AppLog.Log("[DialogueService] sending message to chatGPT", LogLevel.INFO);
                AppLog.Log("[DialogueService] sending message to chatGPT = '" + theText + "'", LogLevel.INFO);
                _chat.AppendUserInput(theText);

                AppLog.Log("[DialogueService] starting await", LogLevel.INFO);
                // and get the response
                string response = await _chat.GetResponseFromChatbotAsync();
                AppLog.Log("[DialogueService] returned from await: '" + response + "'", LogLevel.INFO);
                Console.WriteLine(response);

                AppLog.Log("[DialogueService] starting to parse", LogLevel.INFO);
                ParseResponse(response);
            }
            else if (AI == AIservice.Watson)
            {
                service.Message(OnWatsonResponseReceived, assistantId, sessionId, input: new MessageInput()
                { Text = theText, Options = new MessageInputOptions() { ReturnContext = true } });
            }
        }
        else
        {
            Debug.LogWarning("AI service: SendMessageToAssistant(): trying to send message to assistant before session is established.");
        }
    }

    private void NextStep()
    {
        activityManager.ActivateNextAction();
    }

    private void OnWatsonResponseReceived(DetailedResponse<MessageResponse> response, IBMError error)
    {
        //if (response.Result.Output.Generic != null && response.Result.Output.Generic.Count > 0)
        //{
        //    Debug.LogDebug("DialogueService response: " + response.Result.Output.Generic[0].Text);
        //    if (response.Result.Output.Intents.Capacity > 0) Debug.LogDebug("    -> " + response.Result.Output.Intents[0].Intent.ToString());
        //}

        // check if Watson was able to make sense of the user input, otherwise ask to repeat the input
        if (response.Result.Output.Intents == null && response.Result.Output.Actions == null)
        {
            Debug.LogDebug("I did not understand");
            dSpeechOutputMgr.Speak("I don't understand, can you rephrase?");
        } else
        {
            if (response.Result.Output.Intents != null && response.Result.Output.Intents.Count > 0)
            {
                string answerIntent = response.Result.Output.Intents[0].Intent.ToString();

                // only evaluated if intents are used in dialog
                switch (answerIntent)
                {
                    case "S0-GenderMale": // might also be without hashtag? not sure
                        dUser.Gender = UserProfile.gender.male;
                        break;
                    case "S0-GenderFemale":
                        dUser.Gender = UserProfile.gender.female;
                        break;
                    case "S0-Birthyear":
                        dUser.Age = System.DateTime.Now.Year - int.Parse(response.Result.Output.Entities.Find((x) => x.Entity.ToString() == "sys-number").Value);
                        break;
                    case "name":
                        username = response.Result.Output.Entities.Find((x) => x.Entity.ToString() == "sys-person").Value.ToString();
                        Debug.LogDebug("username = " + username);
                        break;
                    default:
                        break;
                }

            } // any intents recognised?

            if (response.Result.Output.Actions != null && response.Result.Output.Actions.Count > 0)
            {
                string actionName = response.Result.Output.Actions[0].Name;
                Debug.LogDebug("Action Name = " + actionName);
                // check whether it is really the intent we want to check
                // (or do we want to know the name of the dialogue step?)
                switch (actionName)
                {
                    case "jump to":
                        Debug.LogTrace("Jump to action recieved");
                        break;
                    default:
                        break;
                }

            } // any action recognised?

            if (response.Result.Output.Generic != null && response.Result.Output.Generic.Capacity > 0)
            {
                try
                {
                    var res = response.Result.Output.Generic[0].Text;

                    if (!string.IsNullOrEmpty(res))
                    {
                        ParseResponse(res);
                    }
                    else
                    {
                        dSpeechOutputMgr.Speak("Sorry, I don't understand.");
                        dAImgr.check = true;
                    }
                }
                catch (NullReferenceException e)
                {
                    dAImgr.check = true;
                    dSpeechOutputMgr.Speak("I don't understand, can you rephrase?");
                    Debug.LogError($"Somthing went wrong but the conversiontion will be continued. The error is:\n {e}");
                }
            }
            else // no Generic response coming back, so say something diplomatic
            {
                dSpeechOutputMgr.Speak("Sorry, I don't understand.");
            }

            // now all data has been extracted, so we can run through the list of exclusions
            UpdateExercises();
        } // Watson did understand the user
    } // end of method OnResponseReceived

    public void UpdateExercises()
    {
        if ((dUser.Weight != 0) && (dUser.Height != 0))
        {
            dUser.bmi = dUser.Weight / (dUser.Height / 100) ^ 2;
            if (dUser.bmi > 30.0f)
            {
                // = person is obese
                // so let's remove the exercises A3.1 B8.1 B8.2,B8.4, C1.1
                dEC.RemoveExercise("A31");
                dEC.RemoveExercise("B81");
                dEC.RemoveExercise("B82");
                dEC.RemoveExercise("B84");
                dEC.RemoveExercise("C11");
            }
        } // user profile contains weight and height
    }

    public void OnInputReceived(string text)
    {
        AppLog.LogWarning($"[Dialogue Service] onInputReceived arrived in DialogueService ='{text}'", LogLevel.INFO);
        ResponseTextField.text = text;
        SendMessageToAssistantAsync(text).AsAsyncVoid();
    }

    public void SetPrompt(string text)
    {

        AppLog.LogInfo("[DialogueService] Received prompt ='" + text + "'");
        // store the prompt
        AIprompt = text;

        // reset the conversation
        if (createSessionTested && AI == AIservice.openAI)
        {
            AppLog.LogInfo("[DialogueService] resetting conversation");
            _chat = _openAIinterface.Chat.CreateConversation();
            _chat.Model = Model.ChatGPTTurbo;
            _chat.RequestParameters.Temperature = 0;
            _chat.AppendSystemMessage(AIprompt); // prompt injection
            AppLog.LogInfo("[DialogueService] conversation reset done");
        }
    }

    private void ParseResponse(string text)
    {
        if (text.Contains("%%charactername%%"))
        {
            var charName = _character.name.Contains(":") ? _character.name.Split(':')[1] : _character.name;
            text = text.Replace("%%charactername%%", charName);
        }
        else if (text.Contains("%%trigger:step="))
        {
            int commandKeyCount = text.Split("%%").Length - 1;

            if (commandKeyCount == 2)
            {
                var keyEnd = text.IndexOf("%%trigger:step=") + "%%trigger:step=".Length;
                var stepNumberEnd = text.LastIndexOf("%%");
                var stepNumber = text.Substring(keyEnd, stepNumberEnd - keyEnd);

                text = text.Replace("%%trigger:step=" + stepNumber + "%%", " ");

                dAImgr.mySpeechInputMgr.Active = false;
                dAImgr.triggerStep = true;

                if (int.TryParse(stepNumber, out int step))
                {
                    dAImgr.triggerStepNo = step;
                }
                else
                {
                    Debug.LogWarning("[DialogueService] Warning: Could not parse step number from %%trigger:step=xx%% control command in the AI response. Check the response format. For example %%trigger:step=2%% will trigger to jump to step 2.");
                }
            }
            else
            {
                Debug.LogWarning("[DialogueService] Warning: AI response contained control sequence '%%' more than twice (" + commandKeyCount.ToString() + " times). The %% can only be used at the begining and end of a comand. Please update the AI response pattern!");
            }
        }
        else if (text.Contains("%%trigger%%"))
        {
            text = text.Replace("%%trigger%%", " ");
            dAImgr.mySpeechInputMgr.Active = false;
            dAImgr.triggerNext = true;
        }

        // Speak out the response after cleaning the response from any control commands
        // exerting workflow control or calling procedural animations.
        dSpeechOutputMgr.Speak(text);
    }

    public void OnDestroy()
    {
        Debug.LogTrace("DialogueService: deregestering callback for speech2text input");
        dSpeechInputMgr.onInputReceived -= OnInputReceived;

        if (AI == AIservice.Watson)
        {
            Debug.LogTrace("DialogueService: Attempting to delete session");
            service.DeleteSession(OnWatsonDeleteSession, assistantId, sessionId);
        }
    }

// end of class
}