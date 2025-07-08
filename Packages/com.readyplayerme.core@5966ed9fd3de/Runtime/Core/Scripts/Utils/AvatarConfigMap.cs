using System.Collections.Generic;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This static class contains maps that can be used for avatar configuration.
    /// </summary>
    public static class AvatarConfigMap
    {
        /// A map of all possible
        /// <c>Pose</c>
        /// options
        public static readonly Dictionary<Pose, string> Pose = new Dictionary<Pose, string>
        {
            { Core.Pose.APose, "A" },
            { Core.Pose.TPose, "T" }
        };

        /// A map of all possible
        /// <c>TextureAtlas</c>
        /// settings
        public static readonly Dictionary<TextureAtlas, string> TextureAtlas = new Dictionary<TextureAtlas, string>
        {
            { Core.TextureAtlas.None, "none" },
            { Core.TextureAtlas.High, "1024" },
            { Core.TextureAtlas.Medium, "512" },
            { Core.TextureAtlas.Low, "256" }
        };
    }
}
