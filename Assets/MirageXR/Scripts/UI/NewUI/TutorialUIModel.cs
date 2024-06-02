// TODO: Comment the class
public class TutorialModel
{
    public enum MessagePosition
    {
        Top,
        Middle,
        Bottom
    }

    public string Id { get; set; }
    public string Message { get; set; }
    public MessagePosition Position { get; set; } = MessagePosition.Middle;
    public string BtnText { get; set; } = "Cancel";

    public bool HasId => !string.IsNullOrEmpty(Id);

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public override string ToString()
    {
        return $"Id: {Id}, Message: {Message}, Position: {Position}, BtnText: {BtnText}";
    }
}
