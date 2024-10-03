using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// The visionOS Far Caster serves as a means routing the spatial touch interaction collider to the Near-Far Interactor.
    /// This class was mainly tested for Bounded and Unbounded mixed reality modes. It may also work in VR mode, but some additional testing and iteration may be required.
    /// </summary>
    public class VisionOSFarCaster : InteractionCasterBase, ICurveInteractionCaster, IUIModelUpdater
    {
        [Header("Vision Pro Settings")]
        [SerializeField]
        SpatialTouchInputReader m_SpatialTouchInputReader;

        [Header("Filtering Settings")]
        [SerializeField]
        [Tooltip("Layer mask used for limiting sphere cast and sphere overlap targets.")]
        LayerMask m_PhysicsLayerMask = -1;

        /// <summary>
        /// Gets or sets layer mask used for limiting sphere cast and sphere overlap targets.
        /// </summary>
        public LayerMask physicsLayerMask
        {
            get => m_PhysicsLayerMask;
            set => m_PhysicsLayerMask = value;
        }

        [SerializeField]
        QueryTriggerInteraction m_PhysicsTriggerInteraction = QueryTriggerInteraction.Ignore;

        /// <summary>
        /// Determines whether the cast sphere overlap will hit triggers.
        /// </summary>
        public QueryTriggerInteraction physicsTriggerInteraction
        {
            get => m_PhysicsTriggerInteraction;
            set => m_PhysicsTriggerInteraction = value;
        }
        
        bool hasActiveTouch => m_HasTouchReader && m_SpatialTouchInputReader.hasActiveTouch.Value;

        bool m_HasTouchReader;

        NativeArray<Vector3> m_SamplePoints;
        NativeArray<Vector3> m_EmptySamplePoints;

        public NativeArray<Vector3> samplePoints => hasActiveTouch ? m_SamplePoints : m_EmptySamplePoints;

        public Vector3 lastSamplePoint => hasActiveTouch && m_SamplePoints.Length > 1 ? m_SamplePoints[^1] : Vector3.zero;

        /// <inheritdoc />
        protected override bool InitializeCaster()
        {
            if (m_HasTouchReader && isActiveAndEnabled)
                isInitialized = true;
            return isInitialized;
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            m_HasTouchReader = m_SpatialTouchInputReader != null;
            m_SamplePoints = new NativeArray<Vector3>(2, Allocator.Persistent);
            m_EmptySamplePoints = new NativeArray<Vector3>(1, Allocator.Persistent);
        }

        void OnEnable()
        {
            UpdateSamplePoints(Vector3.zero, Vector3.zero);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_SamplePoints.IsCreated)
                m_SamplePoints.Dispose();
            if (m_EmptySamplePoints.IsCreated)
                m_EmptySamplePoints.Dispose();
        }

        /// <inheritdoc />
        protected override void UpdateInternalData()
        {
            base.UpdateInternalData();
#if POLYSPATIAL_1_1_OR_NEWER
            if (!m_SpatialTouchInputReader.TryGetPointerState(out var state))
                return;

            if (state.targetObject != null)
            {
                Vector3 castOriginPoint = effectiveCastOrigin.position;
                Vector3 sampleEndPoint = castOriginPoint + (state.interactionPosition - castOriginPoint).normalized * 10f;
                UpdateSamplePoints(sampleEndPoint, castOriginPoint);
            }
            else
            {
                var originPosition = effectiveCastOrigin.position;
                UpdateSamplePoints(originPosition + state.startInteractionRayDirection * 10f, originPosition);
            }
#endif
        }

        /// <inheritdoc />
        public bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> colliders, List<RaycastHit> raycastHits)
        {
#if POLYSPATIAL_1_1_OR_NEWER
            if (!base.TryGetColliderTargets(interactionManager, colliders))
                return false;

            colliders.Clear();
            raycastHits.Clear();

            if (!m_SpatialTouchInputReader.TryGetPointerState(out var state))
                return false;

            if (state.targetObject == null)
                return false;

            if (state.targetObject.TryGetComponent(out Collider targetCollider) && IsColliderValid(targetCollider))
            {
                colliders.Add(targetCollider);
                raycastHits.Add(new RaycastHit
                {
                    point = targetCollider.ClosestPoint(state.interactionPosition),
                });
                return true;
            }
#endif
            return false;
        }

        bool IsColliderValid(Collider targetCollider)
        {
            bool layerMatch = (m_PhysicsLayerMask & (1 << targetCollider.gameObject.layer)) != 0;
            bool shouldIgnoreFromTrigger = m_PhysicsTriggerInteraction == QueryTriggerInteraction.Ignore && targetCollider.isTrigger;
            return layerMatch && !shouldIgnoreFromTrigger;
        }

        void UpdateSamplePoints(Vector3 hitPoint, Vector3 rayOrigin)
        {
            m_SamplePoints[0] = rayOrigin;
            m_SamplePoints[1] = hitPoint;
        }

        /// <inheritdoc />
        public bool UpdateUIModel(ref TrackedDeviceModel uiModel, bool isSelectActive, in Vector2 scrollDelta)
        {
            if (!isInitialized)
                return false;

            var sampleOrigin = effectiveCastOrigin;
            uiModel.position = sampleOrigin.position;
            uiModel.orientation = sampleOrigin.rotation;
            uiModel.select = isSelectActive;
            uiModel.scrollDelta = scrollDelta;
            uiModel.raycastLayerMask = m_PhysicsLayerMask;
            uiModel.interactionType = UIInteractionType.Ray;

            var raycastPoints = uiModel.raycastPoints;
            raycastPoints.Clear();

            UpdateInternalData();
            var numPoints = m_SamplePoints.Length;
            if (numPoints <= 0)
            {
                return false;
            }

            if (raycastPoints.Capacity < numPoints)
                raycastPoints.Capacity = numPoints;

            for (var i = 0; i < numPoints; ++i)
            {
                raycastPoints.Add(m_SamplePoints[i]);
            }

            return true;
        }
    }
}