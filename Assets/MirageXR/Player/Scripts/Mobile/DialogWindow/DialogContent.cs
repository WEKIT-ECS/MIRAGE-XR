public class DialogContent
{
    public string title => _title;
    public string message => _message;
    public DialogButtonContent[] buttonContents => _buttonContents;

    private readonly string _title;
    private readonly string _message;
    private readonly DialogButtonContent[] _buttonContents;

    public DialogContent(string title, string message, DialogButtonContent[] buttonContents)
    {
        _title = title;
        _message = message;
        _buttonContents = buttonContents;
    }
}