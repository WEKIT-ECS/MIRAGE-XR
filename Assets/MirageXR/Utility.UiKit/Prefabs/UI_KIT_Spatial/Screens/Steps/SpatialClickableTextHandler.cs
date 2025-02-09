using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace MirageXR
{
    public class SpatialClickableTextHandler : MonoBehaviour
    {
        [SerializeField] private SpatialHyperlinkObjectView spatialHyperlinkPrefab;
        [SerializeField] private SplineContainer splineContainerPrefab;
        [SerializeField] private TMP_Text _textMeshPro;
        [SerializeField] private Color[] diamondColors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new (1, 0.5f, 0),
            new (0.5f, 0, 1) 
        };
        private int _currentColorIndex = 0;
        
        private Camera _camera;
        private Dictionary<string, GameObject> _hyperlinkInstances = new();
        private Dictionary<string, SplineContainer> _splineInstances = new();
        private Dictionary<string, Vector3> _initialLinkPositions = new();
        private Dictionary<string, Vector3> _lastPrefabPositions = new();
        private Dictionary<string, Vector3> _lastLinkPositions = new();
        private Dictionary<string, int> _linkIndexCache = new ();
        private HashSet<string> _activeLinkIds = new();
        private Transform spawnParent;

        [SerializeField] private float curveStrength = 1.0f;

   private void Start()
    {
        _camera = Camera.main;
        
        spawnParent = GameObject.Find("Anchor1")?.transform;
        if (spawnParent == null)
        {
            spawnParent = null;
        }

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
        
        RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
    }

    private void OnEditorModeChanged(bool value)
    {
        HideSplineObjects(value);
    }

    private void HideSplineObjects(bool value)
    {
        if (_hyperlinkInstances != null)
        {
            foreach (var item in _hyperlinkInstances)
            {
                if (item.Value != null)
                {
                    item.Value.SetActive(!value);
                }
            }
        }

        if (_splineInstances != null)
        {
            foreach (var spline in _splineInstances)
            {
                if (spline.Value != null && spline.Value.gameObject != null)
                {
                    spline.Value.gameObject.SetActive(!value);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse button
        {
            HandleClick();
        }
        
        foreach (var linkId in _activeLinkIds)
        {
            if (!_hyperlinkInstances.ContainsKey(linkId) || !_splineInstances.ContainsKey(linkId))
                continue;

            var currentPrefabPosition = _hyperlinkInstances[linkId].transform.position;
            var currentLinkPosition = GetLinkWorldPosition(linkId);
            
            if (currentPrefabPosition != _lastPrefabPositions[linkId] || currentLinkPosition != _lastLinkPositions[linkId])
            {
                UpdateSplineLine(linkId);
                _lastPrefabPositions[linkId] = currentPrefabPosition;
                _lastLinkPositions[linkId] = currentLinkPosition;
            }
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
            var linkText = linkInfo.GetLinkText().Replace("[", "").Replace("]", "");
            var linkId = linkInfo.GetLinkID();
            Debug.Log($"Clicked on link: {linkId}");

            if (_hyperlinkInstances.ContainsKey(linkId))
            {
                var isCurrentlyVisible = _hyperlinkInstances[linkId].activeSelf;
                ToggleHyperlinkVisibility(linkId, !isCurrentlyVisible);
            }
            else
            {
                var linkPosition = GetLinkWorldPosition(linkId);
                var hyperlinkInstance = CreateHyperlinkPrefab(linkPosition, linkText);
                _hyperlinkInstances[linkId] = hyperlinkInstance;
                _initialLinkPositions[linkId] = linkPosition;
                
                _lastLinkPositions[linkId] = linkPosition; 
                _lastPrefabPositions[linkId] = hyperlinkInstance.transform.position; 
                
                _activeLinkIds.Add(linkId);
                _linkIndexCache[linkId] = linkIndex; 

                CreateSplineLine(linkId);
            }
        }
    }
    
    private void ToggleHyperlinkVisibility(string linkId, bool isVisible)
    {
        if (_hyperlinkInstances.ContainsKey(linkId))
        {
            _hyperlinkInstances[linkId].SetActive(isVisible); 
        }

        if (_splineInstances.ContainsKey(linkId))
        {
            _splineInstances[linkId].gameObject.SetActive(isVisible);
        }
    }

    private Vector3 GetLinkWorldPosition(string linkId)
    {
        if (!_linkIndexCache.ContainsKey(linkId))
        {
            var index = _textMeshPro.textInfo.linkInfo.ToList().FindIndex(link => link.GetLinkID() == linkId);
            if (index == -1)
            {
                Debug.LogError($"Link with ID {linkId} not found.");
                return Vector3.zero;
            }
            _linkIndexCache[linkId] = index;
        }
        
        var linkInfo = _textMeshPro.textInfo.linkInfo[_linkIndexCache[linkId]];
        var charInfo = _textMeshPro.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex];
        return _textMeshPro.transform.TransformPoint(charInfo.bottomLeft);
    }

    private GameObject CreateHyperlinkPrefab(Vector3 startPosition, string linkText)
    {
        var spawnPosition = startPosition + Vector3.up / Random.Range(2, 4); // temp

        var hyperlinkInstance = Instantiate(spatialHyperlinkPrefab.gameObject, spawnPosition, Quaternion.identity, spawnParent);
        var spatialView = hyperlinkInstance.GetComponent<SpatialHyperlinkObjectView>();

        if (spatialView != null)
        {
            spatialView.SetText(linkText);
            var diamondColor = GetNextColor();
            spatialView.SetDiamondColor(diamondColor);
        }
        else
        {
            Debug.LogError("SpatialHyperlinkObjectView component not found on the prefab.");
        }

        return hyperlinkInstance;
    }
    
    private Color GetNextColor()
    {
        var color = diamondColors[_currentColorIndex];
        _currentColorIndex = (_currentColorIndex + 1) % diamondColors.Length;
        return color;
    }

    private void CreateSplineLine(string linkId)
    {
        var splinePrefab = Instantiate(splineContainerPrefab, spawnParent);

        splinePrefab.transform.localPosition = Vector3.zero; 
        splinePrefab.transform.localRotation = Quaternion.identity; 
        
        var splineContainer = splinePrefab.GetComponent<SplineContainer>();
        _splineInstances[linkId] = splineContainer;

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
        
        var startPosition = GetLinkWorldPosition(linkId);
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
        
        var splineExtrude = splineContainer.GetComponent<SplineExtrude>();
        if (splineExtrude != null)
        {
            splineExtrude.Rebuild();
        }
    }
    }
}