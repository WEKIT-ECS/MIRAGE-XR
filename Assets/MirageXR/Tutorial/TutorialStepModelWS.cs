using MirageXR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Model for World Space tutorial steps.
/// </summary>
public class TutorialStepModelWS : TutorialStepModel
{
    /// <summary>
    /// Always returns World.
    /// </summary>
    [JsonProperty]
    public override StepType StepType => StepType.World;

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
    /// The object in focus, which is to be highlighted. Should be id of findable
    /// GameObject.
    /// </summary>
    [JsonProperty]
    public string FocusObject { get; set; }

    /// <summary>
    /// Actual target within the object in focus. Should be id of a child/component of the
    /// GameObject that <see cref="FocusObject"/> identifies.
    /// </summary>
    [JsonProperty]
    public string ActualTarget { get; set; }

    /// <summary>
    /// Textual guidance for the user.
    /// </summary>
    [JsonProperty]
    public string Message { get; set; }

    /// <summary>
    /// The type of arrow to display in the tutorial step.
    /// </summary>
    [JsonProperty]
    public TutorialArrowFactory.ArrowType ArrowType { get; set; } = TutorialArrowFactory.ArrowType.DEFAULT;

    /// <summary>
    /// The position offset for the arrow.
    /// </summary>
    [JsonProperty]
    public Vector3 arrowPositionOffset { get; set; } = Vector3.zero;

    /// <summary>
    /// The rotation offset for the arrow.
    /// </summary>
    [JsonProperty]
    public Vector3 arrowRotationOffset { get; set; } = Vector3.zero;

    /// <summary>
    /// The event that invokes the completion of the tutorial step.
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
