#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;

namespace Obi
{
    public struct BurstBoxQuery : BurstLocalOptimization.IDistanceFunction
    {
        public BurstQueryShape shape;
        public BurstAffineTransform colliderToSolver;

        public void Evaluate(float4 point, float4 radii, quaternion orientation, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            float4 center = shape.center * colliderToSolver.scale;
            float4 size = shape.size * colliderToSolver.scale * 0.5f;

            // clamp the point to the surface of the box:
            point = colliderToSolver.InverseTransformPointUnscaled(point) - center;

           /*if (shape.is2D != 0)
                point[2] = 0;*/

            // get minimum distance for each axis:
            float4 distances = size - math.abs(point);

            if (distances.x >= 0 && distances.y >= 0 && distances.z >= 0)
            {
                // find minimum distance in all three axes and the axis index:
                float min = float.MaxValue;
                int axis = 0;

                for (int i = 0; i < 3; ++i)
                {
                    if (distances[i] < min)
                    {
                        min = distances[i];
                        axis = i;
                    }
                }

                projectedPoint.normal = float4.zero;
                projectedPoint.point = point;

                projectedPoint.normal[axis] = point[axis] > 0 ? 1 : -1;
                projectedPoint.point[axis] = size[axis] * projectedPoint.normal[axis];
            }
            else
            {
                projectedPoint.point = math.clamp(point, -size, size);
                projectedPoint.normal = math.normalizesafe(point - projectedPoint.point);
            }

            projectedPoint.point = colliderToSolver.TransformPointUnscaled(projectedPoint.point + center + projectedPoint.normal * shape.contactOffset);
            projectedPoint.normal = colliderToSolver.TransformDirection(projectedPoint.normal);
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

            var colliderPoint = BurstLocalOptimization.Optimize<BurstBoxQuery>(ref this, positions, orientations, radii, simplices, simplexStart, simplexSize,
                                                        ref simplexBary, out float4 convexPoint, optimizationIterations, optimizationTolerance);

            co.queryPoint = colliderPoint.point;
            co.normal = colliderPoint.normal;
            co.simplexBary = simplexBary;

            results.Enqueue(co);
        }

    }

}
#endif