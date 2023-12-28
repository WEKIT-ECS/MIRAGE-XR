#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Mathematics;
using Unity.Collections;

namespace Obi
{
    public struct BurstContact : IConstraint, System.IComparable<BurstContact>
    {
        public float4 pointA; // point A, expressed as simplex barycentric coords for simplices, as a solver-space position for colliders.
        public float4 pointB; // point B, expressed as simplex barycentric coords for simplices, as a solver-space position for colliders.

        public float4 normal;
        public float4 tangent;
        public float4 bitangent;

        public float distance;

        float normalLambda;
        float tangentLambda;
        float bitangentLambda;
        float stickLambda;
        float rollingFrictionImpulse;

        public int bodyA;
        public int bodyB;

        public float normalInvMassA;
        public float tangentInvMassA;
        public float bitangentInvMassA;

        public float normalInvMassB;
        public float tangentInvMassB;
        public float bitangentInvMassB;

        public double pad0; // padding to ensure correct alignment to 128 bytes.

        public int GetParticleCount() { return 2; }
        public int GetParticle(int index) { return index == 0 ? bodyA : bodyB; }

        public override string ToString()
        {
            return bodyA + "," + bodyB;
        }

        public int CompareTo(BurstContact other)
        {
            int first = bodyA.CompareTo(other.bodyA);
            if (first == 0)
                return bodyB.CompareTo(other.bodyB);
            return first;
        }

        public float TotalNormalInvMass
        {
            get { return normalInvMassA + normalInvMassB; }
        }

        public float TotalTangentInvMass
        {
            get { return tangentInvMassA + tangentInvMassB; }
        }

        public float TotalBitangentInvMass
        {
            get { return bitangentInvMassA + bitangentInvMassB; }
        }

        public void CalculateBasis(float4 relativeVelocity)
        {
            tangent = math.normalizesafe(relativeVelocity - math.dot(relativeVelocity, normal) * normal);
            bitangent = math.normalizesafe(new float4(math.cross(normal.xyz, tangent.xyz),0));
        }

        public void CalculateContactMassesA(float invMass,
                                            float4 inverseInertiaTensor,
                                            float4 position,
                                            quaternion orientation,
                                            float4 contactPoint,
                                            bool rollingContacts)
        {
            // initialize inverse linear masses:
            normalInvMassA = tangentInvMassA = bitangentInvMassA = invMass;

            if (rollingContacts)
            {
                float4 rA = contactPoint - position;
                float4x4 solverInertiaA = BurstMath.TransformInertiaTensor(inverseInertiaTensor, orientation);

                normalInvMassA += BurstMath.RotationalInvMass(solverInertiaA, rA, normal);
                tangentInvMassA += BurstMath.RotationalInvMass(solverInertiaA, rA, tangent);
                bitangentInvMassA += BurstMath.RotationalInvMass(solverInertiaA, rA, bitangent);
            }
        }

        public void CalculateContactMassesB(float invMass,
                                            float4 inverseInertiaTensor,
                                            float4 position,
                                            quaternion orientation,
                                            float4 contactPoint,
                                            bool rollingContacts)
        {
            // initialize inverse linear masses:
            normalInvMassB = tangentInvMassB = bitangentInvMassB = invMass;

            if (rollingContacts)
            {
                float4 rB = contactPoint - position;
                float4x4 solverInertiaB = BurstMath.TransformInertiaTensor(inverseInertiaTensor, orientation);

                normalInvMassB += BurstMath.RotationalInvMass(solverInertiaB, rB, normal);
                tangentInvMassB += BurstMath.RotationalInvMass(solverInertiaB, rB, tangent);
                bitangentInvMassB += BurstMath.RotationalInvMass(solverInertiaB, rB, bitangent);
            }
        }


        public void CalculateContactMassesB(in BurstRigidbody rigidbody, in BurstAffineTransform solver2World)
        {
            float4 rB = solver2World.TransformPoint(pointB) - rigidbody.com;

            // initialize inverse linear masses:
            normalInvMassB = tangentInvMassB = bitangentInvMassB = rigidbody.inverseMass;
            normalInvMassB += BurstMath.RotationalInvMass(rigidbody.inverseInertiaTensor, rB, normal);
            tangentInvMassB += BurstMath.RotationalInvMass(rigidbody.inverseInertiaTensor, rB, tangent);
            bitangentInvMassB += BurstMath.RotationalInvMass(rigidbody.inverseInertiaTensor, rB, bitangent);
        }

        public float SolveAdhesion(float4 posA, float4 posB, float stickDistance, float stickiness, float dt)
        {

            if (TotalNormalInvMass <= 0 || stickDistance <= 0 || stickiness <= 0 || dt <= 0)
                return 0;

            distance = math.dot(posA - posB, normal);

            // calculate stickiness position correction:
            float constraint = stickiness * (1 - math.max(distance / stickDistance, 0)) * dt;

            // calculate lambda multiplier:
            float dlambda = -constraint / TotalNormalInvMass;

            // accumulate lambda:
            float newStickinessLambda = math.min(stickLambda + dlambda, 0);

            // calculate lambda change and update accumulated lambda:
            float lambdaChange = newStickinessLambda - stickLambda;
            stickLambda = newStickinessLambda;

            return lambdaChange;
        }

        public float SolvePenetration(float4 posA, float4 posB, float maxDepenetrationDelta)
        {

            if (TotalNormalInvMass <= 0)
                return 0;

            //project position delta to normal vector:
            distance = math.dot(posA - posB, normal);

            // calculate max projection distance based on depenetration velocity:
            float maxProjection = math.max(-distance - maxDepenetrationDelta, 0);

            // calculate lambda multiplier:
            float dlambda = -(distance + maxProjection) / TotalNormalInvMass;

            // accumulate lambda:
            float newLambda = math.max(normalLambda + dlambda, 0);

            // calculate lambda change and update accumulated lambda:
            float lambdaChange = newLambda - normalLambda;
            normalLambda = newLambda;

            return lambdaChange;
        }

        public float2 SolveFriction(float4 relativeVelocity, float staticFriction, float dynamicFriction, float dt)
        {
            float2 lambdaChange = float2.zero;

            if (TotalTangentInvMass <= 0 || TotalBitangentInvMass <= 0 ||
                (dynamicFriction <= 0 && staticFriction <= 0) || (normalLambda <= 0 && stickLambda <= 0))
                return lambdaChange;

            // calculate delta projection on both friction axis:
            float tangentPosDelta = math.dot(relativeVelocity, tangent);
            float bitangentPosDelta = math.dot(relativeVelocity, bitangent);

            // calculate friction pyramid limit:
            float dynamicFrictionCone = normalLambda / dt * dynamicFriction;
            float staticFrictionCone  = normalLambda / dt * staticFriction;

            // tangent impulse:
            float tangentLambdaDelta = -tangentPosDelta / TotalTangentInvMass; 
            float newTangentLambda = tangentLambda + tangentLambdaDelta;

            if (math.abs(newTangentLambda) > staticFrictionCone)
                newTangentLambda = math.clamp(newTangentLambda, -dynamicFrictionCone, dynamicFrictionCone);

            lambdaChange[0] = newTangentLambda - tangentLambda;
            tangentLambda = newTangentLambda;

            // bitangent impulse:
            float bitangentLambdaDelta = -bitangentPosDelta / TotalBitangentInvMass;
            float newBitangentLambda = bitangentLambda + bitangentLambdaDelta;

            if (math.abs(newBitangentLambda) > staticFrictionCone)
                newBitangentLambda = math.clamp(newBitangentLambda, -dynamicFrictionCone, dynamicFrictionCone);

            lambdaChange[1] = newBitangentLambda - bitangentLambda;
            bitangentLambda = newBitangentLambda;

            return lambdaChange;
        }


        public float SolveRollingFriction(float4 angularVelocityA,
                                          float4 angularVelocityB,
                                          float rollingFriction,
                                          float invMassA,
                                          float invMassB,
                                          ref float4 rolling_axis)
        {
            float totalInvMass = invMassA + invMassB;
            if (totalInvMass <= 0)
                return 0;
        
            rolling_axis = math.normalizesafe(angularVelocityA - angularVelocityB);

            float vel1 = math.dot(angularVelocityA,rolling_axis);
            float vel2 = math.dot(angularVelocityB,rolling_axis);

            float relativeVelocity = vel1 - vel2;

            float maxImpulse = normalLambda * rollingFriction;
            float newRollingImpulse = math.clamp(rollingFrictionImpulse - relativeVelocity / totalInvMass, -maxImpulse, maxImpulse);
            float rolling_impulse_change = newRollingImpulse - rollingFrictionImpulse;
            rollingFrictionImpulse = newRollingImpulse;
        
            return rolling_impulse_change;
        }
}
}
#endif