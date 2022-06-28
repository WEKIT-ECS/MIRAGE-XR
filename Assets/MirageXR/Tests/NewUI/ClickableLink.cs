using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableLink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string url;

    private TextMeshProUGUI textMeshPro;
    private Camera mainCamera;
    private int pressedLinkIndex = -1;

    void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();

        mainCamera = Camera.main;
        if (textMeshPro.canvas.renderMode == RenderMode.ScreenSpaceOverlay) mainCamera = null;
        else if (textMeshPro.canvas.worldCamera != null) mainCamera = textMeshPro.canvas.worldCamera;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, mainCamera);

        if (linkIndex != -1)
        {
            Application.OpenURL(url);
        }
        else pressedLinkIndex = -1;
    }
}
