#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Mathematics;
using Unity.Collections;

namespace Obi
{
    public struct BurstQueryResult
    {
        public float4 simplexBary; // point A, expressed as simplex barycentric coords for simplices, as a solver-space position for colliders.
        public float4 queryPoint; // point B, expressed as simplex barycentric coords for simplices, as a solver-space position for colliders.
        public float4 normal;
        public float distance;
        public int simplexIndex;
        public int queryIndex;
        public int pad0; // padding to ensure correct alignment.
    }
}
#endif