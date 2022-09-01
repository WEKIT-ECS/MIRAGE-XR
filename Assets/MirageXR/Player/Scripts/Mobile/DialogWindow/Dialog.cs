using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField] private Button _background;
    [SerializeField] private DialogViewBottom _dialogBottomPrefab;
    [SerializeField] private DialogView _dialogMiddlePrefab;
    [SerializeField] private DialogView _dialogMiddleMultilinePrefab;

    private readonly Queue<DialogModel> _queue = new Queue<DialogModel>();
    private DialogView _dialogView;
    private bool _isActive;

    protected void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _isActive = false;
        _background.gameObject.SetActive(false);
    }

    public void ShowBottom(string label, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.Bottom, label, null, contents);
    }

    public void ShowBottom(string label, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.Bottom, label, null, contents);
    }


    public void ShowBottom(string label, bool canBeClosedByOutTap, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.Bottom, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowBottom(string label, bool canBeClosedByOutTap, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.Bottom, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowMiddleMultiline(string label, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.MiddleMultiline, label, null, contents);
    }

    public void ShowMiddleMultiline(string label, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.MiddleMultiline, label, null, contents);
    }

    public void ShowMiddleMultiline(string label, bool canBeClosedByOutTap, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.MiddleMultiline, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowMiddleMultiline(string label, bool canBeClosedByOutTap, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.MiddleMultiline, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowMiddle(string label, string description, string textLeft, Action onClickLeft, string textRight, Action onClickRight, bool canBeClosedByOutTap = true)
    {
        var contents = new List<DialogButtonContent> { new DialogButtonContent(textLeft, onClickLeft), new DialogButtonContent(textRight, onClickRight) };
        Show(DialogType.Middle, label, description, contents, canBeClosedByOutTap);
    }

    private void Show(DialogType type, string label, string description, List<DialogButtonContent> buttonContents, bool canBeClosedByOutTap = true)
    {
        _queue.Enqueue(new DialogModel(type, label, description, Close, buttonContents, canBeClosedByOutTap));

        if (!_isActive)
        {
            ViewDialog(_queue.Dequeue());
        }
    }

    public void Close()
    {
        if (_queue.Count > 0)
        {
            ViewDialog(_queue.Dequeue());
        }
        else
        {
            _isActive = false;
            _background.gameObject.SetActive(false);

            DestroyDialog();
        }
    }

    private void ViewDialog(DialogModel model)
    {
        DestroyDialog();

        _dialogView = CreateDialogView(model);

        _dialogView.gameObject.SetActive(true);
        _background.gameObject.SetActive(true);
        _background.onClick.RemoveAllListeners();
        if (model.canBeClosedByOutTap)
        {
            _background.onClick.AddListener(Close);
        }

        _isActive = true;
    }

    private DialogView CreateDialogView(DialogModel model)
    {
        var prefab = DialogTypeToPrefab(model.dialogType);
        var dialogView = Instantiate(prefab, transform);
        dialogView.transform.SetAsLastSibling();
        dialogView.UpdateView(model);
        return dialogView;
    }

    private DialogView DialogTypeToPrefab(DialogType dialogType)
    {
        switch (dialogType)
        {
            case DialogType.Bottom:
                return _dialogBottomPrefab;
            case DialogType.Middle:
                return _dialogMiddlePrefab;
            case DialogType.MiddleMultiline:
                return _dialogMiddleMultilinePrefab;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DestroyDialog()
    {
        if (_dialogView)
        {
            Destroy(_dialogView.gameObject);
            _dialogView = null;
        }
    }
}
