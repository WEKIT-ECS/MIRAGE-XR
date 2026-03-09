#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    /// <summary>
    /// Spawns an object on physics trigger enter with an <see cref="ARPlane"/>, at the point of contact on the plane.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ARContactSpawnTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The behavior to use to spawn objects.")]
        ObjectSpawner m_ObjectSpawner;

        /// <summary>
        /// The behavior to use to spawn objects.
        /// </summary>
        public ObjectSpawner objectSpawner
        {
            get => m_ObjectSpawner;
            set => m_ObjectSpawner = value;
        }

        [SerializeField]
        [Tooltip("If enabled, spawning will be blocked if the active interactor in the associated XR Interaction Group is hovering or selecting an interactable object.")]
        bool m_BlockSpawnDuringInteraction;

        /// <summary>
        /// If enabled, spawning will be blocked if the active interactor in the associated <see cref="interactionGroup"/> is hovering or selecting an interactable object.
        /// </summary>
        public bool blockSpawnDuringInteraction
        {
            get => m_BlockSpawnDuringInteraction;
            set => m_BlockSpawnDuringInteraction = value;
        }

        [SerializeField]
        [Tooltip("XR Interaction Group associated with this contact spawn trigger.")]
        XRInteractionGroup m_InteractionGroup;

        /// <summary>
        /// XR Interaction Group associated with this contact spawn trigger. Spawning will be blocked if an interactor from this XR Interaction Group is
        /// hovering or selecting and interactable and <see cref="blockSpawnDuringInteraction"/> is enabled.
        /// </summary>
        /// <remarks>If <see langword="null"/>, this scripts attempts to find an <see cref="XRInteractionGroup"/> component on the parent.</remarks>
        public XRInteractionGroup interactionGroup
        {
            get => m_InteractionGroup;
            set => m_InteractionGroup = value;
        }

        [SerializeField]
        [Tooltip("Whether to require that the AR Plane has an alignment of horizontal up to spawn on it.")]
        bool m_RequireHorizontalUpSurface;

        /// <summary>
        /// Whether to require that the <see cref="ARPlane"/> has an alignment of <see cref="PlaneAlignment.HorizontalUp"/> to spawn on it.
        /// </summary>
        public bool requireHorizontalUpSurface
        {
            get => m_RequireHorizontalUpSurface;
            set => m_RequireHorizontalUpSurface = value;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Start()
        {
            if (m_InteractionGroup == null)
            {
                m_InteractionGroup = GetComponentInParent<XRInteractionGroup>();

                if (m_BlockSpawnDuringInteraction && m_InteractionGroup == null)
                    Debug.LogWarning("Interaction group could be found. Spawning objects will not be blocked during hover or select interaction.", this);
            }

            if (m_ObjectSpawner == null)
            {
#if UNITY_2023_1_OR_NEWER
                m_ObjectSpawner = FindAnyObjectByType<ObjectSpawner>();
#else
                m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();
#endif

                if (m_ObjectSpawner == null)
                    Debug.LogWarning("Object spawner could not be found. AR Contact Spawn Trigger will be unable to spawn objects.", this);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if ((blockSpawnDuringInteraction && IsInteractionBlockingSpawn()) ||
                !TryGetSpawnSurfaceData(other, out var surfacePosition, out var surfaceNormal))
                return;

            var infinitePlane = new Plane(surfaceNormal, surfacePosition);
            var contactPoint = infinitePlane.ClosestPointOnPlane(transform.position);
            m_ObjectSpawner.TrySpawnObject(contactPoint, surfaceNormal);
        }

        /// <summary>
        /// Tries to get the surface position and normal from an object to potentially spawn on.
        /// </summary>
        /// <param name="objectCollider">The collider of the object to potentially spawn on.</param>
        /// <param name="surfacePosition">The potential world position of the spawn surface.</param>
        /// <param name="surfaceNormal">The potential normal of the spawn surface.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="objectCollider"/> is a valid spawn surface,
        /// otherwise returns <see langword="false"/>.</returns>
        public bool TryGetSpawnSurfaceData(Collider objectCollider, out Vector3 surfacePosition, out Vector3 surfaceNormal)
        {
            surfacePosition = default;
            surfaceNormal = default;

            var arPlane = objectCollider.GetComponent<ARPlane>();
            if (arPlane == null)
                return false;

            if (m_RequireHorizontalUpSurface && arPlane.alignment != PlaneAlignment.HorizontalUp)
                return false;

            surfaceNormal = arPlane.normal;
            surfacePosition = arPlane.center;
            return true;
        }

        bool IsInteractionBlockingSpawn()
        {
            if (m_InteractionGroup != null && m_InteractionGroup.activeInteractor != null)
            {
                var hoverInteractor = (IXRHoverInteractor)m_InteractionGroup.activeInteractor;
                var selectInteractor = (IXRSelectInteractor)m_InteractionGroup.activeInteractor;
                var isHovering = (hoverInteractor != null) && hoverInteractor.hasHover;
                var isSelecting = (selectInteractor != null) && selectInteractor.hasSelection;
                return isHovering || isSelecting;
            }

            return false;
        }
    }
}
#endif
