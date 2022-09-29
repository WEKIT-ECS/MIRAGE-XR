using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TiltBrush;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    private const float AnimationFadeTime = 0.1f;

    [SerializeField] private Button _background;
    [SerializeField] private DialogViewMiddle _dialogMiddlePrefab;
    [SerializeField] private DialogViewMiddleMultiline _dialogMiddleMultilinePrefab;
    [SerializeField] private DialogViewBottomMultiline _dialogBottomMultilinePrefab;
    [SerializeField] private DialogViewBottomInputField _dialogBottomInputFieldPrefab;

    private readonly Queue<DialogModel> _queue = new Queue<DialogModel>();
    private DialogView _dialogView;
    private CanvasGroup _backgroundCanvasGroup;
    private bool _isActive;

    protected void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _isActive = false;
        _background.gameObject.SetActive(false);
        _backgroundCanvasGroup = _background.GetComponent<CanvasGroup>();
    }

    public void ShowBottomMultiline(string label, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.Bottom, label, null, contents);
    }

    public void ShowBottomMultiline(string label, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.Bottom, label, null, contents);
    }

    public void ShowBottomMultiline(string label, bool canBeClosedByOutTap, params (string text, Action onClick, bool isWarning)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick, t.isWarning)).ToList();
        Show(DialogType.Bottom, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowBottomMultiline(string label, bool canBeClosedByOutTap, params (string text, Action onClick)[] buttonContents)
    {
        var contents = buttonContents.Select(t => new DialogButtonContent(t.text, t.onClick)).ToList();
        Show(DialogType.Bottom, label, null, contents, canBeClosedByOutTap);
    }

    public void ShowBottomInputField(string label, string description, string textLeft, Action<string> onClickLeft, string textRight, Action<string> onClickRight)
    {
        var contents = new List<DialogButtonContent> { new DialogButtonContent(textLeft, onClickLeft), new DialogButtonContent(textRight, onClickRight) };
        Show(DialogType.BottomInputField, label, description, contents);
    }

    public void ShowBottomInputField(string label, string description, bool canBeClosedByOutTap, string textLeft, Action<string> onClickLeft, string textRight, Action<string> onClickRight)
    {
        var contents = new List<DialogButtonContent> { new DialogButtonContent(textLeft, onClickLeft), new DialogButtonContent(textRight, onClickRight) };
        Show(DialogType.BottomInputField, label, description, contents, canBeClosedByOutTap);
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
            ViewDialog(_queue.Dequeue()).AsAsyncVoid();
        }
        else
        {
            CloseDialog().AsAsyncVoid();
        }
    }

    private async Task ViewDialog(DialogModel model)
    {
        await CloseDialog();

        _dialogView = CreateDialogView(model);

        _backgroundCanvasGroup.alpha = 0.0f;
        _background.gameObject.SetActive(true);
        await _backgroundCanvasGroup.DOFade(1.0f, AnimationFadeTime).AsyncWaitForCompletion();
        _background.onClick.RemoveAllListeners();
        await _dialogView.Show();
        if (model.canBeClosedByOutTap)
        {
            _background.onClick.AddListener(Close);
        }

        _isActive = true;
    }

    private DialogView CreateDialogView(DialogModel model)
    {
        var prefab = DialogTypeToPrefab(model.dialogType);
        return DialogView.Create(model, prefab, transform);
    }

    private DialogView DialogTypeToPrefab(DialogType dialogType)
    {
        switch (dialogType)
        {
            case DialogType.Bottom:
                return _dialogBottomMultilinePrefab;
            case DialogType.Middle:
                return _dialogMiddlePrefab;
            case DialogType.MiddleMultiline:
                return _dialogMiddleMultilinePrefab;
            case DialogType.BottomInputField:
                return _dialogBottomInputFieldPrefab;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task CloseDialog()
    {
        if (_dialogView)
        {
            await _dialogView.Close();
            _backgroundCanvasGroup.alpha = 1.0f;
            await _backgroundCanvasGroup.DOFade(0, AnimationFadeTime).AsyncWaitForCompletion();
            _background.gameObject.SetActive(false);
            _isActive = false;
            _dialogView = null;
        }
    }
}
