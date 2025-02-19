using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.VerboseLogging;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    [RequireComponent(typeof(SessionContainerListItem))]
    public class SessionButton : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private SessionContainerListItem _selectedListViewItem;

        public async void OnActivitySelected()
        {
            // download if not local
            if (!_selectedListViewItem.Content.ExistsLocally)
            {
                await DownloadAsync();
            }
            // else: play the activity
            else
            {
                // show loading label
                Loading.Instance.LoadingVisibility(true);

                Debug.LogInfo("Playing activity...");
                Play();
            }
        }

        private async Task DownloadAsync()
        {
            Debug.LogInfo("Downloading session...");
            // reset any error
            _selectedListViewItem.Content.HasError = false;
            // indicate that the system is working
            _selectedListViewItem.IsDownloading = true;
            _selectedListViewItem.UpdateDisplay();

            bool success;
            LearningExperienceEngine.Session arlemFile = _selectedListViewItem.Content.Session;
            Debug.LogInfo($"Downloading from {arlemFile.contextid}/{arlemFile.component}/{arlemFile.filearea}/{arlemFile.itemid}/{arlemFile.filename}");
            using (LearningExperienceEngine.SessionDownloader downloader = new LearningExperienceEngine.SessionDownloader($"{LearningExperienceEngine.UserSettings.domain}/pluginfile.php/{arlemFile.contextid}/{arlemFile.component}/{arlemFile.filearea}/{arlemFile.itemid}/{arlemFile.filename}", arlemFile.sessionid + ".zip"))
            {
                success = await downloader.DownloadZipFileAsync();

                if (success)
                {
                    try
                    {
                        await downloader.UnzipFileAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        success = false;
                    }
                }
            }

            if (success)
            {
                string[] possibleSuffixes = { "-activity.json", "_activity.json", "activity.json" };

                for (int i = 0; i < possibleSuffixes.Length; i++)
                {
                    string activityFileName = Path.Combine(Application.persistentDataPath, _selectedListViewItem.Content.FileIdentifier + possibleSuffixes[i]);
                    if (File.Exists(activityFileName))
                    {
                        string jsonContent = File.ReadAllText(activityFileName);
                        _selectedListViewItem.Content.Activity = JsonUtility.FromJson<LearningExperienceEngine.Activity>(jsonContent);
                        _selectedListViewItem.UpdateDisplay();
                        break;
                    }
                }
            }

            _selectedListViewItem.Content.HasError = !success;
            _selectedListViewItem.IsDownloading = false;
            _selectedListViewItem.UpdateDisplay();
        }

        private async void Play()
        {
            string activityJsonFileName = LearningExperienceEngine.LocalFiles.GetActivityJsonFilename(_selectedListViewItem.Content.FileIdentifier);
            PlayerPrefs.SetString("activityUrl", activityJsonFileName);
            PlayerPrefs.Save();

            // update the view of the activity on Moodle server after loading
            await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.UpdateViewsOfActivity(_selectedListViewItem.Content.ItemID, _selectedListViewItem.Content.ExistsRemotely);

            //StartCoroutine(SwitchToPlayerScene(activityJsonFileName));
            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await activityManager.LoadActivity(activityJsonFileName);

            // Set the activity URL
            activityManager.AbsoluteURL = _selectedListViewItem.Content.AbsoluteURL;
        }

        public async void OnDeleteButtonPressed()
        {
            if (LearningExperienceEngine.LocalFiles.TryDeleteActivity(_selectedListViewItem.Content.Activity.id))
            {
                // reload the activity list
                var listView = FindObjectOfType<SessionListView>();
                await listView.CollectAvailableSessionsAsync();
            }
        }

        // private IEnumerator SwitchToPlayerScene(string activityJsonFileName)
        // {
        // yield return SceneManager.LoadSceneAsync(RootObject.Instance.platformManager.GetPlayerSceneName, LoadSceneMode.Additive);
        // // wait one more frame for everything to set up
        // yield return null;
        // EventManager.ParseActivity(activityJsonFileName);
        // yield return SceneManager.UnloadSceneAsync("ActivitySelection");
        // }
    }
}
