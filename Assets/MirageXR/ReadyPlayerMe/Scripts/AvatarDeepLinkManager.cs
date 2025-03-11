using i5.Toolkit.Core.DeepLinkAPI;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class AvatarDeepLinkManager : MonoBehaviour
	{
		private const string avatarIdParameterName = "id";

		void Start()
		{
			if (!ServiceManager.ServiceExists<DeepLinkingService>())
			{
				ServiceManager.RegisterService<DeepLinkingService>(new DeepLinkingService());
			}
			DeepLinkingService service = ServiceManager.GetService<DeepLinkingService>();

			service.AddDeepLinkListener(this);
		}


		[DeepLink("avatar")]
		public void ReceiveRPMAvatarUrl(DeepLinkArgs args)
		{
			Debug.LogTrace("Received a deep link for an avatar.");
			if (args.Parameters.TryGetValue(avatarIdParameterName, out string id))
			{
				string avatarUrl = $"https://models.readyplayer.me/{id}.glb";
				RootObject.Instance.AvatarLibraryManager.AddAvatar(avatarUrl);
				RootObject.Instance.AvatarLibraryManager.Save();
				UserSettings.AvatarUrl = avatarUrl;
			}
			else
			{
				Debug.LogError("Recieved a deep link for a RPM avatar but it did not contain an id parameter: " + args.DeepLink);
			}
		}
	}
}
