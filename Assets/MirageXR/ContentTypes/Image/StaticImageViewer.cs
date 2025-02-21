using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class StaticImageViewer : MonoBehaviour
    {
        [SerializeField] private GameObject ImageViewer;
        [SerializeField] private GameObject ContentStorage;
        [SerializeField] private GameObject ContentToggle;

        private RawImage _image;

        [SerializeField] private string ImageName;

        private void Start()
        {
            _image = ImageViewer.GetComponent<RawImage>();
        }

        public void ActivateImageViewer()
        {
            StartCoroutine(nameof(ActivateImageViewerRoutine));
        }

        public IEnumerator ActivateImageViewerRoutine()
        {
            if (ImageName.StartsWith("http") == false)
            {
                string dataPath = Application.persistentDataPath;
                string completeImageName = "file://" + dataPath + "/" + ImageName;
                Debug.LogTrace("Trying to load static image from:" + completeImageName);
                WWW www = new WWW(completeImageName);
                yield return www;
                Texture2D imageTex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(imageTex);
                _image.texture = imageTex;
            }
            else
            {
                // Online files stored locally.
                var url = ImageName.Split('/');
                var filename = url[url.Length - 1];

                var completeImageName = $"file://{LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath}/{filename}";

                Debug.LogTrace("Trying to load image from:" + completeImageName);

                WWW www = new WWW(completeImageName);
                yield return www;
                Texture2D imageTex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(imageTex);
                _image.texture = imageTex;

                // Online files.
                /*
                WWW www = new WWW (imageName);
                yield return www;
                Texture2D imageTex = new Texture2D (4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture (imageTex);
                renderer.sharedMaterial.SetTexture ("_MainTex", imageTex);
                */
            }
            ContentStorage.SetActive(false);
            ImageViewer.SetActive(true);
            ContentToggle.SetActive(false);
        }

        public void HideImageViewer()
        {
            ContentStorage.SetActive(true);
            ImageViewer.SetActive(false);
        }

        public void DeactivateImageViewer()
        {
            LearningExperienceEngine.EventManager.Click(); // why?
            HideImageViewer();
            ContentToggle.SetActive(true);
        }
    }
}