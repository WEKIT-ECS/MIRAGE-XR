#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;

namespace Obi
{
    public struct BurstRay : BurstLocalOptimization.IDistanceFunction
    {
        public BurstQueryShape shape;
        public BurstAffineTransform colliderToSolver;

        public void Evaluate(float4 point, float4 radii, quaternion orientation, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            float4x4 simplexToSolver = float4x4.TRS(point.xyz, orientation, radii.xyz);
            float4x4 solverToSimplex = math.inverse(simplexToSolver);
            float4x4 colliderToSimplex = math.mul(solverToSimplex, float4x4.TRS(colliderToSolver.translation.xyz, colliderToSolver.rotation, colliderToSolver.scale.xyz));

            // express ray in simplex space (ellipsoid == scaled sphere)
            float4 rayOrigin = math.mul(colliderToSimplex, new float4(shape.center.xyz,1));
            float4 rayDirection = math.normalizesafe(math.mul(colliderToSimplex, new float4((shape.size - shape.center).xyz,0)));

            float rayDistance = ObiUtils.RaySphereIntersection(rayOrigin.xyz, rayDirection.xyz, float3.zero, 1);

            if (rayDistance < 0)
            {
                point = colliderToSolver.InverseTransformPointUnscaled(point);

                float4 centerLine = BurstMath.NearestPointOnEdge(shape.center * colliderToSolver.scale, shape.size * colliderToSolver.scale, point, out float mu);
                float4 centerToPoint = point - centerLine;
                float distanceToCenter = math.length(centerToPoint);

                float4 normal = centerToPoint / (distanceToCenter + BurstMath.epsilon);

                projectedPoint.point = colliderToSolver.TransformPointUnscaled(centerLine + normal * shape.contactOffset);
                projectedPoint.normal = colliderToSolver.TransformDirection(normal);
            }
            else
            {
                float4 rayPoint = math.mul(simplexToSolver, new float4((rayOrigin + rayDirection * rayDistance).xyz,1));
                float4 normal = math.normalizesafe(new float4((point - rayPoint).xyz,0));

                projectedPoint.point = rayPoint + normal * shape.contactOffset;
                projectedPoint.normal = normal;
            }
        }

        public void Query(int shapeIndex,
                             NativeArray<float4> positions,
                             NativeArray<quaternion> orientations,
                             NativeArray<float4> radii,
                             NativeArray<int> simplices,
                             int simplexIndex,
                             int simplexStart,
                             int simplexSize,

                             NativeQueue<BurstQueryResult>.ParallelWriter results,
                             int optimizationIterations,
                             float optimizationTolerance)
        {
            var co = new BurstQueryResult() { simplexIndex = simplexIndex, queryIndex = shapeIndex };
            float4 simplexBary = BurstMath.BarycenterForSimplexOfSize(simplexSize);

            var colliderPoint = BurstLocalOptimization.Optimize<BurstRay>(ref this, positions, orientations, radii, simplices, simplexStart, simplexSize,
                                                                          ref simplexBary, out float4 convexPoint, optimizationIterations, optimizationTolerance);

            co.queryPoint = colliderPoint.point;
            co.normal = colliderPoint.normal;
            co.simplexBary = simplexBary;

            results.Enqueue(co);
        }
    }

}
#endif