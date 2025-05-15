using i5.Toolkit.Core.DeepLinkAPI;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class AvatarDeepLinkManager : MonoBehaviour
	{
		private const string avatarIdParameterName = "id";

		private void Awake()
		{
			Application.deepLinkActivated += OnDeepLinkActivated;
			if (!string.IsNullOrEmpty(Application.absoluteURL))
			{
				OnDeepLinkActivated(Application.absoluteURL);
			}
		}

		private void OnDeepLinkActivated(string url)
		{
			Debug.Log("Got deep link for " + url, this);

			Uri uri = new Uri(url);
			string path = uri.Authority.ToLower();
			Dictionary<string,string> fragments = UriUtils.GetUriParameters(uri);
			DeepLinkArgs args = new DeepLinkArgs(fragments, uri);
			if (path == "avatar")
			{
				ReceiveRPMAvatarUrl(args);
			}
			else if (path == "rpm")
			{
				ReceiveRPMUrl(args);
			}
			else
			{
				Debug.Log($"Deep link {url} was not processed by AvatarDeepLinkManager.", this);
			}
		}

		public void ReceiveRPMAvatarUrl(DeepLinkArgs args)
		{
			ProcessRPMDeepLink(args, true);
		}

		public void ReceiveRPMUrl(DeepLinkArgs args)
		{
			ProcessRPMDeepLink(args, false);
		}

		private void ProcessRPMDeepLink(DeepLinkArgs args, bool setAvatar)
		{
			Debug.LogTrace($"Received a deep link for a RPM model: {args.DeepLink}", this);
			if (args.Parameters.TryGetValue(avatarIdParameterName, out string id))
			{
				string avatarUrl = $"https://models.readyplayer.me/{id}.glb";
				RootObject.Instance.AvatarLibraryManager.AddAvatar(avatarUrl);
				RootObject.Instance.AvatarLibraryManager.Save();
				if (setAvatar)
				{
					Debug.LogTrace($"Set avatar to {id} based on deep link", this);
					UserSettings.AvatarUrl = avatarUrl;
				}
			}
			else
			{
				Debug.LogError("Recieved a deep link for a RPM model but it did not contain an id parameter: " + args.DeepLink, this);
			}
		}
	}
}
