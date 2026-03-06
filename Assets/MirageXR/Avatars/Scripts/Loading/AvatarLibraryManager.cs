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
                string id = AvatarLoadUtils.GetId(entry);
                uniqueIds.Add(id);
            }
            return uniqueIds.ToList();
        }

        public void AddAvatar(string avatarId)
        {
            avatarId = AvatarLoadUtils.GetId(avatarId);
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
            avatarId = AvatarLoadUtils.GetId(avatarId);
            AvatarList.Remove(avatarId);
            Save();
        }

        public bool ContainsAvatar(string avatarId)
        {
            avatarId = AvatarLoadUtils.GetId(avatarId);
            return AvatarList.Contains(avatarId);
        }
    }
}
