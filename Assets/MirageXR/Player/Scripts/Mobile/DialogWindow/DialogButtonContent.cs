using System;

public class DialogButtonContent
{
    public string text => _text;
    public Action action => _action;
    public bool isWarning => _isWarning;

    private readonly string _text;
    private readonly Action _action;
    private readonly bool _isWarning;

    public DialogButtonContent(string text, Action action = null, bool isWarning = false)
    {
        _text = text;
        _action = action;
        _isWarning = isWarning;
    }
}