using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(Slider))]
public class VideoManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject stopButton;
    [SerializeField] private GameObject pauseButton;

    private VideoPlayer videoPlayer;
    private Slider progress;

    private bool slide = false;

    // Start is called before the first frame update
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        progress = GetComponent<Slider>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (videoPlayer.isPlaying && !slide)
        {
            progress.value = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        }

        ButtonsUpdate();
    }

    /// <summary>
    /// Hide play button and show pause and stop button while video is playing and vice versa
    /// </summary>
    private void ButtonsUpdate()
    {
        if (videoPlayer.isPlaying)
        {
            playButton.SetActive(false);
            pauseButton.SetActive(true);
            stopButton.SetActive(true);
            if (videoPlayer.frame > 0)
                progress.transform.Find("Handle").GetComponent<Image>().enabled = true; //show handle
        }
        else
        {
            playButton.SetActive(true);
            pauseButton.SetActive(false);
            stopButton.SetActive(false);
            progress.value = 0;
            progress.transform.Find("Handle").GetComponent<Image>().enabled = false; //hide handle
        }
    }

    /// <summary>
    /// Play the video
    /// </summary>
    public void PlayVideo()
    {
        videoPlayer.Play();
    }

    /// <summary>
    /// Stop the video
    /// </summary>
    public void StopVideo()
    {
        videoPlayer.Stop();
    }

    /// <summary>
    /// pause the video
    /// </summary>
    public void PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        slide = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float frame = progress.value * videoPlayer.frameCount;
        videoPlayer.frame = (long)frame;
        slide = false;
    }

}
