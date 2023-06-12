using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottomMultiline : DialogViewBottom
{
    private const string BUTTON_NAME = "dialog_bottom_multiline_{0}";

    [SerializeField] private Button _buttonClose;
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] Color warningColor = Color.red;

    public override void UpdateView(DialogModel model)
    {
        int count = 0;
        _textLabel.text = model.label;
        _buttonClose.onClick.AddListener(() => model.onClose?.Invoke());
        foreach (var content in model.contents)
        {
            var button = Instantiate(_buttonPrefab, transform);
            button.name = string.Format(BUTTON_NAME, count++);
            button.onClick.AddListener(() => content.action?.Invoke());
            button.onClick.AddListener(() => model.onClose?.Invoke());
            var text = button.GetComponentInChildren<TMP_Text>();
            if (text)
            {
                text.text = content.text;
                if (content.isWarning)
                {
                    text.color = warningColor;
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
