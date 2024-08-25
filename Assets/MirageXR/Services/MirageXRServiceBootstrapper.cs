using System;
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
			MirageXR.EventManager.XAPIChanged += ChangeXAPI;
		}

		private void OnDisable()
		{
			MirageXR.EventManager.XAPIChanged -= ChangeXAPI;
		}

		protected override void RegisterServices()
		{
#if UNITY_EDITOR
			Debug.MinimumLogLevel = LogLevel.TRACE;
#else
            int logLevel = PlayerPrefs.GetInt("logLevel", 3);
            Debug.MinimumLogLevel = (LogLevel)logLevel;
#endif

			ServiceManager.RegisterService(new WorldAnchorService());
            ServiceManager.RegisterService(new KeywordService());

			ServiceManager.RegisterService(new VestService
			{
				VestEnabled = vestServiceConfiguration.vestEnabled
			});

			if (xAPICredentialsWEKIT != null)
			{
				Debug.LogInfo("[MirageXRServiceBootstrapper] registering xAPI service");
				ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
			}
			else
			{
				AppLog.LogWarning("xAPI credentials not set. You will not be able to use the ExperienceService and xAPI analytics");
			}

			ServiceManager.RegisterService(new VideoAudioTrackGlobalService());

			OpenIDConnectService oidc = new OpenIDConnectService
			{
				OidcProvider = new SketchfabOidcProvider()
			};

#if !UNITY_EDITOR
            oidc.RedirectURI = "https://wekit-ecs.com/sso/callback.php";
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

		private ExperienceAPIClient CreateXAPIClient(string client)
		{
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
			}

#if UNITY_EDITOR
			AppLog.LogInfo("Using fake web connector for xAPI calls since we are in the editor.", this);
			xAPIClient.WebConnector = new XApiMockWebConnector();
#endif

			return xAPIClient;
		}


		private void ChangeXAPI(LearningExperienceEngine.DBManager.LearningRecordStores selectedLRS)
		{
			try
			{
				ServiceManager.RemoveService<ExperienceService>();
			}
			catch (Exception ex)
			{
				AppLog.LogError($"[MirageXRServiceBootstrapper] Tried to unregister xAPI service via i5 ServiceManager, but failed to unregister: {ex.Message}");
			}

			switch (selectedLRS)
			{
				case LearningExperienceEngine.DBManager.LearningRecordStores.WEKIT:
					ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
					break;
			}
		}
	}
}