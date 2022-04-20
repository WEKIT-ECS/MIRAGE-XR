using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using Vuforia;
using MirageXR;
using i5.Toolkit.Core.ServiceCore;

public class MirageXRVideoAnnotationEditor : MonoBehaviour
{
#if UNITY_WSA
    UnityEngine.Windows.WebCam.VideoCapture videoCaptureObject = null;
#endif
    private bool isRecording = false;
    private string SaveFileName = "VideoTest.mp4";
    [SerializeField] private RawImage image;
    //public VideoClip videoToPlay;
    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
    private Transform videoDisplay;

    public void Start()
    {

        videoDisplay = transform.Find("VideoDisplay");
        videoDisplay.GetComponent<Renderer>().enabled = false;
    }

    /// <summary>
    /// entry point for recording
    /// </summary>
    public void RecordVideo()
    {
        Debug.Log("Record Video");
        VuforiaUnity.Deinit();
        isRecording = true;

#if UNITY_WSA
        UnityEngine.Windows.WebCam.VideoCapture.CreateAsync(false, OnVideoCaptureCreated);
#endif
    }
    
#if UNITY_WSA
    private void OnVideoCaptureCreated(UnityEngine.Windows.WebCam.VideoCapture videoCapture)
    {
        Debug.Log("onVideoCaptureCreated");
        
        if (videoCapture != null)
        {
            videoCaptureObject = videoCapture;

            Resolution cameraResolution = UnityEngine.Windows.WebCam.VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            float cameraFramerate = UnityEngine.Windows.WebCam.VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();

            UnityEngine.Windows.WebCam.CameraParameters cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.frameRate = cameraFramerate;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32;

            videoCaptureObject.StartVideoModeAsync(cameraParameters,
                                                UnityEngine.Windows.WebCam.VideoCapture.AudioState.None,
                                                StartRecordingVideo);
        }
        else
        {
            Debug.LogError("Failed to create VideoCapture Instance!");
        }
    }
#endif
  
#if UNITY_WSA
    /// <summary>
    /// Start Recording
    /// </summary>
    /// <param name="result"></param>
    private void StartRecordingVideo(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("startrecordingvideo");
        if (result.success)
        {
            SaveFileName = "WEKIT_Video_" + System.DateTime.Now.ToFileTimeUtc() + ".mp4";
            //string filename = string.Format("TaskStation_{0}.mp4", Time.time);
            //string filepath = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
            string filepath = System.IO.Path.Combine(
                ServiceManager.GetService<ActivityRecorderService>().CurrentArlemFolder, SaveFileName);

            videoCaptureObject.StartRecordingAsync(filepath, OnStartedRecordingVideo);

        }
    }
#endif

#if UNITY_WSA
    /// <summary>
    /// update UI or behaviors to enable stopping
    /// </summary>
    /// <param name="result"></param>
    void OnStartedRecordingVideo(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Started Recording Video!");
        
        //gameObject.GetComponent<MirageXRVideoAnnotationEditor>().changeColor(Color.green);
        // We will stop the video from recording via other input such as a timer or a tap, etc.
    }
#endif

    /// <summary>
    /// User has intended to stop recording
    /// </summary>
    void StopRecordingVideo()
    { 
#if UNITY_WSA
        //gameObject.GetComponent<MirageXRVideoAnnotationEditor>().changeColor(Color.yellow);
        videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
#endif
    }

#if UNITY_WSA
    /// <summary>
    /// recording stopped
    /// </summary>
    /// <param name="result"></param>
    void OnStoppedRecordingVideo(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Stopped Recording Video!");
        isRecording = false;
        videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
        
        VuforiaRuntime.Instance.InitVuforia();
    }
#endif

#if UNITY_WSA
    void OnStoppedVideoCaptureMode(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
       videoCaptureObject.Dispose();
       videoCaptureObject = null;
    }
#endif


    /// <summary>
    /// Entry method to play video
    /// </summary>
    void PlayBackVideo()
    {
        Debug.Log("Play Video");
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        videoDisplay.GetComponent<Renderer>().enabled = true;
        //Add VideoPlayer to the GameObject
        videoPlayer = GameObject.Find("VideoDisplay").AddComponent<VideoPlayer>();
        //videoPlayer = gameObject.transform.Find("VideoDisplay").
        
        Debug.Log("Video Player Name: "+ videoPlayer.name);

        //Add AudioSource
        //audioSource = gameObject.AddComponent<AudioSource>();

        //Disable Play on Awake for both Video and Audio
        videoPlayer.playOnAwake = true;
        //audioSource.playOnAwake = false;

//        string url = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        string url = System.IO.Path.Combine(ServiceManager.GetService<ActivityRecorderService>().CurrentArlemFolder, SaveFileName);

#if !UNITY_EDITOR && UNITY_ANDROID
            url = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
#endif
        //We want to play from video clip not from url
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        Debug.Log("Video Player URL: " + videoPlayer.url);

        //Set Audio Output to AudioSource
        //videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        //Assign the Audio from Video to AudioSource to be played
        //videoPlayer.EnableAudioTrack(0, true);
        //videoPlayer.SetTargetAudioSource(0, audioSource);

        //Set video To Play then prepare Audio to prevent Buffering
        //videoPlayer.clip = videoToPlay;

        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared)
        {
            Debug.Log("Preparing Video");
            yield return null;
        }

        Debug.Log("Done Preparing Video "+ videoPlayer.url);

        //Assign the Texture from Video to RawImage to be displayed
        image.texture = videoPlayer.texture;
        videoDisplay.GetComponent<Renderer>().material.mainTexture = image.texture;

         //Play Video
        videoPlayer.Play();

        //Play Sound
        //audioSource.Play();

        Debug.Log("Playing Video");
        while (videoPlayer.isPlaying)
        {
            Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }

        Debug.Log("Done Playing Video");
    }

}
