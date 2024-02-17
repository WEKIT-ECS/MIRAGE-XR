#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;

namespace Obi
{
    public struct BurstEdgeMesh : BurstLocalOptimization.IDistanceFunction, IBurstCollider
    {

        public BurstColliderShape shape;
        public BurstAffineTransform colliderToSolver;
        public int dataOffset;
        public float dt;

        public EdgeMeshHeader header;
        public NativeArray<BIHNode> edgeBihNodes;
        public NativeArray<Edge> edges;
        public NativeArray<float2> vertices;

        public void Evaluate(float4 point, float4 radii, quaternion orientation, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            point = colliderToSolver.InverseTransformPointUnscaled(point);

            if (shape.is2D != 0)
                point[2] = 0;

            Edge t = edges[header.firstEdge + dataOffset];
            float4 v1 = (new float4(vertices[header.firstVertex + t.i1], 0) + shape.center) * colliderToSolver.scale;
            float4 v2 = (new float4(vertices[header.firstVertex + t.i2], 0) + shape.center) * colliderToSolver.scale;

            float4 nearestPoint = BurstMath.NearestPointOnEdge(v1, v2, point, out float mu);
            float4 normal = math.normalizesafe(point - nearestPoint);

            projectedPoint.normal = colliderToSolver.TransformDirection(normal);
            projectedPoint.point = colliderToSolver.TransformPointUnscaled(nearestPoint + normal * shape.contactOffset);
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
            if (shape.dataIndex < 0) return;

            BIHTraverse(colliderIndex, simplexIndex, simplexStart, simplexSize,
                        positions, orientations, radii, simplices, in simplexBounds, 0, contacts, optimizationIterations, optimizationTolerance);
        }

        private void BIHTraverse(int colliderIndex,
                                 int simplexIndex,
                                 int simplexStart,
                                 int simplexSize,
                                 NativeArray<float4> positions,
                                 NativeArray<quaternion> orientations,
                                 NativeArray<float4> radii,
                                 NativeArray<int> simplices,
                                 in BurstAabb simplexBounds,
                                 int nodeIndex,
                                 NativeQueue<BurstContact>.ParallelWriter contacts,
                                 int optimizationIterations,
                                 float optimizationTolerance)
        {
            var node = edgeBihNodes[header.firstNode + nodeIndex];

            if (node.firstChild >= 0)
            {
                // visit min node:
                if (simplexBounds.min[node.axis] <= node.min + shape.center[node.axis])
                    BIHTraverse(colliderIndex, simplexIndex, simplexStart, simplexSize,
                                positions, orientations, radii, simplices, in simplexBounds,
                                node.firstChild, contacts, optimizationIterations, optimizationTolerance);

                // visit max node:
                if (simplexBounds.max[node.axis] >= node.max + shape.center[node.axis])
                    BIHTraverse(colliderIndex, simplexIndex, simplexStart, simplexSize,
                                positions, orientations, radii, simplices, in simplexBounds,
                                node.firstChild + 1, contacts, optimizationIterations, optimizationTolerance);
            }
            else
            {
                // check for contact against all triangles:
                for (dataOffset = node.start; dataOffset < node.start + node.count; ++dataOffset)
                {
                    Edge t = edges[header.firstEdge + dataOffset];
                    float4 v1 = new float4(vertices[header.firstVertex + t.i1], 0) + shape.center;
                    float4 v2 = new float4(vertices[header.firstVertex + t.i2], 0) + shape.center;
                    BurstAabb edgeBounds = new BurstAabb(v1, v2, shape.contactOffset + 0.01f);

                    if (edgeBounds.IntersectsAabb(simplexBounds, shape.is2D != 0))
                    {
                        var co = new BurstContact() { bodyA = simplexIndex, bodyB = colliderIndex };
                        float4 simplexBary = BurstMath.BarycenterForSimplexOfSize(simplexSize);

                        var colliderPoint = BurstLocalOptimization.Optimize<BurstEdgeMesh>(ref this, positions, orientations, radii, simplices, simplexStart, simplexSize,
                                                                            ref simplexBary, out float4 convexPoint, optimizationIterations, optimizationTolerance);

                        co.pointB = colliderPoint.point;
                        co.normal = colliderPoint.normal;
                        co.pointA = simplexBary;

                        contacts.Enqueue(co);
                    }
                }
            }
        }

    }

}
#endif