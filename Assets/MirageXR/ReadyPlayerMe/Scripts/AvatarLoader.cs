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
		[SerializeField] private GameObject defaultAvatarPrefab;
		[SerializeField] private AvatarConfig avatarConfig;
		[SerializeField] private GameObject loadingIndicator;
		[SerializeField] private bool loadDefaultAvatarOnStart = true;

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

		private AvatarInitializer[] _avatarInitializers;
		private AvatarInitializer[] AvatarInitializers
		{
			get
			{
				if (_avatarInitializers == null)
				{
					_avatarInitializers = GetComponents<AvatarInitializer>();
					_avatarInitializers = _avatarInitializers.OrderBy(item => item.Priority).Reverse().ToArray();
				}
				return _avatarInitializers;
			}
		}

		public string LoadedAvatarUrl { get; private set; }

		public event Action<bool> AvatarLoaded;

		private void Start()
		{
			loadingIndicator.SetActive(false);

			if (loadDefaultAvatarOnStart)
			{
				if (defaultAvatarPrefab != null)
				{
					Debug.LogTrace("Applying default avatar prefab");
					GameObject instance = Instantiate(defaultAvatarPrefab);
					OnLoadCompleted(this, new CompletionEventArgs()
					{
						Avatar = instance,
						Url = "local"
					});
				}
				else
				{
					Debug.LogTrace("Loading default avatar");
					LoadAvatar(defaultAvatarUrl);
				}
			}
		}

		public void LoadAvatar(string avatarUrl)
		{
			if (string.IsNullOrWhiteSpace(avatarUrl))
			{
				return;
			}
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
			Debug.LogDebug($"Loading of avatar from {e.Url} successful", this);
			loadingIndicator.SetActive(false);
			if (CurrentAvatar != null)
			{
				// clean up in opposite order
				for (int i = AvatarInitializers.Length - 1; i >= 0; i--)
				{
					AvatarInitializers[i].CleanupAvatar(CurrentAvatar);
				}
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

			for (int i = 0; i < AvatarInitializers.Length; i++)
			{
				AvatarInitializers[i].InitializeAvatar(CurrentAvatar);
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