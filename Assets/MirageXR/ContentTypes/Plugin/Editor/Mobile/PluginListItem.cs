using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PluginListItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _tmpText;

    private PluginObject _pluginObject;
    private Action<PluginObject> _onClick;

    public void Init(PluginObject pluginObject, Action<PluginObject> onClick)
    {
        _pluginObject = pluginObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = pluginObject.sprite;
        _tmpText.text = pluginObject.pluginName;
    }

    private void OnClick()
    {
        _onClick?.Invoke(_pluginObject);
    }
}