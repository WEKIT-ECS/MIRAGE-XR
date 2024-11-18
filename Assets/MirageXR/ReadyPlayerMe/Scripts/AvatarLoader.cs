using ReadyPlayerMe.Core;
using System;
using UnityEngine;

namespace MirageXR
{
	public class AvatarLoader : MonoBehaviour
	{
		[SerializeField] private string defaultAvatarUrl = "https://models.readyplayer.me/667bed8204fd145bd9e09f19.glb";
		[SerializeField] private AvatarConfig avatarConfig;
		[SerializeField] private GameObject loadingIndicator;

		private AvatarObjectLoader _avatarObjectLoader;

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

			bool firstLoad = _avatarObjectLoader == null;
			if (!string.IsNullOrEmpty(defaultAvatarUrl) && firstLoad)
			{
				Debug.LogTrace("Loading default avatar");
				LoadAvatar(defaultAvatarUrl);
			}
		}

		private void OnLoadFailed(object sender, FailureEventArgs e)
		{
			loadingIndicator.SetActive(false);
			AvatarLoaded?.Invoke(false);
		}

		private void OnLoadCompleted(object sender, CompletionEventArgs e)
		{
			Debug.LogDebug("Loading of avatar successful", this);
			loadingIndicator.SetActive(false);
			ApplyAvatar(e);
			AvatarLoaded?.Invoke(true);
		}

		// apply the avatar to our player object
		private void ApplyAvatar(CompletionEventArgs e)
		{
			LoadedAvatarUrl = e.Url;
			e.Avatar.transform.position = transform.position;
			AvatarMeshHelper.TransferMesh(e.Avatar, gameObject);
			Destroy(e.Avatar);
			EyeAnimationHandler eyeAnimation = gameObject.AddComponent<EyeAnimationHandler>();
			eyeAnimation.BlinkInterval = 10f;
		}

		public void LoadAvatar(string avatarUrl)
		{
			Debug.LogDebug("Loading avatar " + avatarUrl, this);
			// remove the eye animation component since we want a fresh one for a new avatar
			EyeAnimationHandler eyeAnimation = gameObject.GetComponent<EyeAnimationHandler>();
			if (eyeAnimation != null)
			{
				Destroy(eyeAnimation);
			}
			avatarUrl = avatarUrl.Trim();
			loadingIndicator.SetActive(true);
			AvatarObjectLoader.LoadAvatar(avatarUrl);
		}
	}
}