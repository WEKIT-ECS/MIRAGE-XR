using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK;
using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;
using System;
using System.Linq;
using System;

public class DialogueService : MonoBehaviour
{

    [SerializeField] private Text ResponseTextField; // inspector slot for drag & drop of the Canvas > Text gameobject
    private SpeechOutputService dSpeechOutputMgr;
    private SpeechInputService dSpeechInputMgr;

    private UserProfile dUser;
    private ExerciseController dEC;

    private MirageXR.CharacterController _character;


    [Space(10)]
    // [Tooltip("The IAM apikey.")]
    // [SerializeField]
    // private string iamApikey = "X4udGLROceeDWMxy8aZ85p_AJLghkkwPtzYwF5IN5NVS";
    // [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
    // [SerializeField]
    // private string serviceUrl = "https://gateway-lon.watsonplatform.net/assistant/api";

    [Tooltip("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
    [SerializeField]
    private string versionDate = "2019-02-28";
    [Tooltip("The assistantId to run the example.")]
    [SerializeField]
    private string assistantId = "b392e763-cfde-44c4-b24a-275c92fc4f9b";
    private AssistantService service;
    private DaimonManager dAImgr;

    private string username;

    private bool createSessionTested = false;
    private bool deleteSessionTested = false;
    private string sessionId;

    public string AssistantID
    {
        get
        {
            return assistantId;
        }
        set
        {
            assistantId = value;
        }
    }


    void Start()
    {
        LogSystem.InstallDefaultReactors();

        dSpeechOutputMgr = GetComponent<SpeechOutputService>();
        dAImgr = GetComponent<DaimonManager>();
        dUser = GetComponent<UserProfile>();
        dEC = GetComponent<ExerciseController>();

        dSpeechInputMgr = GetComponent<SpeechInputService>();
        dSpeechInputMgr.onInputReceived += OnInputReceived;

        _character = dSpeechOutputMgr.myCharacter.GetComponentInParent<MirageXR.CharacterController>();

        Runnable.Run(CreateService());

    }

    private IEnumerator CreateService()
    {
        /*
        if (string.IsNullOrEmpty(iamApikey))
        {
            throw new IBMException("Please provide IAM ApiKey for the service.");
        }

        //  Create credential and instantiate service
        Credentials credentials = null;

        //  Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = iamApikey
        };

        credentials = new Credentials(tokenOptions, serviceUrl);

        //  Wait for tokendata
        while (!credentials.HasIamTokenData())
            yield return null;
		*/

        service = new AssistantService(versionDate); // , credentials);

        while (!service.Authenticator.CanAuthenticate()) // .Credentials.HasIamTokenData()
            yield return null;

        Runnable.Run(CreateSession());

        // Runnable.Run(Examples());
    }

    private IEnumerator CreateSession()
    {
        Debug.Log("CONNECTING TO ASSISTANT: " + assistantId);
        service.CreateSession(OnCreateSession, assistantId);

        while (!createSessionTested)
        {
            yield return null;
        }
    }

    private void OnDeleteSession(DetailedResponse<object> response, IBMError error)
    {
        // Log.Debug("ExampleAssistantV2.OnDeleteSession()", "Session deleted.");
        deleteSessionTested = true;
    }


    public void SendMessageToAssistant(string theText)
    {
        Debug.Log("Sending to assistant service: " + theText);
        if (createSessionTested)
        {
            service.Message(OnResponseReceived, assistantId, sessionId, input: new MessageInput()
            {
                Text = theText,
                Options = new MessageInputOptions()
                {
                    ReturnContext = true
                }
            }
            );

        }
        else
        {
            Debug.Log("WARNING: trying to SendMessageToAssistant before session is established.");
        }

    }

    private void MakeDance()
    {
        Debug.LogError(">>> Starting to dance");
        //dAImgr.Animate("Waving");
    }

    private void OnResponseReceived(DetailedResponse<MessageResponse> response, IBMError error)
    {

        if (response.Result.Output.Generic != null && response.Result.Output.Generic.Count > 0)
        {
            Debug.Log("DialogueService response: " + response.Result.Output.Generic[0].Text);
            if (response.Result.Output.Intents.Capacity > 0) Debug.Log("    -> " + response.Result.Output.Intents[0].Intent.ToString());
        }

        // check if Watson was able to make sense of the user input, otherwise ask to repeat the input
        if (response.Result.Output.Intents == null && response.Result.Output.Actions == null)
        {
            Debug.Log("I did not understand");
            dSpeechOutputMgr.Speak("I don't understand, can you rephrase?");

        }
        else
        {

            if (response.Result.Output.Intents != null && response.Result.Output.Intents.Count > 0)
            {
                string answerIntent = response.Result.Output.Intents[0].Intent.ToString();

                switch (answerIntent)
                {
                    case "dancing":
                        MakeDance();
                        break;
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
                        Debug.Log("username = " + username);
                        break;
                    default:
                        break;
                }

            } // any intents recognised?

            if (response.Result.Output.Actions != null && response.Result.Output.Actions.Count > 0)
            {

                string actionName = response.Result.Output.Actions[0].Name;
                // check whether it is really the intent we want to check
                // (or do we want to know the name of the dialogue step?)
                switch (actionName)
                {
                    case "makeWave":
                        dAImgr.Animate("waving");
                        break;
                    default:
                        break;
                }

            } // any action recognised?

            if (response.Result.Output.Generic != null && response.Result.Output.Generic.Capacity > 0)
            {
                // if the name is char:alla only take alla 
                try
                {
                    var res = response.Result.Output.Generic[0].Text;
                    if (!string.IsNullOrEmpty(res) && res.Contains("%%charactername%%"))
                    {
                        var charName = _character.name.Contains(":") ? _character.name.Split(':')[1] : _character.name;
                        res = res.Replace("%%charactername%%", charName);
                    }
                    else
                        dAImgr.check = true;

                    if (string.IsNullOrEmpty(res))
                        dSpeechOutputMgr.Speak("Sorry, I don't understand.");

                    dSpeechOutputMgr.Speak(res); // + ", " + username
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

    //dSpeechInputMgr.Active = false;

    //myTTS.myVoice = "de-DE_DieterV3Voice";
    //myTTS.Speak(myTranslator.lastTranslationResult);
    //myTTS.myVoice = "en-GB_KateV3Voice";


    // Convert resp to fsdata
    //fsData fsdata = null;
    //fsResult r = _serializer.TrySerialize(response.GetType(), response, out fsdata);
    //if (!r.Succeeded)
    //    throw new IBMException(r.FormattedMessages);

    ////  Convert fsdata to MessageResponse
    //MessageResponse messageResponse = new MessageResponse();
    //object obj = messageResponse;
    //r = _serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
    //if (!r.Succeeded)
    //    throw new IBMException(r.FormattedMessages);

    //object _tempContext = null;
    //(resp as Dictionary<string, object>).TryGetValue("context", out _tempContext);
    //if (_tempContext != null)
    //{

    //    _tempContext = _tempContext as Dictionary<string, object>;
    //}
    //else
    //{
    //    Log.Debug("ExampleConversation.Dialogue()", "Failed to get context");
    //}

    ////object tempIntentsObj = null;
    ////(response as Dictionary<string, object>).TryGetValue("intents", out tempIntentsObj);


    //object _tempText = null;
    //object _tempTextObj = (_tempText as List<object>)[0];
    //string output = _tempTextObj.ToString();
    //if (output != null)
    //{
    //    //replace any <waitX> tags with the value expected by the TTS service
    //    string replaceActionTags = output.ToString();
    //    int pos3 = replaceActionTags.IndexOf("<wait3>");
    //    if (pos3 != -1)
    //    {
    //        replaceActionTags = output.Replace("<wait3>", "<break time='3s'/>");
    //    }
    //    int pos4 = replaceActionTags.IndexOf("<wait4>");
    //    if (pos4 != -1)
    //    {
    //        replaceActionTags = output.Replace("<wait4>", "<break time='4s'/>");
    //    }
    //    int pos5 = replaceActionTags.IndexOf("<wait5>");
    //    if (pos5 != -1)
    //    {
    //        replaceActionTags = output.Replace("<wait5>", "<break time='5s'/>");
    //    }
    //    output = replaceActionTags;
    //}
    //else
    //{
    //    Log.Debug("Extract outputText", "Failed to extract outputText and set for speaking");
    //}

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
        //Debug.Log("onInputReceived arrived in DialogueService: '" + text + "'");
        ResponseTextField.text = text;
        SendMessageToAssistant(text);
    }

    public void OnDestroy()
    {

        Debug.Log("DialogueService: deregestering callback for speech2text input");
        dSpeechInputMgr.onInputReceived -= OnInputReceived;

        Debug.Log("DialogueService: Attempting to delete session");
        service.DeleteSession(OnDeleteSession, assistantId, sessionId);

    }

    private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
    {
        Log.Debug("ExampleAssistantV2.OnCreateSession()", "Session: {0}", response.Result.SessionId);
        sessionId = response.Result.SessionId;
        createSessionTested = true;
    }

}
