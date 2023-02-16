#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using System;
using System.Collections;

namespace Obi
{

    [BurstCompile]
    public struct ApplyBatchedCollisionConstraintsBatchJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<BurstContact> contacts;

        [ReadOnly] public NativeList<int> simplices;
        [ReadOnly] public SimplexCounts simplexCounts;

        [NativeDisableParallelForRestriction] public NativeArray<float4> positions;
        [NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
        [NativeDisableParallelForRestriction] public NativeArray<int> counts;

        [NativeDisableParallelForRestriction] public NativeArray<quaternion> orientations;
        [NativeDisableParallelForRestriction] public NativeArray<quaternion> orientationDeltas;
        [NativeDisableParallelForRestriction] public NativeArray<int> orientationCounts;

        [ReadOnly] public Oni.ConstraintParameters constraintParameters;
        [ReadOnly] public BatchData batchData;

        public void Execute(int workItemIndex)
        {
            int start, end;
            batchData.GetConstraintRange(workItemIndex, out start, out end);

            for (int i = start; i < end; ++i)
            {
                int simplexStartA = simplexCounts.GetSimplexStartAndSize(contacts[i].bodyA, out int simplexSizeA);
                int simplexStartB = simplexCounts.GetSimplexStartAndSize(contacts[i].bodyB, out int simplexSizeB);

                for (int j = 0; j < simplexSizeA; ++j)
                {
                    int particleIndex = simplices[simplexStartA + j];
                    BurstConstraintsBatchImpl.ApplyPositionDelta(particleIndex, constraintParameters.SORFactor, ref positions, ref deltas, ref counts);
                    BurstConstraintsBatchImpl.ApplyOrientationDelta(particleIndex, constraintParameters.SORFactor, ref orientations, ref orientationDeltas, ref orientationCounts);
                }

                for (int j = 0; j < simplexSizeB; ++j)
                {
                    int particleIndex = simplices[simplexStartB + j];
                    BurstConstraintsBatchImpl.ApplyPositionDelta(particleIndex, constraintParameters.SORFactor, ref positions, ref deltas, ref counts);
                    BurstConstraintsBatchImpl.ApplyOrientationDelta(particleIndex, constraintParameters.SORFactor, ref orientations, ref orientationDeltas, ref orientationCounts);
                }

            }
            
        }

    }
}
#endif