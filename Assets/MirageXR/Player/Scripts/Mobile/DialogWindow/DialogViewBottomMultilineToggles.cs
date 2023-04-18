using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottomMultilineToggles : DialogViewBottom
{
    [SerializeField] private Button _buttonClose;
    [SerializeField] private Toggle _togglePrefab;
    [SerializeField] Color warningColor = Color.red;
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
            if (text)
            {
                text.text = content.text;
                if (content.isWarning)
                {
                    text.color = warningColor;
                }

                if (text.text == DBManager.domain)
                {
                    toggle.isOn = true;
                }
                else
                {
                    toggle.isOn = false;
                }
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
