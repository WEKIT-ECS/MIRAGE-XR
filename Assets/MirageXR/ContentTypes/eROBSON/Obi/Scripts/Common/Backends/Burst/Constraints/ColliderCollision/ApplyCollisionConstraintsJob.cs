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
    public struct ApplyCollisionConstraintsBatchJob : IJob
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

        public void Execute()
        {
            for (int i = 0; i < contacts.Length; ++i)
            {
                int simplexStart = simplexCounts.GetSimplexStartAndSize(contacts[i].bodyA, out int simplexSize);

                for (int j = 0; j < simplexSize; ++j)
                {
                    int particleIndex = simplices[simplexStart + j];
                    BurstConstraintsBatchImpl.ApplyPositionDelta(particleIndex, constraintParameters.SORFactor, ref positions, ref deltas, ref counts);
                    BurstConstraintsBatchImpl.ApplyOrientationDelta(particleIndex, constraintParameters.SORFactor, ref orientations, ref orientationDeltas, ref orientationCounts);
                }
            }
        }

    }

}
#endif