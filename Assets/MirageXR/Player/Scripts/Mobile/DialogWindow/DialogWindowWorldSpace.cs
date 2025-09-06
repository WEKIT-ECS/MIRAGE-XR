using System;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class DialogWindowWorldSpace : DialogWindow
{
    private float DISTANCE = 0.8f;

    [SerializeField] private Text _txtTitle;
    [SerializeField] private Text _txtMessage;

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

    private Transform _cameraTransform;
    private Transform _parentTransform;

    protected override void Init()
    {
        base.Init();
        _cameraTransform = Camera.main.transform;
        _parentTransform = transform.parent;
    }

    protected override GameObject _titleGameObject => _txtTitle.gameObject;

    protected override void SetupWindowPosition()
    {
        _parentTransform.position = _cameraTransform.position + _cameraTransform.forward * DISTANCE;
        _parentTransform.rotation = _cameraTransform.rotation;
    }

    protected override void InstantiateDialogButton(DialogButtonContent content)
    {
        var btn = Instantiate(_dialogButtonPrefab, _buttonsTransform);
        if (content.action != null) btn.onClick.AddListener(content.action.Invoke);
        btn.onClick.AddListener(Hide);
        var pressableButton = btn.GetComponent<PressableButton>();
        if (pressableButton)
            pressableButton.ButtonPressed.AddListener(Hide);
        var tmpText = btn.GetComponentInChildren<Text>();
        if (tmpText) tmpText.text = content.text;
    }
}