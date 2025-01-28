using TMPro;
using UnityEngine;

public class SpatialClickableTextHandler : MonoBehaviour
{
    [SerializeField] private GameObject spatialHyperlinkPrefab; 
    [SerializeField] private Transform spawnParent;

    private TMP_Text _textMeshPro;
    private Camera _camera;

    private void Start()
    {
        _textMeshPro = GetComponent<TMP_Text>();
        _camera = Camera.main;

        if (_textMeshPro == null || spatialHyperlinkPrefab == null)
        {
            Debug.LogError("TextMeshPro component or CubePrefab is not assigned.");
        }
        
        var mainCameraObject = GameObject.Find("Main Camera");
        if (mainCameraObject != null)
        {
            _camera = mainCameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("Camera with name 'Main Camera' not found!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse button
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        var mousePosition = Input.mousePosition;
        if (_camera == null) return;
        
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, mousePosition, _camera);
        if (linkIndex != -1)
        {
            var linkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();
            Debug.Log($"Clicked on link: {linkId}");

            var linkPosition = GetLinkWorldPosition(linkInfo);
            var prefab = CreateHyperlinkPrefab(linkPosition);
            //CreateSplineLine(linkPosition, prefab.transform);
        }
    }
    
    private Vector3 GetLinkWorldPosition(TMP_LinkInfo linkInfo)
    {
        var charInfo = _textMeshPro.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex];
        var worldPosition = _textMeshPro.transform.TransformPoint(charInfo.bottomLeft);
        return worldPosition;
    }

    private GameObject CreateHyperlinkPrefab(Vector3 startPosition)
    {
        var spawnPosition = startPosition + Vector3.up / 5;
        return Instantiate(spatialHyperlinkPrefab, spawnPosition, Quaternion.identity, spawnParent);
    }
}
