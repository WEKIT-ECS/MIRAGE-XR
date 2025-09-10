using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
	public static class RPMUtils
	{
		public static string ExtractAvatarIDfromURL(string avatarUrl)
		{
			Uri uri = new Uri(avatarUrl);
			string path = Path.GetDirectoryName(uri.AbsolutePath);
			return path;
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
