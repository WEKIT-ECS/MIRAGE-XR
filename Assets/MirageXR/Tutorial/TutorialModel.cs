using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/// <summary>
/// Model for all tutorials that are to be processed by the 
/// TutorialManager.
/// </summary>
public class TutorialModel
{
    /// <summary>
    /// Name of the tutorial. Optional.
    /// </summary>
    [JsonProperty]
    public string Name { get; set; }

    /// <summary>
    /// Intended platform for the tutorial to be played on. Optional.
    /// </summary>
    [JsonProperty]
    public string IntendedPlatform { get; set; }

    /// <summary>
    /// Tutorial steps to be experienced, in order.
    /// </summary>
    [JsonProperty]
    public List<TutorialStepModel> Steps { get; set; }
}

/// <summary>
/// Step types available so far. Different step types handled by
/// different handlers and should be matched to this enum.
/// </summary>
public enum StepType
{
    UI,
    World,
    EventOnly
}

/// <summary>
/// The abstract model for all tutorial steps.
/// </summary>
[JsonConverter(typeof(TutorialStepConverter))]
public abstract class TutorialStepModel
{
    /// <summary>
    /// The type of this step. Indicated which step handler it should
    /// be processed by.
    /// </summary>
    public abstract StepType StepType { get; }

    /// <summary>
    /// A period of time that should be waited before processing the step (like showing).
    /// This is mostly for waiting for elements of the scene to load first.
    /// </summary>
    [JsonProperty]
    public float DelayInMilliseconds { get; private set; } = 0f;

    /// <summary>
    /// This method should tell if the step is valid or not based on sub-class specific
    /// determinants.
    /// </summary>
    /// <returns>True if valid, false if not.</returns>
    public abstract bool IsValid();
}

/// <summary>
/// Custom converter for TutorialStep to handle the serialization and deserialization of derived types.
/// </summary>
public class TutorialStepConverter : JsonConverter
{
    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(TutorialStepModel).IsAssignableFrom(objectType);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of the object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
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
            case StepType.EventOnly:
                step = new TutorialStepModelEO();
                break;
            default:
                throw new ArgumentException("Invalid step type");
        }

        serializer.Populate(jsonObject.CreateReader(), step);
        return step;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var jsonObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);

        // Manually add the StepType property to ensure it's serialized correctly
        jsonObject.AddFirst(new Newtonsoft.Json.Linq.JProperty("StepType", (value as TutorialStepModel).StepType));

        jsonObject.WriteTo(writer);
    }
}

