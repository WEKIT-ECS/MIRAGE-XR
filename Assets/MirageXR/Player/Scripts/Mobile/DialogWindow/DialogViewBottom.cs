using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public abstract class DialogViewBottom : DialogView
{
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
