using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewBottomMultiline : DialogView
{
    [SerializeField] private Button _buttonClose;
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

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    protected override Task OnShowAnimation()
    {
        var rectTransform = (RectTransform)transform;
        var position = rectTransform.localPosition;
        var height = rectTransform.rect.height;
        rectTransform.localPosition = new Vector3(position.x, position.y - height, position.z);
        return rectTransform.DOLocalMoveY(position.y, AnimationTime).AsyncWaitForCompletion();
    }

    protected override Task OnCloseAnimation()
    {
        var rectTransform = (RectTransform)transform;
        var position = rectTransform.localPosition;
        var height = rectTransform.rect.height;
        return rectTransform.DOLocalMoveY(position.y - height, AnimationTime).AsyncWaitForCompletion();
    }
}
