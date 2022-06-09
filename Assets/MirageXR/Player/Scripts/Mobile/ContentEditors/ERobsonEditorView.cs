using MirageXR;
using System;
using UnityEngine;

public class ERobsonEditorView : PopupEditorBase
{
    public override ContentType editorForType => ContentType.EROBSON;

    [SerializeField] private Transform _contentContainer;
    [SerializeField] private eROBSONListItem _eROBSONListItemPrefab;
    [SerializeField] private eROBSONObject[] _eROBSONObjects;

    private string _prefabName;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        for (int i = _contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = _contentContainer.GetChild(i);
            Destroy(child);
        }

        foreach (var eROBSONObject in _eROBSONObjects)
        {
            var item = Instantiate(_eROBSONListItemPrefab, _contentContainer);
            item.Init(eROBSONObject, OnAccept);
        }
    }

    private void OnAccept(string prefabName)
    {
        _prefabName = prefabName;
        OnAccept();
    }

    protected override void OnAccept()
    {
        if (_content != null)
        {
            EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        _content.predicate = $"eRobson:{_prefabName}";
        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);

        Close();
    }
}
