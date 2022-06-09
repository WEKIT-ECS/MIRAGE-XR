using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class eROBSONListItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private Text _unityText;
    [SerializeField] private TMP_Text _tmpText;

    private eROBSONObject _eROBSONObject;
    private Action<string> _onClick;

    public void Init(eROBSONObject vfxObject, Action<string> onClick)
    {
        _eROBSONObject = vfxObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = _eROBSONObject.sprite;
        if (_unityText) _unityText.text = _eROBSONObject.label;
        if (_tmpText) _tmpText.text = _eROBSONObject.label;
    }

    private void OnClick()
    {
        _onClick?.Invoke(_eROBSONObject.prefabName);
    }
}
