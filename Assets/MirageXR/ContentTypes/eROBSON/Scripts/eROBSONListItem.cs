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

    private eROBSONObject _eRobsonObject;
    private Action<string> _onClick;

    public void Init(eROBSONObject eRobsonObject, Action<string> onClick)
    {
        _eRobsonObject = eRobsonObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = _eRobsonObject.sprite;
        if (_unityText) _unityText.text = _eRobsonObject.label;
        if (_tmpText) _tmpText.text = _eRobsonObject.label;
    }

    private void OnClick()
    {
        _onClick?.Invoke(_eRobsonObject.prefabName);
    }
}
