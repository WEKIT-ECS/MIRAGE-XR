namespace ReadyPlayerMe.Core.Analytics
{
    public interface IAnalyticsRuntimeLogger
    {
        void LogRunQuickStartScene();
        void LogLoadPersonalAvatarButton();
        void LogPersonalAvatarLoading(string url);

    }
}
