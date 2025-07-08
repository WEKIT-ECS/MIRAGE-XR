using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public class AvatarManifest
    {
        private const string RELATIVE_PATH = "/AvatarManifest.json";
        private Dictionary<string, DateTime> avatarRecords = new Dictionary<string, DateTime>();
        public Dictionary<string, DateTime> AvatarRecords => avatarRecords;

        public DateTime GetAvatarLastLoadDate(string guid)
        {
            return avatarRecords.TryGetValue(guid, out DateTime date) ? date : DateTime.UtcNow;
        }

        public void AddAvatar(string guid)
        {
            if (!avatarRecords.ContainsKey(guid))
            {
                avatarRecords.Add(guid, DateTime.UtcNow);
            }
        }

        public void RemoveAvatar(string guid)
        {
            avatarRecords.Remove(guid);
        }

        public Dictionary<string, DateTime> Load()
        {
            var json = ReadFromFile();
            if (json != string.Empty)
            {
                avatarRecords = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(json);
            }
            var existingAvatars = AvatarCache.GetExistingAvatarIds();
            foreach (var existingAvatar in existingAvatars)
            {
                if (!avatarRecords.ContainsKey(existingAvatar))
                {
                    avatarRecords.Add(existingAvatar, DateTime.UtcNow);
                }
            }

            return avatarRecords;
        }

        public void Save()
        {
            WriteToFile(JsonConvert.SerializeObject(avatarRecords));
        }

        public void Clear()
        {
            avatarRecords.Clear();
        }

        private void WriteToFile(string json)
        {
            var path = GetFilePath();

            using (var writer = new StreamWriter(path, false))
            {
                writer.Write(json);
            }
        }

        private string GetFilePath()
        {
            return $"{DirectoryUtility.GetAvatarsDirectoryPath()}{RELATIVE_PATH}";
        }


        private string ReadFromFile()
        {
            var path = GetFilePath();
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var json = reader.ReadToEnd();
                    return json;
                }
            }
            Debug.LogWarning("Manifest file not found");

            return "";
        }

        public string[] GetIdsByOldestDate()
        {
            return avatarRecords.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        }
    }
}
