using System;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Particle Attachment", 820)]
    [RequireComponent(typeof(ObiActor))]
    [ExecuteInEditMode]
    public class ObiParticleAttachment : MonoBehaviour
    {
        public enum AttachmentType
        {
            Static,
            Dynamic
        }

        [SerializeField] [HideInInspector] private ObiActor m_Actor;
        [SerializeField] [HideInInspector] private Transform m_Target;

        [SerializeField] [HideInInspector] private ObiParticleGroup m_ParticleGroup;
        [SerializeField] [HideInInspector] private AttachmentType m_AttachmentType = AttachmentType.Static;
        [SerializeField] [HideInInspector] private bool m_ConstrainOrientation = false;
        [SerializeField] [HideInInspector] private float m_Compliance = 0;
        [SerializeField] [HideInInspector] [Delayed] private float m_BreakThreshold = float.PositiveInfinity;

        // private variables are serialized during script reloading, to keep their value. Must mark them explicitly as non-serialized.
        [NonSerialized] private ObiPinConstraintsBatch pinBatch;
        [NonSerialized] private ObiColliderBase attachedCollider;
        [NonSerialized] private int attachedColliderHandleIndex;

        [NonSerialized] private int[] m_SolverIndices;
        [NonSerialized] private Vector3[] m_PositionOffsets = null;
        [NonSerialized] private Quaternion[] m_OrientationOffsets = null;

        /// <summary>  
        /// The actor this attachment is added to.
        /// </summary> 
        public ObiActor actor
        {
            get { return m_Actor; }
        }

        /// <summary>  
        /// The target transform that the <see cref="particleGroup"/> should be attached to.
        /// </summary> 
        public Transform target
        {
            get { return m_Target; }
            set
            {
                if (value != m_Target)
                {
                    m_Target = value;
                    Bind();
                }
            }
        }

        /// <summary>  
        /// The particle group that should be attached to the <see cref="target"/>.
        /// </summary> 
        public ObiParticleGroup particleGroup
        {
            get
            {
                return m_ParticleGroup;
            }

            set
            {
                if (value != m_ParticleGroup)
                {
                    m_ParticleGroup = value;
                    Bind();
                }
            }
        }

        /// <summary>  
        /// Whether this attachment is currently bound or not.
        /// </summary> 
        public bool isBound
        {
            get { return m_Target != null && m_SolverIndices != null && m_PositionOffsets != null; }
        }

        /// <summary>  
        /// Type of attachment, can be either static or dynamic.
        /// </summary> 
        public AttachmentType attachmentType
        {
            get { return m_AttachmentType; }
            set
            {
                if (value != m_AttachmentType)
                {
                    DisableAttachment(m_AttachmentType);
                    m_AttachmentType = value;
                    EnableAttachment(m_AttachmentType);
                }
            }
        }

        /// <summary>  
        /// Should this attachment constraint particle orientations too?
        /// </summary>
        public bool constrainOrientation
        {
            get { return m_ConstrainOrientation; }
            set
            {
                if (value != m_ConstrainOrientation)
                {
                    DisableAttachment(m_AttachmentType);
                    m_ConstrainOrientation = value;
                    EnableAttachment(m_AttachmentType);
                }
            }
        }


        /// <summary>  
        /// Constraint compliance, in case this attachment is dynamic.
        /// </summary>
        /// High compliance values will increase the attachment's elasticity.
        public float compliance
        {
            get { return m_Compliance; }
            set
            {
                if (!Mathf.Approximately(value, m_Compliance))
                {
                    m_Compliance = value;
                    if (m_AttachmentType == AttachmentType.Dynamic && pinBatch != null)
                    {
                        for (int i = 0; i < m_SolverIndices.Length; ++i)
                            pinBatch.stiffnesses[i * 2] = m_Compliance;
                    }
                }
            }
        }

        /// <summary>  
        /// Force thershold above which the attachment should break.
        /// </summary>
        /// Only affects dynamic attachments, as static attachments do not work with forces.
        public float breakThreshold
        {
            get { return m_BreakThreshold; }
            set
            {
                if (!Mathf.Approximately(value, m_BreakThreshold))
                {
                    m_BreakThreshold = value;
                    if (m_AttachmentType == AttachmentType.Dynamic && pinBatch != null)
                    {
                        for (int i = 0; i < m_SolverIndices.Length; ++i)
                            pinBatch.breakThresholds[i] = m_BreakThreshold;
                    }
                }
            }
        }

        private void OnEnable()
        {
            m_Actor = GetComponent<ObiActor>();
            m_Actor.OnBlueprintLoaded += Actor_OnBlueprintLoaded;
            m_Actor.OnPrepareStep += Actor_OnPrepareStep;
            m_Actor.OnEndStep += Actor_OnEndStep;

            if (m_Actor.solver != null)
                Actor_OnBlueprintLoaded(m_Actor, m_Actor.sourceBlueprint);

            EnableAttachment(m_AttachmentType);
        }

        private void OnDisable()
        {
            DisableAttachment(m_AttachmentType);

            m_Actor.OnBlueprintLoaded -= Actor_OnBlueprintLoaded;
            m_Actor.OnPrepareStep -= Actor_OnPrepareStep;
            m_Actor.OnEndStep -= Actor_OnEndStep;
        }

        private void OnValidate()
        {
            m_Actor = GetComponent<ObiActor>();

            // do not re-bind: simply disable and re-enable the attachment.
            DisableAttachment(AttachmentType.Static);
            DisableAttachment(AttachmentType.Dynamic);
            EnableAttachment(m_AttachmentType);
        }

        void Actor_OnBlueprintLoaded(ObiActor act, ObiActorBlueprint blueprint)
        {
            Bind();
        }

        void Actor_OnPrepareStep(ObiActor act, float stepTime)
        {
            // Attachments must be updated at the start of the step, before performing any simulation.
            UpdateAttachment();
        }

        private void Actor_OnEndStep(ObiActor act, float stepTime)
        {
            // dynamic attachments must be tested at the end of the step, once constraint forces have been calculated.
            // if there's any broken constraint, flag pin constraints as dirty for remerging at the start of the next step.
            BreakDynamicAttachment(stepTime);
        }

        private void Bind()
        {
            // Disable attachment.
            DisableAttachment(m_AttachmentType);

            if (m_Target != null && m_ParticleGroup != null && m_Actor.isLoaded)
            {
                Matrix4x4 bindMatrix = m_Target.worldToLocalMatrix * m_Actor.solver.transform.localToWorldMatrix;

                m_SolverIndices = new int[m_ParticleGroup.Count];
                m_PositionOffsets = new Vector3[m_ParticleGroup.Count];
                m_OrientationOffsets = new Quaternion[m_ParticleGroup.Count];

                for (int i = 0; i < m_ParticleGroup.Count; ++i)
                {
                    int particleIndex = m_ParticleGroup.particleIndices[i];
                    if (particleIndex >= 0 && particleIndex < m_Actor.solverIndices.Length)
                    {
                        m_SolverIndices[i] = m_Actor.solverIndices[particleIndex];
                        m_PositionOffsets[i] = bindMatrix.MultiplyPoint3x4(m_Actor.solver.positions[m_SolverIndices[i]]);
                    }
                    else
                    {
                        Debug.LogError("The particle group \'" + m_ParticleGroup.name + "\' references a particle that does not exist in the actor \'" + m_Actor.name + "\'.");
                        m_SolverIndices = null;
                        m_PositionOffsets = null;
                        m_OrientationOffsets = null;
                        return;
                    }
                }

                if (m_Actor.usesOrientedParticles)
                {
                    Quaternion bindOrientation = bindMatrix.rotation;

                    for (int i = 0; i < m_ParticleGroup.Count; ++i)
                    {
                        int particleIndex = m_ParticleGroup.particleIndices[i];
                        if (particleIndex >= 0 && particleIndex < m_Actor.solverIndices.Length)
                            m_OrientationOffsets[i] = bindOrientation * m_Actor.solver.orientations[m_SolverIndices[i]];
                    }
                }
            }
            else
            {
                m_PositionOffsets = null;
                m_OrientationOffsets = null;
            }

            EnableAttachment(m_AttachmentType);
        }


        private void EnableAttachment(AttachmentType type)
        {

            if (enabled && m_Actor.isLoaded && isBound)
            {
                var solver = m_Actor.solver;

                switch (type)
                {
                    case AttachmentType.Dynamic:

                        var pins = m_Actor.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;
                        attachedCollider = m_Target.GetComponent<ObiColliderBase>();

                        if (pins != null && attachedCollider != null && pinBatch == null)
                        {
                            // create a new data batch with all our pin constraints:
                            pinBatch = new ObiPinConstraintsBatch(pins);
                            for (int i = 0; i < m_SolverIndices.Length; ++i)
                            {
                                pinBatch.AddConstraint(m_SolverIndices[i],
                                                       attachedCollider,
                                                       m_PositionOffsets[i],
                                                       m_OrientationOffsets[i],
                                                       m_Compliance,
                                                       constrainOrientation ? 0 : 10000,
                                                       m_BreakThreshold);

                                pinBatch.activeConstraintCount++;
                            }

                            // add the batch to the actor:
                            pins.AddBatch(pinBatch);

                            // store the attached collider's handle:
                            attachedColliderHandleIndex = -1;
                            if (attachedCollider.Handle != null)
                                attachedColliderHandleIndex = attachedCollider.Handle.index;

                            m_Actor.SetConstraintsDirty(Oni.ConstraintType.Pin);
                        }

                        break;

                    case AttachmentType.Static:

                        for (int i = 0; i < m_SolverIndices.Length; ++i)
                            if (m_SolverIndices[i] >= 0 && m_SolverIndices[i] < solver.invMasses.count)
                                solver.invMasses[m_SolverIndices[i]] = 0;

                        if (m_Actor.usesOrientedParticles && m_ConstrainOrientation)
                        {
                            for (int i = 0; i < m_SolverIndices.Length; ++i)
                                if (m_SolverIndices[i] >= 0 && m_SolverIndices[i] < solver.invRotationalMasses.count)
                                    solver.invRotationalMasses[m_SolverIndices[i]] = 0;
                        }

                        m_Actor.UpdateParticleProperties();

                        break;

                }
            }

        }

        private void DisableAttachment(AttachmentType type)
        {
            if (isBound)
            {
                switch (type)
                {
                    case AttachmentType.Dynamic:

                        if (pinBatch != null)
                        {
                            var pins = m_Actor.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
                            if (pins != null)
                            {
                                pins.RemoveBatch(pinBatch);
                                if (actor.isLoaded)
                                    m_Actor.SetConstraintsDirty(Oni.ConstraintType.Pin);
                            }

                            attachedCollider = null;
                            pinBatch = null;
                            attachedColliderHandleIndex = -1;
                        }

                        break;

                    case AttachmentType.Static:

                        var solver = m_Actor.solver;
                        var blueprint = m_Actor.sourceBlueprint;

                        for (int i = 0; i < m_SolverIndices.Length; ++i)
                        {
                            int solverIndex = m_SolverIndices[i];
                            if (solverIndex >= 0 && solverIndex < solver.invMasses.count)
                                solver.invMasses[solverIndex] = blueprint.invMasses[i];
                        }

                        if (m_Actor.usesOrientedParticles)
                        {
                            for (int i = 0; i < m_SolverIndices.Length; ++i)
                            {
                                int solverIndex = m_SolverIndices[i];
                                if (solverIndex >= 0 && solverIndex < solver.invRotationalMasses.count)
                                    solver.invRotationalMasses[solverIndex] = blueprint.invRotationalMasses[i];
                            }
                        }

                        m_Actor.UpdateParticleProperties();

                        break;

                }
            }
        }

        private void UpdateAttachment()
        {

            if (enabled && m_Actor.isLoaded && isBound)
            {
                var solver = m_Actor.solver;

                switch (m_AttachmentType)
                {
                    case AttachmentType.Dynamic:

                        // in case the handle has been updated/invalidated (for instance, when disabling the target) rebuild constraints:
                        if (attachedCollider != null &&
                            attachedCollider.Handle != null &&
                            attachedCollider.Handle.index != attachedColliderHandleIndex)
                        {
                            attachedColliderHandleIndex = attachedCollider.Handle.index;
                            m_Actor.SetConstraintsDirty(Oni.ConstraintType.Pin);
                        }

                        break;

                    case AttachmentType.Static:

                        var blueprint = m_Actor.sourceBlueprint;
                        bool targetActive = m_Target.gameObject.activeInHierarchy;

                        // Build the attachment matrix:
                        Matrix4x4 attachmentMatrix = solver.transform.worldToLocalMatrix * m_Target.localToWorldMatrix;

                        // Fix all particles in the group and update their position 
                        // Note: skip assignment to startPositions if you want attached particles to be interpolated too.
                        for (int i = 0; i < m_SolverIndices.Length; ++i)
                        {
                            int solverIndex = m_SolverIndices[i];

                            if (solverIndex >= 0 && solverIndex < solver.invMasses.count)
                            {
                                if (targetActive)
                                {
                                    solver.invMasses[solverIndex] = 0;
                                    solver.velocities[solverIndex] = Vector3.zero;
                                    solver.startPositions[solverIndex] = solver.positions[solverIndex] = attachmentMatrix.MultiplyPoint3x4(m_PositionOffsets[i]);
                                }else
                                    solver.invMasses[solverIndex] = blueprint.invMasses[i];
                            }
                        }

                        if (m_Actor.usesOrientedParticles && m_ConstrainOrientation)
                        {
                            Quaternion attachmentRotation = attachmentMatrix.rotation;

                            for (int i = 0; i < m_SolverIndices.Length; ++i)
                            {
                                int solverIndex = m_SolverIndices[i];

                                if (solverIndex >= 0 && solverIndex < solver.invRotationalMasses.count)
                                {
                                    if (targetActive)
                                    {
                                        solver.invRotationalMasses[solverIndex] = 0;
                                        solver.angularVelocities[solverIndex] = Vector3.zero;
                                        solver.startOrientations[solverIndex] = solver.orientations[solverIndex] = attachmentRotation * m_OrientationOffsets[i];
                                    }
                                    else
                                        solver.invRotationalMasses[solverIndex] = blueprint.invRotationalMasses[i];
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void BreakDynamicAttachment(float stepTime)
        {

            if (enabled && m_AttachmentType == AttachmentType.Dynamic && m_Actor.isLoaded && isBound)
            {

                var solver = m_Actor.solver;

                var actorConstraints = m_Actor.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
                var solverConstraints = solver.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;

                bool dirty = false;
                if (actorConstraints != null && pinBatch != null)
                {
                    int pinBatchIndex = actorConstraints.batches.IndexOf(pinBatch);
                    if (pinBatchIndex >= 0 && pinBatchIndex < actor.solverBatchOffsets[(int)Oni.ConstraintType.Pin].Count)
                    {
                        int offset = actor.solverBatchOffsets[(int)Oni.ConstraintType.Pin][pinBatchIndex];
                        var solverBatch = solverConstraints.batches[pinBatchIndex];

                        float sqrTime = stepTime * stepTime;
                        for (int i = 0; i < pinBatch.activeConstraintCount; i++)
                        {
                            // In case the handle has been created/destroyed.
                            if (pinBatch.pinBodies[i] != attachedCollider.Handle)
                            {
                                pinBatch.pinBodies[i] = attachedCollider.Handle;
                                dirty = true;
                            }

                            // in case the constraint has been broken:
                            if (-solverBatch.lambdas[(offset + i) * 4 + 3] / sqrTime > pinBatch.breakThresholds[i])
                            {
                                pinBatch.DeactivateConstraint(i);
                                dirty = true;
                            }
                        }
                    }
                }

                // constraints are recreated at the start of a step.
                if (dirty)
                    m_Actor.SetConstraintsDirty(Oni.ConstraintType.Pin);
            }
        }
    }
}
