using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottomMultilineToggles : DialogViewBottom
{
    [SerializeField] private Button _buttonClose;
    [SerializeField] private Toggle _togglePrefab;
    [SerializeField] private ToggleGroup _toggleGroup;

    public override void UpdateView(DialogModel model)
    {
        _textLabel.text = model.label;
        _buttonClose.onClick.AddListener(() => model.onClose?.Invoke());
        foreach (var content in model.contents)
        {
            var toggle = Instantiate(_togglePrefab, transform);
            toggle.onValueChanged.AddListener((bool isOn) => content.action?.Invoke());
            toggle.onValueChanged.AddListener((bool isOn) => model.onClose?.Invoke());
            var text = toggle.GetComponentInChildren<TMP_Text>();
            var m_toggle = toggle.GetComponentInChildren<Toggle>();
            m_toggle.group = _toggleGroup;
            if (text)
            {
                m_toggle.isOn = content.isSelected;
                text.text = content.text;
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
