namespace ReadyPlayerMe.Core.Analytics
{
    public interface IAnalyticsEditorLogger
    {
        void Enable();
        void Disable();
        void IdentifyUser();
        void LogOpenProject();
        void LogCloseProject();
        void LogOpenDocumentation(string target);
        void LogOpenFaq(string target);
        void LogOpenDiscord(string target);
        void LogLoadAvatarFromDialog(string avatarUrl, bool eyeAnimation, bool voiceHandler);
        void LogUpdatePartnerURL(string previousSubdomain, string newSubdomain);
        void LogOpenDialog(string dialog);
        void LogBuildApplication(string target, string appName, bool productionBuild);
        void LogMetadataDownloaded(double duration);
        void LogAvatarLoaded(double duration);
        void LogCheckForUpdates();
        void LogSetLoggingEnabled(bool isLoggingEnabled);
        void LogSetCachingEnabled(bool isCachingEnabled);
        void LogClearLocalCache();
        void LogViewPrivacyPolicy();
        void LogShowInExplorer();
        void LogFindOutMore(HelpSubject subject);
        void LogOpenSetupGuide();
        void LogOpenIntegrationGuide();
        void LogLoadQuickStartScene();
        void LogOpenAvatarDocumentation();
        void LogOpenAnimationDocumentation();
        void LogOpenAvatarCreatorDocumentation();
        void LogOpenOptimizationDocumentation();
        void LogAvatarCreatorSampleImported();
        void LogPackageInstalled(string id, string name);
    }
}
