using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using Sentry;
using System;

namespace MirageXR
{
   /// <summary>
   /// Class <c>ExceptionManager</c> registers for all serious exceptions and opens a user dialogue 
   /// to ask to send a report to sentry.io; the main exceptions registered for are:
   /// Null Reference, Divide by Zero, Out of Memory, Index Out of Range.
   /// </summary>
   public class ExceptionManager : MonoBehaviour
   {
        //[SerializeField] private SentrySdk sentry; 
        //private const string SENTRY_DSN = "https://b23911205078e7a81bf1489e8aa0fabe@o4506320008118272.ingest.sentry.io/4506320009428992";
        // test account for exception manager testing: https://d453859222cb87c3b20ffb6813e10801@o4506874936098816.ingest.us.sentry.io/4506874937147392

        /// <summary>
        /// Initialize takes care of setting up the hook for receiving log messages.
        /// </summary>
        public void Initialize()
       {
            //sentry ??= new GameObject("ExceptionManagerSentry").AddComponent<SentrySdk>();
            //sentry.Dsn = SENTRY_DSN;
            //SentrySdk.Debug = false;
            Debug.LogInfo("Installing hook for exceptions");
            Application.logMessageReceived += LogCaughtException;
        }

        /// <summary>
        /// LogCaughtException is the callback for any exception notifications, reporting
        /// logText, stackTrace, and logType to Sentry.
        /// </summary>
       private static void LogCaughtException(string logText, string stackTrace, LogType logType)
       {
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                Debug.LogInfo("[ExceptionManager] caught an exception: " + logText);
                // might want to add a dialogue to capture feedback? probably annoying to the user.
                // SentrySdk.CaptureUserFeedback(eventId, "user@example.com", "It broke.", "The User");
                SentrySdk.ConfigureScope(scope =>
                {
                    if (RootObject.Instance.ActivityManagerOld.Activity != null && RootObject.Instance.ActivityManagerOld.SessionId != null)
                    {
                        scope.SetTag("sessionID", RootObject.Instance.ActivityManagerOld.SessionId);
                        scope.SetTag("actionID", RootObject.Instance.ActivityManagerOld.ActiveActionId);
                    }
                    else
                    {
                        scope.SetTag("sessionID", string.Empty);
                        scope.SetTag("actionID", string.Empty);
                    }
                });
                SentrySdk.CaptureMessage($"ExceptionManager: [{logType}] {logText}, trace: {stackTrace}");
            }
       }

       /// <summary>
       /// OnDestry removes the hook for the log message callback
       /// </summary>
       private void OnDestroy()
       {
           Application.logMessageReceived -= LogCaughtException;
       }
   }
}