using TMPro;
using UnityEngine;

public abstract class DialogView : MonoBehaviour
{
    [SerializeField] protected TMP_Text _textLabel;

    public abstract void UpdateView(DialogModel model);
}
