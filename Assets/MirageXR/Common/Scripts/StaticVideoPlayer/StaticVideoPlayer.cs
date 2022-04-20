using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace MirageXR
{
    public class StaticVideoPlayer : MonoBehaviour
    {
        [SerializeField] private GameObject VideoPlayer;
        [SerializeField] private GameObject ContentStorage;
        [SerializeField] private GameObject ContentToggle;

        [SerializeField] private string VideoName;

        public void ActivateVideoPlayer()
        {
            ContentStorage.SetActive(false);

            var videoPlayer = VideoPlayer.GetComponent<VideoPlayer>();
            VideoPlayer.SetActive(true);

            if (VideoName.StartsWith("http") == false)
            {
                // Locally stored file
                string dataPath = Application.persistentDataPath;
                string completeVideoName = "file://" + dataPath + "/" + VideoName;
                Debug.Log("Trying to load video: " + completeVideoName);
                videoPlayer.url = completeVideoName;
            }
            else
            {
                // Online file stored locally
                var url = VideoName.Split('/');
                var filename = url[url.Length - 1];

                var completeVideoName = "file://" + ActivityManager.Instance.Path + "/" + filename;
                Debug.Log("Trying to load video: " + completeVideoName);
                videoPlayer.url = completeVideoName;
            }

            if (VideoName.StartsWith("resources://"))
            {
                var cleanedVideoName = VideoName.Replace("resources://", "");
                // Video stored in Unity project's "Resources" folder
                if (cleanedVideoName.EndsWith(".mp4") || cleanedVideoName.EndsWith(".mov"))
                {
                    cleanedVideoName = cleanedVideoName.Substring(0, cleanedVideoName.Length - 4);
                }
                Debug.Log("Trying to load video: " + cleanedVideoName);
                VideoClip videoClip = Resources.Load(cleanedVideoName, typeof(VideoClip)) as VideoClip;
                videoPlayer.clip = videoClip;
            }

            videoPlayer.Play();
            ContentToggle.SetActive(false);
        }

        public void HideVideoPlayer()
        {
            ContentStorage.SetActive(true);
            VideoPlayer.GetComponent<VideoPlayer>().Stop();
            VideoPlayer.SetActive(false);
        }

        public void DeactivateVideoPlayer()
        {
            EventManager.Click();
            HideVideoPlayer();
            ContentToggle.SetActive(true);
        }
    }
}