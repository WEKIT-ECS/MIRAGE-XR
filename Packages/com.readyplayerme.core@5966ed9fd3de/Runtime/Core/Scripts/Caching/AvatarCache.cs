using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for managing the avatar cache that is used for storing the avatar assets locally.
    /// </summary>
    public static class AvatarCache
    {
        /// Calculate cache subfolder name based on hash for avatar Config.
        public static string GetAvatarConfigurationHash(AvatarConfig avatarConfig = null)
        {
            var hash = avatarConfig ? Hash128.Compute(AvatarConfigProcessor.ProcessAvatarConfiguration(avatarConfig)).ToString() : Hash128.Compute("none").ToString();
            return hash;
        }

        /// Clears the avatars from the persistent cache.
        public static void Clear()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            DeleteFolder(path);
#if UNITY_EDITOR
            DeleteFolder(DirectoryUtility.GetAvatarsPersistantPath());
#endif
        }

        private static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
#if UNITY_EDITOR
            path += ".meta";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
#endif
        }

        public static string[] GetExistingAvatarIds()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            if (!Directory.Exists(path)) return Array.Empty<string>();
            var directoryInfo = new DirectoryInfo(path);
            var avatarIds = directoryInfo.GetDirectories().Select(subdir => subdir.Name).ToArray();
            return avatarIds;
        }

        /// Deletes all data for a specific avatar variant (based on parameter hash) from persistent cache.
        public static void DeleteAvatarVariantFolder(string guid, string paramHash)
        {
            DeleteFolder($"{DirectoryUtility.GetAvatarsDirectoryPath()}/{guid}/{paramHash}");
        }

        /// Deletes stored data a specific avatar from persistent cache.
        public static void DeleteAvatarFolder(string guid)
        {
            var path = $"{DirectoryUtility.GetAvatarsDirectoryPath()}/{guid}";
            DeleteFolder(path);
        }

        /// deletes a specific avatar model (.glb file) from persistent cache, while leaving the metadata.json file
        public static void DeleteAvatarModel(string guid, string parametersHash)
        {
            var path = $"{DirectoryUtility.GetAvatarsDirectoryPath()}/{guid}/{parametersHash}";
            if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);

#if UNITY_EDITOR
                foreach (DirectoryInfo dir in info.GetDirectories())
                {
                    AssetDatabase.DeleteAsset($"Assets/{DirectoryUtility.DefaultAvatarFolder}/{guid}/{dir.Name}");
                }

#else
                foreach (DirectoryInfo dir in info.GetDirectories())
                {
                    Directory.Delete(dir.FullName, true);
                }
#endif
            }
        }

        /// Is there any avatars present in the persistent cache.
        public static bool IsCacheEmpty()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            return !Directory.Exists(path) ||
                   Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;
        }

        /// Total Avatars stored in persistent cache.
        public static int GetAvatarCount()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            return !Directory.Exists(path) ? 0 : new DirectoryInfo(path).GetDirectories().Length;

        }

        /// Total Avatar variants stored for specific avatar GUID in persistent cache.
        public static int GetAvatarVariantCount(string avatarGuid)
        {
            var path = $"{DirectoryUtility.GetAvatarsDirectoryPath()}/{avatarGuid}";
            return !Directory.Exists(path) ? 0 : new DirectoryInfo(path).GetDirectories().Length;

        }

        /// Total size of avatar stored in persistent cache. Returns total bytes.
        public static long GetCacheSize()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            return !Directory.Exists(path) ? 0 : DirectoryUtility.GetDirectorySize(new DirectoryInfo(path));
        }

        public static float GetCacheSizeInMb()
        {
            var path = DirectoryUtility.GetAvatarsDirectoryPath();
            return !Directory.Exists(path) ? 0 : DirectoryUtility.GetFolderSizeInMb(path);
        }

        public static float GetAvatarDataSizeInMb(string avatarGuid)
        {
            var path = $"{DirectoryUtility.GetAvatarsDirectoryPath()}/{avatarGuid}";
            return DirectoryUtility.GetFolderSizeInMb(path);
        }
    }
}
