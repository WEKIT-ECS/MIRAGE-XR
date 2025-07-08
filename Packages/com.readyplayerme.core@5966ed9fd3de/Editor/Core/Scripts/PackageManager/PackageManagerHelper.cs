using System;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    public abstract class PackageManagerHelper
    {
        private const string TAG = nameof(PackageManagerHelper);
        private const int THREAD_SLEEP_TIME = 100;
        private const float TIMEOUT_FOR_PACKAGE_INSTALLATION = 20f;
        private const string READY_PLAYER_ME_PACKAGE_PATH = "Packages/com.readyplayerme.core";

        public static bool IsPackageInstalled(string name)
        {
            return GetPackageList().Any(info => info.name == name);
        }

        /// <summary>
        ///     Get the list of unity packages installed in the current project.
        /// </summary>
        /// <returns>An array of <c>PackageInfo</c>.</returns>
        public static PackageInfo[] GetPackageList()
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(THREAD_SLEEP_TIME);

            if (listRequest.Error == null)
            {
                return listRequest.Result.ToArray();
            }

            SDKLogger.Log(TAG, "Error: " + listRequest.Error.message);
            return Array.Empty<PackageInfo>();

        }

        public static string GetSdkPackageSourceUrl()
        {
            var sdkPackageInfo = PackageInfo.FindForAssetPath(READY_PLAYER_ME_PACKAGE_PATH);

            return sdkPackageInfo?.packageId
                .Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
        }

        public static void AddPackage(string identifier)
        {
            var startTime = Time.realtimeSinceStartup;
            var addRequest = Client.Add(identifier);

            while (!addRequest.IsCompleted && Time.realtimeSinceStartup - startTime < TIMEOUT_FOR_PACKAGE_INSTALLATION)
                Thread.Sleep(THREAD_SLEEP_TIME);

            if (Time.realtimeSinceStartup - startTime >= TIMEOUT_FOR_PACKAGE_INSTALLATION)
            {
                Debug.LogError($"Package installation timed out for {identifier}. Please try again.");
            }
            if (addRequest.Error != null)
            {
                Debug.LogError("Error: " + addRequest.Error.message);
            }
        }
    }
}
