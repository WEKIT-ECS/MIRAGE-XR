using LearningExperienceEngine;
using System;
using UnityEngine;
using MirageXR;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.ExperienceAPI;
using Newtonsoft.Json.Linq;
using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.VerboseLogging;

namespace MirageXR
{
    /// <summary>
    /// Service that handles the connection to the xAPI
    /// </summary>
    public class ExperienceService : IService
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private readonly Actor anonymousActor = new Actor("anonymous@wekit-ecs.com", "An Anonymous Actor");
        private readonly string mirageIRIroot = "https://wekit-ecs.com";

        private ExperienceAPIClient xAPIClient;

        /// <summary>
        /// Creates a new service and populates it with the given xAPI client
        /// </summary>
        /// <param name="xAPIClient">The set up client which will be used for accessing the xAPI</param>
        public ExperienceService(ExperienceAPIClient xAPIClient)
        {
            this.xAPIClient = xAPIClient;
        }

        /// <summary>
        /// Initializes the service
        /// Subscribes to the events that are of interest for xAPI logging
        /// </summary>
        /// <param name="owner">The service manager which administers this service</param>
        public void Initialize(IServiceManager owner)
        {
            // Register to lib-lee events.
            LearningExperienceEngine.EventManager.OnToggleObject += OnToggleObject; // true ? predicate : null
            LearningExperienceEngine.EventManager.OnActivityLoadedStamp += ActivityLoadedStamp; // launch
            LearningExperienceEngine.EventManager.OnCompletedMeasurement += CompletedMeasurement;
            LearningExperienceEngine.EventManager.OnStepActivatedStamp += StepActivatedStamp; // start
            LearningExperienceEngine.EventManager.OnStepDeactivatedStamp += StepDeactivatedStamp; // experienced

            // Register to view events
            LearningExperienceEngine.EventManager.OnActivityCompletedStamp += ActivityCompletedStamp; // completd

        }

        /// <summary>
        /// Cleans up the service
        /// Unsubscribes from all subscribed events
        /// </summary>
        public void Cleanup()
        {
            // Deregister from event manager events.
            LearningExperienceEngine.EventManager.OnToggleObject -= OnToggleObject;
            LearningExperienceEngine.EventManager.OnActivityLoadedStamp -= ActivityLoadedStamp;
            LearningExperienceEngine.EventManager.OnCompletedMeasurement -= CompletedMeasurement;
            LearningExperienceEngine.EventManager.OnStepActivatedStamp -= StepActivatedStamp;
            LearningExperienceEngine.EventManager.OnStepDeactivatedStamp -= StepDeactivatedStamp;

            // Deregister from view events.
            LearningExperienceEngine.EventManager.OnActivityCompletedStamp -= ActivityCompletedStamp;
        }

        // !!!!!!! This never gets called !!!!!!!!!
        //private async void StartActivity()
        //{
        //    Statement statement = GenerateStatement("http://adlnet.gov/expapi/verbs/launched", "http://competenceanalytics.com/ActivityName="); // + activityManager.Activity.name);
        //    await xAPIClient.SendStatementAsync(statement);
        //}

        /// <summary>
        /// Called if an augmentation object is toggled.
        /// </summary>
        /// <param name="act">The augmentation that is toggled</param>
        /// <param name="isActivating">Tells you whether the augmentation is getting activated or deactivated</param>
        private async void OnToggleObject(LearningExperienceEngine.ToggleObject act, bool isActivating)
        {
            if (isActivating && act.predicate != null)
            {
                // some data contain empty url for hands which produces errors
                // TODO: check if this is a bug in the data creation
                if (!string.IsNullOrEmpty(act.url))
                {
                    Verb verb = null;
                    XApiObject obj = null;

                    string predicate = act.predicate;
                    string[] predicateParts = predicate.Split(':');

                    // See if predicate conforms to "type:name" format
                    // >1 because there should be at least two, e.g. "act(1):something(2)"
                    if (predicateParts.Length > 1)
                    {
                        string augmentationType = predicateParts[0];
                        string augmentationName = predicateParts[1];

                        // Glyph augmentation type
                        if (augmentationType.Equals("act"))
                        {
                            // TODO: Add augmentation statements
                            switch (augmentationName)
                            {
                                case string str when str.Contains("Allow"):
                                    break;
                                case string str when str.Contains("Deny"):
                                    break;
                                case string str when str.Contains("Highlight"):
                                    verb = new Verb("http://id.tincanapi.com/verb/focused");
                                    verb.displayLanguageDictionary.Add("en-us", "focused_on");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("OpenBox"):
                                    verb = new Verb("http://activitystrea.ms/schema/1.0/open");
                                    verb.displayLanguageDictionary.Add("en-us", "opened");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("CloseBox"):
                                    verb = new Verb("http://activitystrea.ms/schema/1.0/close");
                                    verb.displayLanguageDictionary.Add("en-us", "closed");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Pack"):
                                    verb = new Verb(createVerbIRI("packed"));
                                    verb.displayLanguageDictionary.Add("en-us", "packed");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Unpack"):
                                    verb = new Verb(createVerbIRI("unpacked"));
                                    verb.displayLanguageDictionary.Add("en-us", "unpacked");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Pick"):
                                    verb = new Verb(createVerbIRI("picked"));
                                    verb.displayLanguageDictionary.Add("en-us", "picked_up");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Place"):
                                    verb = new Verb(createVerbIRI("placed"));
                                    verb.displayLanguageDictionary.Add("en-us", "placed");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Screw"):
                                    verb = new Verb(createVerbIRI("screwed"));
                                    verb.displayLanguageDictionary.Add("en-us", "screwed_in");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Unscrew"):
                                    verb = new Verb(createVerbIRI("unscrewed"));
                                    verb.displayLanguageDictionary.Add("en-us", "unscrewed");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Rotate"):
                                    verb = new Verb(createVerbIRI("rotated"));
                                    verb.displayLanguageDictionary.Add("en-us", "rotated");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Lower"):
                                    verb = new Verb(createVerbIRI("lowered"));
                                    verb.displayLanguageDictionary.Add("en-us", "lowered");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Locate"):
                                    verb = new Verb(createVerbIRI("located"));
                                    verb.displayLanguageDictionary.Add("en-us", "located");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Lubricate"):
                                    verb = new Verb(createVerbIRI("lubricated"));
                                    verb.displayLanguageDictionary.Add("en-us", "lubricated");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Measure"):
                                    // There is a dedicated event for this (CompletedMeasurement)
                                    break;
                                case string str when str.Contains("Paint"):
                                    verb = new Verb(createVerbIRI("painted"));
                                    verb.displayLanguageDictionary.Add("en-us", "painted");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Point"):
                                    verb = new Verb("http://id.tincanapi.com/verb/focused");
                                    verb.displayLanguageDictionary.Add("en-us", "focused_on");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Plug"):
                                    verb = new Verb(createVerbIRI("plugged"));
                                    verb.displayLanguageDictionary.Add("en-us", "plugged_in");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Unplug"):
                                    verb = new Verb(createVerbIRI("unplugged"));
                                    verb.displayLanguageDictionary.Add("en-us", "unplugged");
                                    obj = new XApiObject(act.url);
                                    break;
                                case string str when str.Contains("Unfasten"):
                                    verb = new Verb(createVerbIRI("unfastened"));
                                    verb.displayLanguageDictionary.Add("en-us", "unfastened");
                                    obj = new XApiObject(act.url);
                                    break;
                            }
                        }
                        // Visual effect type
                        else if (augmentationType.Equals("vfx"))
                        {
                            verb = new Verb(createVerbIRI("noticed"));
                            verb.displayLanguageDictionary.Add("en-us", "noticed");
                            obj = new XApiObject(act.url);
                        }
                        // Character model type
                        else if (augmentationType.Equals("char"))
                        {
                            verb = new Verb(createVerbIRI("met"));
                            verb.displayLanguageDictionary.Add("en-us", "met");
                            obj = new XApiObject(act.url);
                        }
                        else if (augmentationType.Contains("model"))
                        {
                            verb = new Verb("http://id.tincanapi.com/verb/viewed");
                            obj = new XApiObject(act.url);
                        }
                        else if (augmentationType.Equals("plugin"))
                        {
                            // TODO: Add plugin statements
                        }
                    }
                    // If the predicate is just one word
                    else
                    {
                        switch (predicate)
                        {
                            // Handle label type.
                            case "label":
                                verb = new Verb("http://id.tincanapi.com/verb/viewed");
                                obj = new XApiObject(act.url);
                                break;

                            // Handle detect type.
                            case "detect":
                                verb = new Verb("http://activitystrea.ms/schema/1.0/find");
                                verb.displayLanguageDictionary.Add("en-us", "found");
                                obj = new XApiObject(act.url);
                                break;

                            // Audio type.
                            case "audio":
                            case "sound":
                                verb = new Verb("http://activitystrea.ms/schema/1.0/listen");
                                verb.displayLanguageDictionary.Add("en-us", "listened_to");
                                obj = new XApiObject(act.url);
                                break;

                            // Video type.
                            case "video":
                                verb = new Verb("http://activitystrea.ms/schema/1.0/watch");
                                verb.displayLanguageDictionary.Add("en-us", "watched");
                                obj = new XApiObject(act.url);
                                break;

                            // Post it label type.
                            case "postit":
                                break;

                            // The frigging 3D model crap type.
                            case "3dmodel":
                            case "model":
                                verb = new Verb("http://id.tincanapi.com/verb/viewed");
                                obj = new XApiObject(act.url);
                                break;

                            // Ghost tracks type.
                            case "ghosttracks":
                                verb = new Verb("http://activitystrea.ms/schema/1.0/follow");
                                verb.displayLanguageDictionary.Add("en-us", "followed");
                                obj = new XApiObject(act.url);
                                break;

                            // Ghost hands type.
                            case "hands":
                                verb = new Verb("http://activitystrea.ms/schema/1.0/watch");
                                verb.displayLanguageDictionary.Add("en-us", "watched");
                                obj = new XApiObject(act.url);
                                break;

                            // Image type.
                            case "image":
                                verb = new Verb("http://id.tincanapi.com/verb/viewed");
                                obj = new XApiObject(act.url);
                                break;

                            // Ignored
                            case "potentiometer":
                            case "menutoggle":
                            case "filterIn":
                            case "filterOut":
                            case "":
                                break; // Ignore end
                            default:
                                // Log predicate name
                                verb = new Verb(mirageIRIroot + "/ActionPredicate=" + act.predicate);
                                obj = new XApiObject(mirageIRIroot + "/ActionID=" + act.id);
                                break;
                        }
                    }

                    if (verb != null && obj != null)
                    {
                        // Add object name and create statement
                        obj.AddName(act.text);
                        Statement statement = GenerateStatement(verb, obj);

                        // Add context activity
                        Context context = new Context();
                        string parentActivityIRI = activityManager.AbsoluteURL;
                        if (parentActivityIRI == null)
                        {
                            parentActivityIRI = createActivityIRI(activityManager.Activity.id);
                        }
                        context.AddParentActivity(parentActivityIRI);
                        statement.context = context;

                        WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
                        if (resp.Code >= 400)
                        {
                            AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"URL field of poi {act.poi} in task station {act.id} is empty");
                }
            }
        }

        private async void CompletedMeasurement(string measurementValue, string measuringTool)
        {
            Verb verb = new Verb("https://w3id.org/xapi/dod-isd/verbs/measured");
            XApiObject obj = new XApiObject(mirageIRIroot + "/objectID/tool/" + measuringTool);
            obj.AddName(measuringTool);
            Statement statement = GenerateStatement(verb, obj);

            Result result = new Result();
            string measurementIRI = mirageIRIroot + "/result/extension/measuredValue";
            result.AddMeasurementAttempt(measurementIRI, measurementValue);
            statement.result = result;

            //await xAPIClient.SendStatementAsync(statement);
            WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
            if (resp.Code >= 400)
            {
                AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
            }
        }

        // called if an activity is completed
        // sends an xAPI statement that indicates the activity completion
        private async void ActivityCompletedStamp(string deviceID, string activityID, string timestamp)
        {
            Verb verb = new Verb("http://activitystrea.ms/schema/1.0/complete");
            verb.displayLanguageDictionary.Add("en-us", "completed");
            XApiObject obj = new XApiObject(mirageIRIroot + "/ActivityID=" + activityManager.Activity.id);
            obj.AddName(activityManager.Activity.name);
            Statement statement = GenerateStatement(verb, obj);

            //await xAPIClient.SendStatementAsync(statement);
            WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
            if (resp.Code >= 400)
            {
                AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
            }
        }

        // called if an activity is loaded
        // sends an xAPI statement that indicates that an activity was loaded
        private async void ActivityLoadedStamp(string deviceID, string activityID, string stamp)
        {
            Verb verb = new Verb("http://adlnet.gov/expapi/verbs/initialized");
            XApiObject obj = new XApiObject(mirageIRIroot + "/ActivityID=" + activityManager.Activity.id);
            obj.AddName(activityManager.Activity.name);
            Statement statement = GenerateStatement(verb, obj);

            //await xAPIClient.SendStatementAsync(statement);
            WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
            if (resp.Code >= 400)
            {
                AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
            }
        }

        // called if an action step is activated
        // sends an xAPI statement that indicates the step activation
        private async void StepActivatedStamp(string deviceID, LearningExperienceEngine.Action activatedAction, string stamp)
        {
            Verb verb = new Verb("http://activitystrea.ms/schema/1.0/start");
            XApiObject obj = new XApiObject(mirageIRIroot + "/stepID=" + activatedAction.id);
            Statement statement = GenerateStatement(verb, obj);

            verb.displayLanguageDictionary.Add("en-us", "started");
            // Add action step title as object name
            obj.AddName(activatedAction.instruction.title);

            // Add context activity
            Context context = new Context();
            string parentActivityIRI = activityManager.AbsoluteURL;
            if (parentActivityIRI == null)
            {
                parentActivityIRI = createActivityIRI(activityManager.Activity.id);
            }
            context.AddParentActivity(parentActivityIRI);
            statement.context = context;

            //await xAPIClient.SendStatementAsync(statement);
            WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
            if (resp.Code >= 400)
            {
                AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
            }
        }

        // called if an action step is deactivated
        // sends an xAPI statement that indicates the step deactivation
        private async void StepDeactivatedStamp(string deviceID, LearningExperienceEngine.Action deactivatedAction, string stamp)
        {
            Verb verb = new Verb("http://activitystrea.ms/schema/1.0/experience");
            XApiObject obj = new XApiObject(mirageIRIroot + "/stepID=" + deactivatedAction.id);
            Statement statement = GenerateStatement(verb, obj);

            verb.displayLanguageDictionary.Add("en-us", "experienced");
            // Add action step title as object name
            obj.AddName(deactivatedAction.instruction.title);

            // Add context activity
            Context context = new Context();
            string parentActivityIRI = activityManager.AbsoluteURL;
            if (parentActivityIRI == null)
            {
                parentActivityIRI = createActivityIRI(activityManager.Activity.id);
            }
            context.AddParentActivity(parentActivityIRI);
            statement.context = context;

            //await xAPIClient.SendStatementAsync(statement);
            WebResponse<string> resp = await xAPIClient.SendStatementAsync(statement);
            if (resp.Code >= 400)
            {
                AppLog.LogError("[ExperienceService] xAPI endpoint reports error in response to the statement sent: " + resp.ErrorMessage + ", xAPI endpoint: " + xAPIClient.XApiEndpoint);
            }
        }


        private Statement GenerateStatement(string verbID, string objectID)
        {
            Actor statementActor = ProduceActor();
            Verb statementVerb = new Verb(verbID);
            XApiObject statementObject = new XApiObject(objectID);
            Statement retVal = new Statement(statementActor, statementVerb, statementObject);
            retVal.timestamp = DateTime.UtcNow;

            return retVal;
        }

        private Statement GenerateStatement(Verb verb, XApiObject obj)
        {
            Actor statementActor = ProduceActor();
            Statement retVal = new Statement(statementActor, verb, obj);
            retVal.timestamp = DateTime.UtcNow;

            return retVal;
        }

        /// <summary>
        /// A helper method to generate an xAPI Actor.
        /// </summary>
        /// <returns>The Actor based on the currently logged in user or the anonymous Actor.</returns>
        private Actor ProduceActor()
        {
            if (UserSettings.LoggedIn)
            {
                if (!string.IsNullOrEmpty(UserSettings.usermail))
                {
                    Actor retVal = new Actor(UserSettings.usermail);
                    if (!string.IsNullOrEmpty(UserSettings.username))
                    {
                        retVal.name = UserSettings.username;
                    }
                    return retVal;
                }
            }
            return anonymousActor;
        }

        /// <summary>
        /// Creates a custom IRI for activities to be used in xAPI Statement Context.
        /// To be used only if a direct URL from ARLEM isn't available.
        /// </summary>
        /// <param name="activityID">The activity ID (session ID).</param>
        /// <returns>Returns the generated IRI</returns>
        private string createActivityIRI(string activityID)
        {
            return mirageIRIroot + "/context/ActivityID=" + activityID;
        }

        private string createVerbIRI(string verbID)
        {
            return mirageIRIroot + "/verb/" + verbID;
        }
    }
}
