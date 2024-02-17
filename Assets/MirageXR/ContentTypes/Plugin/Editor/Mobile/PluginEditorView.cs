using System;
using MirageXR;
using UnityEngine;

public class PluginEditorView : PopupEditorBase
{
    public override ContentType editorForType => ContentType.PLUGIN;

    [SerializeField] private Transform _contentContainer;
    [SerializeField] private PluginListItem _pluginListItemPrefab;
    [SerializeField] private PluginObject[] _pluginObjects;

    private App _app;

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

        foreach (var pluginObject in _pluginObjects)
        {
            var item = Instantiate(_pluginListItemPrefab, _contentContainer);
            item.Init(pluginObject, OnAccept);
        }
    }

    private void OnAccept(PluginObject pluginObject)
    {
        _app = PluginToApp(pluginObject);
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

        _content.predicate = $"plugin:{_app.name}";
        _content.url = _app.manifest;
        _step.appIDs.Add(_app.id);

        RootObject.Instance.workplaceManager.workplace.apps.Add(_app);

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);

        Close();
    }

    private static App PluginToApp(PluginObject plugin)
    {
        const string type = "App";
        var app = new App
        {
            id = Guid.NewGuid().ToString(),
            name = plugin.pluginName,
            type = type,
            manifest = plugin.manifest
        };

        return app;
    }
}
