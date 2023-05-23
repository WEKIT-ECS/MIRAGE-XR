namespace MRTKUtilities.Application
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Class for access of application settings, including saving and restoring on disk.
    /// 
    /// NOTE:
    /// We're reading appsettings.json from Assets\StreamingAssets\Resources. This file shouldn't
    /// be stored in your repo. To run this locally or publish an app package, you should add this
    /// file during development or in the pipeline so the application knows the secrets to 
    /// authenticate and such.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the application settings.
        /// </summary>
        private AppSettings _settings = new AppSettings();
        public AppSettings Settings => _settings;

        /// <summary>
        /// Read the settings from the appsettings.json file in Assets/Resources/appsettings.json.
        /// </summary>
        public bool LoadSettings()
        {
            // always initialize settings
            _settings = new AppSettings();
            string path = Path.Combine(Application.streamingAssetsPath, "appsettings.json");

            try
            {
                Debug.Log($"Retrieving settings from {path}");
                if (!File.Exists(path))
                {
                    Debug.Log($"SettingsManager.LoadSettings ERROR: Make sure settings are provided in `{path}`. See README for details on the settings.");
                    return false;
                }

                // load from disk and deserialize
                string json = File.ReadAllText(path);
                _settings = JsonConvert.DeserializeObject<AppSettings>(json);

                if (!Settings.IsValid())
                {
                    Debug.LogError($"SettingsManager.LoadSettings ERROR: Settings are invalid. See README for details on the settings.");
                    return false;
                }

                // log only first part of the secrets
                Debug.Log($"ClientId: {Settings.ClientId.Substring(0, 4)}***");
                Debug.Log($"TenantId: {Settings.TenantId.Substring(0, 4)}***");
                Debug.Log($"Scopes: {Settings.Scopes.Substring(0, 10)}***");
                Debug.Log($"Resource: {Settings.Resource.Substring(0, 10)}***");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager.Load ERROR:\n{ex}");
                return false;
            }
        }
    }
}