using MirageXR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepModelWS : TutorialStepModel
{
    [JsonProperty]
    public override StepType StepType => StepType.World;

    public override bool IsValid()
    {
        if (FinishEvent != TutorialManager.TutorialEvent.NON_EVENT)
        {
            return true;
        }
        return false;
    }

    [JsonProperty]
    public string FocusObject { get; set; }

    [JsonProperty]
    public string ActualTarget { get; set; }

    [JsonProperty]
    public string Message { get; set; }

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public TutorialManager.TutorialEvent FinishEvent { get; set; }

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public List<TutorialManager.TutorialEvent> CloseEvents { get; set; }
}
