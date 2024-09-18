using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Model for all tutorial steps that are UI-based.
/// </summary>
public class TutorialStepModelUI : TutorialStepModel
{
    /// <summary>
    /// Always returns UI.
    /// </summary>
    [JsonProperty]
    public override StepType StepType => StepType.UI;

    /// <summary>
    /// Determines whether the tutorial step is valid.
    /// </summary>
    /// <returns><c>true</c> if the step is valid; otherwise, <c>false</c>.</returns>
    public override bool IsValid()
    {
        if (string.IsNullOrEmpty(Message) && string.IsNullOrEmpty(Id))
        {
            return false;
        }
        return true;
    }

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
    /// Defines if the current step has the option of "going next", i.e. if
    /// the step provides an option to the user of only interacting with the 
    /// tutorial UI to progress the tutorial.
    /// </summary>
    [JsonProperty]
    public bool CanGoNext { get; set; } = false;

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

    /// <summary>
    /// Formats the class for output.
    /// </summary>
    /// <returns>The class in string for output.</returns>
    public override string ToString()
    {
        return $"Id: {Id}, Message: {Message}, Position: {Position}, BtnText: {BtnText}";
    }
}
