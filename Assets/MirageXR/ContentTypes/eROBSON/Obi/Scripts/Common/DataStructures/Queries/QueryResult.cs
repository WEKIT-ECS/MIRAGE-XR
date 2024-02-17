using UnityEngine;
using System.Runtime.InteropServices;

namespace Obi
{
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    public struct QueryResult
    {
        public Vector4 simplexBary;   /**< Barycentric coords of nearest point in simplex  */
        public Vector4 queryPoint;    /**< Nearest point in query shape*/
        public Vector4 normal;        /**< Closest direction between simplex and query shape. */
        public float distance;        /**< Distance between simplex and query shape.*/
        public int simplexIndex;      /**< Index of the simplex in the solver.*/
        public int queryIndex;        /**< Index of the query that spawned this result.*/
    }
}
