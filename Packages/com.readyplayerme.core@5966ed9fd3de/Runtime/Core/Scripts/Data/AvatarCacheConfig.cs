using System;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    [Serializable]
    public enum LimitStrategy
    {
        AvatarCount,
        CacheSize
    }

    [Serializable]
    public struct AvatarCacheConfig
    {
        [Tooltip("Strategy or rules to apply when enforcing cache limit.")]
        public LimitStrategy limitStrategy;
        [Tooltip("Limit for total number of avatars in the cache.")]
        public int avatarCountLimit;
        [Tooltip("Limit of cache folder size in MB.")]
        public float avatarCacheSizeLimit;
    }
}
