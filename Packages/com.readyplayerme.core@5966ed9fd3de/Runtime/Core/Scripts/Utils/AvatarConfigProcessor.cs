using System.Collections.Generic;
using System.Linq;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for the <see cref="AvatarConfig" />.
    /// </summary>
    public static class AvatarConfigProcessor
    {
        private const string TAG = nameof(AvatarConfigProcessor);

        private const string PARAM_TRUE = "true";
        private const string PARAM_FALSE = "false";
        private const string PROCESSING_AVATAR_CONFIGURATION = "Processing Avatar Configuration";

        /// <summary>
        /// This method converts the <see cref="AvatarConfig" /> data and combines it into a <c>string</c> that can be added to
        /// an avatar URL.
        /// </summary>
        /// <param name="avatarConfig">Stores the settings of the <see cref="AvatarConfig" /> to use when requesting the avatar.</param>
        /// <returns>The <see cref="AvatarConfig" /> parameters combined as a <c>string</c>.</returns>
        public static string ProcessAvatarConfiguration(AvatarConfig avatarConfig)
        {
            SDKLogger.Log(TAG, PROCESSING_AVATAR_CONFIGURATION);

            var queryBuilder = new QueryBuilder();
            queryBuilder.AddKeyValue(AvatarAPIParameters.POSE, AvatarConfigMap.Pose[avatarConfig.Pose]);
            queryBuilder.AddKeyValue(AvatarAPIParameters.LOD, ((int) avatarConfig.Lod).ToString());
            queryBuilder.AddKeyValue(AvatarAPIParameters.TEXTURE_ATLAS, AvatarConfigMap.TextureAtlas[avatarConfig.TextureAtlas]);
            queryBuilder.AddKeyValue(AvatarAPIParameters.TEXTURE_SIZE_LIMIT, ProcessTextureSizeLimit(avatarConfig.TextureSizeLimit).ToString());
            queryBuilder.AddKeyValue(AvatarAPIParameters.TEXTURE_CHANNELS, ProcessTextureChannels(avatarConfig.TextureChannel));
            if (avatarConfig.MorphTargets.Count > 0)
            {
                queryBuilder.AddKeyValue(AvatarAPIParameters.MORPH_TARGETS, CombineMorphTargetNames(avatarConfig.MorphTargets));
            }
            queryBuilder.AddKeyValue(AvatarAPIParameters.USE_HANDS, GetBoolStringValue(avatarConfig.UseHands));
            queryBuilder.AddKeyValue(AvatarAPIParameters.USE_DRACO, GetBoolStringValue(avatarConfig.UseDracoCompression));
            queryBuilder.AddKeyValue(AvatarAPIParameters.USE_MESHOPT, GetBoolStringValue(avatarConfig.UseMeshOptCompression));

            return queryBuilder.Query;
        }

        private static string GetBoolStringValue(bool value)
        {
            return value ? PARAM_TRUE : PARAM_FALSE;
        }

        /// <summary>
        /// Processes the <paramref name="textureSize" /> and ensures it is a valid value.
        /// </summary>
        /// <param name="textureSize">The value to process.</param>
        /// <returns>A validated <c>int</c>/returns>
        public static int ProcessTextureSizeLimit(int textureSize)
        {
            return textureSize % 2 == 0 ? textureSize : textureSize + 1;
        }

        /// <summary>
        /// Combines the <paramref name="channels"/> in into a single valid textureChannel parameter.
        /// </summary>
        /// <param name="channels">A list of texture channel</param>
        /// <returns>A query string of combined texture channels</returns>
        public static string ProcessTextureChannels(IReadOnlyCollection<TextureChannel> channels)
        {
            if (!channels.Any())
            {
                return "none";
            }

            var parameter = string.Join(",", channels.Select(channel =>
            {
                var channelString = channel.ToString();
                return char.ToLowerInvariant(channelString[0]) + channelString.Substring(1);
            }));

            return parameter;
        }

        /// <summary>
        /// Combines the list of morph target name strings in <paramref name="morphTargetNames" /> into a single valid value.
        /// </summary>
        /// <param name="morphTargetNames">A list of morph targets as strings.</param>
        /// <returns>A query string of combined morph target names, each name separated by ','.</returns>
        public static string CombineMorphTargetNames(IReadOnlyCollection<string> morphTargetNames)
        {
            return morphTargetNames.Count == 0 ? string.Empty : $"{string.Join(",", morphTargetNames)}";
        }
    }
}
