using i5.Toolkit.Core.VerboseLogging;
using Sentry;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
   /// Class <c>ExceptionManager</c> registers for all serious exceptions and opens a user dialogue 
   /// to ask to send a report to sentry.io; the main exceptions registered for are:
   /// Null Reference, Divide by Zero, Out of Memory, Index Out of Range.
   /// </summary>

   public class ExceptionManager : MonoBehaviour
   {
       public void Initialize()
       {
           AppLog.Log("Installing hook for exceptions", LogLevel.INFO );

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