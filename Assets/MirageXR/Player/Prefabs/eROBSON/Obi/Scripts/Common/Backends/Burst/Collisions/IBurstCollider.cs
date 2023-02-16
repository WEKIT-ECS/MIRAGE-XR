#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;

namespace Obi
{
    interface IBurstCollider
    {
        void Contacts(int colliderIndex,
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
                      float optimizationTolerance);
    }
}
#endif