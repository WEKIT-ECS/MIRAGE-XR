using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MirageXR
{
    public class SimpleBoundingBox : MonoBehaviour
    {
        [SerializeField] private float _handleSize = 0.05f;
        [SerializeField] private Color _handleColor = Color.white;
        [SerializeField] private Color _lineColor = Color.cyan;

        public Transform Target;

        public class RotationConfiguration
        {
            private readonly SimpleBoundingBox _owner;
            private bool _showX = true;
            private bool _showY = true;
            private bool _showZ = true;

            public RotationConfiguration(SimpleBoundingBox owner)
            {
                _owner = owner;
            }

            public bool ShowHandleForX
            {
                get => _showX;
                set
                {
                    _showX = value;
                    _owner?.UpdateHandleVisibility();
                }
            }

            public bool ShowHandleForY
            {
                get => _showY;
                set
                {
                    _showY = value;
                    _owner?.UpdateHandleVisibility();
                }
            }

            public bool ShowHandleForZ
            {
                get => _showZ;
                set
                {
                    _showZ = value;
                    _owner?.UpdateHandleVisibility();
                }
            }
        }

        public RotationConfiguration RotationHandlesConfig { get; private set; }

        // UnityEvents matching MRTK BoundsControl API for drop-in compatibility
        public UnityEvent RotateStarted = new UnityEvent();
        public UnityEvent RotateStopped = new UnityEvent();
        public UnityEvent ScaleStarted = new UnityEvent();
        public UnityEvent ScaleStopped = new UnityEvent();
        public UnityEvent TranslateStarted = new UnityEvent();
        public UnityEvent TranslateStopped = new UnityEvent();

        // Typed Action delegates
        public Action<Vector3> OnScaleStarted;
        public Action<Vector3> OnScaleChanged;
        public Action<Vector3> OnScaleEnded;
        public Action<Quaternion> OnRotationStarted;
        public Action<Quaternion> OnRotationChanged;
        public Action<Quaternion> OnRotationEnded;

        private List<BoundingBoxHandle> _handles = new List<BoundingBoxHandle>();
        private List<BoundingBoxHandle> _rotationHandles = new List<BoundingBoxHandle>();
        private LineRenderer _lineRenderer;
        private BoxCollider _targetBoundsCollider;

        private Material _scaleHandleMaterial;
        private Material _rotationHandleMaterial;
        private Vector3 _initialScale;
        private Quaternion _initialRotation;
        private float _accumulatedScaleRatio = 1.0f;
        private float _accumulatedAngle = 0.0f;

        private LearningExperienceEngine.ToggleObject _obj;
        private string _targetId;
        private bool _isLocked;
        private bool _isHandleInteractionActive;
        private bool _parentGrabWasEnabled;
        private XRGrabInteractable _parentGrabInteractable;
        private Vector3 _boundsCenter;
        private Vector3 _boundsExtents;
        private bool _isInitialized;
        private bool _isShowing = true;

        private static LearningExperienceEngine.ActivityManager _activityManager =>
            LearningExperienceEngine.LearningExperienceEngine.Instance?.ActivityManagerOld;

        public bool Active
        {
            get => enabled && gameObject.activeInHierarchy && _isShowing;
            set => SetEnabled(value);
        }

        public BoxCollider TargetBounds
        {
            get
            {
                if (_targetBoundsCollider == null)
                {
                    _targetBoundsCollider = GetComponent<BoxCollider>();
                    if (_targetBoundsCollider == null)
                    {
                        _targetBoundsCollider = gameObject.AddComponent<BoxCollider>();
                        _targetBoundsCollider.isTrigger = true;
                    }
                }

                _targetBoundsCollider.center = _boundsCenter;
                _targetBoundsCollider.size = _boundsExtents * 2f;
                return _targetBoundsCollider;
            }
        }

        public Bounds Bounds => new Bounds(_boundsCenter, _boundsExtents * 2f);

        private void Awake()
        {
            RotationHandlesConfig = new RotationConfiguration(this);
        }

        public void Setup(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;
            _targetId = obj.poi;
            LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            UpdateState();
        }

        public void Setup(string id)
        {
            _targetId = id;
            if (RootObject.Instance?.LEE?.ActivityManager != null)
            {
                RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
            }
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            UpdateState();
        }

        public void SetEnabled(bool isEnabled)
        {
            _isShowing = isEnabled;
            this.enabled = isEnabled;

            foreach (var h in _handles)
            {
                if (h != null) h.gameObject.SetActive(isEnabled);
            }

            UpdateHandleVisibility();

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = isEnabled;
            }
        }

        public void UpdateHandleVisibility()
        {
            if (!_isShowing || !enabled)
            {
                foreach (var h in _rotationHandles)
                {
                    if (h != null) h.gameObject.SetActive(false);
                }
                return;
            }

            for (int i = 0; i < _rotationHandles.Count; i++)
            {
                var h = _rotationHandles[i];
                if (h == null) continue;

                if (i < 4)
                {
                    h.gameObject.SetActive(RotationHandlesConfig.ShowHandleForX);
                }
                else if (i < 8)
                {
                    h.gameObject.SetActive(RotationHandlesConfig.ShowHandleForY);
                }
                else
                {
                    h.gameObject.SetActive(RotationHandlesConfig.ShowHandleForZ);
                }
            }
        }

        public void OnlyRotateAround(BoundingRotationType boundingRotationType)
        {
            switch (boundingRotationType)
            {
                case BoundingRotationType.X:
                    RotationHandlesConfig.ShowHandleForX = true;
                    RotationHandlesConfig.ShowHandleForY = false;
                    RotationHandlesConfig.ShowHandleForZ = false;
                    break;
                case BoundingRotationType.Y:
                    RotationHandlesConfig.ShowHandleForX = false;
                    RotationHandlesConfig.ShowHandleForY = true;
                    RotationHandlesConfig.ShowHandleForZ = false;
                    break;
                case BoundingRotationType.Z:
                    RotationHandlesConfig.ShowHandleForX = false;
                    RotationHandlesConfig.ShowHandleForY = false;
                    RotationHandlesConfig.ShowHandleForZ = true;
                    break;
                case BoundingRotationType.ALL:
                default:
                    RotationHandlesConfig.ShowHandleForX = true;
                    RotationHandlesConfig.ShowHandleForY = true;
                    RotationHandlesConfig.ShowHandleForZ = true;
                    break;
            }
        }

        private void OnEditModeChanged(bool editModeActive)
        {
            UpdateState();
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _targetId)
            {
                if (_obj != null) _obj.positionLock = locked;
                _isLocked = locked;
                UpdateState();
            }
        }

        private void UpdateState()
        {
            bool editMode = false;
            if (_activityManager != null)
            {
                editMode |= _activityManager.EditModeActive;
            }
            if (RootObject.Instance?.LEE?.ActivityManager != null)
            {
                editMode |= RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            }

            bool isLocked = (_obj != null && _obj.positionLock) || _isLocked;
            bool show = editMode && !isLocked;
            SetEnabled(show);
        }

        private void OnDestroy()
        {
            RestoreParentGrabInteractable();

            LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
            if (RootObject.Instance != null && RootObject.Instance.LEE != null && RootObject.Instance.LEE.ActivityManager != null)
            {
                RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditModeChanged;
            }

            if (_scaleHandleMaterial != null)
            {
                Destroy(_scaleHandleMaterial);
                _scaleHandleMaterial = null;
            }
            if (_rotationHandleMaterial != null)
            {
                Destroy(_rotationHandleMaterial);
                _rotationHandleMaterial = null;
            }
        }

        private void OnDisable()
        {
            RestoreParentGrabInteractable();
        }

        private void Start()
        {
            if (Target == null) Target = transform;

            CreateHandles();
            CreateRotationHandles();
            CreateLineRenderer();

            // Initial update if Setup was called before Start
            if (_obj != null || !string.IsNullOrEmpty(_targetId))
            {
                UpdateState();
            }
            else
            {
                SetEnabled(false);
            }

            UpdateBounds();
            _isInitialized = true;
        }

        private Material CreateHandleMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            var mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
            return mat;
        }

        private void CreateHandles()
        {
            if (_scaleHandleMaterial == null)
            {
                _scaleHandleMaterial = CreateHandleMaterial(_handleColor);
            }

            for (int i = 0; i < 8; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Handle_{i}";
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * _handleSize;
                var renderer = go.GetComponent<Renderer>();
                if (renderer) renderer.material = _scaleHandleMaterial;

                var handle = go.AddComponent<BoundingBoxHandle>();
                handle.Type = BoundingBoxHandle.HandleType.Scale;
                handle.OnDownHandle += OnHandleDown;
                handle.OnDragHandle += OnHandleDrag;
                handle.OnUpHandle += OnHandleUp;

                // 0: ---, 1: --+, 2: -+-, 3: -++, 4: +--, 5: +-+, 6: ++-, 7: +++
                Vector3 dir = new Vector3(
                    (i & 4) == 0 ? -1 : 1,
                    (i & 2) == 0 ? -1 : 1,
                    (i & 1) == 0 ? -1 : 1
                );
                handle.Axis = dir;

                _handles.Add(handle);
            }
        }

        private void CreateRotationHandles()
        {
            if (_rotationHandleMaterial == null)
            {
                _rotationHandleMaterial = CreateHandleMaterial(Color.yellow);
            }

            // 12 edges
            for (int i = 0; i < 12; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"RotHandle_{i}";
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * _handleSize * 0.8f;
                var renderer = go.GetComponent<Renderer>();
                if (renderer) renderer.material = _rotationHandleMaterial;

                var handle = go.AddComponent<BoundingBoxHandle>();
                handle.Type = BoundingBoxHandle.HandleType.Rotate;
                handle.OnDownHandle += OnHandleDown;
                handle.OnDragHandle += OnHandleDrag;
                handle.OnUpHandle += OnHandleUp;

                if (i < 4) handle.Axis = Vector3.right;       // 0-3: X-axis rotation
                else if (i < 8) handle.Axis = Vector3.up;     // 4-7: Y-axis rotation
                else handle.Axis = Vector3.forward;           // 8-11: Z-axis rotation

                _rotationHandles.Add(handle);
            }
        }

        private void CreateLineRenderer()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.startWidth = 0.005f;
            _lineRenderer.endWidth = 0.005f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = _lineColor;
            _lineRenderer.endColor = _lineColor;
            _lineRenderer.positionCount = 16;
        }

        private void Update()
        {
            if (Target != null && transform.hasChanged)
            {
                UpdateBounds();
                transform.hasChanged = false;
            }
        }

        public void UpdateBounds()
        {
            if (Target == null) return;

            Bounds localBounds = GetLocalBounds(Target.gameObject);
            Vector3 center = localBounds.center;
            Vector3 extents = localBounds.extents;
            _boundsCenter = center;
            _boundsExtents = extents;

            float maxDim = Mathf.Max(extents.x, extents.y, extents.z) * 2.0f;
            float dynamicHandleSize = maxDim > Mathf.Epsilon ? maxDim * 0.08f : _handleSize;

            for (int i = 0; i < _handles.Count; i++)
            {
                Vector3 dir = _handles[i].Axis;
                Vector3 pos = center + Vector3.Scale(extents, dir);
                _handles[i].transform.localPosition = pos;
                _handles[i].transform.localScale = Vector3.one * dynamicHandleSize;
            }

            SetRotHandlePos(0, center, new Vector3(0, -extents.y, -extents.z));
            SetRotHandlePos(1, center, new Vector3(0, -extents.y, extents.z));
            SetRotHandlePos(2, center, new Vector3(0, extents.y, -extents.z));
            SetRotHandlePos(3, center, new Vector3(0, extents.y, extents.z));

            SetRotHandlePos(4, center, new Vector3(-extents.x, 0, -extents.z));
            SetRotHandlePos(5, center, new Vector3(-extents.x, 0, extents.z));
            SetRotHandlePos(6, center, new Vector3(extents.x, 0, -extents.z));
            SetRotHandlePos(7, center, new Vector3(extents.x, 0, extents.z));

            SetRotHandlePos(8, center, new Vector3(-extents.x, -extents.y, 0));
            SetRotHandlePos(9, center, new Vector3(-extents.x, extents.y, 0));
            SetRotHandlePos(10, center, new Vector3(extents.x, -extents.y, 0));
            SetRotHandlePos(11, center, new Vector3(extents.x, extents.y, 0));

            float rotHandleSize = dynamicHandleSize * 0.8f;
            foreach (var h in _rotationHandles)
            {
                if (h != null) h.transform.localScale = Vector3.one * rotHandleSize;
            }

            UpdateLines(center, extents);
            UpdateHandleVisibility();

            if (_targetBoundsCollider != null)
            {
                _targetBoundsCollider.center = center;
                _targetBoundsCollider.size = extents * 2f;
            }
        }

        private void SetRotHandlePos(int index, Vector3 center, Vector3 offset)
        {
            if (index < _rotationHandles.Count && _rotationHandles[index] != null)
            {
                _rotationHandles[index].transform.localPosition = center + offset;
            }
        }

        private Bounds GetLocalBounds(GameObject go)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            bool first = true;

            foreach (var r in renderers)
            {
                if (ShouldSkipRenderer(r)) continue;

                if (TryGetRendererLocalBounds(r, out Bounds rendererBounds))
                {
                    EncapsulateTransformedBounds(ref bounds, ref first, rendererBounds, r.transform.localToWorldMatrix);
                }
                else
                {
                    EncapsulateWorldBounds(ref bounds, ref first, r.bounds);
                }
            }

            return bounds;
        }

        private bool ShouldSkipRenderer(Renderer renderer)
        {
            if (renderer.GetComponent<LineRenderer>()) return true;
            if (renderer.GetComponent<ParticleSystem>()) return true;
            return renderer.GetComponentInParent<BoundingBoxHandle>() != null;
        }

        private bool TryGetRendererLocalBounds(Renderer renderer, out Bounds bounds)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                bounds = skinnedMeshRenderer.localBounds;
                return true;
            }

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                bounds = meshFilter.sharedMesh.bounds;
                return true;
            }

            bounds = new Bounds();
            return false;
        }

        private void EncapsulateTransformedBounds(ref Bounds bounds, ref bool first, Bounds sourceBounds, Matrix4x4 localToWorld)
        {
            Vector3 min = sourceBounds.min;
            Vector3 max = sourceBounds.max;

            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z)));
        }

        private void EncapsulatePoint(ref Bounds bounds, ref bool first, Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            if (first)
            {
                bounds = new Bounds(localPoint, Vector3.zero);
                first = false;
            }
            else
            {
                bounds.Encapsulate(localPoint);
            }
        }

        private void EncapsulateWorldBounds(ref Bounds bounds, ref bool first, Bounds worldBounds)
        {
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, min.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, min.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, max.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, max.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, min.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, min.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, max.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, max.y, max.z));
        }

        private void UpdateLines(Vector3 center, Vector3 extents)
        {
            if (_lineRenderer == null) return;

            Vector3 min = center - extents;
            Vector3 max = center + extents;

            Vector3 p0 = new Vector3(min.x, min.y, min.z);
            Vector3 p1 = new Vector3(max.x, min.y, min.z);
            Vector3 p2 = new Vector3(max.x, min.y, max.z);
            Vector3 p3 = new Vector3(min.x, min.y, max.z);

            Vector3 p4 = new Vector3(min.x, max.y, min.z);
            Vector3 p5 = new Vector3(max.x, max.y, min.z);
            Vector3 p6 = new Vector3(max.x, max.y, max.z);
            Vector3 p7 = new Vector3(min.x, max.y, max.z);

            Vector3[] points = new Vector3[] {
                p0, p1, p2, p3, p0,
                p4, p5, p6, p7, p4,
                p5, p1, p2, p6, p7, p3
            };

            _lineRenderer.positionCount = points.Length;
            _lineRenderer.SetPositions(points);
        }

        private void OnHandleDown(BoundingBoxHandle handle)
        {
            _isHandleInteractionActive = true;
            if (Target != null)
            {
                _initialScale = Target.localScale;
                _initialRotation = Target.localRotation;
            }
            _accumulatedScaleRatio = 1.0f;
            _accumulatedAngle = 0.0f;

            if (_parentGrabInteractable == null)
            {
                _parentGrabInteractable = transform.parent != null ? transform.parent.GetComponentInParent<XRGrabInteractable>() : null;
            }

            if (_parentGrabInteractable != null)
            {
                _parentGrabWasEnabled = _parentGrabInteractable.enabled;
                _parentGrabInteractable.enabled = false;
            }

            if (handle.Type == BoundingBoxHandle.HandleType.Scale)
            {
                ScaleStarted?.Invoke();
                if (Target != null) OnScaleStarted?.Invoke(Target.localScale);
            }
            else if (handle.Type == BoundingBoxHandle.HandleType.Rotate)
            {
                RotateStarted?.Invoke();
                if (Target != null) OnRotationStarted?.Invoke(Target.rotation);
            }
        }

        private void OnHandleDrag(BoundingBoxHandle handle, Vector3 delta)
        {
            if (!_isHandleInteractionActive || Target == null) return;

            var gridManager = RootObject.Instance?.GridManager;
            bool snap = gridManager != null && gridManager.snapEnabled;

            if (handle.Type == BoundingBoxHandle.HandleType.Scale)
            {
                Vector3 handleOffset = handle.transform.localPosition - _boundsCenter;
                Vector3 direction = handleOffset.normalized;

                Vector3 localDelta = transform.InverseTransformVector(delta);
                float dragAmount = Vector3.Dot(localDelta, direction);

                if (Mathf.Abs(dragAmount) > 0.0001f)
                {
                    float radius = Mathf.Max(handleOffset.magnitude, 0.0001f);
                    float deltaRatio = dragAmount / radius;
                    _accumulatedScaleRatio += deltaRatio;
                    _accumulatedScaleRatio = Mathf.Max(0.01f, _accumulatedScaleRatio);

                    if (snap)
                    {
                        float step = Mathf.Max(0.01f, gridManager.scaleStep / 100f);
                        float snappedRatio = Mathf.Max(step, Mathf.Round(_accumulatedScaleRatio / step) * step);
                        Target.localScale = _initialScale * snappedRatio;
                    }
                    else
                    {
                        Target.localScale = _initialScale * _accumulatedScaleRatio;
                    }

                    OnScaleChanged?.Invoke(Target.localScale);
                    UpdateBounds();
                }
            }
            else if (handle.Type == BoundingBoxHandle.HandleType.Rotate)
            {
                Vector3 axis = handle.Axis;
                Vector3 handlePos = handle.transform.localPosition - _boundsCenter;
                Vector3 tangent = Vector3.Cross(axis, handlePos).normalized;
                Vector3 localDelta = transform.InverseTransformVector(delta);

                float dragAmount = Vector3.Dot(localDelta, tangent);

                if (Mathf.Abs(dragAmount) > 0.0001f)
                {
                    float radius = Mathf.Max(handlePos.magnitude, 0.0001f);
                    float deltaAngle = dragAmount / radius * Mathf.Rad2Deg;
                    _accumulatedAngle += deltaAngle;

                    if (snap)
                    {
                        float step = Mathf.Max(1f, gridManager.angleStep);
                        float snappedAngle = Mathf.Round(_accumulatedAngle / step) * step;
                        Target.localRotation = _initialRotation * Quaternion.AngleAxis(snappedAngle, axis);
                    }
                    else
                    {
                        Target.localRotation = _initialRotation * Quaternion.AngleAxis(_accumulatedAngle, axis);
                    }

                    OnRotationChanged?.Invoke(Target.rotation);
                    UpdateBounds();
                }
            }
        }

        private void OnHandleUp(BoundingBoxHandle handle)
        {
            if (!_isHandleInteractionActive) return;

            _accumulatedScaleRatio = 1.0f;
            _accumulatedAngle = 0.0f;

            RestoreParentGrabInteractable();

            if (Target != null)
            {
                if (handle.Type == BoundingBoxHandle.HandleType.Scale)
                {
                    OnScaleEnded?.Invoke(Target.localScale);
                    ScaleStopped?.Invoke();
                }
                else if (handle.Type == BoundingBoxHandle.HandleType.Rotate)
                {
                    OnRotationEnded?.Invoke(Target.rotation);
                    RotateStopped?.Invoke();
                }
            }
        }

        private void RestoreParentGrabInteractable()
        {
            if (!_isHandleInteractionActive) return;

            _isHandleInteractionActive = false;

            if (_parentGrabInteractable != null)
            {
                _parentGrabInteractable.enabled = _parentGrabWasEnabled;
            }
        }
    }
}
