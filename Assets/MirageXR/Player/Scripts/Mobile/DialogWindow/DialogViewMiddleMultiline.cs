using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewMiddleMultiline : DialogView
{
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] Color warningColor = Color.red;

    public override void UpdateView(DialogModel model)
    {
        _textLabel.text = model.label;
        foreach (var content in model.contents)
        {
            var newObject = Instantiate(_buttonPrefab, transform);
            var button = newObject.GetComponentInChildren<Button>();
            if (button)
            {
                button.onClick.AddListener(() => content.action?.Invoke());
                button.onClick.AddListener(() => model.onClose?.Invoke());
            }

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
