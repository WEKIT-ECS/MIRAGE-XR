using System;
using System.IO;
using UnityEngine;
#if UNITY_WSA || UNITY_EDITOR
using System.Linq;
using UnityEngine.Windows.WebCam;
#endif

public class NativeCameraController
{
    private const int DEFAULT_MAX_SIZE = 1024;

    public static void TakePicture(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {
#if UNITY_WSA || UNITY_EDITOR
        TakePictureUWP(callback, showHolograms, maxSize);
#elif UNITY_VISIONOS || VISION_OS
        TakePictureVisionPro(callback, showHolograms, maxSize);
#else
        TakePictureMobile(callback, maxSize);
#endif
    }

    public static void StartRecordingVideo(string filePath, Action<bool, string> callback, bool showHolograms = false)
    {
#if UNITY_WSA
        StartVideoRecordingUWPAsync(filePath, callback, showHolograms: showHolograms);
#else
        StartVideoRecordingMobileAsync(filePath, callback);
#endif
    }

    public static Texture2D GetVideoThumbnail(string filePath)
    {
        return NativeCamera.GetVideoThumbnail(filePath, markTextureNonReadable: false);
    }

    public static void StopRecordVideo()
    {
#if UNITY_WSA
        StopVideoRecordingUWP();
#else
        StopVideoRecordingMobile();
#endif
    }

#if UNITY_VISIONOS || VISION_OS
    private static void TakePictureVisionPro(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {

	try {

	 	RenderTexture renderTexture;
       		RenderTexture.active = renderTexture;
        	Camera.main.targetTexture = renderTexture;
        	Camera.main.Render();

        	Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        	screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        	screenshot.Apply();

        	Camera.main.targetTexture = null;
        	RenderTexture.active = null;

        	callback(true, tex);
	} catch (exception e) {
		Debug.LogError(e);
                callback(false, null);
	}
    }
#endif


    private static void TakePictureMobile(Action<bool, Texture2D> callback, int maxSize = DEFAULT_MAX_SIZE)
    {
        NativeCamera.TakePicture(path =>
        {
            try
            {
                if (path == null)
                {
                    Debug.LogError("Couldn't get picture");
                    callback(false, null);
                    return;
                }

                var texture = NativeCamera.LoadImageAtPath(path, maxSize, false);
                if (texture == null)
                {
                    Debug.LogError($"Couldn't load texture from {path}");
                }

                File.Delete(path);
                callback(texture != null, texture);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                callback(false, null);
            }
        }, maxSize);
    }

    private static void StartVideoRecordingMobileAsync(string filePath, Action<bool, string> callback)
    {
        NativeCamera.RecordVideo(path =>
        {
            try
            {
#if UNITY_EDITOR
                if (path != null)
                {
                    File.Copy(path, filePath);
                    callback.Invoke(true, filePath);
                }
                else
                {
                    Debug.LogWarning("Fake video-recording in editor: No file was selected. Returning false");
                    callback.Invoke(false, null);
                }
#else
                if (path != null)
                {
                    File.Move(path, filePath);
                    callback.Invoke(true, filePath);
                }
                else
                {
                    Debug.LogWarning("No file was selected.");
                    callback.Invoke(false, null);
                }
#endif

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                callback.Invoke(false, filePath);
            }
        });
    }

    private static void StopVideoRecordingMobile()
    {
        Debug.Log($"no need to call this function ({nameof(StopVideoRecordingMobile)}) for mobile devices");
    }

#if UNITY_WSA || UNITY_EDITOR
    private static void TakePictureUWP(Action<bool, Texture2D> callback, bool showHolograms = false, int maxSize = DEFAULT_MAX_SIZE)
    {
        var resolutions = PhotoCapture.SupportedResolutions.Where(t => t.height <= maxSize && t.width <= maxSize).ToList();

        if (resolutions.Count == 0)
        {
            Debug.LogWarning($"No extension was found for the specified parameters (maxSize == {maxSize}). The maximum available resolution will be used.");
            resolutions = PhotoCapture.SupportedResolutions.ToList();
        }

        if (resolutions.Count == 0)
        {
            Debug.LogWarning("Could not take image. No resolutions are available. Is the webcam accessbile?");
            callback(false, null);
            return;
        }

        var cameraResolution = resolutions.OrderByDescending(res => res.width * res.height).First();

        PhotoCapture photoCapture;

        PhotoCapture.CreateAsync(showHolograms, captureObject =>
        {
            photoCapture = captureObject;

            if (photoCapture != null)
            {
                var cameraParameters = new CameraParameters
                {
                    hologramOpacity = showHolograms ? 1.0f : 0.0f,
                    cameraResolutionWidth = cameraResolution.width,
                    cameraResolutionHeight = cameraResolution.height,
                    pixelFormat = CapturePixelFormat.BGRA32
                };

                photoCapture.StartPhotoModeAsync(cameraParameters, result =>
                {
                    if (result.success)
                    {
                        photoCapture.TakePhotoAsync((captureResult, frame) =>
                        {
                            var texture = new Texture2D(cameraResolution.width, cameraResolution.height);
                            frame.UploadImageDataToTexture(texture);
                            photoCapture.StopPhotoModeAsync(photoCaptureResult =>
                            {
                                photoCapture.Dispose();
                                callback(true, texture);
                            });
                        });
                    }
                    else
                    {
                        callback(false, null);
                    }
                });
            }
            else
            {
                callback(false, null);
            }
        });
    }
#endif

#if UNITY_WSA || UNITY_EDITOR
    private static VideoCapture _videoCapture;
    private static string _filePath;
    private static Action<bool, string> _recordCallback;

    private static void StartVideoRecordingUWPAsync(string filePath, Action<bool, string> callback,
        VideoCapture.AudioState audioState = VideoCapture.AudioState.MicAudio, bool showHolograms = false)
    {

#if UNITY_EDITOR
        string pickedFile = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select video", "", new string[] { "Video files", "mp4,mov,wav,avi", "All files", "*" });
        if (callback != null)
        {
            var isValidPath = !string.IsNullOrEmpty(pickedFile);
            callback.Invoke(isValidPath, isValidPath ? pickedFile : null);
            ResetVideoRecorder();
        }
        return;
#endif


        if (_videoCapture != null && _videoCapture.IsRecording)
        {
            Debug.Log("Is already recording");
            return;
        }

        VideoCapture.CreateAsync(showHolograms, captureObject =>
        {
            if (captureObject != null)
            {
                _videoCapture = captureObject;

                Debug.Log("onVideoCaptureCreated");

                var parameters = CreateCameraVideoParameters();
                _videoCapture.StartVideoModeAsync(parameters, audioState, result =>
                {
                    Debug.Log("start recording video");
                    if (result.success)
                    {
                        _videoCapture.StartRecordingAsync(filePath, captureResult =>
                        {
                            Debug.Log("Started Recording Video!");
                            _filePath = filePath;
                            _recordCallback = callback;
                        });
                    }
                    else
                    {
                        Debug.LogError("VideoCaptureResult was not successful");
                        _recordCallback.Invoke(false, null);
                        ResetVideoRecorder();
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to create VideoCapture Instance!");
                _recordCallback.Invoke(false, null);
                ResetVideoRecorder();
            }
        });
    }

    private static void ResetVideoRecorder()
    {
        _recordCallback = null;
        _videoCapture?.Dispose();
        _videoCapture = null;
    }

    private static void StopVideoRecordingUWP()
    {
        if (_videoCapture == null || !_videoCapture.IsRecording)
        {
            Debug.LogError("No recording taking place");
            return;
        }

        _videoCapture.StopRecordingAsync(result =>
        {
            Debug.Log("Stopped Recording Video!");
            _videoCapture.StopVideoModeAsync(captureResult =>
            {
                _recordCallback.Invoke(captureResult.success, captureResult.success ? _filePath : null);
                ResetVideoRecorder();
                Debug.Log(captureResult);
            });
        });
    }

    private static CameraParameters CreateCameraVideoParameters()
    {
        Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).Last();
        float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending(fps => fps).First();

        var cameraParameters = new CameraParameters
        {
            hologramOpacity = 0.0f,
            frameRate = cameraFramerate,
            cameraResolutionWidth = cameraResolution.width,
            cameraResolutionHeight = cameraResolution.height,
            pixelFormat = CapturePixelFormat.BGRA32
        };
        return cameraParameters;
    }
#endif
}
