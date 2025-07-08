using System;
using System.IO;
using System.Threading.Tasks;
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
		/// <summary>
		/// Configuration options for the sensor framework
		/// </summary>
		[SerializeField]
		private static bool SensorsEnabled = false;
		//private VestServiceConfiguration vestServiceConfiguration;

		/// <summary>
		/// API secrets for xAPI
		/// </summary>
		[SerializeField]
		private string _WEKITAuthToken;

		//[SerializeField]
		//private ExperienceAPIClientCredentials xAPICredentialsWEKIT;

		[SerializeField] private DeepLinkDefinition deepLinkAPI;

		private void OnEnable()
		{
			EventManager.XAPIChanged += ChangeXAPI;
		}

		private void OnDisable()
		{
			EventManager.XAPIChanged -= ChangeXAPI;
		}

		protected async override void RegisterServices()
		{
			Debug.LogInfo("ServiceBootstrapper: registering services");
#if UNITY_EDITOR
			Debug.MinimumLogLevel = LogLevel.TRACE;
#else
				int logLevel = PlayerPrefs.GetInt("logLevel", 3);
				Debug.MinimumLogLevel = (LogLevel)logLevel;
#endif

			await ReadConfig();

			/*ServiceManager.RegisterService(new WorldAnchorService());
			ServiceManager.RegisterService(new KeywordService());
			ServiceManager.RegisterService(new VestService
			{
				VestEnabled = SensorsEnabled // vestServiceConfiguration.vestEnabled
			});*/

			if (_WEKITAuthToken != null)
			{
				Debug.LogInfo("[MirageXRServiceBootstrapper] registering xAPI service");
				ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
			}
			else
			{
				AppLog.LogWarning("xAPI credentials not set. You will not be able to use the ExperienceService and xAPI analytics");
			}

			ServiceManager.RegisterService(new VideoAudioTrackGlobalService());

			if (!ServiceManager.ServiceExists<DeepLinkingService>())
			{
				ServiceManager.RegisterService(new DeepLinkingService());
			}
			DeepLinkingService deepLinks = ServiceManager.GetService<DeepLinkingService>();

			deepLinkAPI = new DeepLinkDefinition();
			deepLinks.AddDeepLinkListener(deepLinkAPI);
			Debug.LogInfo("ServiceBootstrapper: done registering services");
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
						AuthorizationToken = _WEKITAuthToken,
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


		private void ChangeXAPI(LearningExperienceEngine.UserSettings.LearningRecordStores selectedLRS)
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
				case LearningExperienceEngine.UserSettings.LearningRecordStores.WEKIT:
					ServiceManager.RegisterService(new ExperienceService(CreateXAPIClient("WEKIT")));
					break;
			}
		}

		/// <summary>
		/// Reads the configuration file and initializes the AuthManager with API id and secret.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		private async Task ReadConfig()
		{
			Debug.LogInfo("reading xAPI config data in MirageXRServiceBootstrapper");
			const string fileName = "xAPICredentials";
			const string AuthTokenKey = "AUTH_TOKEN";

			string AuthToken = null;

			var filepath = Resources.Load(fileName) as TextAsset;
			if (filepath == null)
			{
				throw new Exception($"Failed to load config file: {fileName}");
			}

			using var sr = new StringReader(filepath.text);
			while (await sr.ReadLineAsync() is { } line)
			{
				var parts = line.Split('=');

				switch (parts[0].ToUpper())
				{
					case AuthTokenKey:
						AuthToken = parts[1].Trim();
						break;
				}
			}

			if (string.IsNullOrEmpty(AuthToken))
			{
				throw new Exception("can't read xAPI Auth Token");
			}

			_WEKITAuthToken = AuthToken;

			return;
		}

	}
}