using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContentSelectorView : PopupBase
{
    [SerializeField] private Transform _listContent;
    [SerializeField] private ContentSelectorListItem _contentSelectorListItemPrefab;
    [SerializeField] private ContentHintView _contentHintViewPrefab;

    private IEnumerable<PopupEditorBase> _editors;
    private MirageXR.Action _currentStep;
    
    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        foreach (var type in _editors.Select(t => t.editorForType).Distinct())
        {
            var item = Instantiate(_contentSelectorListItemPrefab, _listContent);
            item.Init(type, OnListItemClick, OnListItemHintClick);
        }
    }

    private void OnListItemClick(ContentType type)
    {
        Close();
        var editor = _editors.FirstOrDefault(t => t.editorForType == type);
        if (editor == null)
        {
            Debug.LogError($"there is no editor for the type {type}");
            return;
        }
        PopupsViewer.Instance.Show(editor, _currentStep);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _editors = (IEnumerable<PopupEditorBase>)args[0];
            _currentStep = (MirageXR.Action)args[1];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void OnListItemHintClick(ContentType type)
    {
        PopupsViewer.Instance.Show(_contentHintViewPrefab, type);
    }
}
