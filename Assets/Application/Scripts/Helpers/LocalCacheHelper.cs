namespace MRTKUtilities.Application
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
#if WINDOWS_UWP
using Windows.Storage;
#endif
    
    /// <summary>
    /// Helper class for managing local caching on disk.
    /// 
    /// TODO: 
    /// Change this to what you need for your project. In the 
    /// </summary>
    public static class LocalCacheHelper
    {
        private const string CACHE_ID_FILE = "cache.id";
        private const string DATA_CACHE_FILE = "data.json";

        // setting for timeout of data in cache
        private static TimeSpan DataMaxValidity = TimeSpan.FromDays(1);

        /// <summary>
        /// Initialize the cache. We'll check for the current id against
        /// the given id to work with. If we're switching id's, it's a new
        /// context (probably new user), so we clear the current cache.
        /// </summary>
        /// <param name="cacheId">Cache ID to use.</param>
        public static void Initialize(string cacheId)
        {
            string curCacheId = GetCacheId();

            if (!string.IsNullOrEmpty(curCacheId) && cacheId != curCacheId)
            {
                // we are operating with another cache id, so clear contents
                ClearCache();
            }

            if (cacheId != curCacheId)
            {
                SetCacheId(cacheId);
            }
        }

        /// <summary>
        /// Get the ID of the cache. This is stored in a textfile in the
        /// cache folder.
        /// </summary>
        /// <returns>Cache ID from the textfile or empty when not found.</returns>
        public static string GetCacheId()
        {
            string cacheId = string.Empty;
            string filepath = Path.Combine(GetCachePath(), CACHE_ID_FILE);
            if (File.Exists(filepath))
            {
                cacheId = File.ReadAllText(filepath);
            }

            return cacheId;
        }

        /// <summary>
        /// Set the ID of the cache. This is stored in a textfile in the
        /// cache folder.
        /// </summary>
        /// <param name="cacheId">Cache id.</param>
        public static void SetCacheId(string cacheId)
        {
            string filepath = Path.Combine(GetCachePath(), CACHE_ID_FILE);
            File.WriteAllText(filepath, cacheId);
        }

        /// <summary>
        /// Store the data in a JSON cache file.
        /// 
        /// TODO:
        /// Change the type of data for your project.
        /// </summary>
        /// <param name="data">Data structure.</param>
        public static void StoreData(List<string> data)
        {
            string filepath = Path.Combine(GetCachePath(), DATA_CACHE_FILE);

            DataCache cache = new DataCache
            {
                LastUpdate = DateTime.Now,
                Data = data,
            };
            string json = JsonConvert.SerializeObject(cache);
            File.WriteAllText(filepath, json);
        }

        /// <summary>
        /// Read the projects data from a JSON cache file.
        /// We'll check the las update with the <see cref="DataMaxValidity"/> configuration
        /// to determine if we consider this cache stale or not. If it's considered stale,
        /// we'll return null as well and clear the cache.
        /// 
        /// TODO:
        /// Change the type of data for your project.
        /// </summary>
        public static List<string> ReadData()
        {
            string filepath = Path.Combine(GetCachePath(), DATA_CACHE_FILE);
            if (!File.Exists(filepath))
            {
                return null;
            }
            string json = File.ReadAllText(filepath);
            DataCache cache = JsonConvert.DeserializeObject<DataCache>(json);

            if (DateTime.Now.Subtract(cache.LastUpdate) > DataMaxValidity)
            {
                // data considered stale
                File.Delete(filepath);
                return null;
            }

            return cache.Data;
        }

        /// <summary>
        /// Delete all data files and folders from the cache.
        /// </summary>
        public static void ClearCache()
        {
            string path = GetCachePath();

            string fullpath = Path.Combine(path, CACHE_ID_FILE);
            if (File.Exists(fullpath)) File.Delete(fullpath);

            fullpath = Path.Combine(path, DATA_CACHE_FILE);
            if (File.Exists(fullpath)) File.Delete(fullpath);
        }

        private static string GetCachePath()
        {
#if WINDOWS_UWP
            return ApplicationData.Current.LocalCacheFolder.Path;
#else
            return UnityEngine.Application.persistentDataPath;
#endif
        }
    }
}