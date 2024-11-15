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

		private AvatarObjectLoader avatarObjectLoader;

		public string LoadedAvatarUrl { get; private set; }

		public event Action<bool> AvatarLoaded;

		private void Start()
		{
			loadingIndicator.SetActive(false);
			avatarObjectLoader = new AvatarObjectLoader();
			if (avatarConfig != null)
			{
				avatarObjectLoader.AvatarConfig = avatarConfig;
			}
			else
			{
				Debug.LogWarning("No avatar configuration set. The import of ReadyPlayerMe avatars might not work as expected.", this);
			}
			avatarObjectLoader.OnCompleted += OnLoadCompleted;
			avatarObjectLoader.OnFailed += OnLoadFailed;

			if (!string.IsNullOrEmpty(defaultAvatarUrl))
			{
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
			avatarUrl = avatarUrl.Trim();
			loadingIndicator.SetActive(true);
			avatarObjectLoader.LoadAvatar(avatarUrl);
		}
	}
}