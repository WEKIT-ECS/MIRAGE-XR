using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogViewMiddleMultiline : DialogView
{
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private Color warningColor = Color.red;

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

                // Necessary for mobile viewing tutorial to know where to point to
                if (text.text == "Open to view")
                {
                    TutorialItem titem = button.gameObject.AddComponent(typeof(TutorialItem)) as TutorialItem;
                    titem.Id = "open_to_view";
                    titem.InteractableObject = button.gameObject;
                }
            }
        }
    }

    protected override Task OnShowAnimation()
    {
        transform.localScale = Vector3.zero;
        return transform.DOScale(Vector3.one, AnimationTime).AsyncWaitForCompletion();
    }

    protected override Task OnCloseAnimation()
    {
        transform.localScale = Vector3.one;
        return transform.DOScale(Vector3.zero, AnimationTime).AsyncWaitForCompletion();
    }
}
