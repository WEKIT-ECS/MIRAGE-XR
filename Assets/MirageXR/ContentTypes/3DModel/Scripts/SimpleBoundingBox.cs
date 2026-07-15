using System;
using System.Collections.Generic;
using UnityEngine;
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
        private Vector3[] _corners = new Vector3[8]; // 8 corners

        private LearningExperienceEngine.ToggleObject _obj;
        private string _targetId;
        private bool _isLocked;
        
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
            LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
            if (RootObject.Instance != null && RootObject.Instance.LEE != null && RootObject.Instance.LEE.ActivityManager != null)
            {
                RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditModeChanged;
            }
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

            // Assuming Target has renderers
            Bounds bounds = new Bounds(Target.localPosition, Vector3.zero);
            // We want local bounds of the target relative to this object if this is parent?
            // Actually, we probably want this BoundingBox object to be a parent or sibling of the target.
            // Let's assume BoundingBox is a parent or wrapper.
            // But if Target is arbitrary, we need to know its bounds.
            
            // For simplicity in this specific task (preview), the Target will likely be a child of this object 
            // OR this object is attached TO the target object (Target == transform).
            // If attached to target, Target.localPosition is 0.
            
            // Get combined bounds of children
            Renderer[] renderers = Target.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
            
            // Convert world bounds to local space of THIS transform
            // This is tricky if rotations involved.
            // Simplifying: Assume this BoundingBox is ATTACHED to the model root (so local space matches).
            // Use local bounds from meshes.
            
            Bounds localBounds = GetLocalBounds(Target.gameObject);
            Vector3 center = localBounds.center;
            Vector3 extents = localBounds.extents;

            // Calculate dynamic handle size (e.g., 5% of the largest dimension, with a minimum fallback)
            float maxDim = Mathf.Max(extents.x, extents.y, extents.z) * 2.0f;
            float dynamicHandleSize = Mathf.Max(maxDim * 0.05f, 0.05f); // 5% or at least 5cm

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
             if (renderers.Length > 0)
             {
                 // Transform points to local space
                 bool first = true;
                 foreach (var r in renderers)
                 {
                     if(r.GetComponent<LineRenderer>()) continue; // Skip lines
                     if(r.GetComponent<ParticleSystem>()) continue;
                     
                     // If it's a handle, skip
                     if(r.transform.parent == transform) continue;

                     MeshFilter mf = r.GetComponent<MeshFilter>();
                     if (mf && mf.sharedMesh)
                     {
                         Bounds mb = mf.sharedMesh.bounds;
                         Vector3[] verts = new Vector3[8];
                         
                         // Get world corners of mesh bounds
                         Vector3 min = mb.min;
                         Vector3 max = mb.max;
                         // ... transform to this.worldToLocalMatrix
                         
                         // Approximation: Just use world bounds and inverse transform
                         Bounds wb = r.bounds;
                         Vector3 wMin = wb.min;
                         Vector3 wMax = wb.max;
                         
                         Vector3[] wCorners = new Vector3[] {
                             new Vector3(wMin.x, wMin.y, wMin.z),
                             new Vector3(wMin.x, wMin.y, wMax.z),
                             new Vector3(wMin.x, wMax.y, wMin.z),
                             new Vector3(wMin.x, wMax.y, wMax.z),
                             new Vector3(wMax.x, wMin.y, wMin.z),
                             new Vector3(wMax.x, wMin.y, wMax.z),
                             new Vector3(wMax.x, wMax.y, wMin.z),
                             new Vector3(wMax.x, wMax.y, wMax.z)
                         };
                         
                         foreach(var wc in wCorners)
                         {
                             Vector3 lc = transform.InverseTransformPoint(wc);
                             if (first) { bounds = new Bounds(lc, Vector3.zero); first = false; }
                             else bounds.Encapsulate(lc);
                         }
                     }
                 }
             }
             return bounds;
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

        private void OnHandleUp(BoundingBoxHandle handle)
        {
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
    }
}
