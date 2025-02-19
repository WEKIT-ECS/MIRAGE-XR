using LearningExperienceEngine;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.VerboseLogging;
using System.IO;
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

                VestConfig = JsonUtility.FromJson<LearningExperienceEngine.Sensor>(sensorConfig);

                AppLog.LogInfo($"[VestService] Sensor config file loaded from {filePath}");
            }
            catch
            {
                AppLog.LogInfo("[VestService] Sensor config file doesn't seem to be valid");
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
                LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.PlayerReset().AsAsyncVoid();
                AppLog.LogInfo("[VestService] Sensor communication is now enabled.");
            }
            else
            {
                AppLog.LogInfo("[VestService] Can not enable sensor communication because no valid configuration file could be found.");
            }
        }

        private void DisableVest()
        {
            if (VestConfig != null)
            {
                VestEnabled = false;
                LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.PlayerReset().AsAsyncVoid();
                AppLog.LogInfo("[VestService] Sensor communication disabled.");
            }
            else
            {
                AppLog.LogInfo("[VestService] Sensor communication already disabled since no valid configuration file could be found.");
            }
        }
    }
}