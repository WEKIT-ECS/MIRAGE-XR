#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Obi
{
    public static class BurstMath
    {

        public const float epsilon = 0.0000001f;
        public const float zero = 0;
        public const float one = 1;
        public static readonly float golden = (math.sqrt(5.0f) + 1) / 2.0f;

        // multiplies a column vector by a row vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 multrnsp(float4 column, float4 row)
        {
            return new float3x3(column[0] * row[0], column[0] * row[1], column[0] * row[2],
                                column[1] * row[0], column[1] * row[1], column[1] * row[2],
                                column[2] * row[0], column[2] * row[1], column[2] * row[2]);
        }

        // multiplies a column vector by a row vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 multrnsp4(float4 column, float4 row)
        {
            return new float4x4(column[0] * row[0], column[0] * row[1], column[0] * row[2], 0,
                                column[1] * row[0], column[1] * row[1], column[1] * row[2], 0,
                                column[2] * row[0], column[2] * row[1], column[2] * row[2], 0,
                                0, 0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 project(this float4 vector, float4 onto)
        {
            float len = math.lengthsq(onto);
            if (len < epsilon)
                return float4.zero;
            return math.dot(onto, vector) * onto / len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 TransformInertiaTensor(float4 tensor, quaternion rotation)
        {
            float4x4 rotMatrix = rotation.toMatrix();
            return math.mul(rotMatrix, math.mul(tensor.asDiagonal(), math.transpose(rotMatrix)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RotationalInvMass(float4x4 inverseInertiaTensor, float4 point, float4 direction)
        {
            float4 cr = math.mul(inverseInertiaTensor, new float4(math.cross(point.xyz, direction.xyz), 0));
            return math.dot(math.cross(cr.xyz, point.xyz), direction.xyz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 GetParticleVelocityAtPoint(float4 position, float4 prevPosition, float4 point, float dt)
        {
            // no angular velocity, so calculate and return linear velocity only:
            return BurstIntegration.DifferentiateLinear(position, prevPosition, dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 GetParticleVelocityAtPoint(float4 position, float4 prevPosition, quaternion orientation, quaternion prevOrientation, float4 point, float dt)
        {
            // calculate both linear and angular velocities:
            float4 linearVelocity = BurstIntegration.DifferentiateLinear(position, prevPosition, dt);
            float4 angularVelocity = BurstIntegration.DifferentiateAngular(orientation, prevOrientation, dt);
            return linearVelocity + new float4(math.cross(angularVelocity.xyz, (point - prevPosition).xyz), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 GetRigidbodyVelocityAtPoint(int rigidbodyIndex,
                                                         float4 point,
                                                         NativeArray<BurstRigidbody> rigidbodies,
                                                         NativeArray<float4> linearDeltas,
                                                         NativeArray<float4> angularDeltas,
                                                         BurstAffineTransform solverToWorld) 
        {
            float4 linear  = rigidbodies[rigidbodyIndex].velocity + linearDeltas[rigidbodyIndex];
            float4 angular = rigidbodies[rigidbodyIndex].angularVelocity + angularDeltas[rigidbodyIndex];
            float4 r = solverToWorld.TransformPoint(point) - rigidbodies[rigidbodyIndex].com;

            // Point is assumed to be expressed in solver space. Since rigidbodies are expressed in world space, we need to convert the
            // point to world space, and convert the resulting velocity back to solver space.
            return solverToWorld.InverseTransformVector(linear + new float4(math.cross(angular.xyz, r.xyz), 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 GetRigidbodyVelocityAtPoint(int rigidbodyIndex,
                                                         float4 point,
                                                         NativeArray<BurstRigidbody> rigidbodies,
                                                         BurstAffineTransform solverToWorld)
        {
            float4 linear = rigidbodies[rigidbodyIndex].velocity;
            float4 angular = rigidbodies[rigidbodyIndex].angularVelocity;
            float4 r = solverToWorld.TransformPoint(point) - rigidbodies[rigidbodyIndex].com;

            // Point is assumed to be expressed in solver space. Since rigidbodies are expressed in world space, we need to convert the
            // point to world space, and convert the resulting velocity back to solver space.
            return solverToWorld.InverseTransformVector(linear + new float4(math.cross(angular.xyz, r.xyz), 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyImpulse(int rigidbodyIndex,
                                        float4 impulse,
                                        float4 point,
                                        NativeArray<BurstRigidbody> rigidbodies,
                                        NativeArray<float4> linearDeltas,
                                        NativeArray<float4> angularDeltas,
                                        BurstAffineTransform solverToWorld)
        {
            float4 impulseWS = solverToWorld.TransformVector(impulse);
            float4 r = solverToWorld.TransformPoint(point) - rigidbodies[rigidbodyIndex].com;
            linearDeltas[rigidbodyIndex]  += rigidbodies[rigidbodyIndex].inverseMass * impulseWS;
            angularDeltas[rigidbodyIndex] += math.mul(rigidbodies[rigidbodyIndex].inverseInertiaTensor, new float4(math.cross(r.xyz, impulseWS.xyz), 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyDeltaQuaternion(int rigidbodyIndex,
                                                quaternion rotation,
                                                quaternion delta,
                                                NativeArray<float4> angularDeltas,
                                                BurstAffineTransform solverToWorld,
                                                float dt)
        {
            quaternion rotationWS = math.mul(solverToWorld.rotation, rotation);
            quaternion deltaWS = math.mul(solverToWorld.rotation, delta);

            // convert quaternion delta to angular acceleration:
            quaternion newRotation = math.normalize(new quaternion(rotationWS.value + deltaWS.value));
            angularDeltas[rigidbodyIndex] += BurstIntegration.DifferentiateAngular(newRotation, rotationWS, dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OneSidedNormal(float4 forward, ref float4 normal)
        {
            float dot = math.dot(normal.xyz, forward.xyz);
            if (dot < 0) normal -= 2 * dot * forward;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EllipsoidRadius(float4 normSolverDirection, quaternion orientation, float3 radii)
        {
            float3 localDir = math.mul(math.conjugate(orientation), normSolverDirection.xyz);
            float sqrNorm = math.lengthsq(localDir / radii);
            return sqrNorm > epsilon ? math.sqrt(1 / sqrNorm) : radii.x;
        }

        public static quaternion ExtractRotation(float4x4 matrix, quaternion rotation, int iterations)
        {
            float4x4 R;
            for (int i = 0; i < iterations; ++i)
            {
                R = rotation.toMatrix();
                float3 omega = (math.cross(R.c0.xyz, matrix.c0.xyz) + math.cross(R.c1.xyz, matrix.c1.xyz) + math.cross(R.c2.xyz, matrix.c2.xyz)) /
                               (math.abs(math.dot(R.c0.xyz, matrix.c0.xyz) + math.dot(R.c1.xyz, matrix.c1.xyz) + math.dot(R.c2.xyz, matrix.c2.xyz)) + BurstMath.epsilon);

                float w = math.length(omega);
                if (w < BurstMath.epsilon)
                    break;

                rotation = math.normalize(math.mul(quaternion.AxisAngle((1.0f / w) * omega, w), rotation));
            }
            return rotation;
        }

        // decomposes a quaternion in swing and twist around a given axis:
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwingTwist(quaternion q, float3 twistAxis, out quaternion swing, out quaternion twist)
        {
            float dot = math.dot(q.value.xyz, twistAxis);
            float3 p = twistAxis * dot;
            twist = math.normalizesafe(new quaternion(p[0], p[1], p[2], q.value.w));
            swing = math.mul(q, math.conjugate(twist));
        }

        public static float4x4 toMatrix(this quaternion q)
        {
            float xx = q.value.x * q.value.x;
            float xy = q.value.x * q.value.y;
            float xz = q.value.x * q.value.z;
            float xw = q.value.x * q.value.w;

            float yy = q.value.y * q.value.y;
            float yz = q.value.y * q.value.z;
            float yw = q.value.y * q.value.w;

            float zz = q.value.z * q.value.z;
            float zw = q.value.z * q.value.w;

            return new float4x4(1 - 2 * (yy + zz), 2 * (xy - zw), 2 * (xz + yw), 0,
                                2 * (xy + zw), 1 - 2 * (xx + zz), 2 * (yz - xw), 0,
                                2 * (xz - yw), 2 * (yz + xw), 1 - 2 * (xx + yy), 0,
                                0, 0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 asDiagonal(this float4 v)
        {
            return new float4x4(v.x, 0, 0, 0,
                                0, v.y, 0, 0,
                                0, 0, v.z, 0,
                                0, 0, 0, v.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 diagonal(this float4x4 value)
        {
            return new float4(value.c0[0], value.c1[1], value.c2[2], value.c3[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float frobeniusNorm(this float4x4 m)
        {
            return math.sqrt(math.lengthsq(m.c0) + math.lengthsq(m.c1) + math.lengthsq(m.c2) + math.lengthsq(m.c3));
        }

        public static void EigenSolve(float3x3 D, out float3 S, out float3x3 V)
        {
            // D is symmetric
            // S is a vector whose elements are eigenvalues
            // V is a matrix whose columns are eigenvectors
            S = EigenValues(D);
            float3 V0, V1, V2;

            if (S[0] - S[1] > S[1] - S[2])
            {
                V0 = EigenVector(D, S[0]);
                if (S[1] - S[2] < math.FLT_MIN_NORMAL)
                {
                    V2 = V0.unitOrthogonal();
                }
                else
                {
                    V2 = EigenVector(D, S[2]); V2 -= V0 * math.dot(V0, V2); V2 = math.normalize(V2);
                }
                V1 = math.cross(V2, V0);
            }
            else
            {
                V2 = EigenVector(D, S[2]);
                if (S[0] - S[1] < math.FLT_MIN_NORMAL)
                {
                    V1 = V2.unitOrthogonal();
                }
                else
                {
                    V1 = EigenVector(D, S[1]); V1 -= V2 * math.dot(V2, V1); V1 = math.normalize(V1);
                }
                V0 = math.cross(V1, V2);
            }

            V.c0 = V0;
            V.c1 = V1;
            V.c2 = V2;
        }

        static float3 unitOrthogonal(this float3 input)
        {
            // Find a vector to cross() the input with.
            if (!(input.x < input.z * epsilon)
            || !(input.y < input.z * epsilon))
            {
                float invnm = 1 / math.length(input.xy);
                return new float3(-input.y * invnm, input.x * invnm, 0);
            }
            else
            {
                float invnm = 1 / math.length(input.yz);
                return new float3(0, -input.z * invnm, input.y * invnm);
            }
        }

        // D is symmetric, S is an eigen value
        static float3 EigenVector(float3x3 D, float S)
        {
            // Compute a cofactor matrix of D - sI.
            float3 c0 = D.c0; c0[0] -= S;
            float3 c1 = D.c1; c1[1] -= S;
            float3 c2 = D.c2; c2[2] -= S;

            // Upper triangular matrix
            float3 c0p = new float3(c1[1] * c2[2] - c2[1] * c2[1], 0, 0);
            float3 c1p = new float3(c2[1] * c2[0] - c1[0] * c2[2], c0[0] * c2[2] - c2[0] * c2[0], 0);
            float3 c2p = new float3(c1[0] * c2[1] - c1[1] * c2[0], c1[0] * c2[0] - c0[0] * c2[1], c0[0] * c1[1] - c1[0] * c1[0]);

            // Get a column vector with a largest norm (non-zero).
            float C01s = c1p[0] * c1p[0];
            float C02s = c2p[0] * c2p[0];
            float C12s = c2p[1] * c2p[1];
            float3 norm = new float3(c0p[0] * c0p[0] + C01s + C02s,
                                     C01s + c1p[1] * c1p[1] + C12s,
                                     C02s + C12s + c2p[2] * c2p[2]);

            // index of largest:
            int index = 0;
            if (norm[0] > norm[1] && norm[0] > norm[2])
                index = 0;
            else if (norm[1] > norm[0] && norm[1] > norm[2])
                index = 1;
            else
                index = 2;

            float3 V = float3.zero;

            // special case
            if (norm[index] < math.FLT_MIN_NORMAL)
            {
                V[0] = 1; return V;
            }
            else if (index == 0)
            {
                V[0] = c0p[0]; V[1] = c1p[0]; V[2] = c2p[0];
            }
            else if (index == 1)
            {
                V[0] = c1p[0]; V[1] = c1p[1]; V[2] = c2p[1];
            }
            else
            {
                V = c2p;
            }
            return math.normalize(V);
        }

        static float3 EigenValues(float3x3 D)
        {
            float one_third = 1 / 3.0f;
            float one_sixth = 1 / 6.0f;
            float three_sqrt = math.sqrt(3.0f);

            float3 c0 = D.c0;
            float3 c1 = D.c1;
            float3 c2 = D.c2;

            float m = one_third * (c0[0] + c1[1] + c2[2]);

            // K is D - I*diag(S)
            float K00 = c0[0] - m;
            float K11 = c1[1] - m;
            float K22 = c2[2] - m;

            float K01s = c1[0] * c1[0];
            float K02s = c2[0] * c2[0];
            float K12s = c2[1] * c2[1];

            float q = 0.5f * (K00 * (K11 * K22 - K12s) - K22 * K01s - K11 * K02s) + c1[0] * c2[1] * c0[2];
            float p = one_sixth * (K00 * K00 + K11 * K11 + K22 * K22 + 2 * (K01s + K02s + K12s));

            float p_sqrt = math.sqrt(p);

            float tmp = p * p * p - q * q;
            float phi = one_third * math.atan2(math.sqrt(math.max(0, tmp)), q);
            float phi_c = math.cos(phi);
            float phi_s = math.sin(phi);
            float sqrt_p_c_phi = p_sqrt * phi_c;
            float sqrt_p_3_s_phi = p_sqrt * three_sqrt * phi_s;

            float e0 = m + 2 * sqrt_p_c_phi;
            float e1 = m - sqrt_p_c_phi - sqrt_p_3_s_phi;
            float e2 = m - sqrt_p_c_phi + sqrt_p_3_s_phi;

            float aux;
            if (e0 > e1)
            {
                aux = e0;
                e0 = e1;
                e1 = aux;
            }
            if (e0 > e2)
            {
                aux = e0;
                e0 = e2;
                e2 = aux;
            }
            if (e1 > e2)
            {
                aux = e1;
                e1 = e2;
                e2 = aux;
            }

            return new float3(e2, e1, e0);
        }

        public struct CachedTri
        {
            public float4 vertex;
            public float4 edge0;
            public float4 edge1;
            public float4 data;

            public void Cache(float4 v1,
                              float4 v2,
                              float4 v3)
            {
                vertex = v1;
                edge0 = v2 - v1;
                edge1 = v3 - v1;
                data = float4.zero;
                data[0] = math.dot(edge0, edge0);
                data[1] = math.dot(edge0, edge1);
                data[2] = math.dot(edge1, edge1);
                data[3] = data[0] * data[2] - data[1] * data[1];
            }
        }

        public static float4 NearestPointOnTri(in CachedTri tri,
                                               float4 p,
                                               out float4 bary)
        {
            float4 v0 = tri.vertex - p;
            float b0 = math.dot(tri.edge0, v0);
            float b1 = math.dot(tri.edge1, v0);
            float t0 = tri.data[1] * b1 - tri.data[2] * b0;
            float t1 = tri.data[1] * b0 - tri.data[0] * b1;

            if (t0 + t1 <= tri.data[3])
            {
                if (t0 < zero)
                {
                    if (t1 < zero)  // region 4
                    {
                        if (b0 < zero)
                        {
                            t1 = zero;
                            if (-b0 >= tri.data[0])  // V0
                                t0 = one;
                            else  // E01
                                t0 = -b0 / tri.data[0];
                        }
                        else
                        {
                            t0 = zero;
                            if (b1 >= zero)  // V0
                                t1 = zero;
                            else if (-b1 >= tri.data[2])  // V2
                                t1 = one;
                            else  // E20
                                t1 = -b1 / tri.data[2];
                        }
                    }
                    else  // region 3
                    {
                        t0 = zero;
                        if (b1 >= zero)  // V0
                            t1 = zero;
                        else if (-b1 >= tri.data[2])  // V2
                            t1 = one;
                        else  // E20
                            t1 = -b1 / tri.data[2];
                    }
                }
                else if (t1 < zero)  // region 5
                {
                    t1 = zero;
                    if (b0 >= zero)  // V0
                        t0 = zero;
                    else if (-b0 >= tri.data[0])  // V1
                        t0 = one;
                    else  // E01
                        t0 = -b0 / tri.data[0];
                }
                else  // region 0, interior
                {
                    float invDet = one / tri.data[3];
                    t0 *= invDet;
                    t1 *= invDet;
                }
            }
            else
            {
                float tmp0, tmp1, numer, denom;

                if (t0 < zero)  // region 2
                {
                    tmp0 = tri.data[1] + b0;
                    tmp1 = tri.data[2] + b1;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = tri.data[0] - 2 * tri.data[1] + tri.data[2];
                        if (numer >= denom)  // V1
                        {
                            t0 = one;
                            t1 = zero;
                        }
                        else  // E12
                        {
                            t0 = numer / denom;
                            t1 = one - t0;
                        }
                    }
                    else
                    {
                        t0 = zero;
                        if (tmp1 <= zero)  // V2
                            t1 = one;
                        else if (b1 >= zero)  // V0
                            t1 = zero;
                        else  // E20
                            t1 = -b1 / tri.data[2];
                    }
                }
                else if (t1 < zero)  // region 6
                {
                    tmp0 = tri.data[1] + b1;
                    tmp1 = tri.data[0] + b0;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = tri.data[0] - 2 * tri.data[1] + tri.data[2];
                        if (numer >= denom)  // V2
                        {
                            t1 = one;
                            t0 = zero;
                        }
                        else  // E12
                        {
                            t1 = numer / denom;
                            t0 = one - t1;
                        }
                    }
                    else
                    {
                        t1 = zero;
                        if (tmp1 <= zero)  // V1
                            t0 = one;
                        else if (b0 >= zero)  // V0
                            t0 = zero;
                        else  // E01
                            t0 = -b0 / tri.data[0];
                    }
                }
                else  // region 1
                {
                    numer = tri.data[2] + b1 - tri.data[1] - b0;
                    if (numer <= zero)  // V2
                    {
                        t0 = zero;
                        t1 = one;
                    }
                    else
                    {
                        denom = tri.data[0] - 2 * tri.data[1] + tri.data[2];
                        if (numer >= denom)  // V1
                        {
                            t0 = one;
                            t1 = zero;
                        }
                        else  // 12
                        {
                            t0 = numer / denom;
                            t1 = one - t0;
                        }
                    }
                }
            }

            bary = new float4(1 - (t0 + t1), t0, t1,0);
            return tri.vertex + t0 * tri.edge0 + t1 * tri.edge1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 NearestPointOnEdge(float4 a, float4 b, float4 p, out float mu, bool clampToSegment = true)
        {
            float4 ap = p - a;
            float4 ab = b - a;

            mu = math.dot(ap, ab) / math.dot(ab, ab);

            if (clampToSegment)
                mu = math.saturate(mu);

            return a + ab * mu;
        }

        public static float4 NearestPointsTwoEdges(float4 a, float4 b, float4 c, float4 d, out float mu1, out float mu2)
        {
            float4 dc = d - c;
            float lineDirSqrMag = math.dot(dc, dc);
            float4 inPlaneA = a - (math.dot(a - c, dc) / lineDirSqrMag * dc);
            float4 inPlaneB = b - (math.dot(b - c, dc) / lineDirSqrMag * dc);

            float4 inPlaneBA = inPlaneB - inPlaneA;
            float t = math.dot(c - inPlaneA, inPlaneBA) / math.dot(inPlaneBA, inPlaneBA);

            //t = (inPlaneA != inPlaneB) ? t : 0f; // Zero's t if parallel
            float4 segABtoLineCD = math.lerp(a, b, math.saturate(t));

            float4 segCDtoSegAB = NearestPointOnEdge(c, d, segABtoLineCD, out mu1);
            float4 segABtoSegCD = NearestPointOnEdge(a, b, segCDtoSegAB, out mu2);

            return segCDtoSegAB;
        }

        public static float4 BaryCoords(in float4 A,
                                        in float4 B,
                                        in float4 C,
                                        in float4 P)
        {

            // Compute vectors
            float4 v0 = C - A;
            float4 v1 = B - A;
            float4 v2 = P - A;

            // Compute dot products
            float dot00 = math.dot(v0, v0);
            float dot01 = math.dot(v0, v1);
            float dot02 = math.dot(v0, v2);
            float dot11 = math.dot(v1, v1);
            float dot12 = math.dot(v1, v2);

            // Compute barycentric coordinates
            float det = dot00 * dot11 - dot01 * dot01;
            if (math.abs(det) > epsilon)
            {
                float u = (dot11 * dot02 - dot01 * dot12) / det;
                float v = (dot00 * dot12 - dot01 * dot02) / det;
                return new float4(1 - u - v, v, u, 0);
            }
            return float4.zero;

        }

        public static float4 BaryCoords2(in float4 A,
                                         in float4 B,
                                         in float4 P)
        {
            float4 v0 = P - A;
            float4 v1 = B - A;
            float y = math.sqrt(math.dot(v0, v0) / (math.dot(v1, v1) + epsilon));
            return new float4(1 - y, y, 0, 0);
        }

        public static float4 BaryIntrpl(in float4 p1, in float4 p2, in float4 p3, in float4 coords)
        {
            return coords[0] * p1 + coords[1] * p2 + coords[2] * p3;
        }

        public static float4 BaryIntrpl(in float4 p1, in float4 p2, in float4 coords)
        {
            return coords[0] * p1 + coords[1] * p2;
        }

        public static float BaryIntrpl(float p1, float p2, float p3, float4 coords)
        {
            return coords[0] * p1 + coords[1] * p2 + coords[2] * p3;
        }

        public static float BaryIntrpl(float p1, float p2, float4 coords)
        {
            return coords[0] * p1 + coords[1] * p2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BaryScale(float4 coords)
        {
            return 1.0f / math.dot(coords, coords);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 BarycenterForSimplexOfSize(int simplexSize)
        {
            float value = 1f / simplexSize;
            float4 center = float4.zero;
            for (int i = 0; i < simplexSize; ++i)
                center[i] = value;
            return center;
        }

        public static unsafe void RemoveRangeBurst<T>(this NativeList<T> list, int index, int count)
            where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)index >= (uint)list.Length)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range in NativeList of '{list.Length}' Length.");
            }
#endif

            int elemSize = UnsafeUtility.SizeOf<T>();
            byte* basePtr = (byte*)list.GetUnsafePtr();

            UnsafeUtility.MemMove(basePtr + (index * elemSize), basePtr + ((index + count) * elemSize), elemSize * (list.Length - count - index));

            // No easy way to change length so we just loop this unfortunately.
            for (var i = 0; i < count; i++)
            {
                list.RemoveAtSwapBack(list.Length - 1);
            }
        }
    }
}
#endif