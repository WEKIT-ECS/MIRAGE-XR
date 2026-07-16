using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using MirageXR; 

namespace MirageXR
{
    public class SimpleBoundingBox : MonoBehaviour
    {
        [SerializeField] private float _handleSize = 0.05f;
        [SerializeField] private Color _handleColor = Color.white;
        [SerializeField] private Color _lineColor = Color.cyan;

        public Transform Target;
        public Action<Vector3> OnScaleChanged;
        public Action<Vector3> OnScaleEnded;
        public Action<Quaternion> OnRotationChanged;
        public Action<Quaternion> OnRotationEnded;

        private List<BoundingBoxHandle> _handles = new List<BoundingBoxHandle>();
        private List<BoundingBoxHandle> _rotationHandles = new List<BoundingBoxHandle>();
        private LineRenderer _lineRenderer;

        private LearningExperienceEngine.ToggleObject _obj;
        private string _targetId;
        private bool _isLocked;
        private bool _isHandleInteractionActive;
        private bool _parentGrabWasEnabled;
        private XRGrabInteractable _parentGrabInteractable;
        
        private static LearningExperienceEngine.ActivityManager _activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

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
            // Subscribe to both to be safe, or just the new one if we are sure
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;
            
            UpdateState();
        }

        public void SetEnabled(bool enabled)
        {
            foreach(var h in _handles) h.gameObject.SetActive(enabled);
            foreach(var h in _rotationHandles) h.gameObject.SetActive(enabled);
            if (_lineRenderer) _lineRenderer.enabled = enabled;
            this.enabled = enabled;
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
            bool editMode = _activityManager.EditModeActive || RootObject.Instance.LEE.ActivityManager.IsEditorMode;
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
            if (_obj != null || !string.IsNullOrEmpty(_targetId)) UpdateState();
            else SetEnabled(false); // Hide by default if not setup
            
            UpdateBounds();
        }

        private void CreateHandles()
        {
            for (int i = 0; i < 8; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Handle_{i}";
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * _handleSize;
                var renderer = go.GetComponent<Renderer>();
                if (renderer) renderer.material.color = _handleColor;

                var handle = go.AddComponent<BoundingBoxHandle>();
                handle.Type = BoundingBoxHandle.HandleType.Scale;
                handle.OnDownHandle += OnHandleDown;
                handle.OnDragHandle += OnHandleDrag;
                handle.OnUpHandle += OnHandleUp;
                
                // Direction based on index
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
            // 12 edges
            // X-axis aligned edges (rotate around X): (0, +/-Y, +/-Z)
            // Y-axis aligned edges (rotate around Y): (+/-X, 0, +/-Z)
            // Z-axis aligned edges (rotate around Z): (+/-X, +/-Y, 0)
            
            for (int i = 0; i < 12; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"RotHandle_{i}";
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * _handleSize * 0.8f; // Slightly smaller
                var renderer = go.GetComponent<Renderer>();
                if (renderer) renderer.material.color = Color.yellow; // Distinct color

                var handle = go.AddComponent<BoundingBoxHandle>();
                handle.Type = BoundingBoxHandle.HandleType.Rotate;
                handle.OnDownHandle += OnHandleDown;
                handle.OnDragHandle += OnHandleDrag;
                handle.OnUpHandle += OnHandleUp;

                // Assign Axis based on group
                if (i < 4) handle.Axis = Vector3.right;       // 0-3: X-axis rotation (edges parallel to X)
                else if (i < 8) handle.Axis = Vector3.up;     // 4-7: Y-axis rotation (edges parallel to Y)
                else handle.Axis = Vector3.forward;           // 8-11: Z-axis rotation (edges parallel to Z)

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
            
            // Setup indices for a box
            // 0-1-3-2-0, 4-5-7-6-4, 0-4, 1-5, 2-6, 3-7
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

            // Convert world bounds to local space of THIS transform
            // This is tricky if rotations involved.
            // Simplifying: Assume this BoundingBox is ATTACHED to the model root (so local space matches).
            // Use local bounds from meshes.
            
            Bounds localBounds = GetLocalBounds(Target.gameObject);
            Vector3 center = localBounds.center;
            Vector3 extents = localBounds.extents;

            // Calculate dynamic handle size (e.g., 5% of the largest dimension, with a minimum fallback)
            float maxDim = Mathf.Max(extents.x, extents.y, extents.z) * 2.0f;
            float dynamicHandleSize = Mathf.Max(maxDim * 0.005f, 0.005f); // 5% or at least 5cm //TODO i have to change this before i commit

            for (int i = 0; i < 8; i++)
            {
                 Vector3 dir = _handles[i].Axis;
                 Vector3 pos = center + Vector3.Scale(extents, dir);
                 _handles[i].transform.localPosition = pos;
                 _handles[i].transform.localScale = Vector3.one * dynamicHandleSize;
            }

            // Position Rotation Handles (Edge Midpoints)
            // Order: 4 X-aligned, 4 Y-aligned, 4 Z-aligned
            
            // X-aligned edges: Midpoints have X=0 (relative to center), Y=+/-ext, Z=+/-ext
            // Wait, midpoint of edge parallel to X starts at x=-ext to x=+ext. Midpoint x is center.x.
            // Correct.
            
            // X-aligned (Rotate X)
            SetRotHandlePos(0, center, new Vector3(0, -extents.y, -extents.z));
            SetRotHandlePos(1, center, new Vector3(0, -extents.y, extents.z));
            SetRotHandlePos(2, center, new Vector3(0, extents.y, -extents.z));
            SetRotHandlePos(3, center, new Vector3(0, extents.y, extents.z));
            
            // Y-aligned (Rotate Y) - Edges parallel to Y. Midpoint Y is center.y. X/Z are +/- extents.
            SetRotHandlePos(4, center, new Vector3(-extents.x, 0, -extents.z));
            SetRotHandlePos(5, center, new Vector3(-extents.x, 0, extents.z));
            SetRotHandlePos(6, center, new Vector3(extents.x, 0, -extents.z));
            SetRotHandlePos(7, center, new Vector3(extents.x, 0, extents.z));
            
            // Z-aligned (Rotate Z) - Edges parallel to Z. Midpoint Z is center.z. X/Y are +/- extents.
            SetRotHandlePos(8, center, new Vector3(-extents.x, -extents.y, 0));
            SetRotHandlePos(9, center, new Vector3(-extents.x, extents.y, 0));
            SetRotHandlePos(10, center, new Vector3(extents.x, -extents.y, 0));
            SetRotHandlePos(11, center, new Vector3(extents.x, extents.y, 0));

            float rotHandleSize = dynamicHandleSize * 0.8f;
            foreach(var h in _rotationHandles) h.transform.localScale = Vector3.one * rotHandleSize;
            
            UpdateLines(center, extents);
        }

        private void SetRotHandlePos(int index, Vector3 center, Vector3 offset)
        {
            if (index < _rotationHandles.Count)
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
            // Get world corners of mesh bounds
            Vector3 min = sourceBounds.min;
            Vector3 max = sourceBounds.max;
            // ... transform to this.worldToLocalMatrix

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
            
            // 0: min.x, min.y, min.z
            Vector3 p0 = new Vector3(min.x, min.y, min.z);
            Vector3 p1 = new Vector3(max.x, min.y, min.z);
            Vector3 p2 = new Vector3(max.x, min.y, max.z);
            Vector3 p3 = new Vector3(min.x, min.y, max.z);
            
            Vector3 p4 = new Vector3(min.x, max.y, min.z);
            Vector3 p5 = new Vector3(max.x, max.y, min.z);
            Vector3 p6 = new Vector3(max.x, max.y, max.z);
            Vector3 p7 = new Vector3(min.x, max.y, max.z);
            
            // Continuous path that covers all 12 edges without diagonals
            // Bottom loop: 0->1->2->3->0
            // Up to top: 0->4
            // Top loop: 4->5->6->7->4
            // Remaining verticals (retracing): 4->5->1 (down), 1->2->6 (up), 6->7->3 (down)
            
            Vector3[] points = new Vector3[] {
                p0, p1, p2, p3, p0, // Bottom loop
                p4, p5, p6, p7, p4, // Vertical + Top loop
                p5, p1, p2, p6, p7, p3 // Retrace top->down, bottom->up, top->down
            };
            
            _lineRenderer.positionCount = points.Length;
            _lineRenderer.SetPositions(points);
        }

        private void OnHandleDrag(BoundingBoxHandle handle, Vector3 delta)
        {
            if (!_isHandleInteractionActive) return;

            if (handle.Type == BoundingBoxHandle.HandleType.Scale)
            {
                // Simple uniform scale based on drag
                // Project delta onto handle direction from center?
                
                float sensitivity = 1.0f;
                
                // direction from center to handle
                Vector3 direction = (handle.transform.localPosition - Vector3.zero).normalized; // Assuming center is 0 local
                
                // local delta
                Vector3 localDelta = transform.InverseTransformVector(delta);
                
                float dragAmount = Vector3.Dot(localDelta, direction);
                
                if (Mathf.Abs(dragAmount) > 0.0001f)
                {
                    float scaleFactor = 1.0f + (dragAmount * sensitivity);
                    if (scaleFactor <= 0.0001f) return;

                    Target.localScale *= scaleFactor;
                    OnScaleChanged?.Invoke(Target.localScale);
                    UpdateBounds();
                }
            }
            else if (handle.Type == BoundingBoxHandle.HandleType.Rotate)
            {
                 // Rotation logic
                 // Handle Axis is the rotation axis.
                 Vector3 axis = handle.Axis;
                 
                 // Handle Position relative to center
                 Vector3 handlePos = handle.transform.localPosition;
                 
                 // Tangent vector: Cross(Axis, HandlePos) -> Direction of movement that causes positive rotation?
                 // Let's check: rot around Y (0,1,0). Handle at (1,0,0). Tangent = Cross(Y, X) = -Z? (0,0,-1).
                 // If we move along -Z, does it rotate +Y? 
                 // Left hand rule? Unity is left handed? 
                 // Let's just project local delta onto the tangent.
                 
                 Vector3 tangent = Vector3.Cross(axis, handlePos).normalized;
                 Vector3 localDelta = transform.InverseTransformVector(delta);
                 
                 float dragAmount = Vector3.Dot(localDelta, tangent);
                 
                 if (Mathf.Abs(dragAmount) > 0.0001f)
                 {
                     float sensitivity = 100.0f; // Scale rotation speed
                     float angle = dragAmount * sensitivity;
                     
                     // Rotate around LOCAL axis
                     Target.Rotate(axis, angle, Space.Self);
                     
                     OnRotationChanged?.Invoke(Target.rotation);
                     UpdateBounds();
                 }
            }
        }

        private void OnHandleDown(BoundingBoxHandle handle)
        {
            _isHandleInteractionActive = true;

            if (_parentGrabInteractable == null)
            {
                _parentGrabInteractable = transform.parent != null ? transform.parent.GetComponentInParent<XRGrabInteractable>() : null;
            }

            if (_parentGrabInteractable != null)
            {
                _parentGrabWasEnabled = _parentGrabInteractable.enabled;
                _parentGrabInteractable.enabled = false;
            }
        }

        private void OnHandleUp(BoundingBoxHandle handle)
        {
            if (!_isHandleInteractionActive) return;

            RestoreParentGrabInteractable();

            if (Target != null)
            {
                if (handle.Type == BoundingBoxHandle.HandleType.Scale)
                {
                    OnScaleEnded?.Invoke(Target.localScale);
                }
                else if (handle.Type == BoundingBoxHandle.HandleType.Rotate)
                {
                    OnRotationEnded?.Invoke(Target.rotation);
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
