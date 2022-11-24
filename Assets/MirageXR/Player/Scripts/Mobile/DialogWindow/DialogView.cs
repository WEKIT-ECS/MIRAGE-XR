using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public abstract class DialogView : MonoBehaviour
{
    protected const float AnimationTime = 0.15f;

    [SerializeField] protected TMP_Text _textLabel;

    public abstract void UpdateView(DialogModel model);
    protected abstract Task OnShowAnimation();
    protected abstract Task OnCloseAnimation();

    public virtual async Task Show()
    {
        gameObject.SetActive(true);
        await OnShowAnimation();
    }

    public virtual async Task Close()
    {
        await OnCloseAnimation();
        Destroy(gameObject);
    }

    public static DialogView Create(DialogModel model, DialogView prefab, Transform parent)
    {
        var dialogView = Instantiate(prefab, parent);
        dialogView.transform.SetAsLastSibling();
        dialogView.UpdateView(model);
        dialogView.gameObject.SetActive(false);
        return dialogView;
    }
}
