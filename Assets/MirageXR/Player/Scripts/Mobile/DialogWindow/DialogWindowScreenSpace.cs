using System;
using TMPro;
using UnityEngine;

[Obsolete]
public class DialogWindowScreenSpace : DialogWindow
{
    [SerializeField] private TMP_Text _txtTitle;
    [SerializeField] private TMP_Text _txtMessage;

    protected override string _titleText
    {
        get => _txtTitle.text;
        set => _txtTitle.text = value;
    }

    protected override string _messageText
    {
        get => _txtMessage.text;
        set => _txtMessage.text = value;
    }

    protected override GameObject _titleGameObject => _txtTitle.gameObject;

    protected override void SetupWindowPosition() { /* ignore */ }

    protected override void InstantiateDialogButton(DialogButtonContent content)
    {
        var btn = Instantiate(_dialogButtonPrefab, _buttonsTransform);
        if (content.action != null)
        {
            btn.onClick.AddListener(content.action.Invoke);
        }

        btn.onClick.AddListener(Hide);
        var tmpText = btn.GetComponentInChildren<TMP_Text>();
        if (tmpText)
        {
            tmpText.text = content.text;
        }
    }
}