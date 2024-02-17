#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace Obi
{
    public struct BurstSimplex : BurstLocalOptimization.IDistanceFunction
    {
        public NativeArray<float4> positions;
        public NativeArray<float4> radii;
        public NativeArray<int> simplices;

        public int simplexStart;
        public int simplexSize;

        private BurstMath.CachedTri tri;

        public void CacheData()
        {
            if (simplexSize == 3)
            {
                tri.Cache(positions[simplices[simplexStart]],
                          positions[simplices[simplexStart + 1]],
                          positions[simplices[simplexStart + 2]]);
            }
        }

        public void Evaluate(float4 point, float4 radii, quaternion orientation, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            switch (simplexSize)
            {
                case 1:
                    {
                        float4 p1 = positions[simplices[simplexStart]];
                        projectedPoint.bary = new float4(1, 0, 0, 0);
                        projectedPoint.point = p1;
                    }
                    break;
                case 2:
                    {
                        float4 p1 = positions[simplices[simplexStart]];
                        float4 p2 = positions[simplices[simplexStart + 1]];
                        BurstMath.NearestPointOnEdge(p1, p2, point, out float mu);
                        projectedPoint.bary = new float4(1 - mu, mu, 0, 0);
                        projectedPoint.point = p1 * projectedPoint.bary[0] + p2 * projectedPoint.bary[1];
            
                    }break;
                case 3:
                        projectedPoint.point = BurstMath.NearestPointOnTri(tri, point, out projectedPoint.bary);
                    break;
            }

            projectedPoint.normal = math.normalizesafe(point - projectedPoint.point);
        }

    }

}
#endif