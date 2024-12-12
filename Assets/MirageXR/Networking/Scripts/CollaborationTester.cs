using UnityEngine;

namespace MirageXR
{
	public class CollaborationTester : MonoBehaviour
    {
#if FUSION2
        async void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                await CollaborationManager.Instance.StartNewSession();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                AvatarVisibilityController avatarVisibilityController = FindObjectOfType<AvatarVisibilityController>();
                avatarVisibilityController.Visible = !avatarVisibilityController.Visible;
			}
        }
#endif
    }
}
