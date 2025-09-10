using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
	public static class RPMUtils
	{
		public static string GetAvatarID(string avatarUrl)
		{
			Uri uri = new Uri(avatarUrl);
			string path = Path.GetDirectoryName(uri.AbsolutePath);
			return path;
		}

		public static async Task<bool> IsValidIDAsync(string avatarId)
		{
			UnityWebRequest webRequest = UnityWebRequest.Get($"https://models.readyplayer.me/{avatarId}.json");
			await webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				Debug.Log(webRequest.downloadHandler.text);
			}

			return webRequest.result == UnityWebRequest.Result.Success;
		}
	}
}
