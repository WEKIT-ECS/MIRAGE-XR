using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.WebGLSample
{
    public class DemoCanvas : MonoBehaviour
    {
        [SerializeField] private Button createAvatarButton;

        private void Start()
        {
            if (createAvatarButton != null)
            {
                createAvatarButton.onClick.AddListener(OnCreateAvatar);
            }
        }

        public void OnCreateAvatar()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        WebInterface.SetIFrameVisibility(true);
#endif
        }
    }
}
