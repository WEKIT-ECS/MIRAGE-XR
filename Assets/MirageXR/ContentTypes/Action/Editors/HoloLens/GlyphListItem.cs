using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlyphListItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private Text _unityText;
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private TMP_Text _tmpSize;

    private ActionObject _actionObject;
    private Action<string, Sprite> _onClick;

    public void Init(ActionObject actionObject, Action<string, Sprite> onClick)
    {
        _actionObject = actionObject;
        _onClick = onClick;
        _button.onClick.AddListener(OnClick);
        _image.sprite = _actionObject.sprite;
        if (_unityText) _unityText.text = _actionObject.label;
        if (_tmpText) _tmpText.text = " " + _actionObject.label;
        if (_tmpSize)
        {
            // TODO: get fbx file size
            _tmpSize.text = " ";
        }

        // Tutorial-related init
        TutorialItem titem = GetComponent<TutorialItem>();
        if (titem != null)
        {
            titem.Id = "augmentation_" + _tmpText.text;
        }
    }

    private void OnClick()
    {
        _onClick?.Invoke(_actionObject.prefabName, _actionObject.sprite);
    }
}
