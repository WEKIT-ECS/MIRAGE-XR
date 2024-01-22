using i5.Toolkit.Core.VerboseLogging;
<<<<<<< HEAD
using System.Collections;
using System.Collections.Generic;
=======
>>>>>>> develop
using UnityEngine;

namespace MirageXR
{
<<<<<<< HEAD

   /// <summary>
=======
    /// <summary>
>>>>>>> develop
   /// Class <c>ExceptionManager</c> registers for all serious exceptions and opens a user dialogue 
   /// to ask to send a report to sentry.io; the main exceptions registered for are:
   /// Null Reference, Divide by Zero, Out of Memory, Index Out of Range.
   /// </summary>

   public class ExceptionManager : MonoBehaviour
   {
<<<<<<< HEAD

       // the Sentry instance
       public SentrySdk sentry; 

       // Sentry key
       private const string SentryDsn = "https://b23911205078e7a81bf1489e8aa0fabe@o4506320008118272.ingest.sentry.io/4506320009428992";

       private void Awake()
       {
           AppLog.Log("Installing hook for exceptions", LogLevel.INFO );
           sentry ??= new GameObject("ExceptionManagerSentry").AddComponent<SentrySdk>();
           sentry.Dsn = SentryDsn;

           Application.logMessageReceived += LogCaughtException;
           DontDestroyOnLoad(gameObject); 
       }

       public void LogCaughtException(string logText, string stackTrace, LogType logType)
       {

           if (logType == LogType.Exception)
           {

        AppLog.Log($"ExceptionManager: [{logType}] {logText}, trace: {stackTrace}", LogLevel.CRITICAL);
        SentrySdk.CaptureMessage($"ExceptionManager: [{logType}] {logText}, trace: {stackTrace}");
/*
              RootView_v2.Instance.dialog.ShowMiddle(
                 "A serious error happened!",
                 "This may cause the app to become unstable and we recommend restarting the app, especially if not editing. Send error report to the development team?",
                 "OK", () => SentrySdk.CaptureMessage("ExceptionManager: ["+logType+"]" + logText + ", trace: " + stackTrace),
                 "Cancel", () => AppLog.Log("-> User chose not to report this serious exception", LogLevel.INFO),
                 true);
*/

           }

      }

      private void OnDestroy()
      {
         Application.logMessageReceived -= LogCaughtException;
      }

   }

}
=======
       [SerializeField] private SentrySdk sentry; 

       private const string SENTRY_DSN = "https://b23911205078e7a81bf1489e8aa0fabe@o4506320008118272.ingest.sentry.io/4506320009428992";

       public void Initialize()
       {
           AppLog.Log("Installing hook for exceptions", LogLevel.INFO );
           sentry ??= new GameObject("ExceptionManagerSentry").AddComponent<SentrySdk>();
           sentry.Dsn = SENTRY_DSN;
           sentry.Debug = false;

           Application.logMessageReceived += LogCaughtException;
       }

       private static void LogCaughtException(string logText, string stackTrace, LogType logType)
       {
           if (logType == LogType.Exception)
           {
                SentrySdk.CaptureMessage($"ExceptionManager: [{logType}] {logText}, trace: {stackTrace}");
           }
       }

       private void OnDestroy()
       {
           Application.logMessageReceived -= LogCaughtException;
       }
   }
}
>>>>>>> develop
