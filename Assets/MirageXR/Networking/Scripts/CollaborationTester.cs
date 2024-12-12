using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class CollaborationTester : MonoBehaviour
    {
        // Update is called once per frame
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
    }
}
