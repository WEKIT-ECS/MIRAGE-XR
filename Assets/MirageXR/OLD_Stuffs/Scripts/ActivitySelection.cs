using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirageXR
{
    public class ActivitySelection : MonoBehaviour
    {
        private string _activityUrl;

        [SerializeField] private GameObject ReloadButton;

        private void Start()
        {
            _activityUrl = PlayerPrefs.GetString(name + "-url");

            if (!string.IsNullOrEmpty(_activityUrl))
                ReloadButton.SetActive(true);
        }
        /// <summary>
        /// Load activity on button press.
        /// </summary>
        public void LoadActivity (string url = null)
        {
            // Click.
            EventManager.Click();

            Debug.Log("PLAY BUTTON");

            // Default scenario.
            if (string.IsNullOrEmpty(url))
            {
                // Game object is named after the activity filename which we store to the player prefs.
                PlayerPrefs.SetString("activityUrl", name);
                PlayerPrefs.Save();

                // Once the filename is stored, simply load the main player scene.
                RootObject.Instance.activityManager.LoadActivity(name);
            }

            // For baked in activities. Add url to button configuration.
            else
            {
                
                PlayerPrefs.SetString("activityUrl", url);
                PlayerPrefs.Save();

                // Once the filename is stored, simply load the main player scene.
                //SceneManager.LoadScene(PlatformManager.Instance.GetPlayerSceneName);
                RootObject.Instance.activityManager.LoadActivity(url);
            }
        }

        public void ReloadActivity()
        {
            EventManager.Click();
            Debug.Log("Reloading " + _activityUrl);
            ActivitySelector.Instance.LoadActivity(_activityUrl);
        }
    }
}