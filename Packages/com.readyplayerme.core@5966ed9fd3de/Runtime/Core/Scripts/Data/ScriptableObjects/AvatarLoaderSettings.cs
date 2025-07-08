using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// The <c>AvatarLoaderSettings</c> class is a <c>ScriptableObject</c> that can be used to easily configure the
    /// settings that should be used when loading a Ready Player Me avatar.
    /// </summary>
    public class AvatarLoaderSettings : ScriptableObject
    {

        /// path to the AvatarLoaderSettings relative to the Resources folder
        public const string SETTINGS_PATH = "Settings/AvatarLoaderSettings";
        /// if enabled avatar assets will be stored locally and only downloaded again if the avatar has been updated
        [Tooltip("If enabled avatar assets will be stored locally and only downloaded again if the avatar has been updated.")]
        public bool AvatarCachingEnabled;

        /// assign an <seealso cref="AvatarConfig"/> to change the configuration of your avatar
        public AvatarConfig AvatarConfig;

        public GLTFDeferAgent GLTFDeferAgent;

        /// <summary>
        /// Loads avatar settings from resource at <c>AvatarLoaderSettings.SETTINGS_PATH</c>.
        /// </summary>
        /// <returns>An <c>AvatarLoaderSettings</c> object after loading it from the Resources folder.</returns>
        public static AvatarLoaderSettings LoadSettings()
        {
            return Resources.Load<AvatarLoaderSettings>(SETTINGS_PATH);
        }
    }
}
