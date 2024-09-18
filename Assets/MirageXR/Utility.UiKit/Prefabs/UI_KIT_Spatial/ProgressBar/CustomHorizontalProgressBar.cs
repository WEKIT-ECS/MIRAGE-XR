using TMPro;
using UnityEngine;

namespace MirageXR
{
    public class CustomHorizontalProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _itemDone;
        [SerializeField] private RectTransform _itemDue;
        [SerializeField] private RectTransform _itemOverdue;
        [SerializeField] private TMP_Text _textPercentageOfCompletion;

        private RectTransform _parentRectTransform;

        private void Awake()
        {
            _parentRectTransform = GetComponent<RectTransform>();
        }

        private void UpdateProgressBar(int itemsDone, int itemsDue, int itemsOverdue)
        {
            var parentWidth = _parentRectTransform.rect.width;
            var totalItems = itemsDone + itemsDue + itemsOverdue;

            float widthDone = (itemsDone / (float)totalItems) * parentWidth;
            float widthDue = (itemsDue / (float)totalItems) * parentWidth;
            float widthOverdue = (itemsOverdue / (float)totalItems) * parentWidth;

            _itemDone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthDone);
            _itemDue.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthDue);
            _itemOverdue.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthOverdue);

            _textPercentageOfCompletion.text = $"Complete {Mathf.Round((itemsDone / (float)totalItems) * 100f).ToString()}%";
        }
    }
}
