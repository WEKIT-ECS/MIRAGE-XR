using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReadyPlayerMe.Core
{
    // Custom log handler which can be extended to add writing to a file.
    public class CustomLogHandler : ILogHandler
    {
        private const string CONTEXT = "Ready Player Me";
        private readonly ILogHandler logHandler = Debug.unityLogger.logHandler;

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            logHandler.LogFormat(logType, context, $"[{CONTEXT}] {format}", args);
        }

        public void LogException(Exception exception, Object context)
        {
            logHandler.LogException(exception, context);
        }
    }
}
