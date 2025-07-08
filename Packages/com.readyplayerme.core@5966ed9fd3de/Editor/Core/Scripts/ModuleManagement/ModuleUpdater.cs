using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ReadyPlayerMe.Core.Editor
{
    /// <summary>
    /// It is responsible for checking and updating the Ready Player Me SDK modules.
    /// </summary>
    [InitializeOnLoad]
    public class ModuleUpdater
    {

        private class Release
        {
            [JsonProperty("tag_name")]
            public string Tag;
        }

        private const string PACKAGE_JSON = "package.json";
        private const string PACKAGE_DOMAIN = "com.readyplayerme";

        private const string GITHUB_WEBSITE = "https://github.com";
        private const string GITHUB_API_URL = "https://api.github.com/repos";

        private const int MILLISECONDS_TIMEOUT = 20;
        private const string ASSET_FILTER = "package";

        private const string DONT_ASK = "Dont Ask";
        private const string UPDATE_PACKAGES_WINDOW_TITLE = "Update Packages";
        private const string UPDATE_BUTTON_TEXT = "Update";
        private const string CANCEL_BUTTON_TEXT = "Cancel";
        private const string DONT_ASK_TEXT = "Don't ask";

        private const string AVATAR_LOADER_PACKAGE = "com.readyplayerme.avatarloader";

        static ModuleUpdater()
        {
            EntryPoint.Startup += () => Check(true);
        }

        /// <summary>
        /// Check for Ready Player Me package updates.
        /// </summary>
        [MenuItem("Tools/Ready Player Me/Check For Updates", priority = 23)]
        public static void CheckForUpdates()
        {
            AnalyticsEditorLogger.EventLogger.LogCheckForUpdates();
            Check();
        }

        private static void Check(bool isStartup = false)
        {
            // Get PackageInfo array from RPM Module package.json files
            PackageInfo[] packages = AssetDatabase.FindAssets(ASSET_FILTER)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => x.Contains(PACKAGE_JSON) && x.Contains(PACKAGE_DOMAIN))
                .Select(PackageInfo.FindForAssetPath)
                .ToArray();

            if (packages.Length == 0)
            {
                Debug.Log("No rpm package found");
            }

            // Turn package_name@repo_url#branch_name into https://api.github.com/repos/readyplayerme/repo_name/releases 
            foreach (PackageInfo package in packages)
            {
                var repoUrl = package.packageId.Split('@')[1];
                var releasesUrl = repoUrl
                    .Split(new[] { ".git" }, StringSplitOptions.None)[0]
                    .Replace(GITHUB_WEBSITE, GITHUB_API_URL) + "/releases";

                var packageUrl = repoUrl.Split('#')[0];

                // Experimental or prerelease packages might look like 0.1.0-exp.1, remove after dash to parse with Version
                var version = package.version.Split('-')[0];

                if (isStartup && EditorPrefs.GetBool(DONT_ASK + "-" + package.name))
                {
                    continue;
                }

                FetchReleases(package.name, packageUrl, releasesUrl, new Version(version));
            }
        }

        /// <summary>
        ///     Fetch latest release for each module and prompt for update if available.
        /// </summary>
        /// <param name="packageName">The name of the Unity package.</param>
        /// <param name="packageUrl">The Git URL of the Unity package.</param>
        /// <param name="releasesUrl">The Git URL of the Unity package.</param>
        /// <param name="currentVersion">The current version of the package.</param>
        private static async void FetchReleases(string packageName, string packageUrl, string releasesUrl,
            Version currentVersion)
        {
            UnityWebRequest request = UnityWebRequest.Get(releasesUrl);
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;
                Release[] releases = JsonConvert.DeserializeObject<Release[]>(response);
                Version[] versions = releases!.Select(r => new Version(r.Tag.Substring(1).Split('-')[0])).ToArray();

                Version latestVersion = versions.Max();

                if (latestVersion > currentVersion)
                {
                    DisplayUpdateDialog(packageName, currentVersion, latestVersion, packageUrl);
                }
            }
            else
            {
                Debug.Log($"Failed to fetch {packageName} releases. Error: {request.error} ");
            }
        }

        /// <summary>
        ///     Display a Unity popup with notification about available package updates with buttons to update or skip.
        /// </summary>
        /// <param name="packageName">The name of the Unity package.</param>
        /// <param name="currentVersion">The current version of the package.</param>
        /// <param name="latestVersion">The new version of the package.</param>
        /// <param name="packageUrl">The Git URL of the Unity package.</param>
        private static void DisplayUpdateDialog(string packageName, Version currentVersion, Version latestVersion,
            string packageUrl)
        {
            var shouldUpdate = EditorUtility.DisplayDialogComplex(UPDATE_PACKAGES_WINDOW_TITLE,
                $"New update available for {packageName}\nCurrent version: {currentVersion}\nLatest version: {latestVersion}",
                UPDATE_BUTTON_TEXT,
                CANCEL_BUTTON_TEXT,
                DONT_ASK_TEXT);

            switch (shouldUpdate)
            {
                // Update
                case 0:
                    CheckIfMajorRelease(packageName, currentVersion, latestVersion, packageUrl);
                    break;
                // Cancel
                case 1:
                    // Do nothing
                    break;
                // Don't ask
                case 2:
                    EditorPrefs.SetBool(DONT_ASK + "-" + packageName, true);
                    break;
            }
        }

        private static void CheckIfMajorRelease(string packageName, Version currentVersion, Version latestVersion,
            string packageUrl)
        {
            if (latestVersion.Major > currentVersion.Major)
            {
                BreakingChangeDialog.ShowDialog(() =>
                {
                    UpdateModule(packageName, packageUrl, currentVersion, latestVersion);
                });

            }
            else
            {
                UpdateModule(packageName, packageUrl, currentVersion, latestVersion);
            }
        }

        /// <summary>
        ///     Update the specified module by removing the current version and then adding the specified version.
        /// </summary>
        /// <param name="name">The name of the Unity package.</param>
        /// <param name="url">The Git URL of the Unity package.</param>
        /// <param name="current">The current version of the package.</param>
        /// <param name="latest">The new version of the package.</param>
        private static void UpdateModule(string name, string url, Version current, Version latest)
        {
            url += "#v" + latest;
            CleanRedundantAvatarLoader();
            RemoveRequest removeRequest = Client.Remove(name);
            while (!removeRequest.IsCompleted) Thread.Sleep(MILLISECONDS_TIMEOUT);

            AddRequest addRequest = Client.Add(url);
            while (!addRequest.IsCompleted) Thread.Sleep(MILLISECONDS_TIMEOUT);

            Debug.Log($"Updated {name} from v{current} to v{latest}");
        }

        private static void CleanRedundantAvatarLoader()
        {
            if (!ModuleInstaller.IsModuleInstalled(AVATAR_LOADER_PACKAGE))
            {
                return;
            }

            RemoveRequest removeRequest = Client.Remove(AVATAR_LOADER_PACKAGE);
            while (!removeRequest.IsCompleted) Thread.Sleep(MILLISECONDS_TIMEOUT);
        }
    }
}
