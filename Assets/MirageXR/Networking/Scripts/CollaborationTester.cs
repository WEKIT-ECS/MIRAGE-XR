using UnityEngine;

namespace MirageXR
{
	public class CollaborationTester : MonoBehaviour
    {
#if FUSION2

        private AvatarLoadTester avatarLoadTester;

		private void Start()
		{
			avatarLoadTester = GetComponent<AvatarLoadTester>();
		}


		async void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                await RootObject.Instance.CollaborationManager.StartNewSession();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                Debug.Log("F6");
                if (avatarLoadTester != null && avatarLoadTester._avatarLoader == null)
                {
                    Debug.Log("Getting avatar loader");
                    avatarLoadTester._avatarLoader = FindObjectOfType<AvatarLoader>();
                    Debug.Log("Avatar loader: " +  avatarLoadTester._avatarLoader);
                }

                AvatarVisibilityController avatarVisibilityController = FindObjectOfType<AvatarVisibilityController>();
                avatarVisibilityController.Visible = !avatarVisibilityController.Visible;
			}
        }
#endif
    }
}
