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
    struct BuildSimplexAabbs : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4> radii;
        [ReadOnly] public NativeArray<float> fluidRadii;
        [ReadOnly] public NativeArray<float4> positions;
        [ReadOnly] public NativeArray<float4> velocities;

        // simplex arrays:
        [ReadOnly] public NativeList<int> simplices;
        [ReadOnly] public SimplexCounts simplexCounts;

        [ReadOnly] public NativeArray<int> particleMaterialIndices;
        [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;
        [ReadOnly] public float collisionMargin;
        [ReadOnly] public float continuousCollisionDetection;
        [ReadOnly] public float dt;

        public NativeArray<BurstAabb> simplexBounds;

        public void Execute(int i)
        {
            int simplexStart = simplexCounts.GetSimplexStartAndSize(i, out int simplexSize);

            var bounds = new BurstAabb(float.MaxValue, float.MinValue);
            for (int j = 0; j < simplexSize; ++j)
            {
                int p = simplices[simplexStart + j];

                // Find this particle's stick distance:
                int m = particleMaterialIndices[p];
                float stickDistance = m >= 0 ? collisionMaterials[m].stickDistance : 0;

                // Expand simplex bounds, using both the particle's original position and its velocity:
                bounds.EncapsulateParticle(positions[p], positions[p] + velocities[p] * continuousCollisionDetection * dt,
                                            math.max(radii[p].x + stickDistance, fluidRadii[p] * 0.5f) + collisionMargin);
            }

            simplexBounds[i] = bounds;
        }
    }
}
#endif