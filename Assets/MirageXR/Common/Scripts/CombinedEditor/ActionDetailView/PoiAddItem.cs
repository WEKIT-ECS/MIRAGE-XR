using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PoiAddItem : MonoBehaviour
{
    [SerializeField] private Text textDisplay;
    [SerializeField] private Image iconDisplay;

    private int _index;
    private ContentType _type;

    public delegate void PoiAddItemClickedDeletage(ContentType type);

    public event PoiAddItemClickedDeletage OnPoiAddItemClicked;

    public delegate void PoiHoverEffectDeletage(ContentType type, int index);

    public event PoiHoverEffectDeletage OnPoiHover;

    public void Initialize(ContentType type, int index, string text, Sprite ico)
    {
        _type = type;
        _index = index;
        textDisplay.text = text;
        iconDisplay.sprite = ico;
    }

    public void OnItemClicked()
    {
        OnPoiAddItemClicked?.Invoke(_type);
    }

    public void OnPoiHoverActive()
    {
        OnPoiHover?.Invoke(_type, _index);
    }

    public void ShowHelpText()
    {
        FindObjectOfType<ActionEditor>().ShowHelpText();
    }
}
