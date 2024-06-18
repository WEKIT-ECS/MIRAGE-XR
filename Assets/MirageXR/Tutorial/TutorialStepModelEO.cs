using MirageXR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class TutorialStepModelEO : TutorialStepModel
{
    [JsonProperty]
    public override StepType StepType => StepType.EventOnly;

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public TutorialManager.TutorialEvent FinishEvent { get; set; }

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public List<TutorialManager.TutorialEvent> CloseEvents { get; set; } = new List<TutorialManager.TutorialEvent>();

    public override bool IsValid()
    {
        if (FinishEvent != TutorialManager.TutorialEvent.NON_EVENT)
        {
            return true;
        }
        return false;
    }
}
