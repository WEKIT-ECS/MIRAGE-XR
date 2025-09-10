using Newtonsoft.Json;
using ReadyPlayerMe.Core;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class AvatarLibraryManager : MonoBehaviour
	{
		private string AvatarLibraryPath { get => Path.Combine(Application.persistentDataPath, "avatarLib.json"); }

		public List<string> AvatarList { get; private set; } = new List<string>();

		private Dictionary<string, Texture2D> _cachedAvatarThumbnails = new Dictionary<string, Texture2D>();

		private void Awake()
		{
			Load();
		}

		public void Load()
		{
			if (File.Exists(AvatarLibraryPath))
			{
				string json = File.ReadAllText(AvatarLibraryPath);
				AvatarList = JsonConvert.DeserializeObject<List<string>>(json);
			}
			else
			{
				AvatarList = new List<string>() { AvatarLoader.DefaultAvatarUrl };
			}
			_cachedAvatarThumbnails.Clear();
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(AvatarList);
			File.WriteAllText(AvatarLibraryPath, json);
		}

		public void AddAvatar(string avatarUrl)
		{
			// if it is already in the list, re-insert it at the front
			if (AvatarList.Contains(avatarUrl))
			{
				AvatarList.Remove(avatarUrl);
			}
			AvatarList.Insert(0, avatarUrl);
		}

		public void RemoveAvatar(string avatarUrl)
		{
			AvatarList.Remove(avatarUrl);
			if (_cachedAvatarThumbnails.ContainsKey(avatarUrl))
			{
				Destroy(_cachedAvatarThumbnails[avatarUrl]);
				_cachedAvatarThumbnails.Remove(avatarUrl);
			}
		}

		public bool ContainsAvatar(string avatarUrl)
		{
			return AvatarList.Contains(avatarUrl);
		}

		public async Task<Texture2D> GetThumbnailAsync(string avatarUrl)
		{
			if (!AvatarList.Contains(avatarUrl))
			{
				Debug.LogError($"Avatar with {avatarUrl} is not in the library.", this);
				return null;
			}

			if (_cachedAvatarThumbnails.ContainsKey(avatarUrl))
			{
				Debug.LogTrace("Returning cached avatar thumbnail for " + avatarUrl, this);
				return _cachedAvatarThumbnails[avatarUrl];
			}
			else
			{
				Debug.LogTrace($"Loading avatar thumbnail for {avatarUrl} from the web", this);
				AvatarRenderSettings settings = new AvatarRenderSettings();
				settings.Expression = Expression.None;
				settings.Camera = RenderCamera.Portrait;
				settings.Pose = RenderPose.Relaxed;
				settings.Background = Color.black;
				settings.Quality = 100;
				settings.Size = 800;

				TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();

				AvatarRenderLoader loader = new AvatarRenderLoader();

				void OnCompletedHandler(Texture2D texture)
				{
					texture.wrapMode = TextureWrapMode.Clamp;
					Debug.LogTrace($"Loaded avatar thumbnail for {avatarUrl}", this);
					tcs.SetResult(texture);
					_cachedAvatarThumbnails.Add(avatarUrl, texture);
					loader.OnCompleted -= OnCompletedHandler;
					loader.OnFailed -= OnFailedHandler;
				}
				void OnFailedHandler(FailureType failureType, string message)
				{
					Debug.LogError($"Could not load avatar thumbnail. Reason: {failureType}; {message}", this);
					loader.OnCompleted -= OnCompletedHandler;
					loader.OnFailed -= OnFailedHandler;
					tcs.SetResult(null);
				}

				loader.OnCompleted += OnCompletedHandler;
				loader.OnFailed += OnFailedHandler;

				loader.LoadRender(avatarUrl, settings);

				await tcs.Task;
				return tcs.Task.Result;
			}
		}
	}
}
