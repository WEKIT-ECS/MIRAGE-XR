using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.Samples.AvatarLoading
{
    /// <summary>
    /// This class is a simple <see cref="Monobehaviour"/>  to serve as an example on how to load Ready Player Me avatars and spawn as a <see cref="GameObject"/> into the scene.
    /// </summary>
    public class AvatarLoadingExample : MonoBehaviour
    {
        [SerializeField] [Tooltip("Set this to the URL or shortcode of the Ready Player Me Avatar you want to load.")]
        private string avatarUrl = "https://models.readyplayer.me/638df693d72bffc6fa17943c.glb";

        private GameObject avatar;

        private void Start()
        {
            ApplicationData.Log();
            var avatarLoader = new AvatarObjectLoader();
            // use the OnCompleted event to set the avatar and setup animator
            avatarLoader.OnCompleted += (_, args) =>
            {
                avatar = args.Avatar;
                AvatarAnimationHelper.SetupAnimator(args.Metadata, args.Avatar);
            };
            avatarLoader.LoadAvatar(avatarUrl);
        }

        private void OnDestroy()
        {
            if (avatar != null) Destroy(avatar);
        }
    }
}
