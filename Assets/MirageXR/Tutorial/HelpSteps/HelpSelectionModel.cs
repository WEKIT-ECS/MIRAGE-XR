using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    public class HelpSelectionModel
    {
        [JsonProperty("selection_text")] public string SelectionText { get; set; }
        [JsonProperty("tutorial_steps")] public List<TutorialModel> TutorialSteps { get; set; }
        [JsonProperty("starts_tutorial")] public string StartsTutorial { get; set; }
        [JsonProperty("edit_mode_only")] public bool EditModeOnly { get; set; }

        public HelpSelectionModel()
        {
            SelectionText = "";
            TutorialSteps = new List<TutorialModel>();
            StartsTutorial = "";
            EditModeOnly = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"SelectionText: {SelectionText}");
            sb.AppendLine($"TutorialSteps Count: {TutorialSteps?.Count ?? 0}");
            sb.AppendLine($"StartsTutorial: {StartsTutorial}");
            sb.AppendLine($"EditModeOnly: {EditModeOnly}");
            return sb.ToString();
        }
    }

    public class HelpSelectionMaster
    {
        private const string FILE_NAME_ACTIVITY_SELECTION = "activitySelectionHelp.json";
        private const string FILE_NAME_ACTIVITY_STEPS = "activityStepsHelp.json";
        private const string FILE_NAME_ACTIVITY_INFO = "activityStepsInfo.json";
        private const string FILE_NAME_ACTIVITY_CALIBRATION = "activityCalibrationHelp.json";
        private const string FILE_NAME_STEP_AUGMENTATIONS = "stepAugmentationsHelp.json";
        private const string FILE_NAME_STEP_INFO = "stepInfoHelp.json";

        public void TestRead()
        {
            string path = Path.Combine(Application.dataPath, "MirageXR", "Resources", FILE_NAME_ACTIVITY_SELECTION);
            // Read the JSON string from the file
            string jsonString = File.ReadAllText(path);

            HelpSelectionModel hsm = JsonConvert.DeserializeObject<HelpSelectionModel>(jsonString);
            Debug.Log(hsm);
        }

        public void TestWrite()
        {
            HelpSelectionModel model = new HelpSelectionModel();
            model.SelectionText = "Select this";

            TutorialModel tutModel = new TutorialModel();
            model.TutorialSteps.Add(tutModel);

            model.StartsTutorial = "";

            string jsonString = JsonConvert.SerializeObject(model, Formatting.Indented);
            string path = Path.Combine(Application.dataPath, "MirageXR", "Resources", FILE_NAME_ACTIVITY_SELECTION);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write the JSON string to the file
            File.WriteAllText(path, jsonString);

            // Refresh the AssetDatabase to ensure the file is visible in the Unity Editor
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }


    }
}
