using NSubstitute;
using ReadyPlayerMe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MirageXR
{
	public class AvatarLoader : MonoBehaviour
	{
		[SerializeField] private string defaultAvatarUrl = "https://models.readyplayer.me/667bed8204fd145bd9e09f19.glb";
		[SerializeField] private AvatarConfig avatarConfig;
		[SerializeField] private GameObject loadingIndicator;

		private AvatarObjectLoader _avatarObjectLoader;

		public GameObject CurrentAvatar { get; private set; }

		private AvatarObjectLoader AvatarObjectLoader
		{
			get
			{
				if (_avatarObjectLoader == null)
				{
					_avatarObjectLoader = new AvatarObjectLoader();
					if (avatarConfig != null)
					{
						_avatarObjectLoader.AvatarConfig = avatarConfig;
					}
					else
					{
						Debug.LogWarning("No avatar configuration set. The import of ReadyPlayerMe avatars might not work as expected.", this);
					}
					_avatarObjectLoader.OnCompleted += OnLoadCompleted;
					_avatarObjectLoader.OnFailed += OnLoadFailed;
				}
				return _avatarObjectLoader;
			}
		}

		public string LoadedAvatarUrl { get; private set; }

		public event Action<bool> AvatarLoaded;

		private void Start()
		{
			loadingIndicator.SetActive(false);

			Debug.LogTrace("Loading default avatar");
			LoadAvatar(defaultAvatarUrl);
		}

		public void LoadAvatar(string avatarUrl)
		{
			Debug.LogDebug("Loading avatar " + avatarUrl, this);
			avatarUrl = avatarUrl.Trim();
			loadingIndicator.SetActive(true);
			AvatarObjectLoader.LoadAvatar(avatarUrl);
		}

		private void OnLoadFailed(object sender, FailureEventArgs e)
		{
			loadingIndicator.SetActive(false);
			Debug.LogError("Could not load avatar. Reason: " + e.Message);
			AvatarLoaded?.Invoke(false);
		}

		private void OnLoadCompleted(object sender, CompletionEventArgs e)
		{
			Debug.LogDebug("Loading of avatar successful", this);
			loadingIndicator.SetActive(false);
			if (CurrentAvatar != null)
			{
				Destroy(CurrentAvatar);
			}
			SetupAvatar(e);
			AvatarLoaded?.Invoke(true);
		}

		// apply the avatar to our player object
		private void SetupAvatar(CompletionEventArgs e)
		{
			LoadedAvatarUrl = e.Url;
			CurrentAvatar = e.Avatar;
			// setup transform
			SetupTransform();

			AvatarInitializer[] avatarInitializers = GetComponents<AvatarInitializer>();
			avatarInitializers = avatarInitializers.OrderBy(item => item.Priority).Reverse().ToArray();

			for (int i = 0; i < avatarInitializers.Length; i++)
			{
				avatarInitializers[i].InitializeAvatar(CurrentAvatar);
			}
		}

		private void SetupTransform()
		{
			CurrentAvatar.transform.parent = transform;
			CurrentAvatar.transform.position = Vector3.zero;
			CurrentAvatar.transform.rotation = Quaternion.identity;
		}
	}
}