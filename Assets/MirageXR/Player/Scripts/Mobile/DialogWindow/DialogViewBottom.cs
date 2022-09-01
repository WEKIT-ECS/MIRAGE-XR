using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottom : DialogView
{
    [SerializeField] protected Button _buttonClose;
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] Color warningColor = Color.red;

    public override void UpdateView(DialogModel model)
    {
        _textLabel.text = model.label;
        _buttonClose.onClick.AddListener(() => model.onClose?.Invoke());
        foreach (var content in model.contents)
        {
            var button = Instantiate(_buttonPrefab, transform);
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
    }
}
