using i5.Toolkit.Core.ServiceCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    public class VestService : IService
    {
        public bool VestEnabled;

        public Sensor VestConfig;

        public void Initialize(IServiceManager owner)
        {
            EventManager.OnEnableVest += EnableVest;
            EventManager.OnDisableVest += DisableVest;


            var filePath = Path.Combine(Application.persistentDataPath, "vestconfig.json");

            // If config file doesn't yet exist in the HoloLens folder...
            if (!File.Exists(filePath))
            {
                // Copy the default config file to the HoloLens folder.
                var defaultFile = Resources.Load<TextAsset>("vestconfig");

                File.WriteAllText(filePath, defaultFile.text);
            }

            try
            {
                var sensorConfig = File.ReadAllText(filePath);

                VestConfig = JsonUtility.FromJson<Sensor>(sensorConfig);

                Debug.Log("VEST CONFIG LOADED!");
            }
            catch
            {
                Maggie.Speak("Vest configuration doesn't seem to be valid. You will not be able to enable vest.");
            }
        }

        public void Cleanup()
        {
            EventManager.OnEnableVest -= EnableVest;
            EventManager.OnDisableVest -= DisableVest;
        }

        private void EnableVest()
        {
            if (VestConfig != null)
            {
                VestEnabled = true;
                EventManager.PlayerReset();
                Maggie.Speak("Vest enabled.");
            }
            else 
                Maggie.Speak("Can not enable vest since a valid configuration file was not found.");
        }

        private void DisableVest()
        {
            if (VestConfig != null)
            {
                VestEnabled = false;
                EventManager.PlayerReset();
                Maggie.Speak("Vest disabled.");
            }
            else
                Maggie.Speak("Vest already disabled since a valid configuration file was not found.");
        }
    }
}