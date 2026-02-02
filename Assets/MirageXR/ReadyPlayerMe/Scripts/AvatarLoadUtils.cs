using i5.Toolkit.Core.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public static class AvatarLoadUtils
    {
        public static string ExtractIdFromUrl(string avatarUrl)
        {
            if (Uri.TryCreate(avatarUrl, UriKind.Absolute, out Uri uri))
            {
                Dictionary<string, string> queryParams = UriUtils.GetUriParameters(uri);
                if (queryParams.TryGetValue("avatar_name", out string name))
                {
                    return name;
                }
                return "";
            }
            else
            {
                Debug.LogWarning(avatarUrl + " is not a URL, returning it as is.");
                return avatarUrl;
            }
        }

        public static string GetId(string urlOrId)
        {
            string avatarId;
            if (urlOrId.StartsWith("http://repository.wekit-ecs.com"))
            {
                avatarId = ExtractIdFromUrl(urlOrId);
            }
            else
            {
                avatarId = Regex.Replace(urlOrId, "[^a-zA-Z0-9]", "");
            }
            return avatarId;
        }

        public static string IdToModelUrl(string id)
        {
            // make sure that the ID is alpha numerical to avoid string injection attacks
            id = Regex.Replace(id, "[^a-zA-Z0-9]", "");
            return $"http://repository.wekit-ecs.com:8001/avatar/get?avatar_name={id}";
        }
    }
}
