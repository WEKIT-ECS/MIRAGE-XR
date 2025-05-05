using System;
using MirageXR;
using UnityEngine;

public class VfxEditorView : PopupEditorBase
{
    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Effects;
    
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private VfxListItem _vfxListItemPrefab;
    [SerializeField] private VfxObject[] _vfxObjects;

    private string _prefabName;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
    }
    
    private void UpdateView()
    {
        for (int i = _contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = _contentContainer.GetChild(i);
            Destroy(child);
        }
        
        foreach (var vfxObject in _vfxObjects)
        {
            var item = Instantiate(_vfxListItemPrefab, _contentContainer);
            item.Init(vfxObject, OnAccept);
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
            LearningExperienceEngine.EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        _content.predicate = $"effect:{_prefabName}";
        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();

        Close();
    }
}
