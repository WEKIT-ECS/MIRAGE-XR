using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

public class TutorialModel
{
    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public string IntendedPlatform { get; set; }

    [JsonProperty]
    public List<TutorialStepModel> Steps { get; set; }
}

public enum StepType
{
    UI,
    World
}

[JsonConverter(typeof(TutorialStepConverter))]
public abstract class TutorialStepModel
{
    public abstract StepType StepType { get; }

    public abstract bool IsValid();
}

// Custom converter for TutorialStep to handle the serialization and deserialization of derived types
public class TutorialStepConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(TutorialStepModel).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
        var stepType = jsonObject["StepType"].ToObject<StepType>();

        TutorialStepModel step;
        switch (stepType)
        {
            case StepType.UI:
                step = new TutorialStepModelUI();
                break;
            case StepType.World:
                step = new TutorialStepModelWS();
                break;
            default:
                throw new ArgumentException("Invalid step type");
        }

        serializer.Populate(jsonObject.CreateReader(), step);
        return step;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var jsonObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
        jsonObject.WriteTo(writer);
    }
}
