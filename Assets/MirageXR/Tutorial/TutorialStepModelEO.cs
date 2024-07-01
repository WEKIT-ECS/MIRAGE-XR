using MirageXR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

/// <summary>
/// Model for steps that are Event-Only. They do not have a visual component.
/// Mainly used for if the tutorial is waiting for a complex action to finish (such as Calibration).
/// </summary>
public class TutorialStepModelEO : TutorialStepModel
{
    /// <summary>
    /// Always returns EventOnly.
    /// </summary>
    [JsonProperty]
    public override StepType StepType => StepType.EventOnly;

    /// <summary>
    /// Determines whether the tutorial step is valid.
    /// </summary>
    /// <returns><c>true</c> if the step is valid; otherwise, <c>false</c>.</returns>
    public override bool IsValid()
    {
        if (FinishEvent != TutorialManager.TutorialEvent.NON_EVENT)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// The intended event to finish the step.
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public TutorialManager.TutorialEvent FinishEvent { get; set; }

    /// <summary>
    /// The list of predefined events that should close the tutorial.
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public List<TutorialManager.TutorialEvent> CloseEvents { get; set; } = new List<TutorialManager.TutorialEvent>();
}
