using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    ///     Used for logging in SDK. Currently logs to unity's debugger.
    ///     <remarks>
    ///         Logging is enabled by default, if required can be disabled.
    ///         <code> SDKLogging.logEnabled = false; </code>
    ///     </remarks>
    /// </summary>
    public static class SDKLogger
    {
        public static readonly Logger AvatarLoaderLogger = new Logger(new CustomLogHandler());

        private static bool loggingEnabled;

        static SDKLogger()
        {
            loggingEnabled = IsLoggingEnabled();
        }

        public static bool IsLoggingEnabled()
        {
            return CoreSettingsHandler.CoreSettings.EnableLogging;
        }

        public static void EnableLogging(bool enabled)
        {
            loggingEnabled = enabled;
        }

        public static void Log(string tag, object message)
        {
            if (loggingEnabled)
            {
                AvatarLoaderLogger.Log(tag, message);
            }
        }

        public static void LogWarning(string tag, object message)
        {
            if (loggingEnabled)
            {
                AvatarLoaderLogger.LogWarning(tag, message);
            }
        }
    }
}
