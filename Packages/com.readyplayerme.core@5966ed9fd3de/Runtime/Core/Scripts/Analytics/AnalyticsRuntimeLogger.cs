namespace ReadyPlayerMe.Core.Analytics
{
    public static class AnalyticsRuntimeLogger
    {
        public static readonly IAnalyticsRuntimeLogger EventLogger;

        static AnalyticsRuntimeLogger()
        {
            EventLogger = new AmplitudeRuntimeLogger();
        }
    }
}
