using ReadyPlayerMe.Core;
using System;
using System.Linq;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// Loads and manages an avatar in the scene.
	/// </summary>
	public class AvatarLoader : MonoBehaviour
	{
		[Tooltip("The URL for the default avatar model.")]
		[SerializeField] private string defaultAvatarUrl = "https://models.readyplayer.me/667bed8204fd145bd9e09f19.glb";
		[Tooltip("An avatar prefab to be used as the default avatar (takes priority over the default avatar URL)")]
		[SerializeField] private GameObject defaultAvatarPrefab;
		[Tooltip("ReadyPlayerMe configuration settings for the avatar.")]
		[SerializeField] private AvatarConfig avatarConfig;
		[Tooltip("UI element indicating that an avatar is loading.")]
		[SerializeField] private GameObject loadingIndicator;
		[Tooltip("Flag to indicate if the default avatar should be loaded when the game starts.")]
		[SerializeField] private bool loadDefaultAvatarOnStart = true;

		// Instance of ReadyPlayerMe's AvatarObjectLoader, responsible for loading avatar assets.
		private AvatarObjectLoader _avatarObjectLoader;

		/// <summary>
		/// Currently loaded avatar game object.
		/// </summary>
		public GameObject CurrentAvatar { get; private set; }

		// Gets or initializes the AvatarObjectLoader.
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

		// Array of initializer components ordered by priority.
		private AvatarInitializer[] _avatarInitializers;

		// Gets or initializes the array of AvatarInitializer components on the GameObject.
		// They are iterated over to set up a downloaded avatar
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

		/// <summary>
		/// URL of the last successfully loaded avatar.
		/// </summary>
		public string LoadedAvatarUrl { get; private set; }

		/// <summary>
		/// Event triggered when the avatar has finished loading.
		/// </summary>
		public event Action<bool> AvatarLoaded;

		// Initializes the component and optionally loads the default avatar. 
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

		/// <summary>
		/// Loads an avatar from the specified URL.
		/// </summary>
		/// <param name="avatarUrl">The URL of the avatar model to load.</param>
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

		// Handles the event when avatar loading fails.
		private void OnLoadFailed(object sender, FailureEventArgs e)
		{
			loadingIndicator.SetActive(false);
			Debug.LogError("Could not load avatar. Reason: " + e.Message);
			AvatarLoaded?.Invoke(false);
		}

		// Handles the event when avatar loading completes successfully.
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

		// Sets up the loaded avatar in the scene using the AvatarInitializers
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

		// Parents the transform of the current avatar to this GameObject.
		private void SetupTransform()
		{
			CurrentAvatar.transform.parent = transform;
			CurrentAvatar.transform.position = Vector3.zero;
			CurrentAvatar.transform.rotation = Quaternion.identity;
		}
	}
}