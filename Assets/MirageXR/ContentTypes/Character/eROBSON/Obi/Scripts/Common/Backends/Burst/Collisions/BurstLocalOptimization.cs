#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Runtime.CompilerServices;

namespace Obi
{ 
    public static class BurstLocalOptimization
    {

        /**
         * point in the surface of a signed distance field.
         */
        public struct SurfacePoint
        {
            public float4 bary;
            public float4 point;
            public float4 normal;
        }

        public interface IDistanceFunction
        {
            void Evaluate(float4 point, float4 radii, quaternion orientation, ref SurfacePoint projectedPoint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetInterpolatedSimplexData(int simplexStart,
                                            int simplexSize,
                                            NativeArray<int> simplices,
                                            NativeArray<float4> positions,
                                            NativeArray<quaternion> orientations,
                                            NativeArray<float4> radii,
                                            float4 convexBary,
                                            out float4 convexPoint,
                                            out float4 convexRadii,
                                            out quaternion convexOrientation)
        {
            convexPoint = float4.zero;
            convexRadii = float4.zero;
            convexOrientation = new quaternion(0, 0, 0, 0);
            for (int j = 0; j < simplexSize; ++j)
            {
                int particle = simplices[simplexStart + j];
                convexPoint += positions[particle] * convexBary[j];
                convexRadii += radii[particle] * convexBary[j];
                convexOrientation.value += orientations[particle].value * convexBary[j];
            }
        }

        public static SurfacePoint Optimize<T>(ref T function,
                                               NativeArray<float4> positions,
                                               NativeArray<quaternion> orientations,
                                               NativeArray<float4> radii,
                                               NativeArray<int> simplices,
                                               int simplexStart,
                                               int simplexSize,
                                               ref float4 convexBary,
                                               out float4 convexPoint,
                                               int maxIterations = 16,
                                               float tolerance = 0.004f) where T : struct, IDistanceFunction
        {
            var pointInFunction = new SurfacePoint();

            // get cartesian coordinates of the initial guess:
            GetInterpolatedSimplexData(simplexStart, simplexSize, simplices, positions, orientations, radii, convexBary, out convexPoint, out float4 convexThickness, out quaternion convexOrientation);

            // for a 0-simplex (point), perform a single evaluation:
            if (simplexSize == 1 || maxIterations < 1)
                function.Evaluate(convexPoint, convexThickness, convexOrientation, ref pointInFunction);

            // for a 1-simplex (edge), perform golden ratio search:
            else if (simplexSize == 2)
                GoldenSearch(ref function, simplexStart, simplexSize, positions, orientations, radii, simplices, ref convexPoint, ref convexThickness, ref convexOrientation, ref convexBary, ref pointInFunction, maxIterations, tolerance * 10);

            // for higher-order simplices, use general Frank-Wolfe convex optimization:
            else
                FrankWolfe(ref function, simplexStart, simplexSize, positions, orientations, radii, simplices, ref convexPoint, ref convexThickness, ref convexOrientation, ref convexBary, ref pointInFunction, maxIterations, tolerance);
            
            return pointInFunction;
        }

        // Frank-Wolfe convex optimization algorithm. Returns closest point to a simplex in a signed distance function.
        private static void FrankWolfe<T>(ref T function,
                                          int simplexStart,
                                          int simplexSize,
                                          NativeArray<float4> positions,
                                          NativeArray<quaternion> orientations,
                                          NativeArray<float4> radii,
                                          NativeArray<int> simplices,
                                          ref float4 convexPoint,
                                          ref float4 convexThickness,
                                          ref quaternion convexOrientation,
                                          ref float4 convexBary,
                                          ref SurfacePoint pointInFunction,
                                          int maxIterations,
                                          float tolerance) where T : struct, IDistanceFunction
        {
            for (int i = 0; i < maxIterations; ++i)
            {
                // sample target function:
                function.Evaluate(convexPoint, convexThickness, convexOrientation, ref pointInFunction);

                // find descent direction:
                int descent = 0;
                float gap = float.MinValue;
                for (int j = 0; j < simplexSize; ++j)
                {
                    int particle = simplices[simplexStart + j];
                    float4 candidate = positions[particle] - convexPoint;

                    // here, we adjust the candidate by projecting it to the engrosed simplex's surface:
                    candidate -= pointInFunction.normal * (radii[particle].x - convexThickness.x);

                    float corr = math.dot(-pointInFunction.normal, candidate);
                    if (corr > gap)
                    {
                        descent = j;
                        gap = corr;
                    }
                }

                // if the duality gap is below tolerance threshold, stop iterating.
                if (gap < tolerance)
                    break;

                // update the barycentric coords using 2/(i+2) as  the step factor
                float step = 0.3f * 2.0f / (i + 2);
                convexBary *= 1 - step;
                convexBary[descent] += step;

                // get cartesian coordinates of current solution:
                GetInterpolatedSimplexData(simplexStart, simplexSize, simplices, positions, orientations, radii, convexBary, out convexPoint, out convexThickness, out convexOrientation);
            }   
        }

        private static void GoldenSearch<T>(ref T function,
                                            int simplexStart,
                                            int simplexSize,
                                            NativeArray<float4> positions,
                                            NativeArray<quaternion> orientations,
                                            NativeArray<float4> radii,
                                            NativeArray<int> simplices,
                                            ref float4 convexPoint,
                                            ref float4 convexThickness,
                                            ref quaternion convexOrientation,
                                            ref float4 convexBary,
                                            ref SurfacePoint pointInFunction,
                                            int maxIterations,
                                            float tolerance) where T : struct, IDistanceFunction
        {
            var pointInFunctionD = new SurfacePoint();
            float4 convexPointD, convexThicknessD;
            quaternion convexOrientationD;

            float gr = (math.sqrt(5.0f) + 1) / 2.0f;
            float u = 0, v = 1;
            float c = v - (v - u) / gr;
            float d = u + (v - u) / gr;

            for (int i = 0; i < maxIterations; ++i)
            {
                // if the gap is below tolerance threshold, stop iterating.
                if (math.abs(v - u) < tolerance * (math.abs(c) + math.abs(d)))
                    break;

                GetInterpolatedSimplexData(simplexStart, simplexSize, simplices, positions, orientations, radii, new float4(c, 1 - c, 0, 0), out convexPoint, out convexThickness, out convexOrientation);
                GetInterpolatedSimplexData(simplexStart, simplexSize, simplices, positions, orientations, radii, new float4(d, 1 - d, 0, 0), out convexPointD, out convexThicknessD, out convexOrientationD);

                function.Evaluate(convexPoint, convexThickness, convexOrientation, ref pointInFunction);
                function.Evaluate(convexPointD, convexThicknessD, convexOrientationD, ref pointInFunctionD);

                float4 candidateC = positions[simplices[simplexStart]] - pointInFunction.point;
                float4 candidateD = positions[simplices[simplexStart + 1]] - pointInFunctionD.point;

                candidateC -= pointInFunction.normal * (radii[simplices[simplexStart]].x - convexThickness.x);
                candidateD -= pointInFunctionD.normal * (radii[simplices[simplexStart + 1]].x - convexThicknessD.x);

                if (math.dot(-pointInFunction.normal, candidateC) < math.dot(-pointInFunctionD.normal, candidateD))
                    v = d;
                else
                    u = c;

                c = v - (v - u) / gr;
                d = u + (v - u) / gr;
            }

            float mid = (v + u) * 0.5f;
            convexBary.x = mid;
            convexBary.y = (1 - mid);
            GetInterpolatedSimplexData(simplexStart, simplexSize, simplices, positions, orientations, radii, convexBary, out convexPoint, out convexThickness, out convexOrientation);
            function.Evaluate(convexPoint, convexThickness, convexOrientation, ref pointInFunction);
        }


    }

}
#endif