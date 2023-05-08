using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContentSelectorView_v2 : PopupBase
{
    [SerializeField] private Transform _listContent;
    [SerializeField] private ContentSelectorListItem _contentSelectorListItemPrefab;
    [SerializeField] private ContentHintView _contentHintViewPrefab;

    private IEnumerable<PopupEditorBase> _editors;
    private MirageXR.Action _currentStep;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        // Get the list of augmentations from txt file depends on platform
        var listOfAugmentations = BrandManager.Instance.GetListOfAugmentations();
        foreach (var type in _editors.Select(t => t.editorForType).Distinct())
        {
            if (listOfAugmentations.Contains(type) && type != ContentType.IMAGEMARKER)
            {
                var item = Instantiate(_contentSelectorListItemPrefab, _listContent);
                item.Init(type, OnListItemClick, OnListItemHintClick);
            }
        }
    }

    private void OnListItemClick(ContentType type)
    {
        Close();
        var editor = _editors.FirstOrDefault(t => t.editorForType == type);
        if (editor == null)
        {
            AppLog.LogError($"there is no editor for the type {type}");
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
