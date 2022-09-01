using System;
using System.Collections.Generic;

public enum DialogType
{
    Bottom,
    Middle,
    MiddleMultiline
}

public class DialogModel
{
    public DialogType dialogType => _dialogType;
    public string label => _label;
    public string description => _description;
    public Action onClose => _onClose;
    public List<DialogButtonContent> contents => _contents;
    public bool canBeClosedByOutTap => _canBeClosedByOutTap;

    private readonly DialogType _dialogType;
    private readonly string _label;
    private readonly string _description;
    private readonly Action _onClose;
    private readonly List<DialogButtonContent> _contents;
    private readonly bool _canBeClosedByOutTap;

    public DialogModel(DialogType dialogType, string label, string description, Action onClose, List<DialogButtonContent> contents, bool canBeClosedByOutTap)
    {
        _dialogType = dialogType;
        _label = label;
        _description = description;
        _onClose = onClose;
        _contents = contents;
        _canBeClosedByOutTap = canBeClosedByOutTap;
    }
}
