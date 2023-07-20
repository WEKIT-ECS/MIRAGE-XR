using i5.Toolkit.Core.DeepLinkAPI;
using i5.Toolkit.Core.ExperienceAPI;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public class MirageXRServiceBootstrapper : BaseServiceBootstrapper
    {
        [SerializeField]
        private VestServiceConfiguration vestServiceConfiguration;
        [SerializeField]
        private ExperienceAPIClientCredentials xAPICredentialsWEKIT;
        [SerializeField]
        private ExperienceAPIClientCredentials xAPICredentialsARETE;

        [SerializeField] private DeepLinkDefinition deepLinkAPI;

        private void OnEnable()
        {
            EventManager.XAPIChanged += ChangeXAPI;
        }

        private void OnDisable()
        {
            EventManager.XAPIChanged -= ChangeXAPI;
        }

        protected override void RegisterServices()
        {
#if UNITY_EDITOR
            AppLog.MinimumLogLevel = LogLevel.TRACE;
#else
            AppLog.MinimumLogLevel = LogLevel.INFO;
#endif

            ServiceManager.RegisterService(new WorldAnchorService());
            ServiceManager.RegisterService(new KeywordService());

            ServiceManager.RegisterService(new VestService
            {
                VestEnabled = vestServiceConfiguration.vestEnabled
            });

            if (xAPICredentialsWEKIT != null)
            {
                ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
            }
            else
            {
                Debug.LogWarning("xAPI credentials not set. You will not be able to use the ExperienceService and xAPI analytics");
            }

            ServiceManager.RegisterService(new VideoAudioTrackGlobalService());

            OpenIDConnectService oidc = new OpenIDConnectService
            {
                OidcProvider = new SketchfabOidcProvider()
            };

#if !UNITY_EDITOR
            oidc.RedirectURI = "https://wekit-community.org/sketchfab/callback.php";
#else
            // here could be the link to a nicer web page that tells the user to return to the app
#endif

            ServiceManager.RegisterService(oidc);

            DeepLinkingService deepLinks = new DeepLinkingService();
            ServiceManager.RegisterService(deepLinks);

            deepLinkAPI = new DeepLinkDefinition();
            deepLinks.AddDeepLinkListener(deepLinkAPI);
        }

        protected override void UnRegisterServices()
        {
            ServiceManager.GetService<DeepLinkingService>().RemoveDeepLinkListener(deepLinkAPI);
            ServiceManager.RemoveService<DeepLinkingService>();
        }

        private ExperienceAPIClient CreateXAPIClient(string client) {

            ExperienceAPIClient xAPIClient = null;

            switch (client)
            {
                case "WEKIT":
                    xAPIClient = new ExperienceAPIClient
                    {
                        XApiEndpoint = new System.Uri("https://lrs.wekit-ecs.com/data/xAPI"),
                        AuthorizationToken = xAPICredentialsWEKIT.authToken,
                        Version = "1.0.3",
                    };
                    break;
                case "ARETE":
                    xAPIClient = new ExperienceAPIClient
                    {
                        XApiEndpoint = new System.Uri("https://learninglocker.vicomtech.org/data/xAPI"),
                        AuthorizationToken = xAPICredentialsARETE.authToken,
                        Version = "1.0.3",
                    };
                    break;
            }

            return xAPIClient;
        }


        private void ChangeXAPI(DBManager.LearningRecordStores selectedLRS)
        {
            ServiceManager.RemoveService<ExperienceService>();

            switch (selectedLRS)
            {
                case DBManager.LearningRecordStores.WEKIT:
                    ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
                    break;
            }
        }
    }
}