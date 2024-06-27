using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class CameraFollow : MonoBehaviour
    {
        private const string TARGET_NOT_SET = "Target not set, disabling component";
        private readonly string TAG = typeof(CameraFollow).ToString();
        [SerializeField][Tooltip("The camera that will follow the target")]
        private Camera playerCamera;
        [SerializeField][Tooltip("The target Transform (GameObject) to follow")]
        private Transform target;
        [SerializeField][Tooltip("Defines the camera distance from the player along Z (forward) axis. Value should be negative to position behind the player")]
        private float cameraDistance = -2.4f;
        [SerializeField] private bool followOnStart = true;
        private bool isFollowing;
        
        private void Start()
        {
            if (target == null)
            {
                SDKLogger.LogWarning(TAG, TARGET_NOT_SET);
                enabled = false;
                return;
            }

            if (followOnStart)
            {
                StartFollow();
            }
        }
        
        private void LateUpdate()
        {
            if (isFollowing)
            {
                playerCamera.transform.localPosition = Vector3.forward * cameraDistance;
                playerCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
                transform.position = target.position;
            }
        }

        public void StopFollow()
        {
            isFollowing = false;
        }

        public void StartFollow()
        {
            isFollowing = true;
        }
    }
}
