using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Model for all items in the help selection dialogue.
    /// Should be serialisable.
    /// </summary>
    public class HelpSelectionModel
    {
        /// <summary>
        /// Text that should be shown in the help popup, when the user is selecting help.
        /// </summary>
        [JsonProperty("selection_text")] public string SelectionText { get; set; }
        /// <summary>
        /// Items which are to be shown in order. Can also only be one item.
        /// </summary>
        [JsonProperty("tutorial_steps")] public List<TutorialModelUI> TutorialSteps { get; set; }
        /// <summary>
        /// If given, holds the name of a predefined tutorial from the TutorialManager to be shown
        /// </summary>
        [JsonProperty("starts_tutorial")] public string StartsTutorial { get; set; }
        /// <summary>
        /// Should the selection only be shown in edit mode.
        /// </summary>
        [JsonProperty("edit_mode_only")] public bool EditModeOnly { get; set; }

        public HelpSelectionModel()
        {
            SelectionText = "";
            TutorialSteps = new List<TutorialModelUI>();
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
}
