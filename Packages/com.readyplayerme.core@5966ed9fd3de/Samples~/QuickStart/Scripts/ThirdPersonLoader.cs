using System;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class ThirdPersonLoader : MonoBehaviour
    {
        private readonly Vector3 avatarPositionOffset = new Vector3(0, -0.08f, 0);
        
        [SerializeField][Tooltip("RPM avatar URL or shortcode to load")] 
        private string avatarUrl;
        private GameObject avatar;
        private AvatarObjectLoader avatarObjectLoader;
        [SerializeField][Tooltip("Animator to use on loaded avatar")] 
        private RuntimeAnimatorController animatorController;
        [SerializeField][Tooltip("If true it will try to load avatar from avatarUrl on start")] 
        private bool loadOnStart = true;
        [SerializeField][Tooltip("Preview avatar to display until avatar loads. Will be destroyed after new avatar is loaded")]
        private GameObject previewAvatar;

        public event Action OnLoadComplete;
        
        private void Start()
        {
            avatarObjectLoader = new AvatarObjectLoader();
            avatarObjectLoader.OnCompleted += OnLoadCompleted;
            avatarObjectLoader.OnFailed += OnLoadFailed;
            
            if (previewAvatar != null)
            {
                SetupAvatar(previewAvatar);
            }
            if (loadOnStart)
            {
                LoadAvatar(avatarUrl);
            }
        }

        private void OnLoadFailed(object sender, FailureEventArgs args)
        {
            OnLoadComplete?.Invoke();
        }

        private void OnLoadCompleted(object sender, CompletionEventArgs args)
        {
            if (previewAvatar != null)
            {
                Destroy(previewAvatar);
                previewAvatar = null;
            }
            SetupAvatar(args.Avatar);
            OnLoadComplete?.Invoke();
        }

        private void SetupAvatar(GameObject  targetAvatar)
        {
            if (avatar != null)
            {
                Destroy(avatar);
            }
            
            avatar = targetAvatar;
            // Re-parent and reset transforms
            avatar.transform.parent = transform;
            avatar.transform.localPosition = avatarPositionOffset;
            avatar.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            var controller = GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.Setup(avatar, animatorController);
            }
        }

        public void LoadAvatar(string url)
        {
            //remove any leading or trailing spaces
            avatarUrl = url.Trim(' ');
            avatarObjectLoader.LoadAvatar(avatarUrl);
        }

    }
}
