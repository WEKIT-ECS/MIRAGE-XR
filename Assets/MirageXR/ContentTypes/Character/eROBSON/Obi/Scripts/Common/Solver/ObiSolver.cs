/**
\mainpage Obi documentation
 
Introduction:
------------- 

Obi is a position-based dynamics framework for unity. It enables the simulation of cloth, ropes and fluid in realtime, complete with two-way
rigidbody interaction.
 
Features:
-------------------

- Particles can be pinned both in local space and to rigidbodies (kinematic or not).
- Realistic wind forces.
- Rigidbodies react to particle dynamics, and particles reach to each other and to rigidbodies too.
- Easy prefab instantiation, particle-based actors can be translated, scaled and rotated.
- Custom editor tools.

*/

using UnityEngine;
using Unity.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Obi
{

    /**
     * ObiSolver simulates particles and constraints, provided by a list of ObiActor. Particles belonging to different solvers won't interact with each other in any way.
     */
    [AddComponentMenu("Physics/Obi/Obi Solver", 800)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public sealed class ObiSolver : MonoBehaviour
    {
        static ProfilerMarker m_StateInterpolationPerfMarker = new ProfilerMarker("ApplyStateInterpolation");
        static ProfilerMarker m_UpdateVisibilityPerfMarker = new ProfilerMarker("UpdateVisibility");
        static ProfilerMarker m_GetSolverBoundsPerfMarker = new ProfilerMarker("GetSolverBounds");
        static ProfilerMarker m_TestBoundsPerfMarker = new ProfilerMarker("TestBoundsAgainstCameras");
        static ProfilerMarker m_GetAllCamerasPerfMarker = new ProfilerMarker("GetAllCameras");

        public enum BackendType
        {
            Oni,
            Burst
        }

        public class ObiCollisionEventArgs : System.EventArgs
        {
            public ObiList<Oni.Contact> contacts = new ObiList<Oni.Contact>();  /**< collision contacts.*/
        }

        [Serializable]
        public class ParticleInActor
        {
            public ObiActor actor;
            public int indexInActor;

            public ParticleInActor()
            {
                this.actor = null;
                this.indexInActor = -1;
            }

            public ParticleInActor(ObiActor actor, int indexInActor)
            {
                this.actor = actor;
                this.indexInActor = indexInActor;
            }
        }

        public delegate void SolverCallback(ObiSolver solver);
        public delegate void SolverStepCallback(ObiSolver solver, float stepTime);
        public delegate void CollisionCallback(ObiSolver solver, ObiCollisionEventArgs contacts);

        public event CollisionCallback OnCollision;
        public event CollisionCallback OnParticleCollision;
        public event SolverCallback OnUpdateParameters;

        public event SolverCallback OnPrepareFrame;
        public event SolverStepCallback OnPrepareStep;
        public event SolverStepCallback OnBeginStep;
        public event SolverStepCallback OnSubstep;
        public event SolverCallback OnEndStep;
        public event SolverCallback OnInterpolate;

        [Tooltip("If enabled, will force the solver to keep simulating even when not visible from any camera.")]
        public bool simulateWhenInvisible = true;           /**< Whether to keep simulating the cloth when its not visible by any camera.*/

        private ISolverImpl m_SolverImpl;

        private IObiBackend m_SimulationBackend =
#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
        new BurstBackend();
#elif (OBI_ONI_SUPPORTED)
        new OniBackend();
#else
        new NullBackend();
#endif

        [SerializeField] private BackendType m_Backend = BackendType.Burst;

        public Oni.SolverParameters parameters = new Oni.SolverParameters(Oni.SolverParameters.Interpolation.None,
                                                                          new Vector4(0, -9.81f, 0, 0));

        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        public Space gravitySpace = Space.Self;

        [Range(0, 1)]
        public float worldLinearInertiaScale = 0;           /**< how much does world-space linear inertia affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/

        [Range(0, 1)]
        public float worldAngularInertiaScale = 0;          /**< how much does world-space angular inertia affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/

        [HideInInspector] [NonSerialized] public List<ObiActor> actors = new List<ObiActor>();
        [HideInInspector] [NonSerialized] public ParticleInActor[] m_ParticleToActor;

        private ObiNativeIntList freeList;
        private int[] activeParticles;
        private int activeParticleCount = 0;

        [NonSerialized] public List<int> simplices = new List<int>();
        private List<int> points = new List<int>();      /**< 0-simplices*/
        private List<int> edges = new List<int>();      /**< 1-simplices*/
        private List<int> triangles = new List<int>();      /**< 2-simplices*/
        private SimplexCounts m_SimplexCounts;

        [HideInInspector][NonSerialized] public bool dirtyActiveParticles = true;
        [HideInInspector][NonSerialized] public bool dirtySimplices = true;
        [HideInInspector][NonSerialized] public int dirtyConstraints = 0;

        private ObiCollisionEventArgs collisionArgs = new ObiCollisionEventArgs();
        private ObiCollisionEventArgs particleCollisionArgs = new ObiCollisionEventArgs();

        private int m_contactCount;
        private int m_particleContactCount;
        private float m_MaxScale = 1;
        private UnityEngine.Bounds bounds = new UnityEngine.Bounds();
        private Plane[] planes = new Plane[6];
        private Camera[] sceneCameras = new Camera[1];
        private bool isVisible = true;

        // constraints:
        [NonSerialized] private IObiConstraints[] m_Constraints = new IObiConstraints[Oni.ConstraintTypeCount];

        // constraint parameters:
        public Oni.ConstraintParameters distanceConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters bendingConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters particleCollisionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters particleFrictionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters collisionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters frictionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters skinConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters volumeConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters shapeMatchingConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters tetherConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters pinConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters stitchConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters densityConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 1);
        public Oni.ConstraintParameters stretchShearConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters bendTwistConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
        public Oni.ConstraintParameters chainConstraintParameters = new Oni.ConstraintParameters(false, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);

        // rigidbodies
        ObiNativeVector4List m_RigidbodyLinearVelocities;
        ObiNativeVector4List m_RigidbodyAngularVelocities;

        // colors
        [NonSerialized] private Color[] m_Colors;

        // cell indices
        [NonSerialized] private ObiNativeInt4List m_CellCoords;

        // positions
        [NonSerialized] private ObiNativeVector4List m_Positions;
        [NonSerialized] private ObiNativeVector4List m_RestPositions;
        [NonSerialized] private ObiNativeVector4List m_PrevPositions;
        [NonSerialized] private ObiNativeVector4List m_StartPositions;
        [NonSerialized] private ObiNativeVector4List m_RenderablePositions;

        // orientations
        [NonSerialized] private ObiNativeQuaternionList m_Orientations;
        [NonSerialized] private ObiNativeQuaternionList m_RestOrientations;
        [NonSerialized] private ObiNativeQuaternionList m_PrevOrientations;
        [NonSerialized] private ObiNativeQuaternionList m_StartOrientations;
        [NonSerialized] private ObiNativeQuaternionList m_RenderableOrientations; /**< renderable particle orientations.*/

        // velocities
        [NonSerialized] private ObiNativeVector4List m_Velocities;
        [NonSerialized] private ObiNativeVector4List m_AngularVelocities;

        // masses/inertia tensors
        [NonSerialized] private ObiNativeFloatList m_InvMasses;
        [NonSerialized] private ObiNativeFloatList m_InvRotationalMasses;
        [NonSerialized] private ObiNativeVector4List m_InvInertiaTensors;

        // external forces
        [NonSerialized] private ObiNativeVector4List m_ExternalForces;
        [NonSerialized] private ObiNativeVector4List m_ExternalTorques;
        [NonSerialized] private ObiNativeVector4List m_Wind;

        // deltas
        [NonSerialized] private ObiNativeVector4List m_PositionDeltas;
        [NonSerialized] private ObiNativeQuaternionList m_OrientationDeltas;
        [NonSerialized] private ObiNativeIntList m_PositionConstraintCounts;
        [NonSerialized] private ObiNativeIntList m_OrientationConstraintCounts;

        // particle collisions:
        [NonSerialized] private ObiNativeIntList m_CollisionMaterials;
        [NonSerialized] private ObiNativeIntList m_Phases;
        [NonSerialized] private ObiNativeIntList m_Filters;

        // particle shape:
        [NonSerialized] private ObiNativeVector4List m_Anisotropies;
        [NonSerialized] private ObiNativeVector4List m_PrincipalRadii;
        [NonSerialized] private ObiNativeVector4List m_Normals;

        // fluids
        [NonSerialized] private ObiNativeVector4List m_Vorticities;
        [NonSerialized] private ObiNativeVector4List m_FluidData;
        [NonSerialized] private ObiNativeVector4List m_UserData;
        [NonSerialized] private ObiNativeFloatList m_SmoothingRadii;
        [NonSerialized] private ObiNativeFloatList m_Buoyancies;
        [NonSerialized] private ObiNativeFloatList m_RestDensities;
        [NonSerialized] private ObiNativeFloatList m_Viscosities;
        [NonSerialized] private ObiNativeFloatList m_SurfaceTension;
        [NonSerialized] private ObiNativeFloatList m_VortConfinement;
        [NonSerialized] private ObiNativeFloatList m_AtmosphericDrag;
        [NonSerialized] private ObiNativeFloatList m_AtmosphericPressure;
        [NonSerialized] private ObiNativeFloatList m_Diffusion;

        public ISolverImpl implementation
        {
            get { return m_SolverImpl; }
        }

        public bool initialized
        {
            get { return m_SolverImpl != null; }
        }

        public IObiBackend simulationBackend
        {
            get { return m_SimulationBackend; }
        }

        public BackendType backendType
        {
            set
            {
                if (m_Backend != value)
                {
                    m_Backend = value;
                    UpdateBackend();
                }
            }
            get { return m_Backend; }
        }

        public SimplexCounts simplexCounts
        {
            get { return m_SimplexCounts; }
        }

        public UnityEngine.Bounds Bounds
        {
            get { return bounds; }
        }

        public bool IsVisible
        {
            get { return isVisible; }
        }

        public float maxScale
        {
            get { return m_MaxScale; }
        }

        public int allocParticleCount
        {
            get { return particleToActor.Count(s => s != null && s.actor != null); }
        }

        public int contactCount
        {
            get { return m_contactCount; }
        }

        public int particleContactCount
        {
            get { return m_particleContactCount; }
        }

        public ParticleInActor[] particleToActor
        {
            get
            {
                if (m_ParticleToActor == null)
                    m_ParticleToActor = new ParticleInActor[0];

                return m_ParticleToActor;
            }
        }

        public ObiNativeVector4List rigidbodyLinearDeltas
        {
            get
            {
                if (m_RigidbodyLinearVelocities == null)
                {
                    m_RigidbodyLinearVelocities = new ObiNativeVector4List();
                }
                return m_RigidbodyLinearVelocities;
            }
        }

        public ObiNativeVector4List rigidbodyAngularDeltas
        {
            get
            {
                if (m_RigidbodyAngularVelocities == null)
                {
                    m_RigidbodyAngularVelocities = new ObiNativeVector4List();
                }
                return m_RigidbodyAngularVelocities;
            }
        }

        public Color[] colors
        {
            get
            {
                if (m_Colors == null)
                {
                    m_Colors = new Color[0];
                }
                return m_Colors;
            }
        }

        public ObiNativeInt4List cellCoords
        {
            get
            {
                if (m_CellCoords == null)
                {
                    m_CellCoords = new ObiNativeInt4List(8, 16, new VInt4(int.MaxValue));
                }
                return m_CellCoords;
            }
        }

        #region Position arrays

        public ObiNativeVector4List positions
        {
            get
            {
                if (m_Positions == null)
                    m_Positions = new ObiNativeVector4List();
                return m_Positions;
            }
        }

        public ObiNativeVector4List restPositions
        {
            get
            {
                if (m_RestPositions == null)
                    m_RestPositions = new ObiNativeVector4List();
                return m_RestPositions;
            }
        }

        public ObiNativeVector4List prevPositions
        {
            get
            {
                if (m_PrevPositions == null)
                    m_PrevPositions = new ObiNativeVector4List();
                return m_PrevPositions;
            }
        }

        public ObiNativeVector4List startPositions
        {
            get
            {
                if (m_StartPositions == null)
                    m_StartPositions = new ObiNativeVector4List();
                return m_StartPositions;
            }
        }

        public ObiNativeVector4List renderablePositions
        {
            get
            {
                if (m_RenderablePositions == null)
                    m_RenderablePositions = new ObiNativeVector4List();
                return m_RenderablePositions;
            }
        }

        #endregion

        #region Orientation arrays

        public ObiNativeQuaternionList orientations
        {
            get
            {
                if (m_Orientations == null)
                    m_Orientations = new ObiNativeQuaternionList();
                return m_Orientations;
            }
        }

        public ObiNativeQuaternionList restOrientations
        {
            get
            {
                if (m_RestOrientations == null)
                    m_RestOrientations = new ObiNativeQuaternionList();
                return m_RestOrientations;
            }
        }

        public ObiNativeQuaternionList prevOrientations
        {
            get
            {
                if (m_PrevOrientations == null)
                    m_PrevOrientations = new ObiNativeQuaternionList();
                return m_PrevOrientations;
            }
        }

        public ObiNativeQuaternionList startOrientations
        {
            get
            {
                if (m_StartOrientations == null)
                    m_StartOrientations = new ObiNativeQuaternionList();
                return m_StartOrientations;
            }
        }

        public ObiNativeQuaternionList renderableOrientations
        {
            get
            {
                if (m_RenderableOrientations == null)
                    m_RenderableOrientations = new ObiNativeQuaternionList();
                return m_RenderableOrientations;
            }
        }

        #endregion

        #region Velocity arrays

        public ObiNativeVector4List velocities
        {
            get
            {
                if (m_Velocities == null)
                    m_Velocities = new ObiNativeVector4List();
                return m_Velocities;
            }
        }

        public ObiNativeVector4List angularVelocities
        {
            get
            {
                if (m_AngularVelocities == null)
                    m_AngularVelocities = new ObiNativeVector4List();
                return m_AngularVelocities;
            }
        }

        #endregion

        #region Mass arrays

        public ObiNativeFloatList invMasses
        {
            get
            {
                if (m_InvMasses == null)
                    m_InvMasses = new ObiNativeFloatList();
                return m_InvMasses;
            }
        }

        public ObiNativeFloatList invRotationalMasses
        {
            get
            {
                if (m_InvRotationalMasses == null)
                    m_InvRotationalMasses = new ObiNativeFloatList();
                return m_InvRotationalMasses;
            }
        }

        public ObiNativeVector4List invInertiaTensors
        {
            get
            {
                if (m_InvInertiaTensors == null)
                    m_InvInertiaTensors = new ObiNativeVector4List();
                return m_InvInertiaTensors;
            }
        }

        #endregion

        #region External forces

        public ObiNativeVector4List externalForces
        {
            get
            {
                if (m_ExternalForces == null)
                    m_ExternalForces = new ObiNativeVector4List();
                return m_ExternalForces;
            }
        }

        public ObiNativeVector4List externalTorques
        {
            get
            {
                if (m_ExternalTorques == null)
                    m_ExternalTorques = new ObiNativeVector4List();
                return m_ExternalTorques;
            }
        }

        public ObiNativeVector4List wind
        {
            get
            {
                if (m_Wind == null)
                    m_Wind = new ObiNativeVector4List();
                return m_Wind;
            }
        }

        #endregion

        #region Deltas

        public ObiNativeVector4List positionDeltas
        {
            get
            {
                if (m_PositionDeltas == null)
                    m_PositionDeltas = new ObiNativeVector4List();
                return m_PositionDeltas;
            }
        }

        public ObiNativeQuaternionList orientationDeltas
        {
            get
            {
                if (m_OrientationDeltas == null)
                    m_OrientationDeltas = new ObiNativeQuaternionList(8, 16, new Quaternion(0, 0, 0, 0));
                return m_OrientationDeltas;
            }
        }

        public ObiNativeIntList positionConstraintCounts
        {
            get
            {
                if (m_PositionConstraintCounts == null)
                    m_PositionConstraintCounts = new ObiNativeIntList();
                return m_PositionConstraintCounts;
            }
        }

        public ObiNativeIntList orientationConstraintCounts
        {
            get
            {
                if (m_OrientationConstraintCounts == null)
                    m_OrientationConstraintCounts = new ObiNativeIntList();
                return m_OrientationConstraintCounts;
            }
        }

        #endregion

        #region Shape and phase

        public ObiNativeIntList collisionMaterials
        {
            get
            {
                if (m_CollisionMaterials == null)
                    m_CollisionMaterials = new ObiNativeIntList();
                return m_CollisionMaterials;
            }
        }

        public ObiNativeIntList phases
        {
            get
            {
                if (m_Phases == null)
                    m_Phases = new ObiNativeIntList();
                return m_Phases;
            }
        }

        public ObiNativeIntList filters
        {
            get
            {
                if (m_Filters == null)
                    m_Filters = new ObiNativeIntList();
                return m_Filters;
            }
        }

        public ObiNativeVector4List anisotropies
        {
            get
            {
                if (m_Anisotropies == null)
                    m_Anisotropies = new ObiNativeVector4List();
                return m_Anisotropies;
            }
        }

        public ObiNativeVector4List principalRadii
        {
            get
            {
                if (m_PrincipalRadii == null)
                    m_PrincipalRadii = new ObiNativeVector4List();
                return m_PrincipalRadii;
            }
        }

        public ObiNativeVector4List normals
        {
            get
            {
                if (m_Normals == null)
                    m_Normals = new ObiNativeVector4List();
                return m_Normals;
            }
        }

        #endregion

        #region Fluid properties

        public ObiNativeVector4List vorticities
        {
            get
            {
                if (m_Vorticities == null)
                    m_Vorticities = new ObiNativeVector4List();
                return m_Vorticities;
            }
        }

        public ObiNativeVector4List fluidData
        {
            get
            {
                if (m_FluidData == null)
                    m_FluidData = new ObiNativeVector4List();
                return m_FluidData;
            }
        }

        public ObiNativeVector4List userData
        {
            get
            {
                if (m_UserData == null)
                    m_UserData = new ObiNativeVector4List();
                return m_UserData;
            }
        }

        public ObiNativeFloatList smoothingRadii
        {
            get
            {
                if (m_SmoothingRadii == null)
                    m_SmoothingRadii = new ObiNativeFloatList();
                return m_SmoothingRadii;
            }
        }

        public ObiNativeFloatList buoyancies
        {
            get
            {
                if (m_Buoyancies == null)
                    m_Buoyancies = new ObiNativeFloatList();
                return m_Buoyancies;
            }
        }

        public ObiNativeFloatList restDensities
        {
            get
            {
                if (m_RestDensities == null)
                    m_RestDensities = new ObiNativeFloatList();
                return m_RestDensities;
            }
        }

        public ObiNativeFloatList viscosities
        {
            get
            {
                if (m_Viscosities == null)
                    m_Viscosities = new ObiNativeFloatList();
                return m_Viscosities;
            }
        }

        public ObiNativeFloatList surfaceTension
        {
            get
            {
                if (m_SurfaceTension == null)
                    m_SurfaceTension = new ObiNativeFloatList();
                return m_SurfaceTension;
            }
        }

        public ObiNativeFloatList vortConfinement
        {
            get
            {
                if (m_VortConfinement == null)
                    m_VortConfinement = new ObiNativeFloatList();
                return m_VortConfinement;
            }
        }

        public ObiNativeFloatList atmosphericDrag
        {
            get
            {
                if (m_AtmosphericDrag == null)
                    m_AtmosphericDrag = new ObiNativeFloatList();
                return m_AtmosphericDrag;
            }
        }

        public ObiNativeFloatList atmosphericPressure
        {
            get
            {
                if (m_AtmosphericPressure == null)
                    m_AtmosphericPressure = new ObiNativeFloatList();
                return m_AtmosphericPressure;
            }
        }

        public ObiNativeFloatList diffusion
        {
            get
            {
                if (m_Diffusion == null)
                    m_Diffusion = new ObiNativeFloatList();
                return m_Diffusion;
            }
        }

        #endregion


        void Update()
        {
            var scale = transform.lossyScale;
            m_MaxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
        }

        private void OnDestroy()
        {
            // Remove all actors from the solver. This will trigger Teardown() when the last actor is removed.
            while (actors.Count > 0)
                RemoveActor(actors[actors.Count - 1]);
        }

        private void CreateBackend()
        {
            switch (m_Backend)
            {

#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
                case BackendType.Burst: m_SimulationBackend = new BurstBackend(); break;
#endif
#if (OBI_ONI_SUPPORTED)
                case BackendType.Oni: m_SimulationBackend = new OniBackend(); break;
#endif

                default:
#if (OBI_ONI_SUPPORTED)
                    m_SimulationBackend = new OniBackend();
#else
                    m_SimulationBackend = new NullBackend();
#endif
                    break;
            }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                CreateBackend();

                // Set up local actor and particle buffers:
                actors = new List<ObiActor>();
                freeList = new ObiNativeIntList();
                activeParticles = new int[0];
                m_ParticleToActor = new ParticleInActor[0];
                m_Colors = new Color[0];

                // Create constraints:
                m_Constraints[(int)Oni.ConstraintType.Distance] = new ObiDistanceConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Bending] = new ObiBendConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Aerodynamics] = new ObiAerodynamicConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.StretchShear] = new ObiStretchShearConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.BendTwist] = new ObiBendTwistConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Chain] = new ObiChainConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.ShapeMatching] = new ObiShapeMatchingConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Volume] = new ObiVolumeConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Tether] = new ObiTetherConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Skin] = new ObiSkinConstraintsData();
                m_Constraints[(int)Oni.ConstraintType.Pin] = new ObiPinConstraintsData();

                // Create the solver:
                m_SolverImpl = m_SimulationBackend.CreateSolver(this, 0);

                // Set data arrays:
                m_SolverImpl.ParticleCountChanged(this);
                m_SolverImpl.SetRigidbodyArrays(this);

                // Initialize moving transform:
                InitializeTransformFrame();

                // Set initial parameter values:
                PushSolverParameters();
            }
        }

        public void Teardown()
        {
            if (initialized)
            {
                // Clear all constraints:
                PushConstraints();

                // Destroy the Oni solver:
                m_SimulationBackend.DestroySolver(m_SolverImpl);
                m_SolverImpl = null;

                // Free particle / rigidbody memory:
                FreeParticleArrays();
                FreeRigidbodyArrays();

                freeList.Dispose();
            }
        }

        public void UpdateBackend()
        {
            // remove all actors, this will trigger a teardown:
            List<ObiActor> temp = new List<ObiActor>(actors);
            foreach (ObiActor actor in temp)
                actor.RemoveFromSolver();

            // re-add all actors.
            foreach (ObiActor actor in temp)
                actor.AddToSolver();
        }

        private void FreeRigidbodyArrays()
        {
            rigidbodyLinearDeltas.Dispose();
            rigidbodyAngularDeltas.Dispose();

            m_RigidbodyLinearVelocities = null;
            m_RigidbodyAngularVelocities = null;
        }

        public void EnsureRigidbodyArraysCapacity(int count)
        {
            if (initialized && count >= rigidbodyLinearDeltas.count)
            {
                rigidbodyLinearDeltas.ResizeInitialized(count);
                rigidbodyAngularDeltas.ResizeInitialized(count);

                m_SolverImpl.SetRigidbodyArrays(this);
            }
        }

        private void FreeParticleArrays()
        {
            cellCoords.Dispose();
            startPositions.Dispose();
            startOrientations.Dispose();
            positions.Dispose();
            prevPositions.Dispose();
            restPositions.Dispose();
            velocities.Dispose();
            orientations.Dispose();
            prevOrientations.Dispose();
            restOrientations.Dispose();
            angularVelocities.Dispose();
            invMasses.Dispose();
            invRotationalMasses.Dispose();
            principalRadii.Dispose();
            collisionMaterials.Dispose();
            phases.Dispose();
            filters.Dispose();
            renderablePositions.Dispose();
            renderableOrientations.Dispose();
            anisotropies.Dispose();
            smoothingRadii.Dispose();
            buoyancies.Dispose();
            restDensities.Dispose();
            viscosities.Dispose();
            surfaceTension.Dispose();
            vortConfinement.Dispose();
            atmosphericDrag.Dispose();
            atmosphericPressure.Dispose();
            diffusion.Dispose();
            vorticities.Dispose();
            fluidData.Dispose();
            userData.Dispose();
            externalForces.Dispose();
            externalTorques.Dispose();
            wind.Dispose();
            positionDeltas.Dispose();
            orientationDeltas.Dispose();
            positionConstraintCounts.Dispose();
            orientationConstraintCounts.Dispose();
            normals.Dispose();
            invInertiaTensors.Dispose();

            m_Colors = null;
            m_CellCoords = null;
            m_Positions = null;
            m_RestPositions = null;
            m_PrevPositions = null;
            m_StartPositions = null;
            m_RenderablePositions = null;
            m_Orientations = null;
            m_RestOrientations = null;
            m_PrevOrientations = null;
            m_StartOrientations = null;
            m_RenderableOrientations = null;
            m_Velocities = null;
            m_AngularVelocities = null;
            m_InvMasses = null;
            m_InvRotationalMasses = null;
            m_InvInertiaTensors = null;
            m_ExternalForces = null;
            m_ExternalTorques = null;
            m_Wind = null;
            m_PositionDeltas = null;
            m_OrientationDeltas = null;
            m_PositionConstraintCounts = null;
            m_OrientationConstraintCounts = null;
            m_CollisionMaterials = null;
            m_Phases = null;
            m_Filters = null;
            m_Anisotropies = null;
            m_PrincipalRadii = null;
            m_Normals = null;
            m_Vorticities = null;
            m_FluidData = null;
            m_UserData = null;
            m_SmoothingRadii = null;
            m_Buoyancies = null;
            m_RestDensities = null;
            m_Viscosities = null;
            m_SurfaceTension = null;
            m_VortConfinement = null;
            m_AtmosphericDrag = null;
            m_AtmosphericPressure = null;
            m_Diffusion = null;
        }

        private void EnsureParticleArraysCapacity(int count)
        {
            // only resize if the count is larger than the current amount of particles:
            if (count >= positions.count)
            {
                startPositions.ResizeInitialized(count);
                positions.ResizeInitialized(count);
                prevPositions.ResizeInitialized(count);
                restPositions.ResizeInitialized(count);
                startOrientations.ResizeInitialized(count, Quaternion.identity);
                orientations.ResizeInitialized(count, Quaternion.identity);
                prevOrientations.ResizeInitialized(count, Quaternion.identity);
                restOrientations.ResizeInitialized(count, Quaternion.identity);
                renderablePositions.ResizeInitialized(count);
                renderableOrientations.ResizeInitialized(count, Quaternion.identity);
                velocities.ResizeInitialized(count);
                angularVelocities.ResizeInitialized(count);
                invMasses.ResizeInitialized(count);
                invRotationalMasses.ResizeInitialized(count);
                principalRadii.ResizeInitialized(count);
                collisionMaterials.ResizeInitialized(count);
                phases.ResizeInitialized(count);
                filters.ResizeInitialized(count);
                anisotropies.ResizeInitialized(count * 3);
                smoothingRadii.ResizeInitialized(count);
                buoyancies.ResizeInitialized(count);
                restDensities.ResizeInitialized(count);
                viscosities.ResizeInitialized(count);
                surfaceTension.ResizeInitialized(count);
                vortConfinement.ResizeInitialized(count);
                atmosphericDrag.ResizeInitialized(count);
                atmosphericPressure.ResizeInitialized(count);
                diffusion.ResizeInitialized(count);
                vorticities.ResizeInitialized(count);
                fluidData.ResizeInitialized(count);
                userData.ResizeInitialized(count);
                externalForces.ResizeInitialized(count);
                externalTorques.ResizeInitialized(count);
                wind.ResizeInitialized(count);
                positionDeltas.ResizeInitialized(count);
                orientationDeltas.ResizeInitialized(count, new Quaternion(0, 0, 0, 0));
                positionConstraintCounts.ResizeInitialized(count);
                orientationConstraintCounts.ResizeInitialized(count);
                normals.ResizeInitialized(count);
                invInertiaTensors.ResizeInitialized(count);

                m_SolverImpl.ParticleCountChanged(this);
            }

            if (count >= activeParticles.Length)
            {
                Array.Resize(ref activeParticles, count * 2);
                Array.Resize(ref m_ParticleToActor, count * 2);
                Array.Resize(ref m_Colors, count * 2);
            }
        }

        private void AllocateParticles(int[] particleIndices)
        {

            // If attempting to allocate more particles than we have:
            if (particleIndices.Length > freeList.count)
            {
                int grow = particleIndices.Length - freeList.count;

                // append new free indices:
                for (int i = 0; i < grow; ++i)
                    freeList.Add(positions.count + i);

                // grow particle arrays:
                EnsureParticleArraysCapacity(positions.count + particleIndices.Length);
            }

            // determine first particle in the free list to use:
            int first = freeList.count - particleIndices.Length;

            // copy free indices to the input array:
            freeList.CopyTo(particleIndices, first, particleIndices.Length);

            // shorten the free list:
            freeList.ResizeUninitialized(first);

        }

        private void FreeParticles(int[] particleIndices)
        {
            freeList.AddRange(particleIndices);
        }


        /// <summary>
        /// Adds an actor to the solver.
        /// </summary> 
        /// Attemps to add the actor to this solver returning whether this was successful or not. In case the actor was already added, or had no reference to a blueprint, this operation will return false.
        /// If this was the first actor added to the solver, will attempt to initialize the solver.
        /// While in play mode, if the actor is sucessfully added to the solver, will also call actor.LoadBlueprint().
        /// <param name="actor"> An actor.</param>  
        /// <returns>
        /// Whether the actor was sucessfully added.
        /// </returns> 
        public bool AddActor(ObiActor actor)
        {
            if (actor == null)
                return false;

            if ((actors == null || !actors.Contains(actor)) && actor.sourceBlueprint != null)
            {
                // If the solver is not initialized yet, do so:
                Initialize();

                actor.solverIndices = new int[actor.sourceBlueprint.particleCount];

                AllocateParticles(actor.solverIndices);

                for (int i = 0; i < actor.solverIndices.Length; ++i)
                    particleToActor[actor.solverIndices[i]] = new ObiSolver.ParticleInActor(actor, i);

                actors.Add(actor);

                actor.LoadBlueprint(this);
            }

            return true;

        }

        /// <summary>  
        /// Attempts to remove an actor from this solver, and returns  whether this was sucessful or not. 
        /// </summary>
        /// Will only reurn true if the actor had been previously added successfully to this solver. 
        /// If the actor is sucessfully removed from the solver, will also call actor.UnloadBlueprint(). Once the last actor is removed from the solver,
        /// this method will attempt to tear down the solver.
        /// <param name="actor"> An actor.</param>  
        /// <returns>
        /// Whether the actor was sucessfully removed.
        /// </returns> 
        public bool RemoveActor(ObiActor actor)
        {

            if (actor == null)
                return false;

            // Find actor index in our actors array:
            int index = actors.IndexOf(actor);

            // If we are in charge of this actor indeed, perform all steps necessary to release it.
            if (index >= 0)
            {
                actor.UnloadBlueprint(this);

                for (int i = 0; i < actor.solverIndices.Length; ++i)
                    particleToActor[actor.solverIndices[i]] = null;

                FreeParticles(actor.solverIndices);

                actors.RemoveAt(index);

                actor.solverIndices = null;

                // If this was the last actor in the solver, tear it down:
                if (actors.Count == 0)
                    Teardown();

                return true;
            }

            return false;
        }

        /// <summary>  
        /// Updates solver parameters. 
        /// </summary>
        /// Call this after modifying solver or constraint parameters.
        public void PushSolverParameters()
        {
            if (!initialized)
                return;

            m_SolverImpl.SetParameters(parameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Distance, ref distanceConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Bending, ref bendingConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.ParticleCollision, ref particleCollisionConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.ParticleFriction, ref particleFrictionConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Collision, ref collisionConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Friction, ref frictionConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Density, ref densityConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Skin, ref skinConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Volume, ref volumeConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.ShapeMatching, ref shapeMatchingConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Tether, ref tetherConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Pin, ref pinConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Stitch, ref stitchConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.StretchShear, ref stretchShearConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.BendTwist, ref bendTwistConstraintParameters);

            m_SolverImpl.SetConstraintGroupParameters(Oni.ConstraintType.Chain, ref chainConstraintParameters);

            if (OnUpdateParameters != null)
                OnUpdateParameters(this);

        }

        /// <summary>  
        /// Returns the parameters used by a given constraint type. 
        /// </summary>
        /// If you know the type of the constraints at runtime,
        /// this is the same as directly accessing the appropiate public Oni.ConstraintParameters struct in the solver.
        /// <param name="constraintType"> Type of the constraints whose parameters will be returned by this method.</param>  
        /// <returns>
        /// Parameters for the constraints of the specified type.
        /// </returns> 
        public Oni.ConstraintParameters GetConstraintParameters(Oni.ConstraintType constraintType)
        {
            switch (constraintType)
            {
                case Oni.ConstraintType.Distance: return distanceConstraintParameters;
                case Oni.ConstraintType.Bending: return bendingConstraintParameters;
                case Oni.ConstraintType.ParticleCollision: return particleCollisionConstraintParameters;
                case Oni.ConstraintType.ParticleFriction: return particleFrictionConstraintParameters;
                case Oni.ConstraintType.Collision: return collisionConstraintParameters;
                case Oni.ConstraintType.Friction: return frictionConstraintParameters;
                case Oni.ConstraintType.Skin: return skinConstraintParameters;
                case Oni.ConstraintType.Volume: return volumeConstraintParameters;
                case Oni.ConstraintType.ShapeMatching: return shapeMatchingConstraintParameters;
                case Oni.ConstraintType.Tether: return tetherConstraintParameters;
                case Oni.ConstraintType.Pin: return pinConstraintParameters;
                case Oni.ConstraintType.Stitch: return stitchConstraintParameters;
                case Oni.ConstraintType.Density: return densityConstraintParameters;
                case Oni.ConstraintType.StretchShear: return stretchShearConstraintParameters;
                case Oni.ConstraintType.BendTwist: return bendTwistConstraintParameters;
                case Oni.ConstraintType.Chain: return chainConstraintParameters;

                default: return new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 1);
            }
        }

        /// <summary>  
        /// Returns the runtime representation of constraints of a given type being simulated by this solver.
        /// </summary>  
        /// <param name="type"> Type of the constraints that will be returned by this method.</param>  
        /// <returns>
        /// The runtime constraints of the type speficied.
        /// </returns> 
        public IObiConstraints GetConstraintsByType(Oni.ConstraintType type)
        {
            int index = (int)type;
            if (m_Constraints != null && index >= 0 && index < m_Constraints.Length)
                return m_Constraints[index];
            return null;
        }

        private void PushActiveParticles()
        {
            activeParticleCount = 0;

            for (int i = 0; i < actors.Count; ++i)
            {
                ObiActor currentActor = actors[i];

                if (currentActor.isActiveAndEnabled)
                {
                    for (int j = 0; j < currentActor.activeParticleCount; ++j)
                    {
                        activeParticles[activeParticleCount] = currentActor.solverIndices[j];
                        activeParticleCount++;
                    }
                }
            }

            m_SolverImpl.SetActiveParticles(activeParticles, activeParticleCount);
            dirtyActiveParticles = false;

            // push simplices when active particles change.
            PushSimplices();
        }

        private void PushSimplices()
        {
            simplices.Clear();
            points.Clear();
            edges.Clear();
            triangles.Clear();

            for (int i = 0; i < actors.Count; ++i)
            {
                ObiActor currentActor = actors[i];

                if (currentActor.isActiveAndEnabled && currentActor.isLoaded)
                {
                    //simplex based contacts
                    if (currentActor.surfaceCollisions)
                    {
                        if (currentActor.blueprint.points != null)
                            for (int j = 0; j < currentActor.blueprint.points.Length; ++j)
                            {
                                int actorIndex = currentActor.blueprint.points[j];

                                if (actorIndex < currentActor.activeParticleCount)
                                    points.Add(currentActor.solverIndices[actorIndex]);
                            }

                        if (currentActor.blueprint.edges != null)
                            for (int j = 0; j < currentActor.blueprint.edges.Length / 2; ++j)
                            {
                                int actorIndex1 = currentActor.blueprint.edges[j * 2];
                                int actorIndex2 = currentActor.blueprint.edges[j * 2 + 1];

                                if (actorIndex1 < currentActor.activeParticleCount && actorIndex2 < currentActor.activeParticleCount)
                                {
                                    edges.Add(currentActor.solverIndices[actorIndex1]);
                                    edges.Add(currentActor.solverIndices[actorIndex2]);
                                }
                            }

                        if (currentActor.blueprint.triangles != null)
                            for (int j = 0; j < currentActor.blueprint.triangles.Length / 3; ++j)
                            {
                                int actorIndex1 = currentActor.blueprint.triangles[j * 3];
                                int actorIndex2 = currentActor.blueprint.triangles[j * 3 + 1];
                                int actorIndex3 = currentActor.blueprint.triangles[j * 3 + 2]; // TODO: +1: degenerate triangles. check out!

                                if (actorIndex1 < currentActor.activeParticleCount &&
                                    actorIndex2 < currentActor.activeParticleCount &&
                                    actorIndex3 < currentActor.activeParticleCount)
                                {
                                    triangles.Add(currentActor.solverIndices[actorIndex1]);
                                    triangles.Add(currentActor.solverIndices[actorIndex2]);
                                    triangles.Add(currentActor.solverIndices[actorIndex3]);
                                }
                            }
                    }
                    // particle based contacts
                    else
                    {
                        // generate a point simplex out of each active particle:
                        for (int j = 0; j < currentActor.activeParticleCount; ++j)
                            points.Add(currentActor.solverIndices[j]);
                    }
                }
            }

            simplices.Capacity = points.Count + edges.Count + triangles.Count;
            simplices.AddRange(points);
            simplices.AddRange(edges);
            simplices.AddRange(triangles);

            m_SimplexCounts = new SimplexCounts(points.Count, edges.Count / 2, triangles.Count / 3);
            cellCoords.ResizeInitialized(m_SimplexCounts.simplexCount);

            m_SolverImpl.SetSimplices(simplices.ToArray(), m_SimplexCounts);
            dirtySimplices = false;

        }

        private void PushConstraints()
        {
            // Clear all dirty constraints:
            for (int i = 0; i < Oni.ConstraintTypeCount; ++i)
                if (m_Constraints[i] != null && ((1 << i) & dirtyConstraints) != 0)
                    m_Constraints[i].Clear();

            // Iterate over all actors, merging their batches together:
            for (int k = 0; k < actors.Count; ++k)
            {
                if (actors[k].isLoaded)
                {
                    for (int i = 0; i < Oni.ConstraintTypeCount; ++i)
                        if (m_Constraints[i] != null && ((1 << i) & dirtyConstraints) != 0)
                        {
                            var constraints = actors[k].GetConstraintsByType((Oni.ConstraintType)i);
                            m_Constraints[i].Merge(actors[k], constraints);
                        }
                }
            }

            // Readd the constraints to the solver:
            for (int i = 0; i < Oni.ConstraintTypeCount; ++i)
                if (m_Constraints[i] != null && ((1 << i) & dirtyConstraints) != 0)
                    m_Constraints[i].AddToSolver(this);

            // Reset the dirty flag:
            dirtyConstraints = 0;
        }

        /**
         * Checks if any particle in the solver is visible from at least one camera. If so, sets isVisible to true, false otherwise.
         */
        private void UpdateVisibility()
        {

            using (m_UpdateVisibilityPerfMarker.Auto())
            {

                using (m_GetSolverBoundsPerfMarker.Auto())
                {
                    // get bounds in solver space:
                    Vector3 min = Vector3.zero, max = Vector3.zero;
                    m_SolverImpl.GetBounds(ref min, ref max);
                    bounds.SetMinMax(min, max);
                }

                if (bounds.AreValid())
                {
                    using (m_TestBoundsPerfMarker.Auto())
                    {
                        // transform bounds to world space:
                        bounds = bounds.Transform(transform.localToWorldMatrix);

                        using (m_GetAllCamerasPerfMarker.Auto())
                        {
                            Array.Resize(ref sceneCameras, Camera.allCamerasCount);
                            Camera.GetAllCameras(sceneCameras);
                        }

                        foreach (Camera cam in sceneCameras)
                        {
                            GeometryUtility.CalculateFrustumPlanes(cam, planes);
                            if (GeometryUtility.TestPlanesAABB(planes, bounds))
                            {
                                if (!isVisible)
                                {
                                    isVisible = true;
                                    foreach (ObiActor actor in actors)
                                        actor.OnSolverVisibilityChanged(isVisible);
                                }
                                return;
                            }
                        }
                    }
                }

                if (isVisible)
                {
                    isVisible = false;
                    foreach (ObiActor actor in actors)
                        actor.OnSolverVisibilityChanged(isVisible);
                }
            }
        }

        private void InitializeTransformFrame()
        {
            Vector4 translation = transform.position;
            Vector4 scale = transform.lossyScale;
            Quaternion rotation = transform.rotation;

            m_SolverImpl.InitializeFrame(translation, scale, rotation);
        }

        private void UpdateTransformFrame(float dt)
        {
            Vector4 translation = transform.position;
            Vector4 scale = transform.lossyScale;
            Quaternion rotation = transform.rotation;

            m_SolverImpl.UpdateFrame(translation, scale, rotation, dt);
            m_SolverImpl.ApplyFrame(worldLinearInertiaScale, worldAngularInertiaScale, dt);
        }

        public void PrepareFrame()
        {
            if (OnPrepareFrame != null)
                OnPrepareFrame(this);

            foreach (ObiActor actor in actors)
                actor.PrepareFrame();
        }

        /// <summary>
        /// Signals the start of a new time step.
        /// </summary>
        /// Pushes active particles (if dirtyActiveParticles is true), and runtime constraints (if dirtyConstraints != 0).
        /// Updates the solver's nertial reference frame. Calls begin step callbacks.
        /// Finally, it schedules execution of simulation tasks at the beginng on a physics step (most notably, collision detection), and returns a handle to the job that will perform them.
        /// <param name="stepTime"> Duration of the entire time step (in seconds).</param>  
        /// <returns>
        /// A handle to the job.
        /// </returns> 
        public IObiJobHandle BeginStep(float stepTime)
        {
            if (!isActiveAndEnabled || !initialized)
                return null;

            if (OnPrepareStep != null)
                OnPrepareStep(this, stepTime);

            foreach (ObiActor actor in actors)
                actor.PrepareStep(stepTime);

            // Update the active particles array:
            if (dirtyActiveParticles)
                PushActiveParticles();

            // Update the simplices array:
            if (dirtySimplices)
                PushSimplices();

            // Update constraint batches:
            if (dirtyConstraints != 0)
                PushConstraints();

            // Update inertial frame:
            UpdateTransformFrame(stepTime);

            // Update gravity:
            parameters.gravity = gravitySpace == Space.World ? transform.InverseTransformVector(gravity) : gravity;
            if (initialized)
                m_SolverImpl.SetParameters(parameters);

            // Copy positions / orientations at the start of the step, for interpolation:
            startPositions.CopyFrom(positions);
            startOrientations.CopyFrom(orientations);

            if (OnBeginStep != null)
                OnBeginStep(this, stepTime);

            foreach (ObiActor actor in actors)
                actor.BeginStep(stepTime);

            // Perform collision detection:
            if (simulateWhenInvisible || isVisible)
                return m_SolverImpl.CollisionDetection(stepTime);

            return null;
        }

        /// <summary>
        /// Schedules the job to advance the simulation a given amount of time, then returns a handle to this job.
        /// </summary>  
        /// <param name="substepTime"> Amount of time to advance (in seconds).</param>  
        /// <returns>
        /// A handle to the job.
        /// </returns> 
        public IObiJobHandle Substep(float stepTime, float substepTime, int index)
        {

            // Only update the solver if it is visible, or if we must simulate even when invisible.
            if (isActiveAndEnabled && (simulateWhenInvisible || isVisible) && initialized)
            {
                if (OnSubstep != null)
                    OnSubstep(this, substepTime);

                foreach (ObiActor actor in actors)
                    actor.Substep(substepTime);

                // Update the solver (this is internally split in tasks so multiple solvers can be updated in parallel)
                return m_SolverImpl.Substep(stepTime, substepTime, index);
            }

            return null;
        }

        /// <summary>
        /// Wraps up a simulation step: resets external forces and calls collision callbacks.
        /// </summary>  
        /// <param name="substepTime"> Size of the last substep performed this step (in seconds).</param>  
        public void EndStep(float substepTime)
        {
            if (!initialized)
                return;

            m_contactCount = implementation.GetConstraintCount(Oni.ConstraintType.Collision);
            m_particleContactCount = implementation.GetConstraintCount(Oni.ConstraintType.ParticleCollision);

            if (OnCollision != null)
            {
                collisionArgs.contacts.SetCount(m_contactCount);

                if (m_contactCount > 0)
                    implementation.GetCollisionContacts(collisionArgs.contacts.Data, m_contactCount);

                OnCollision(this, collisionArgs);

            }

            if (OnParticleCollision != null)
            {
                particleCollisionArgs.contacts.SetCount(m_particleContactCount);

                if (m_particleContactCount > 0)
                    implementation.GetParticleCollisionContacts(particleCollisionArgs.contacts.Data, m_particleContactCount);

                OnParticleCollision(this, particleCollisionArgs);

            }

            m_SolverImpl.ResetForces();

            if (OnEndStep != null)
                OnEndStep(this);

            foreach (ObiActor actor in actors)
                actor.EndStep(substepTime);
        }

        /// <summary>
        /// Finalizes the frame by performing physics state interpolation.
        /// </summary>
        /// This is usually used for mesh generation, rendering setup and other tasks that must take place after all physics steps for this frame are done.
        /// <param name="stepTime"> Duration of this time step (in seconds). Note this is the entire timestep, not just the ast substep.</param>
        /// <param name="unsimulatedTime"> Remaining time that could not be simulated during this step (in seconds). This is used to interpolate physics state. </param>  
        public void Interpolate(float stepTime, float unsimulatedTime)
        {
            if (!isActiveAndEnabled || !initialized)
                return;

            // Only perform interpolation if the solver is visible, or if we must simulate even when invisible.
            if (simulateWhenInvisible || isVisible)
            {
                using (m_StateInterpolationPerfMarker.Auto())
                {
                    // interpolate physics state:
                    m_SolverImpl.ApplyInterpolation(startPositions, startOrientations, stepTime, unsimulatedTime);
                }
            }

            UpdateVisibility();

            if (OnInterpolate != null)
                OnInterpolate(this);

            foreach (ObiActor actor in actors)
                actor.Interpolate();

        }

        public void ReleaseJobHandles()
        {
            if (!initialized)
                return;

            m_SolverImpl.ReleaseJobHandles();
        }

        /// <summary>
        /// Performs multiple spatial queries in parallel against all simplices in the solver, and returns a list of results.
        /// </summary>
        /// All other query/raycast methods are built on top of this one. Use it when you need maximum flexibility/performance.
        /// <param name="shapes"> List of query shapes to test against all simplices in the solver.</param>
        /// <param name="transforms"> List of transforms, must have the same size as the shapes list. </param>
        /// <param name="results">
        /// This list will contain results for all queries, in no specific order.
        /// Use the queryIndex member of each query result to correlate each result to the query that spawned it. For instance:
        /// a query result with queryIndex 5, belongs to the query shape at index 5 in the input shapes list.
        /// </param>
        public void SpatialQuery(ObiNativeQueryShapeList shapes, ObiNativeAffineTransformList transforms, ObiNativeQueryResultList results)
        {
            if (!initialized || shapes == null || transforms == null || results == null || shapes.count != transforms.count)
                return;

            m_SolverImpl.SpatialQuery(shapes, transforms, results);
        }

        /// <summary>
        /// Performs a single spatial queries against all simplices in the solver, and returns a list of results.
        /// </summary>
        /// <param name="shape"> Query shape to test against all simplices in the solver.</param>
        /// <param name="transform"> Transform applied to the query shape. </param>
        /// <returns>
        /// An array that contains the query results.
        /// </returns>
        public QueryResult[] SpatialQuery(QueryShape shape, AffineTransform transform)
        {
            if (!initialized)
                return null;

            var queries = new ObiNativeQueryShapeList();
            var transforms = new ObiNativeAffineTransformList();
            var results = new ObiNativeQueryResultList();

            queries.Add(shape);
            transforms.Add(transform);

            m_SolverImpl.SpatialQuery(queries, transforms, results);

            var resultsArray = results.ToArray();

            queries.Dispose();
            transforms.Dispose();
            results.Dispose();

            return resultsArray;
        }

        /// <summary>
        /// Performs a single raycast  against all simplices in the solver, and returns the result.
        /// </summary>
        /// <param name="ray"> Ray to cast against all simplices in the solver. Expressed in world space.</param>
        /// <param name="hitInfo"> Struct containing hit info, if any. </param>
        /// <param name="filter"> Filter (mask, category) used to filter out collisions against certain simplices. </param>
        /// <param name="maxDistance"> Ray length. </param>
        /// <param name="rayThickness">
        /// Ray thickness. If the ray hits a simplex, hitInfo will contain a point on the simplex.
        /// If it merely passes near the simplex (within its thickness distance, but no actual hit), it will contain the point on the ray closest to the simplex surface. </param>
        /// <returns>
        /// Whether the ray hit anything. If the ray did not hit, the hitInfo will contain a simplexIndex of -1 and a distance equal to maxDistance.
        /// </returns>
        public bool Raycast(Ray ray, out QueryResult hitInfo, int filter, float maxDistance = 100, float rayThickness = 0)
        {
            var result = Raycast(new List<Ray> { ray }, filter, maxDistance, rayThickness);
            if (result != null && result.Length > 0 && result[0].simplexIndex >= 0)
            {
                hitInfo = result[0];
                return true;
            }
            else
            {
                hitInfo = new QueryResult() { distance = maxDistance };
                return false;
            }
        }

        /// <summary>
        /// Performs multiple raycasts in parallel against all simplices in the solver, and returns the results.
        /// </summary>
        /// <param name="rays"> List of rays to cast against all simplices in the solver. Expressed in world space.</param>
        /// <param name="filter"> Filter (mask, category) used to filter out collisions against certain simplices. </param>
        /// <param name="maxDistance"> Ray length. </param>
        /// <param name="rayThickness">
        /// Ray thickness. If the ray hits a simplex, hitInfo will contain a point on the simplex.
        /// If it merely passes near the simplex (within its thickness distance, but no actual hit), it will contain the point on the ray closest to the simplex surface. </param>
        /// <returns>
        /// This list will contain results for all raycasts, in no specific order.
        /// Use the queryIndex member of each query result to correlate each result to the raycast that spawned it. For instance:
        /// a query result with queryIndex 5, belongs to the raycast at index 5 in the input rays list.
        /// </returns>
        public QueryResult[] Raycast(List<Ray> rays, int filter, float maxDistance = 100, float rayThickness = 0)
        {
            if (!initialized)
                return null;

            var queries = new ObiNativeQueryShapeList();
            var transforms = new ObiNativeAffineTransformList();
            var results = new ObiNativeQueryResultList();
            var resultArray = new QueryResult[rays.Count];

            for (int i = 0; i < rays.Count; ++i)
            {
                queries.Add(new QueryShape()
                {
                    type = QueryShape.QueryType.Ray,
                    center = rays[i].origin,
                    size = rays[i].origin + rays[i].direction * maxDistance,
                    contactOffset = rayThickness,
                    maxDistance = 0.0005f,
                    filter = filter
                });

                transforms.Add(new AffineTransform(Vector4.zero, Quaternion.identity, Vector4.one));

                resultArray[i] = new QueryResult { distance = maxDistance, simplexIndex = -1, queryIndex = -1 };
            }

            m_SolverImpl.SpatialQuery(queries, transforms, results);

            Matrix4x4 solver2World = transform.localToWorldMatrix;
            for (int i = 0; i < results.count; ++i)
            {
                int rayIndex = results[i].queryIndex;
                var pointWS = solver2World.MultiplyPoint3x4(results[i].queryPoint + results[i].normal * results[i].distance);

                if (results[i].distance <= 0.001f)
                {
                    // project the hit on the ray:
                    float rayDistance = (pointWS.x - rays[rayIndex].origin.x) * rays[rayIndex].direction.x +
                                        (pointWS.y - rays[rayIndex].origin.y) * rays[rayIndex].direction.y +
                                        (pointWS.z - rays[rayIndex].origin.z) * rays[rayIndex].direction.z;

                    // keep the closest hit:
                    if (rayDistance < resultArray[rayIndex].distance)
                    {
                        resultArray[rayIndex] = results[i];
                        resultArray[rayIndex].distance = rayDistance;
                        resultArray[rayIndex].queryPoint = rays[rayIndex].origin + rays[rayIndex].direction * rayDistance;
                    }
                }
            }

            queries.Dispose();
            transforms.Dispose();
            results.Dispose();

            return resultArray;
        }
    }

}
