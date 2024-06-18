using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Model for all tutorial steps that are UI-based.
/// </summary>
public class TutorialStepModelUI : TutorialStepModel
{
    [JsonProperty]
    public override StepType StepType => StepType.UI;

    /// <summary>
    /// Enum for the vertical positioning of the message popup.
    /// </summary>
    public enum MessagePosition
    {
        Top,
        Middle,
        Bottom
    }

    /// <summary>
    /// ID used to find the item using the TutorialScript system.
    /// </summary>
    [JsonProperty]
    public string Id { get; set; }

    /// <summary>
    /// Textual guidance for the user.
    /// </summary>
    [JsonProperty]
    public string Message { get; set; }

    /// <summary>
    /// Defines the verticality of the message.
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public MessagePosition Position { get; set; } = MessagePosition.Middle;

    /// <summary>
    /// The text on the button used to close the tutorial.
    /// </summary>
    [JsonProperty]
    public string BtnText { get; set; } = "Cancel";

    /// <summary>
    /// Is the Id field given. If not, no attempt will be made to find or 
    /// highlight a UI item.
    /// </summary>
    [JsonIgnore]
    public bool HasId => !string.IsNullOrEmpty(Id);

    /// <summary>
    /// Is the message field given. If not, no textual popup will be shown.
    /// </summary>
    [JsonIgnore]
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public override string ToString()
    {
        return $"Id: {Id}, Message: {Message}, Position: {Position}, BtnText: {BtnText}";
    }
}
