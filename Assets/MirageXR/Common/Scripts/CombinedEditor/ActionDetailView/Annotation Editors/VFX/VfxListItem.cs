using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VfxListItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private Text _unityText;
    [SerializeField] private TMP_Text _tmpText;

    private VfxObject _vfxObject;
    private Action<string> _onClick;

    public void Init(VfxObject vfxObject, Action<string> onClick)
    {
        _vfxObject = vfxObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = _vfxObject.sprite;
        if (_unityText) _unityText.text = _vfxObject.label;
        if (_tmpText) _tmpText.text = _vfxObject.label;
    }

    private void OnClick()
    {
        _onClick?.Invoke(_vfxObject.prefabName);
    }
}
