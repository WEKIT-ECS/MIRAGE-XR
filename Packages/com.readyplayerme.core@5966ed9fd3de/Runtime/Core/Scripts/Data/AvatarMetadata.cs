using System;
using System.IO;
using Newtonsoft.Json;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This structure holds information about the avatar that is retrieved from the Url.
    /// </summary>
    [Serializable]
    public struct AvatarMetadata
    {
        [JsonConverter(typeof(BodyTypeConverter))]
        public BodyType BodyType;
        public OutfitGender OutfitGender;
        public DateTime UpdatedAt;
        public string SkinTone;

        public static bool IsUpdated(AvatarMetadata newMetadata, AvatarMetadata previousMetadata)
        {
            return newMetadata.UpdatedAt != previousMetadata.UpdatedAt;
        }

        /// <summary>
        /// Loads the avatar metadata from the specified file path.
        /// </summary>
        /// <param name="path">The path to the meta data <c>.json</c> file.</param>
        /// <returns>The loaded <see cref="AvatarMetadata" />.</returns>
        public static AvatarMetadata LoadFromFile(string path)
        {
            if (!File.Exists(path)) return new AvatarMetadata();
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<AvatarMetadata>(json);
        }
    }
}
