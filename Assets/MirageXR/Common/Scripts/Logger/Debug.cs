using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug : UnityEngine.Debug
{
    public static LogLevel MinimumLogLevel
    {
        get => AppLog.MinimumLogLevel;
        set
        {
            AppLog.MinimumLogLevel = value;
        }
    }

    public static void LogCritical(object message, Object context)
    {
        AppLog.LogCritical(message.ToString(), context);
    }

    public static new void LogError(object message)
    {
        AppLog.LogError(message.ToString());
    }

    public static new void LogError(object message, Object context)
    {
        AppLog.LogError(message.ToString(), context);
    }

    public static new void LogWarning(object message)
    {
        AppLog.LogWarning(message.ToString());
    }

    public static new void LogWarning(object message, Object context)
    {
        AppLog.LogWarning(message.ToString(), context);
    }

    public static new void Log(object message)
    {
        AppLog.LogInfo(message.ToString());
    }

    public static new void Log(object message, Object context)
    {
		AppLog.LogInfo(message.ToString(), context);
	}

	public static void LogInfo(object message, Object context = null)
	{
		AppLog.LogInfo(message.ToString(), context);
	}

	public static void LogDebug(object message, Object context = null)
	{
		AppLog.LogDebug(message.ToString(), context);
	}

	public static void LogTrace(object message, Object context = null)
	{
        AppLog.LogTrace(message.ToString(), context);
	}
}
