using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable CS1998

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// A Unity MonoBehaviour class for capturing photos from the device's camera.
    /// Allows starting and stopping the camera, capturing photos, and handling camera permissions.
    /// </summary>
    public class PhotoCaptureElement : MonoBehaviour
    {
#if !RPM_DISABLE_CAMERA_PERMISSION
        [Header("Settings")]
        [SerializeField] private RawImage cameraTextureTarget;
        [SerializeField] private bool initializeOnEnable = true;

        [Space(5)]
        [Header("Events")]
        public UnityEvent<Texture2D> OnPhotoCaptured;

        private WebCamTexture cameraTexture;

        private bool isInitialized;

        private int videoRotationAngle;
        private bool videoVerticallyMirrored;

        private CancellationTokenSource ctxSource;

        private void OnEnable()
        {
            if (initializeOnEnable)
            {
                StartCamera();
            }
        }

        private void OnDisable()
        {
            ctxSource?.Cancel();
            StopCamera();
        }

        /// <summary>
        /// Starts the device's camera, requesting camera permissions if needed.
        /// </summary>
        public async void StartCamera()
        {
            await RequestCameraPermission();

            if (!isInitialized)
            {
                InitializeCamera();
            }

            if (cameraTexture != null && !cameraTexture.isPlaying)
            {
                cameraTexture.Play();

                var currentRotation = cameraTextureTarget.transform.rotation;
                cameraTextureTarget.transform.rotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y, cameraTexture.videoRotationAngle);

                if (!cameraTexture.videoVerticallyMirrored)
                {
                    cameraTextureTarget.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        /// <summary>
        /// Stops the device's camera.
        /// </summary>
        public void StopCamera()
        {
            if (cameraTexture != null && cameraTexture.isPlaying)
            {
                cameraTexture.Stop();
            }
        }

        /// <summary>
        /// Takes a photo from the camera and invokes the onPhotoCaptured event with the captured texture.
        /// </summary>
        public void TakePhoto()
        {
            if (cameraTexture == null || !cameraTexture.isPlaying)
                return;

            var texture = new Texture2D(cameraTextureTarget.texture.width, cameraTextureTarget.texture.height, TextureFormat.ARGB32, false);
            texture.SetPixels(cameraTexture.GetPixels());
            texture.Apply();

            OnPhotoCaptured?.Invoke(texture);
        }

        /// <summary>
        /// Requests camera permissions from the user for IOS and Android devices.
        /// </summary>
        private async Task RequestCameraPermission()
        {
            ctxSource?.Cancel();
            ctxSource = new CancellationTokenSource();

#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }

            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && !ctxSource.IsCancellationRequested)
            {
                await Task.Yield();
            }

#elif UNITY_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                var async = Application.RequestUserAuthorization(UserAuthorization.Microphone);
                while (!async.isDone && !ctxSource.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
#endif

        }

        /// <summary>
        /// Finds the device's camera and sets up the camera texture.
        /// </summary>
        private void InitializeCamera()
        {
            var webCamDevice = GetWebCamDevice();
            SetupPhotoBoothTexture(webCamDevice?.name);
            isInitialized = true;
        }

        /// <summary>
        /// Sets up the camera texture with the provided texture name.
        /// </summary>
        /// <param name="textureName">The name for the created WebCamTexture</param>
        private void SetupPhotoBoothTexture(string textureName)
        {
            var size = cameraTextureTarget.rectTransform.sizeDelta;
            cameraTexture = new WebCamTexture(textureName, (int) size.x, (int) size.y);
            cameraTextureTarget.texture = cameraTexture;
        }

        /// <summary>
        /// Tries to find the device's front-facing camera or returns default camera.
        /// </summary>
        /// <returns>Returns the WebCamDevice if found</returns>
        private static WebCamDevice? GetWebCamDevice()
        {
            var devices = WebCamTexture.devices;

            if (devices.Length == 0)
                return null;

            var webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);

            return webCamDevice.Equals(default(WebCamDevice)) ? devices[0] : webCamDevice;
        }
#endif
    }
}
