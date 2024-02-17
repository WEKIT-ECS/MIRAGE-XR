using i5.Toolkit.Core.ServiceCore;
using System.IO;
using TiltBrush;
using UnityEngine;

namespace MirageXR
{
    public class VestService : IService
    {
        public bool VestEnabled { get; set; }

        public Sensor VestConfig { get; private set; }

        public void Initialize(IServiceManager owner)
        {
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
        }

        private void EnableVest()
        {
            if (VestConfig != null)
            {
                VestEnabled = true;
                RootObject.Instance.activityManager.PlayerReset().AsAsyncVoid();
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
                RootObject.Instance.activityManager.PlayerReset().AsAsyncVoid();
                Maggie.Speak("Vest disabled.");
            }
            else
                Maggie.Speak("Vest already disabled since a valid configuration file was not found.");
        }
    }
}