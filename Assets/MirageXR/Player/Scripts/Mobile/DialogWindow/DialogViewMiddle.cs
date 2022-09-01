using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewMiddle : DialogView
{
    [SerializeField] private TMP_Text _description;
    [SerializeField] protected Button _buttonClose;
    [SerializeField] private Button _buttonLeft;
    [SerializeField] private Button _buttonRight;

    public override void UpdateView(DialogModel model)
    {
        _textLabel.text = model.label;
        _description.text = model.description;
        _buttonClose.onClick.AddListener(() => model.onClose?.Invoke());

        if (model.contents.Count != 2)
        {
            Debug.LogError("buttons content does not equal 2");
            return;
        }

        _buttonLeft.onClick.AddListener(() => model.contents[0].action?.Invoke());
        _buttonLeft.onClick.AddListener(() => model.onClose?.Invoke());
        var textLeft = _buttonLeft.GetComponentInChildren<TMP_Text>();
        if (textLeft)
        {
            textLeft.text = model.contents[0].text;
        }

        _buttonRight.onClick.AddListener(() => model.contents[1].action?.Invoke());
        _buttonRight.onClick.AddListener(() => model.onClose?.Invoke());
        var textRight = _buttonRight.GetComponentInChildren<TMP_Text>();
        if (textRight)
        {
            textRight.text = model.contents[1].text;
        }
    }
}
