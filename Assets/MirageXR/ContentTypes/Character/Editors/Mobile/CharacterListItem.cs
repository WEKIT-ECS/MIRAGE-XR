using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _tmpText;

    private CharacterObject _characterObject;
    private Action<string> _onClick;

    public void Init(CharacterObject characterObject, Action<string> onClick)
    {
        _characterObject = characterObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = _characterObject.sprite;
        _tmpText.text = "  " + _characterObject.label;
    }

    private void OnClick()
    {
        _onClick?.Invoke(_characterObject.prefabName);
    }
}