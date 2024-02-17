using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableLink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string url;

    private TextMeshProUGUI _textMeshPro;
    private Camera _mainCamera;
    private int _pressedLinkIndex = -1;

    void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();

        _mainCamera = Camera.main;
        if (_textMeshPro.canvas.renderMode == RenderMode.ScreenSpaceOverlay) _mainCamera = null;
        else if (_textMeshPro.canvas.worldCamera != null) _mainCamera = _textMeshPro.canvas.worldCamera;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, Input.mousePosition, _mainCamera);

        if (linkIndex != -1)
        {
            Application.OpenURL(url);
        }
        else _pressedLinkIndex = -1;
    }
}
