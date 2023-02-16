#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;

namespace Obi
{
    public struct BurstSphere : BurstLocalOptimization.IDistanceFunction, IBurstCollider
    {
        public BurstColliderShape shape;
        public BurstAffineTransform colliderToSolver;
        public float dt;

        public void Evaluate(float4 point, float4 radii, quaternion orientation, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            float4 center = shape.center * colliderToSolver.scale;
            point = colliderToSolver.InverseTransformPointUnscaled(point) - center;

            if (shape.is2D != 0)
                point[2] = 0;

            float radius = shape.size.x * math.cmax(colliderToSolver.scale.xyz);
            float distanceToCenter = math.length(point);

            float4 normal = point / (distanceToCenter + BurstMath.epsilon);

            projectedPoint.point = colliderToSolver.TransformPointUnscaled(center + normal * (radius + shape.contactOffset));
            projectedPoint.normal = colliderToSolver.TransformDirection(normal);
        }

        public void Contacts(int colliderIndex,
                             int rigidbodyIndex,
                             NativeArray<BurstRigidbody> rigidbodies,

                             NativeArray<float4> positions,
                             NativeArray<quaternion> orientations,
                             NativeArray<float4> velocities,
                             NativeArray<float4> radii,

                             NativeArray<int> simplices,
                             in BurstAabb simplexBounds,
                             int simplexIndex,
                             int simplexStart,
                             int simplexSize,

                             NativeQueue<BurstContact>.ParallelWriter contacts,
                             int optimizationIterations,
                             float optimizationTolerance)
        {
            var co = new BurstContact() { bodyA = simplexIndex, bodyB = colliderIndex };
            float4 simplexBary = BurstMath.BarycenterForSimplexOfSize(simplexSize);

            var colliderPoint = BurstLocalOptimization.Optimize<BurstSphere>(ref this, positions, orientations, radii, simplices, simplexStart, simplexSize,
                                                                             ref simplexBary, out float4 convexPoint, optimizationIterations, optimizationTolerance);

            co.pointB = colliderPoint.point;
            co.normal = colliderPoint.normal;
            co.pointA = simplexBary;

            contacts.Enqueue(co);
        }
    }

}
#endif