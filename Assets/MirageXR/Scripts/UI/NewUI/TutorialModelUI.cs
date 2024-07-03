
/// <summary>
/// Model for all tutorial steps that are UI-based.
/// </summary>
public class TutorialModelUI
{
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
    public string Id { get; set; }
    /// <summary>
    /// Textual guidance for the user.
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// Defines the verticality of the message.
    /// </summary>
    public MessagePosition Position { get; set; } = MessagePosition.Middle;
    /// <summary>
    /// The text on the button used to close the tutorial.
    /// </summary>
    public string BtnText { get; set; } = "Cancel";

    /// <summary>
    /// Is the Id field given. If not, no attempt will be made to find or 
    /// highlight a UI item.
    /// </summary>
    public bool HasId => !string.IsNullOrEmpty(Id);

    /// <summary>
    /// Is the message field given. If not, no textual popup will be shown.
    /// </summary>
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public override string ToString()
    {
        return $"Id: {Id}, Message: {Message}, Position: {Position}, BtnText: {BtnText}";
    }
}
