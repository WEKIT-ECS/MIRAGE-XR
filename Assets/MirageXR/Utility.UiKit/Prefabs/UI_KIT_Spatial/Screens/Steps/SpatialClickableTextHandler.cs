using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace MirageXR
{
    public class SpatialClickableTextHandler : MonoBehaviour
    {
        [SerializeField] private GameObject spatialHyperlinkPrefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private SplineContainer splineContainerPrefab;

        private TMP_Text _textMeshPro;
        private Camera _camera;
        private Dictionary<string, GameObject> _hyperlinkInstances = new();
        private Dictionary<string, SplineContainer> _splineInstances = new();

        [SerializeField] private float curveStrength = 1.0f;

    private void Start()
    {
        _textMeshPro = GetComponent<TMP_Text>();
        _camera = Camera.main;

        if (_textMeshPro == null || spatialHyperlinkPrefab == null)
        {
            Debug.LogError("TextMeshPro component or spatialHyperlinkPrefab is not assigned.");
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
        
        foreach (var linkId in _hyperlinkInstances.Keys)
        {
            UpdateSplineLine(linkId);
        }
    }

    private void HandleClick()
    {
        var mousePosition = Input.mousePosition;
        if (_camera == null) return;

        var linkIndex = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, mousePosition, _camera);
        if (linkIndex != -1)
        {
            var linkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();
            Debug.Log($"Clicked on link: {linkId}");

            if (!_hyperlinkInstances.ContainsKey(linkId))
            {
                var linkPosition = GetLinkWorldPosition(linkInfo);
                var hyperlinkInstance = CreateHyperlinkPrefab(linkPosition);
                _hyperlinkInstances[linkId] = hyperlinkInstance;
                
                CreateSplineLine(linkId, linkPosition, hyperlinkInstance.transform.position);
            }
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
        var spawnPosition = startPosition + Vector3.up / 2;
        return Instantiate(spatialHyperlinkPrefab, spawnPosition, Quaternion.identity, spawnParent);
    }

    private void CreateSplineLine(string linkId, Vector3 startPosition, Vector3 endPosition)
    {
        var splineContainer = Instantiate(splineContainerPrefab, spawnParent);
        var _splineContainer = splineContainer.GetComponent<SplineContainer>();
        
        _splineInstances[linkId] = _splineContainer;

        UpdateSplineLine(linkId);
    }

    private void UpdateSplineLine(string linkId)
    {
        if (!_hyperlinkInstances.ContainsKey(linkId) || !_splineInstances.ContainsKey(linkId))
        {
            return;
        }

        var hyperlinkInstance = _hyperlinkInstances[linkId];
        var splineContainer = _splineInstances[linkId];

        var startPosition = GetLinkWorldPosition(_textMeshPro.textInfo.linkInfo[GetLinkIndexById(linkId)]);
        var endPosition = hyperlinkInstance.transform.position;

        var localStartPosition = splineContainer.transform.InverseTransformPoint(startPosition);
        var localEndPosition = splineContainer.transform.InverseTransformPoint(endPosition);

        splineContainer.Spline.Clear();

        var startKnot = new BezierKnot(localStartPosition);
        var endKnot = new BezierKnot(localEndPosition);

        var direction = (localEndPosition - localStartPosition).normalized;
        var perpendicular = new Vector3(-direction.y, direction.x, direction.z) * curveStrength;

        startKnot.TangentOut = new float3(perpendicular);
        endKnot.TangentIn = new float3(-perpendicular);

        splineContainer.Spline.Add(startKnot);
        splineContainer.Spline.Add(endKnot);

        // Update mesh
        var splineExtrude = splineContainer.GetComponent<SplineExtrude>();
        if (splineExtrude != null)
        {
            splineExtrude.Rebuild();
        }
    }

    private int GetLinkIndexById(string linkId)
    {
        for (int i = 0; i < _textMeshPro.textInfo.linkInfo.Length; i++)
        {
            if (_textMeshPro.textInfo.linkInfo[i].GetLinkID() == linkId)
            {
                return i;
            }
        }
        return -1;
    }
    }
}