using Newtonsoft.Json;
using ReadyPlayerMe.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
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
				AvatarList = ConvertToIds(AvatarList);
			}
			else
			{
				AvatarList = new List<string>();
				AddAvatar(AvatarLoader.DefaultAvatarUrl);
			}
			_cachedAvatarThumbnails.Clear();
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(AvatarList);
			File.WriteAllText(AvatarLibraryPath, json);
		}

		// conversion function for backwards compatibility
		// converts full URLs to the new save format which only stores the IDs
		private List<string> ConvertToIds(List<string> mixedFormatList)
		{
			HashSet<string> uniqueIds = new HashSet<string>();
			foreach (string entry in mixedFormatList)
			{
				string id = RPMUtils.GetId(entry);
				uniqueIds.Add(id);
			}
			return uniqueIds.ToList();
		}

		public void AddAvatar(string avatarId)
		{
			avatarId = RPMUtils.GetId(avatarId);
			// if it is already in the list, re-insert it at the front
			if (AvatarList.Contains(avatarId))
			{
				AvatarList.Remove(avatarId);
			}
			AvatarList.Insert(0, avatarId);
			Save();
		}

		public void RemoveAvatar(string avatarId)
		{
			avatarId = RPMUtils.GetId(avatarId);
			AvatarList.Remove(avatarId);
			if (_cachedAvatarThumbnails.ContainsKey(avatarId))
			{
				Destroy(_cachedAvatarThumbnails[avatarId]);
				_cachedAvatarThumbnails.Remove(avatarId);
			}
			Save();
		}

		public bool ContainsAvatar(string avatarId)
		{
			avatarId = RPMUtils.GetId(avatarId);
			return AvatarList.Contains(avatarId);
		}

		public async Task<Texture2D> GetThumbnailAsync(string avatarId)
		{
			avatarId = RPMUtils.GetId(avatarId);
			if (!AvatarList.Contains(avatarId))
			{
				Debug.LogError($"Avatar with ID {avatarId} is not in the library.", this);
				return null;
			}

			if (_cachedAvatarThumbnails.ContainsKey(avatarId))
			{
				Debug.LogTrace("Returning cached avatar thumbnail for avatar ID" + avatarId, this);
				return _cachedAvatarThumbnails[avatarId];
			}
			else
			{
				Debug.LogTrace($"Loading avatar thumbnail for ID {avatarId} from the web", this);
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
					Debug.LogTrace($"Loaded avatar thumbnail for ID {avatarId}", this);
					tcs.SetResult(texture);
					_cachedAvatarThumbnails.Add(avatarId, texture);
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

				loader.LoadRender($"https://models.readyplayer.me/{avatarId}.glb", settings);

				await tcs.Task;
				return tcs.Task.Result;
			}
		}
	}
}
