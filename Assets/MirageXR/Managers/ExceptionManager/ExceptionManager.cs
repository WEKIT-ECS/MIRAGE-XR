using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using Sentry;

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
       public void Initialize()
       {
            //sentry ??= new GameObject("ExceptionManagerSentry").AddComponent<SentrySdk>();
            //sentry.Dsn = SENTRY_DSN;
            //SentrySdk.Debug = false;
            AppLog.Log("Installing hook for exceptions", LogLevel.INFO);
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