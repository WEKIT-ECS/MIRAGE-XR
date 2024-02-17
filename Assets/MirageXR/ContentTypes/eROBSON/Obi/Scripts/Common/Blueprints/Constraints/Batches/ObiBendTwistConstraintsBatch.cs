using UnityEngine;
using System.Collections.Generic;
using System;

namespace Obi
{
    [Serializable]
    public class ObiBendTwistConstraintsBatch : ObiConstraintsBatch
    {
        protected IBendTwistConstraintsBatchImpl m_BatchImpl;

        /// <summary>
        /// Rest darboux vector for each constraint.
        /// </summary>
        [HideInInspector] public ObiNativeQuaternionList restDarbouxVectors = new ObiNativeQuaternionList();            

        /// <summary>
        /// 3 compliance values for each constraint, one for each local axis (x,y,z)
        /// </summary>
        [HideInInspector] public ObiNativeVector3List stiffnesses = new ObiNativeVector3List();                             

        /// <summary>
        /// two floats per constraint: plastic yield and creep.
        /// </summary>
        [HideInInspector] public ObiNativeVector2List plasticity = new ObiNativeVector2List();                            

        public override Oni.ConstraintType constraintType
        {
            get { return Oni.ConstraintType.BendTwist; }
        }

        public override IConstraintsBatchImpl implementation
        {
            get { return m_BatchImpl; }
        }

        public ObiBendTwistConstraintsBatch(ObiBendTwistConstraintsData constraints = null) : base()
        {
        }

        public void AddConstraint(Vector2Int indices, Quaternion restDarboux)
        {
            RegisterConstraint();

            this.particleIndices.Add(indices[0]);
            this.particleIndices.Add(indices[1]);
            this.restDarbouxVectors.Add(restDarboux);
            this.stiffnesses.Add(Vector3.zero);
            this.plasticity.Add(Vector2.zero);
        }

        public override void Clear()
        {
            base.Clear();
            particleIndices.Clear();
            restDarbouxVectors.Clear();
            stiffnesses.Clear();
            plasticity.Clear();
        }

        public override void GetParticlesInvolved(int index, List<int> particles)
        {
            particles.Add(particleIndices[index * 2]);
            particles.Add(particleIndices[index * 2 + 1]);
        }

        protected override void SwapConstraints(int sourceIndex, int destIndex)
        {
            particleIndices.Swap(sourceIndex * 2, destIndex * 2);
            particleIndices.Swap(sourceIndex * 2 + 1, destIndex * 2 + 1);
            restDarbouxVectors.Swap(sourceIndex, destIndex);
            stiffnesses.Swap(sourceIndex, destIndex);
            plasticity.Swap(sourceIndex, destIndex);
        }

        public override void Merge(ObiActor actor, IObiConstraintsBatch other)
        {
            var batch = other as ObiBendTwistConstraintsBatch;
            var user = actor as IBendTwistConstraintsUser;

            if (batch != null && user != null)
            {
                if (!user.bendTwistConstraintsEnabled)
                  return;

                particleIndices.ResizeUninitialized((m_ActiveConstraintCount + batch.activeConstraintCount) * 2);
                restDarbouxVectors.ResizeUninitialized(m_ActiveConstraintCount + batch.activeConstraintCount);
                stiffnesses.ResizeUninitialized(m_ActiveConstraintCount + batch.activeConstraintCount);
                plasticity.ResizeUninitialized(m_ActiveConstraintCount + batch.activeConstraintCount);
                lambdas.ResizeInitialized((m_ActiveConstraintCount + batch.activeConstraintCount) * 3);

                restDarbouxVectors.CopyFrom(batch.restDarbouxVectors, 0, m_ActiveConstraintCount, batch.activeConstraintCount);

                for (int i = 0; i < batch.activeConstraintCount; ++i)
                {
                    stiffnesses[m_ActiveConstraintCount + i] = user.GetBendTwistCompliance(batch, i);
                    plasticity[m_ActiveConstraintCount + i] = user.GetBendTwistPlasticity(batch, i);
                }

                for (int i = 0; i < batch.activeConstraintCount * 2; ++i)
                    particleIndices[m_ActiveConstraintCount * 2 + i] = actor.solverIndices[batch.particleIndices[i]];

                base.Merge(actor, other);
            }
        }

        public override void AddToSolver(ObiSolver solver)
        {
            // Create distance constraints batch directly.
            m_BatchImpl = solver.implementation.CreateConstraintsBatch(constraintType) as IBendTwistConstraintsBatchImpl;

            if (m_BatchImpl != null)
                m_BatchImpl.SetBendTwistConstraints(particleIndices, restDarbouxVectors, stiffnesses, plasticity, lambdas, m_ActiveConstraintCount);
            
        }

        public override void RemoveFromSolver(ObiSolver solver)
        {
            //Remove batch:
            solver.implementation.DestroyConstraintsBatch(m_BatchImpl as IConstraintsBatchImpl);
        }
    }
}
