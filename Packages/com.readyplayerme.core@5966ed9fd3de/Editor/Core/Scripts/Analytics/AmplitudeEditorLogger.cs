using System;
using System.Collections.Generic;
using ReadyPlayerMe.Core.Editor;
using UnityEditor;
using UnityEngine;
using static ReadyPlayerMe.Core.Analytics.Constants;

namespace ReadyPlayerMe.Core.Analytics
{
    public class AmplitudeEditorLogger : IAnalyticsEditorLogger
    {
        private const string SDK_TARGET = "Unity";

        private readonly Dictionary<HelpSubject, string> helpDataMap = new()
        {
            { HelpSubject.AvatarCaching, "avatar caching" },
            { HelpSubject.Subdomain, "subdomain" },
            { HelpSubject.AvatarConfig, "avatar config" },
            { HelpSubject.GltfDeferAgent, "gltf defer agent" },
            { HelpSubject.LoadingAvatars, "download avatar into scene" },
            {
                HelpSubject.Avatars, "avatars body type"
            }
        };

        private bool isEnabled;
        private readonly AppData appData;

        public AmplitudeEditorLogger(bool isEnabled)
        {
            this.isEnabled = isEnabled;
            appData = ApplicationData.GetData();
        }

        public void Enable()
        {
            isEnabled = true;
            if (!AmplitudeEventLogger.IsSessionIdSet())
            {
                GenerateSessionId();
            }
            ToggleAnalytics(true);
        }

        public void Disable()
        {
            ToggleAnalytics(false);
            isEnabled = false;
            AmplitudeEventLogger.SetSessionId(0);
        }

        public void IdentifyUser()
        {
            if (!isEnabled) return;
            if (!AmplitudeEventLogger.IsSessionIdSet())
            {
                GenerateSessionId();
            }
            SetUserProperties();
        }

        public void LogOpenProject()
        {
            if (!isEnabled) return;
            GenerateSessionId();
            AmplitudeEventLogger.LogEvent(EventName.OPEN_PROJECT);
        }

        public void LogCloseProject()
        {
            LogEvent(EventName.CLOSE_PROJECT);
        }

        public void LogOpenDocumentation(string target)
        {
            LogEvent(EventName.OPEN_DOCUMENTATION, new Dictionary<string, object>
            {
                { Properties.TARGET, target }
            });
        }

        public void LogOpenFaq(string target)
        {
            LogEvent(EventName.OPEN_FAQ, new Dictionary<string, object>
            {
                { Properties.TARGET, target }
            });
        }

        public void LogOpenDiscord(string target)
        {
            LogEvent(EventName.OPEN_DISCORD, new Dictionary<string, object>
            {
                { Properties.TARGET, target }
            });
        }

        public void LogLoadAvatarFromDialog(string avatarUrl, bool eyeAnimation, bool voiceHandler)
        {
            LogEvent(EventName.LOAD_AVATAR_FROM_DIALOG, new Dictionary<string, object>
            {
                { Properties.AVATAR_URL, avatarUrl },
                { Properties.EYE_ANIMATION, eyeAnimation },
                { Properties.VOICE_HANDLER, voiceHandler }
            });
        }

        public void LogUpdatePartnerURL(string previousSubdomain, string newSubdomain)
        {
            LogEvent(EventName.UPDATED_PARTNER_URL, new Dictionary<string, object>
            {
                { Properties.PREVIOUS_SUBDOMAIN, previousSubdomain },
                { Properties.NEW_SUBDOMAIN, newSubdomain }
            }, new Dictionary<string, object>
            {
                { Properties.SUBDOMAIN, newSubdomain }
            });
        }

        public void LogOpenDialog(string dialog)
        {
            LogEvent(EventName.OPEN_DIALOG, new Dictionary<string, object>
            {
                { Properties.DIALOG, dialog }
            });
        }

        public void LogBuildApplication(string target, string appName, bool productionBuild)
        {
            LogEvent(EventName.BUILD_APPLICATION, new Dictionary<string, object>
            {
                { Properties.TARGET, target },
                { Properties.APP_NAME, appName },
                { Properties.PRODUCTION_BUILD, productionBuild },
                { Properties.APP_IDENTIFIER, Application.identifier }
            });
        }

        public void LogMetadataDownloaded(double duration)
        {
            LogEvent(EventName.METADATA_DOWNLOADED, new Dictionary<string, object>
            {
                { Properties.DURATION, duration }
            });
        }

        public void LogAvatarLoaded(double duration)
        {
            LogEvent(EventName.AVATAR_LOADED, new Dictionary<string, object>
            {
                { Properties.DURATION, duration }
            });
        }

        public void LogCheckForUpdates()
        {
            LogEvent(EventName.CHECK_FOR_UPDATES);
        }

        public void LogSetLoggingEnabled(bool isLoggingEnabled)
        {
            LogEvent(EventName.SET_LOGGING_ENABLED, new Dictionary<string, object>
            {
                { Properties.LOGGING_ENABLED, isLoggingEnabled }
            });
        }

        public void LogSetCachingEnabled(bool isCachingEnabled)
        {
            LogEvent(EventName.SET_CACHING_ENABLED, new Dictionary<string, object>
            {
                { Properties.CACHING_ENABLED, isCachingEnabled }
            });
        }

        public void LogClearLocalCache()
        {
            LogEvent(EventName.CLEAR_LOCAL_CACHE);
        }

        public void LogViewPrivacyPolicy()
        {
            LogEvent(EventName.PRIVACY_POLICY);
        }

        public void LogShowInExplorer()
        {
            LogEvent(EventName.SHOW_IN_EXPLORER);
        }

        public void LogFindOutMore(HelpSubject subject)
        {
            LogEvent(EventName.FIND_OUT_MORE, new Dictionary<string, object>
            {
                { Properties.CONTEXT, helpDataMap[subject] }
            });
        }

        public void LogOpenSetupGuide()
        {
            LogEvent(EventName.OPEN_SETUP_GUIDE);
        }

        public void LogOpenIntegrationGuide()
        {
            LogEvent(EventName.OPEN_INTEGRATION_GUIDE);
        }

        public void LogLoadQuickStartScene()
        {
            LogEvent(EventName.LOAD_QUICK_START_SCENE);
        }

        public void LogOpenAvatarDocumentation()
        {
            LogEvent(EventName.OPEN_AVATAR_DOCUMENTATION);
        }

        public void LogOpenAnimationDocumentation()
        {
            LogEvent(EventName.OPEN_ANIMATION_DOCUMENTATION);
        }

        public void LogOpenAvatarCreatorDocumentation()
        {
            LogEvent(EventName.OPEN_AVATAR_CREATOR_DOCUMENTATION);
        }

        public void LogOpenOptimizationDocumentation()
        {
            LogEvent(EventName.OPEN_OPTIMIZATION_DOCUMENTATION);
        }

        public void LogAvatarCreatorSampleImported()
        {
            LogEvent(EventName.AVATAR_CREATOR_SAMPLE_IMPORTED);
        }

        public void LogPackageInstalled(string id, string name)
        {
            LogEvent(EventName.INSTALL_PACKAGE, new Dictionary<string, object>
            {
                { "id", id },
                { "name", name }
            });
        }

        private void SetUserProperties()
        {
            var userProperties = new Dictionary<string, object>
            {
                { Properties.SDK_SOURCE_URL, PackageManagerHelper.GetSdkPackageSourceUrl() },
                { Properties.ENGINE_VERSION, appData.UnityVersion },
                { Properties.RENDER_PIPELINE, appData.RenderPipeline },
                { Properties.SUBDOMAIN, appData.PartnerName },
                { Properties.APP_NAME, PlayerSettings.productName },
                { Properties.SDK_TARGET, SDK_TARGET },
                { Properties.APP_IDENTIFIER, Application.identifier },
                { Properties.ALLOW_ANALYTICS, true }
            };

            var modules = ModuleList.GetInstalledModuleVersionDictionary();

            foreach (var module in modules)
            {
                userProperties.Add(module.Key, module.Value);
            }

            LogEvent(EventName.SET_USER_PROPERTIES, null, userProperties);
        }

        private void GenerateSessionId()
        {
            AmplitudeEventLogger.SetSessionId(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }

        private void ToggleAnalytics(bool allow)
        {
            LogEvent(EventName.ALLOW_ANALYTICS, new Dictionary<string, object>
            {
                { Properties.ALLOW, allow }
            }, new Dictionary<string, object>
            {
                { Properties.ENGINE_VERSION, appData.UnityVersion },
                { Properties.RENDER_PIPELINE, appData.RenderPipeline },
                { Properties.SUBDOMAIN, appData.PartnerName },
                { Properties.APP_NAME, PlayerSettings.productName },
                { Properties.SDK_TARGET, "Unity" },
                { Properties.APP_IDENTIFIER, Application.identifier },
                { Properties.ALLOW_ANALYTICS, allow }
            });
        }

        private void LogEvent(string eventName, Dictionary<string, object> eventProperties = null, Dictionary<string, object> userProperties = null)
        {
            if (!isEnabled) return;

            AmplitudeEventLogger.LogEvent(eventName, eventProperties, userProperties);
        }
    }
}
