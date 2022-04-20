using i5.Toolkit.Core.DeepLinkAPI;
using i5.Toolkit.Core.ExperienceAPI;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using UnityEngine;

namespace MirageXR
{
    public class MirageXRServiceBootstrapper : BaseServiceBootstrapper
    {
        [SerializeField]
        private VestServiceConfiguration vestServiceConfiguration;
        [SerializeField]
        private ExperienceAPIClientCredentials xAPICredentials;

        [SerializeField] private DeepLinkDefinition deepLinkAPI;

        protected override void RegisterServices()
        {
            ServiceManager.RegisterService(new WorldAnchorService());
            ServiceManager.RegisterService(new KeywordService());

            ServiceManager.RegisterService(new VestService()
            {
                VestEnabled = vestServiceConfiguration.vestEnabled
            });

            if (xAPICredentials != null)
            {
                ExperienceAPIClient xAPIClient = new ExperienceAPIClient()
                {
                    XApiEndpoint = new System.Uri("https://lrs.wekit-ecs.com/data/xAPI"),
                    AuthorizationToken = xAPICredentials.authToken,
                    Version = "1.0.3"
                };
                ServiceManager.RegisterService(new ExperienceService(xAPIClient));
            }
            else
            {
                Debug.LogWarning("xAPI credentials not set. You will not be able to use the ExperienceService and xAPI analytics");
            }

            ServiceManager.RegisterService(new VideoAudioTrackGlobalService());
            ServiceManager.RegisterService(new EditorSceneService());

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
    }
}