using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class AvatarLibraryManager : MonoBehaviour
	{
		private string AvatarLibraryPath { get => Path.Combine(Application.persistentDataPath, "avatarLib.json"); }

		public List<string> AvatarList { get; private set; } = new List<string>();

		private Dictionary<string, Texture2D> _cachedAvatarThumbnails = new Dictionary<string, Texture2D>();

		private AvatarRenderLoader _avatarRenderLoader = new AvatarRenderLoader();

		private void Awake()
		{
			Load();
		}

		private void OnDestroy()
		{
			Save();
		}

		public void Load()
		{
			if (File.Exists(AvatarLibraryPath))
			{
				string json = File.ReadAllText(AvatarLibraryPath);
				AvatarList = JsonSerializer.Deserialize<List<string>>(json);
				_cachedAvatarThumbnails.Clear();
			}
		}

		public void Save()
		{
			string json = JsonSerializer.Serialize<List<string>>(AvatarList);
			File.WriteAllText(AvatarLibraryPath, json);
		}

		public void AddAvatar(string avatarUrl)
		{
			if (!AvatarList.Contains(avatarUrl))
			{
				AvatarList.Add(avatarUrl);
			}
		}

		public void RemoveAvatar(string avatarUrl)
		{
			AvatarList.Remove(avatarUrl);
			if (_cachedAvatarThumbnails.ContainsKey(avatarUrl))
			{
				_cachedAvatarThumbnails.Remove(avatarUrl);
			}
		}

		public async Task<Texture2D> GetThumbnailAsync(string avatarUrl)
		{
			if (!AvatarList.Contains(avatarUrl))
			{
				Debug.LogError($"Avatar with {avatarUrl} is not in the library.");
				return null;
			}

			if (_cachedAvatarThumbnails.ContainsKey(avatarUrl))
			{
				return _cachedAvatarThumbnails[avatarUrl];
			}
			else
			{
				AvatarRenderSettings settings = new AvatarRenderSettings();
				settings.Expression = Expression.None;
				settings.Camera = RenderCamera.Portrait;
				settings.Pose = RenderPose.Relaxed;
				settings.Background = Color.black;
				settings.Quality = 100;
				settings.Size = 800;

				TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();

				void OnCompletedHandler(Texture2D texture)
				{
					tcs.SetResult(texture);
					_avatarRenderLoader.OnCompleted -= OnCompletedHandler;
				}

				_avatarRenderLoader.OnCompleted += OnCompletedHandler;

				_avatarRenderLoader.LoadRender(avatarUrl, settings);

				await tcs.Task;
				return tcs.Task.Result;
			}
		}
	}
}
