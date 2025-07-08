using System.Collections.Generic;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public static class CacheManager
    {
        private const string TAG = nameof(CacheManager);

        public static void EnforceCacheLimit(AvatarCacheConfig config, AvatarManifest manifest)
        {
            if (config.limitStrategy == LimitStrategy.CacheSize)
            {
                EnforceCacheSize(config.avatarCacheSizeLimit, manifest);
            }
            else
            {
                EnforceAvatarLimit(config.avatarCountLimit, manifest);
            }
        }

        public static void EnforceAvatarLimit(int avatarLimit, AvatarManifest manifest)
        {
            var currentAvatarCount = AvatarCache.GetAvatarCount();
            if (currentAvatarCount <= avatarLimit)
            {
                SDKLogger.Log(TAG, "Avatar count is below limit.");
                return;
            }
            SDKLogger.Log(TAG, $"{manifest.GetIdsByOldestDate().Length}");
            var queue = new Queue<string>(manifest.GetIdsByOldestDate());
            var previousAvatarCount = currentAvatarCount;
            while (currentAvatarCount > avatarLimit)
            {
                if (queue.Count == 0)
                {
                    SDKLogger.LogWarning(TAG, "The queue is empty. Cannot delete more avatars.");
                    break;
                }
                var avatarId = queue.Dequeue();
                AvatarCache.DeleteAvatarFolder(avatarId);
                manifest.RemoveAvatar(avatarId);
                currentAvatarCount--;
            }
            manifest.Save();
            SDKLogger.Log(TAG, $"{previousAvatarCount - currentAvatarCount} avatars deleted.");
        }

        public static void EnforceCacheSize(float cacheSizeLimitMb, AvatarManifest manifest)
        {
            var currentCacheSize = AvatarCache.GetCacheSizeInMb();
            if (currentCacheSize <= cacheSizeLimitMb)
            {
                Debug.Log("Avatar cache size is below limit.");
                return;
            }
            SDKLogger.Log(TAG, $"{manifest.GetIdsByOldestDate().Length}");
            var queue = new Queue<string>(manifest.GetIdsByOldestDate());
            var previousCacheSize = currentCacheSize;
            var avatarsDeleted = 0;
            while (currentCacheSize > cacheSizeLimitMb)
            {
                if (queue.Count == 0)
                {
                    SDKLogger.LogWarning(TAG, "The queue is empty. Cannot delete more avatars.");
                    break;
                }

                var avatarId = queue.Dequeue();
                var avatarSize = AvatarCache.GetAvatarDataSizeInMb(avatarId);
                AvatarCache.DeleteAvatarFolder(avatarId);
                manifest.RemoveAvatar(avatarId);
                currentCacheSize -= avatarSize;
                avatarsDeleted++;
            }
            manifest.Save();
            SDKLogger.Log(TAG, $"{avatarsDeleted} avatars and {previousCacheSize - currentCacheSize} MB deleted.");

        }
    }
}
