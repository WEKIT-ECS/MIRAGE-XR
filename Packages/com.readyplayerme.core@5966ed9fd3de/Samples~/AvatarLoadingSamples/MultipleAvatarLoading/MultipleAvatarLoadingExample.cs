using System.Collections;
using System.Collections.Generic;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.Samples.AvatarLoading
{
    /// <summary>
    /// This class is a simple <see cref="Monobehaviour"/>  to serve as an example on how to load Ready Player Me avatars and spawn as a <see cref="GameObject"/> into the scene.
    /// </summary>
    public class MultipleAvatarLoadingExample : MonoBehaviour
    {
        private const int RADIUS = 1;
        [SerializeField] [Tooltip("Set this to the URL or shortcodes of the Ready Player Me Avatar you want to load.")]
        private string[] avatarUrls =
        {
            "https://models.readyplayer.me/638df5fc5a7d322604bb3a58.glb",
            "https://models.readyplayer.me/638df70ed72bffc6fa179596.glb",
            "https://models.readyplayer.me/638df75e5a7d322604bb3dcd.glb",
            "https://models.readyplayer.me/638df7d1d72bffc6fa179763.glb"
        };
        private List<GameObject> avatarList;

        private void Start()
        {
            ApplicationData.Log();

            avatarList = new List<GameObject>();
            var urlSet = new HashSet<string>(avatarUrls);

            StartCoroutine(LoadAvatars(urlSet));
        }

        /// This method is used to cleanup/destroy avatar <c>GameObject</c>'s when they are no longer needed.
        private void OnDestroy()
        {
            StopAllCoroutines();
            if (avatarList != null)
            {
                foreach (GameObject avatar in avatarList)
                {
                    Destroy(avatar);
                }
                avatarList.Clear();
                avatarList = null;
            }
        }

        /// Loops through all the avatar urls in the <paramref name="urlSet"/> and loads them one after the other.
        private IEnumerator LoadAvatars(HashSet<string> urlSet)
        {
            var loading = false;

            foreach (var url in urlSet)
            {
                loading = true;
                var loader = new AvatarObjectLoader();
                // use the OnCompleted event setup the animator and run the OnAvatarLoaded method
                loader.OnCompleted += (sender, args) =>
                {
                    loading = false;
                    AvatarAnimationHelper.SetupAnimator(args.Metadata, args.Avatar);
                    OnAvatarLoaded(args.Avatar);
                };
                loader.LoadAvatar(url);

                yield return new WaitUntil(() => !loading);
            }
        }

        /// This method is called after the avatar has been loadded and setup in the scene and is used to set the position of the <c>GameObject</c> in the scene.
        private void OnAvatarLoaded(GameObject avatar)
        {
            if (avatarList != null)
            {
                avatarList.Add(avatar);
                avatar.transform.position = Quaternion.Euler(90, 0, 0) * Random.insideUnitCircle * RADIUS;
            }
            else
            {
                Destroy(avatar);
            }
        }
    }
}
