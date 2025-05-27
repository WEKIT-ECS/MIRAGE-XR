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
            var text = toggle.GetComponentInChildren<TMP_Text>();
            toggle.group = _toggleGroup;
            if (text)
            {
                toggle.SetIsOnWithoutNotify(content.isSelected);
                text.text = content.text;
            }

            toggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    content.action?.Invoke();
                    model.onClose?.Invoke();
                }
            });
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
