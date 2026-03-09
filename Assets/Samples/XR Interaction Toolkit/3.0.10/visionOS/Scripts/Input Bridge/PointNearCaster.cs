using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Simple interaction caster that uses an overlap sphere on the transform position to find nearfield colliders.
    /// Exists because <see cref="SphereInteractionCaster"/> requires continuous input tracking, which visionOS does not provide.
    /// </summary>
    public class PointNearCaster : InteractionCasterBase
    {
        const int k_MaxRaycastHits = 10;
        readonly Collider[] m_OverlapSphereColliderHits = new Collider[k_MaxRaycastHits];

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

        [Header("Sphere Casting Settings")]
        [SerializeField]
        [Tooltip("Radius of the sphere cast.")]
        float m_CastRadius = 0.025f;

        /// <summary>
        /// Radius of the sphere cast.
        /// </summary>
        public float castRadius
        {
            get => m_CastRadius;
            set => m_CastRadius = value;
        }

        PhysicsScene m_LocalPhysicsScene;

        /// <inheritdoc />
        protected override bool InitializeCaster()
        {
            if (!isActiveAndEnabled)
                return false;

            if (!isInitialized)
                m_LocalPhysicsScene = gameObject.scene.GetPhysicsScene();

            isInitialized = true;
            return isInitialized;
        }

        /// <inheritdoc />
        public override bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets)
        {
            if (!base.TryGetColliderTargets(interactionManager, targets))
                return false;

            var numberOfOverlaps = m_LocalPhysicsScene.OverlapSphere(effectiveCastOrigin.position, castRadius, m_OverlapSphereColliderHits,
                m_PhysicsLayerMask, m_PhysicsTriggerInteraction);

            for (var i = 0; i < numberOfOverlaps; ++i)
                targets.Add(m_OverlapSphereColliderHits[i]);

            return numberOfOverlaps > 0;
        }
    }
}
