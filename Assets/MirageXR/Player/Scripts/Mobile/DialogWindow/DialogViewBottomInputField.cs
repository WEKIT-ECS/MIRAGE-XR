using i5.Toolkit.Core.VerboseLogging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottomInputField : DialogViewBottom
{
    [SerializeField] private Button _buttonClose;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _buttonLeft;
    [SerializeField] private Button _buttonRight;

    public override void UpdateView(DialogModel model)
    {
        if (model.contents.Count != 2)
        {
            Debug.LogError("buttons content does not equal 2");
            return;
        }

        _textLabel.text = model.label;
        _buttonClose.onClick.AddListener(() => model.onClose?.Invoke());

        var placeholder = (TMP_Text)_inputField.placeholder;
        placeholder.text = model.description;

        SetupButton(_buttonLeft, _inputField, model.contents[0], model);
        SetupButton(_buttonRight, _inputField, model.contents[1], model);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private static void SetupButton(Button button, TMP_InputField inputField, DialogButtonContent content, DialogModel model)
    {
        button.onClick.AddListener(() => content.stringAction?.Invoke(inputField.text));
        button.onClick.AddListener(() => model.onClose?.Invoke());
        var textRight = button.GetComponentInChildren<TMP_Text>();
        if (textRight)
        {
            textRight.text = content.text;
        }
    }
}
