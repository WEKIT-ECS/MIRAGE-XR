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
	/// <summary>
	/// Handles deep links that select and load user avatars.
	/// </summary>
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
			Debug.Log("Received a deep link to for " + url, this);

			Uri uri = new Uri(url);
			string path = uri.Authority.ToLower();
			Dictionary<string,string> fragments = UriUtils.GetUriParameters(uri);
			DeepLinkArgs args = new DeepLinkArgs(fragments, uri);
			if (path == "avatar")
			{
				ReceiveAvatarUrl(args);
			}
			else
			{
				Debug.Log($"Deep link {url} was not processed by AvatarDeepLinkManager.", this);
			}
		}

		/// <summary>
		/// Processes an avatar deep link and sets the linked avatar as the user's avatar.
		/// </summary>
		/// <param name="args">The parsed avatar deep-link arguments.</param>
		public void ReceiveAvatarUrl(DeepLinkArgs args)
		{
			ProcessAvatarDeepLink(args, true);
		}

		private void ProcessAvatarDeepLink(DeepLinkArgs args, bool setAvatar)
		{
			Debug.LogTrace($"Received a deep link to download, cache, and configure a user avatar model: {args.DeepLink}", this);
			if (args.Parameters.TryGetValue(avatarIdParameterName, out string avatarId))
			{
				RootObject.Instance.AvatarLibraryManager.Add(avatarId);
				if (setAvatar)
				{
					Debug.LogTrace($"Set avatar to {avatarId} based on deep link", this);
					string avatarUrl = AvatarLoadUtils.IdToModelUrl(avatarId);
					UserSettings.AvatarUrl = avatarUrl;
				}
			}
			else
			{
				Debug.LogError("Recieved a deep link for a user avatar model, but it did not contain an id parameter (a folder unique name, not GUID): " + args.DeepLink, this);
			}
		}
	}
}
