using System.IO;
using System.Linq;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public static class DirectoryUtility
    {
        private const float BYTES_IN_MEGABYTE = 1024 * 1024;

        /// The directory where avatar files will be downloaded.
        public static string DefaultAvatarFolder { get; set; } = "Ready Player Me/Avatars";

        public static void ValidateAvatarSaveDirectory(string guid, string paramsHash = null)
        {
            ValidateDirectory(GetAvatarSaveDirectory(guid, paramsHash));
        }

        public static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetAvatarSaveDirectory(string guid, string paramsHash = null)
        {
            return paramsHash == null ? $"{GetAvatarsDirectoryPath()}/{guid}" : $"{GetAvatarsDirectoryPath()}/{guid}/{paramsHash}";
        }

        public static string GetRelativeProjectPath(string guid, string paramsHash = null)
        {
            return paramsHash == null ? $"Assets/{DefaultAvatarFolder}/{guid}" : $"Assets/{DefaultAvatarFolder}/{guid}/{paramsHash}";
        }

        public static long GetDirectorySize(DirectoryInfo directoryInfo)
        {
            // Add file sizes.
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            var size = fileInfos.Sum(fi => fi.Length);

            // Add subdirectory sizes.
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            size += directoryInfos.Sum(GetDirectorySize);
            return size;
        }

        public static float GetFolderSizeInMb(string folderPath)
        {
            var bytes = GetDirectorySize(new DirectoryInfo(folderPath));
            return !Directory.Exists(folderPath) ? 0 : BytesToMegabytes(bytes);
        }

        private static float BytesToMegabytes(long bytes)
        {
            return bytes / BYTES_IN_MEGABYTE;
        }

        public static string GetAvatarsDirectoryPath()
        {
#if UNITY_EDITOR
            return $"{Application.dataPath}/{DefaultAvatarFolder}";
#else
            return GetAvatarsPersistantPath();
#endif
        }

        public static string GetAvatarsPersistantPath()
        {
            return $"{Application.persistentDataPath}/{DefaultAvatarFolder}";
        }
    }
}
