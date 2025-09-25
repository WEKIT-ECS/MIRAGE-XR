using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
	public static class RPMUtils
	{
		public static string ExtractIdFromUrl(string avatarUrl)
		{
			Uri uri = new Uri(avatarUrl);
			string path = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
			return path;
		}

		public static string GetId(string urlOrId)
		{
			string avatarId;
			if (urlOrId.StartsWith("https://models.readyplayer.me/"))
			{
				avatarId = ExtractIdFromUrl(urlOrId);
			}
			else
			{
				avatarId = Regex.Replace(urlOrId, "[^a-zA-Z0-9]", "");
			}
			return avatarId;
		}

		public static string IdToUrl(string id)
		{
			// make sure that the ID is alpha numerical to avoid string injection attacks
			id = Regex.Replace(id, "[^a-zA-Z0-9]", "");
			return $"https://models.readyplayer.me/{id}.glb";
		}

		public static async Task<bool> IsValidIDAsync(string avatarId)
		{
			RPMMetaData metaData = await GetMetadataAsync(avatarId);
			if (metaData == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static async Task<RPMMetaData> GetMetadataAsync(string avatarId)
		{
			UnityWebRequest webRequest = UnityWebRequest.Get($"https://models.readyplayer.me/{avatarId}.json");
			await webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				string json = webRequest.downloadHandler.text;
				RPMMetaData answer = JsonConvert.DeserializeObject<RPMMetaData>(json);
				return answer;
			}
			else
			{
				return null;
			}
		}
	}
}
