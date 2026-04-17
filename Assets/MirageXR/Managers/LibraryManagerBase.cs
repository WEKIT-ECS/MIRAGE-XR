using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MirageXR
{
    public abstract class LibraryManagerBase : MonoBehaviour
    {
        public abstract string LibraryFileName { get; }
        public abstract string DefaultContentId { get; }

        private string LibraryPath { get => Path.Combine(Application.persistentDataPath, LibraryFileName); }

        public List<string> LibraryContent { get; private set; } = new List<string>();

        protected virtual void Awake()
        {
            Load();
        }

        public virtual void Load()
        {
            if (File.Exists(LibraryPath))
            {
                string json = File.ReadAllText(LibraryPath);
                LibraryContent = JsonConvert.DeserializeObject<List<string>>(json);
                LibraryContent = ConvertToIds(LibraryContent);
            }
            else
            {
                LibraryContent = new List<string>();
                Add(DefaultContentId);
            }
        }

        public virtual void Save()
        {
            string json = JsonConvert.SerializeObject(LibraryContent);
            File.WriteAllText(LibraryPath, json);
        }

        // conversion function for backwards compatibility
        // converts full URLs to the new save format which only stores the IDs
        private List<string> ConvertToIds(List<string> mixedFormatList)
        {
            HashSet<string> uniqueIds = new HashSet<string>();
            foreach (string entry in mixedFormatList)
            {
                string id = GetId(entry);
                uniqueIds.Add(id);
            }
            return uniqueIds.ToList();
        }

        protected abstract string GetId(string entry);

        public virtual void Add(string elementId)
        {
            elementId = GetId(elementId);
            // if it is already in the list, re-insert it at the front
            if (LibraryContent.Contains(elementId))
            {
                LibraryContent.Remove(elementId);
            }
            LibraryContent.Insert(0, elementId);
            Save();
        }

        public void Remove(string elementId)
        {
            elementId = GetId(elementId);
            LibraryContent.Remove(elementId);
            Save();
        }

        public bool Contains(string elementId)
        {
            elementId = GetId(elementId);
            return LibraryContent.Contains(elementId);
        }
    }
}
