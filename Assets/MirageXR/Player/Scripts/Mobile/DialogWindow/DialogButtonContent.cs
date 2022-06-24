using System;

public class DialogButtonContent
{
    public string text => _text;
    public Action action => _action;

    private readonly string _text;
    private readonly Action _action;

    public DialogButtonContent(string text, Action action)
    {
        _text = text;
        _action = action;
    }

    public DialogButtonContent(string text)
    {
        _text = text;
        _action = null;
    }
}