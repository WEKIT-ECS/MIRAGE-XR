using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LearningExperienceEngine.DataModel;
using TMPro;
using Unity.Mathematics;
using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Splines;
using UnityEngine.UI;
using Activity = LearningExperienceEngine.DataModel.Activity;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MirageXR
{
    public class InfoScreenSpatialView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textTitle;
        [SerializeField] private TMP_Text _textTitle_Collapsed;
        [SerializeField] private TMP_Text _textDescription;
        [SerializeField] private StepsMediaListItemView stepsMediaListItemViewPrefab;
        [SerializeField] private Transform containerMedia;
        [SerializeField] private Toggle collapsePanelToggle;
        [SerializeField] private Toggle collapsePanelToggle_Collapsed;
        [SerializeField] private Toggle stepCompletedToggle;
        [SerializeField] private Toggle stepCompletedToggle_Collapsed;
        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private GameObject _mainScreen_Collapsed;
        [SerializeField] private GameObject _windowControls;
        [SerializeField] private GameObject _windowContainer;
        [SerializeField] private Button previousStepButton;
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button previousStepButton_Collapsed;
        [SerializeField] private Button nextStepButton_Collapsed;
        [Space]
        [SerializeField] private SplineContainer splineContainerPrefab;
        [SerializeField] private float curveStrength = 1.0f;
        
        private readonly List<StepsMediaListItemView> _mediaListItemViews = new();
        
        private ActivityStep _step;
        private Camera _camera;
        
        private Transform _spawnParent;
        private readonly Dictionary<string, GameObject> _hyperlinkInstances = new(); 
        private readonly Dictionary<string, SplineContainer> _splineInstances = new();
        private readonly Dictionary<string, int> _linkIndexCache = new(); 
        private readonly Dictionary<string, (Vector3 startPosition, Vector3 endPosition)> _previousPositions = new();

        public void Initialize(ActivityStep step)
        {
            _spawnParent = GameObject.Find("Anchor")?.transform;
            _camera = Camera.main;
            EnhancedTouchSupport.Enable();
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
            
            nextStepButton.onClick.AddListener(OnNextStepClicked);
            previousStepButton.onClick.AddListener(OnPreviousStepClicked);
            nextStepButton_Collapsed.onClick.AddListener(OnNextStepClicked);
            previousStepButton_Collapsed.onClick.AddListener(OnPreviousStepClicked);
            collapsePanelToggle.onValueChanged.AddListener(OnStepCompletedToggleValueChanged);
            collapsePanelToggle_Collapsed.onValueChanged.AddListener(OnStepCompletedToggleCollapsedValueChanged);
            UpdateView(step); 
        }
        
        private void Update()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            if (Input.GetMouseButtonDown(0)) // left mouse button
            {
                HandleClick();
            }
#endif
            
#if VISION_OS
            if (EnhancedTouchSupport.enabled)
            {
                var activeTouches = Touch.activeTouches;
                if (activeTouches.Count > 0)
                {
                    var touch = activeTouches[0];
                    SpatialPointerState touchData = EnhancedSpatialPointerSupport.GetPointerState(touch);
                    if (touchData.phase == SpatialPointerPhase.Began)
                    {
                        HandleSpatialClick(touchData.interactionPosition);
                    }
                }
            }
#endif
            
            foreach (var linkId in _splineInstances.Keys)
            {
                if (!_hyperlinkInstances.TryGetValue(linkId, out var hyperlinkInstance) || hyperlinkInstance == null)
                {
                    CleanupLink(linkId); 
                    continue;
                }
                var currentStartPosition = GetLinkWorldPosition(linkId);
                var currentEndPosition = hyperlinkInstance.transform.position;
                
                if (_previousPositions.TryGetValue(linkId, out var previousPositions) &&
                    previousPositions.startPosition == currentStartPosition &&
                    previousPositions.endPosition == currentEndPosition)
                {
                    continue;
                }

                UpdateSplineLine(linkId);
                _previousPositions[linkId] = (currentStartPosition, currentEndPosition);
            }
        }
        private void CleanupLink(string linkId)
        {
            _hyperlinkInstances.Remove(linkId);
            if (_splineInstances.TryGetValue(linkId, out var spline))
            {
                Destroy(spline.gameObject);
                _splineInstances.Remove(linkId);
            }
            _previousPositions.Remove(linkId);
        }
        
        private void HandleSpatialClick(Vector3 interactionPosition)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Camera is not assigned.");
                return;
            }
            
            Vector2 screenPosition = _camera.WorldToScreenPoint(interactionPosition);
    
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_textDescription, screenPosition, _camera);
            if (linkIndex != -1)
            {
                var linkInfo = _textDescription.textInfo.linkInfo[linkIndex];
                var linkId = linkInfo.GetLinkID();
                ToggleHyperlinkVisibility(linkId);
            }
        }

        private void HandleClick()
        {
            var mousePosition = Input.mousePosition;

            if (_camera == null)
            {
                Debug.LogWarning("Camera is not assigned.");
                return;
            }

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_textDescription, mousePosition, _camera);
            if (linkIndex != -1)
            {
                var linkInfo = _textDescription.textInfo.linkInfo[linkIndex];
                var linkId = linkInfo.GetLinkID();
                ToggleHyperlinkVisibility(linkId);
            }
        }

        private void ToggleHyperlinkVisibility(string linkId)
        {
            if (_hyperlinkInstances.ContainsKey(linkId))
            {
                var hyperlinkInstance = _hyperlinkInstances[linkId];
                var isActive = !hyperlinkInstance.activeSelf;
                
                hyperlinkInstance.SetActive(isActive);
                
                if (_splineInstances.ContainsKey(linkId))
                {
                    var splineContainer = _splineInstances[linkId];
                    splineContainer.gameObject.SetActive(isActive);
                }
            }
        }
        
        private void OnEditorModeChanged(bool value)
        {
            _windowContainer.SetActive(!value);
            if (_windowContainer.activeSelf)
            {
                CreateSplinesForHyperlinks();
            }
            else
            {
                foreach (var instance in _hyperlinkInstances.Values.Where(h => h != null))
                {
                    instance.SetActive(true);
                }
                ClearSplines();
            }
        }
        
        private void OnActivityLoaded(Activity activity)
        {
            CheckAndUpdateSplines();
        }
        private void CheckAndUpdateSplines()
        {
            var linkInfos = _textDescription.textInfo.linkInfo;
            var splineContainers = new List<GameObject>();
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.StartsWith("spline__"))
                {
                    splineContainers.Add(obj);
                    var linkId = obj.name.Substring("spline__".Length);
                    var splineContainer = obj.GetComponent<SplineContainer>();
                    if (splineContainer != null && !_splineInstances.ContainsKey(linkId))
                    {
                        _splineInstances[linkId] = splineContainer;
                    }
                }
            }

            var splinesToRemove = new List<GameObject>();
            foreach (var splineContainer in splineContainers)
            {
                var foundMatch = false;
                var linkId = splineContainer.name.Substring("spline__".Length);
                foreach (var linkInfo in linkInfos)
                {
                    if (splineContainer.name == "spline__" + linkInfo.GetLinkID())
                    {
                        foundMatch = true;
                        break;
                    }
                }
                
                if (!foundMatch)
                {
                    splinesToRemove.Add(splineContainer);
                    if (_splineInstances.ContainsKey(linkId))
                    {
                        _splineInstances.Remove(linkId);
                    }
                }
            }
            
            foreach (var splineToRemove in splinesToRemove)
            {
                splineContainers.Remove(splineToRemove);
                Destroy(splineToRemove);
            }
            
            foreach (var linkInfo in linkInfos)
            {
                var foundMatch = false;
                foreach (var splineContainer in splineContainers)
                {
                    if (splineContainer.name == "spline__" + linkInfo.GetLinkID())
                    {
                        foundMatch = true;
                        break;
                    }
                }
                
                if (!foundMatch)
                {
                    // create spline
                    var linkId = linkInfo.GetLinkID();
                    CreateSplinesForHyperlinks();
                    UpdateSplineLine(linkId);
                }
            }
        }
        private void CreateSplinesForHyperlinks()
        {
            _textDescription.ForceMeshUpdate();
            var linkInfos = _textDescription.textInfo.linkInfo;
            foreach (var linkInfo in linkInfos)
            {
                var linkId = linkInfo.GetLinkID();
                var prefabName = "hyperlink__" + linkId;
                var hyperlinkPrefab = GameObject.Find(prefabName);

                if (hyperlinkPrefab != null)
                {
                    _hyperlinkInstances[linkId] = hyperlinkPrefab;
                    if (!_splineInstances.ContainsKey(linkId) && GameObject.Find("spline__" + linkId) == null)
                    {
                        CreateSplineLine(linkId);
                    }
                    else if (_splineInstances.ContainsKey(linkId))
                    {
                        UpdateSplineLine(linkId);
                    }
                }
            }
        }
        
        private void ClearSplines()
        {
            foreach (var spline in _splineInstances.Values)
            {
                Destroy(spline.gameObject);
            }
            _splineInstances.Clear();
            _hyperlinkInstances.Clear();
            _previousPositions.Clear(); 
        }
        private void CreateSplineLine(string linkId)
        {
            var existingSpline = GameObject.Find("spline__" + linkId);
            if (existingSpline != null)
            {
                var splineContainer = existingSpline.GetComponent<SplineContainer>();
                if (splineContainer != null && !_splineInstances.ContainsKey(linkId))
                {
                    _splineInstances[linkId] = splineContainer;
                    UpdateSplineLine(linkId);
                }
                return;
            }
            
            var splinePrefab = Instantiate(splineContainerPrefab, _spawnParent);
            var particleSys = splinePrefab.gameObject.GetComponentInChildren<ParticleSystem>();
            if (particleSys != null)
            {
                var rend = particleSys.GetComponent<Renderer>();
                if (rend != null)
                {
                    var propBlock = new MaterialPropertyBlock();
                    var prefabName = "hyperlink__" + linkId;
                    var diamond = GameObject.Find(prefabName);
                    var meshRenderer = diamond.gameObject.GetComponentInChildren<MeshRenderer>();
                    var diamondColor = meshRenderer.material.GetColor("_BaseColor");
                    propBlock.SetColor("_BaseColor", diamondColor);
                    rend.SetPropertyBlock(propBlock);
                }
                else
                {
                    Debug.LogError("No Renderer found on Particle System in prefab instance: " + splinePrefab.name);
                }
            }
            else
            {
                Debug.LogError("No Particle System found on prefab instance: " + splinePrefab.name);
            }
            
            splinePrefab.name = "spline__" + linkId;

            splinePrefab.transform.localPosition = Vector3.zero;
            splinePrefab.transform.localRotation = Quaternion.identity;

            var splineContainerNew = splinePrefab.GetComponent<SplineContainer>();
            _splineInstances[linkId] = splineContainerNew;

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
        private Vector3 GetLinkWorldPosition(string linkId)
        {
            if (!_linkIndexCache.TryGetValue(linkId, out var index))
            {
                index = _textDescription.textInfo.linkInfo.ToList().FindIndex(link => link.GetLinkID() == linkId);
                if (index == -1)
                {
                    Debug.LogError($"Link with ID {linkId} not found.");
                    return Vector3.zero;
                }
                _linkIndexCache[linkId] = index;
            }

            var linkInfo = _textDescription.textInfo.linkInfo[index];
            var firstCharInfo = _textDescription.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex];
            var lastCharInfo = _textDescription.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength - 1];

            var bottomLeft = firstCharInfo.bottomLeft;
            var topRight = lastCharInfo.topRight;
            var centerLocal = (bottomLeft + topRight) * 0.5f;
    
            var centerWorld = _textDescription.transform.TransformPoint(centerLocal); 
            centerWorld += _textDescription.transform.forward * -0.01f;

            return centerWorld;
        }

        private void OnStepCompletedToggleValueChanged(bool value)
        {
            _mainScreen.SetActive(false);
            _mainScreen_Collapsed.SetActive(true);
            _windowControls.SetActive(false);
            UpdateToggleStates();
        }
        
        private void OnStepCompletedToggleCollapsedValueChanged(bool value)
        {
            _mainScreen.SetActive(true);
            _mainScreen_Collapsed.SetActive(false);
            _windowControls.SetActive(true);
            UpdateToggleStates();
        }
        
        private void UpdateToggleStates()
        {
            switch (_mainScreen.activeSelf)
            {
                case true when !_mainScreen_Collapsed.activeSelf:
                    collapsePanelToggle.isOn = false;
                    collapsePanelToggle_Collapsed.isOn = true;
                    break;
                case false when _mainScreen_Collapsed.activeSelf:
                    collapsePanelToggle.isOn = false;
                    collapsePanelToggle_Collapsed.isOn = true;
                    break;
            }
        }
        
        private void OnPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        } 
        
        public async void UpdateView(ActivityStep step)
        {
            if (step == null)
            {
                return; 
            }
            _step = step;  
            _textTitle.text = step.Name;
            _textTitle_Collapsed.text = step.Name;
            
            var data = HyperlinkPositionData.SplitPositionsFromText(_step.Description);
            _textDescription.text = AddLinkTagsToBrackets(data.DisplayText);
            
            var isEditorMode = RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            OnEditorModeChanged(isEditorMode);
            
            foreach (var item in _mediaListItemViews)
            {
                Destroy(item.gameObject);
            }
            _mediaListItemViews.Clear();
            
            if (_step?.Attachment != null)
            {
                foreach (var file in _step.Attachment)
                {
                    var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                    var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, file.Id);

                    if (texture != null)
                    {
                        var item = Instantiate(stepsMediaListItemViewPrefab, containerMedia);
                        item.Initialize(file, texture, _step.Id, (stepId, fileModel) =>
                        {
                            RootObject.Instance.LEE.StepManager.RemoveAttachment(stepId, fileModel.Id);
                        });
                        item.Interactable = false;
                        _mediaListItemViews.Add(item);
                    }
                }
            }
        }
        private string AddLinkTagsToBrackets(string inputText)
        {
            var pattern = @"\[([^\[\]]+)\]";
            var result = Regex.Replace(inputText, pattern, match =>
            {
                var content = match.Groups[1].Value;
                return $"<link={content}><color=#8F9CFF>[{content}]</color></link>";
            });

            return result;
        }
    }
}
