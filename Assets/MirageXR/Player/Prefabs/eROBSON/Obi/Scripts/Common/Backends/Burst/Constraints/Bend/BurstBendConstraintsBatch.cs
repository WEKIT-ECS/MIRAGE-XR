#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections;

namespace Obi
{
    public class BurstBendConstraintsBatch : BurstConstraintsBatchImpl, IBendConstraintsBatchImpl
    {
        private NativeArray<float> restBends;
        private NativeArray<float2> stiffnesses;
        private NativeArray<float2> plasticity;

        BendConstraintsBatchJob projectConstraints;
        ApplyBendConstraintsBatchJob applyConstraints;

        public BurstBendConstraintsBatch(BurstBendConstraints constraints)
        {
            m_Constraints = constraints;
            m_ConstraintType = Oni.ConstraintType.Bending;
        }

        public void SetBendConstraints(ObiNativeIntList particleIndices, ObiNativeFloatList restBends, ObiNativeVector2List bendingStiffnesses, ObiNativeVector2List plasticity, ObiNativeFloatList lambdas, int count)
        {
            this.particleIndices = particleIndices.AsNativeArray<int>();
            this.restBends = restBends.AsNativeArray<float>();
            this.stiffnesses = bendingStiffnesses.AsNativeArray<float2>();
            this.plasticity = plasticity.AsNativeArray<float2>();
            this.lambdas = lambdas.AsNativeArray<float>();
            m_ConstraintCount = count;

            projectConstraints.particleIndices = this.particleIndices;
            projectConstraints.restBends = this.restBends;
            projectConstraints.stiffnesses = this.stiffnesses;
            projectConstraints.plasticity = this.plasticity;
            projectConstraints.lambdas = this.lambdas;

            applyConstraints.particleIndices = this.particleIndices;
        }

        public override JobHandle Evaluate(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            projectConstraints.positions = solverImplementation.positions;
            projectConstraints.invMasses = solverImplementation.invMasses;
            projectConstraints.deltas = solverImplementation.positionDeltas;
            projectConstraints.counts = solverImplementation.positionConstraintCounts;
            projectConstraints.deltaTime = substepTime;

            return projectConstraints.Schedule(m_ConstraintCount, 32, inputDeps);
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
            var parameters = solverAbstraction.GetConstraintParameters(m_ConstraintType);

            applyConstraints.positions = solverImplementation.positions;
            applyConstraints.deltas = solverImplementation.positionDeltas;
            applyConstraints.counts = solverImplementation.positionConstraintCounts;
            applyConstraints.sorFactor = parameters.SORFactor;

            return applyConstraints.Schedule(m_ConstraintCount, 64, inputDeps);
        }

        [BurstCompile]
        public struct BendConstraintsBatchJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> particleIndices;
            [ReadOnly] public NativeArray<float2> stiffnesses;
            [ReadOnly] public NativeArray<float2> plasticity; //plastic yield, creep
            public NativeArray<float> restBends;
            public NativeArray<float> lambdas;

            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float> invMasses;

            [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
            [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<int> counts;

            [ReadOnly] public float deltaTime;

            public void Execute(int i)
            {
                int p1 = particleIndices[i * 3];
                int p2 = particleIndices[i * 3 + 1];
                int p3 = particleIndices[i * 3 + 2];

                float w1 = invMasses[p1];
                float w2 = invMasses[p2];
                float w3 = invMasses[p3];

                float wsum = w1 + w2 + 2 * w3;

                float4 bendVector = positions[p3] - (positions[p1] + positions[p2] + positions[p3]) / 3.0f;
                float bend = math.length(bendVector);

             
                float constraint = bend - restBends[i];

                constraint = math.max(0, constraint - stiffnesses[i].x) +
                             math.min(0, constraint + stiffnesses[i].x);

                // plasticity:
                if (math.abs(constraint) > plasticity[i].x)  
                    restBends[i] += constraint * plasticity[i].y * deltaTime;

                // calculate time adjusted compliance
                float compliance = stiffnesses[i].y / (deltaTime * deltaTime);

                // since the third particle moves twice the amount of the other 2, the modulus of its gradient is 2:
                float dlambda = (-constraint - compliance * lambdas[i]) / (wsum + compliance + BurstMath.epsilon);
                float4 correction = dlambda * bendVector / (bend + BurstMath.epsilon);

                lambdas[i] += dlambda;

                deltas[p1] -= correction * 2 * w1;
                deltas[p2] -= correction * 2 * w2;
                deltas[p3] += correction * 4 * w3;

                counts[p1]++;
                counts[p2]++;
                counts[p3]++;
            }
        }

        [BurstCompile]
        public struct ApplyBendConstraintsBatchJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> particleIndices;
            [ReadOnly] public float sorFactor;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> positions;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> counts;

            public void Execute(int i)
            {
                int p1 = particleIndices[i * 3];
                int p2 = particleIndices[i * 3 + 1];
                int p3 = particleIndices[i * 3 + 2];

                if (counts[p1] > 0)
                {
                    positions[p1] += deltas[p1] * sorFactor / counts[p1];
                    deltas[p1] = float4.zero;
                    counts[p1] = 0;
                }

                if (counts[p2] > 0)
                {
                    positions[p2] += deltas[p2] * sorFactor / counts[p2];
                    deltas[p2] = float4.zero;
                    counts[p2] = 0;
                }

                if (counts[p3] > 0)
                {
                    positions[p3] += deltas[p3] * sorFactor / counts[p3];
                    deltas[p3] = float4.zero;
                    counts[p3] = 0;
                }
            }
        }
    }
}
#endif