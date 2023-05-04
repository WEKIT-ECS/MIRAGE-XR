using System;

public class DialogButtonContent
{
    public string text => _text;
    public Action action => _action;
    public Action<string> stringAction => _stringAction;
    public bool isWarning => _isWarning;
    public bool isSelected => _isSelected;

    private readonly string _text;
    private readonly Action _action;
    private readonly Action<string> _stringAction;
    private readonly bool _isWarning;
    private readonly bool _isSelected;

    public DialogButtonContent(string text)
    {
        _text = text;
    }

    public DialogButtonContent(string text, Action action, bool isWarning = false, bool isSelected = false)
    {
        _text = text;
        _action = action;
        _isWarning = isWarning;
        _isSelected = isSelected;
    }

    public DialogButtonContent(string text, Action<string> stringAction)
    {
        _text = text;
        _stringAction = stringAction;
    }
}