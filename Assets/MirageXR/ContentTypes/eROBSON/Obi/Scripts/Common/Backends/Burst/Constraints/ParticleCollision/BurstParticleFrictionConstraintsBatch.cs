#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections;

namespace Obi
{
    public class BurstParticleFrictionConstraintsBatch : BurstConstraintsBatchImpl, IParticleFrictionConstraintsBatchImpl
    {
        public BatchData batchData;

        public BurstParticleFrictionConstraintsBatch(BurstParticleFrictionConstraints constraints)
        {
            m_Constraints = constraints;
            m_ConstraintType = Oni.ConstraintType.ParticleFriction;
        }

        public BurstParticleFrictionConstraintsBatch(BatchData batchData) : base()
        {
            this.batchData = batchData;
        }

        public override JobHandle Initialize(JobHandle inputDeps, float substepTime)
        {
            return inputDeps;
        }

        public override JobHandle Evaluate(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            if (!((BurstSolverImpl)constraints.solver).particleContacts.IsCreated)
                return inputDeps;

            var projectConstraints = new ParticleFrictionConstraintsBatchJob()
            {
                positions = solverImplementation.positions,
                prevPositions = solverImplementation.prevPositions,
                orientations = solverImplementation.orientations,
                prevOrientations = solverImplementation.prevOrientations,

                invMasses = solverImplementation.invMasses,
                invInertiaTensors = solverImplementation.invInertiaTensors,
                radii = solverImplementation.principalRadii,
                particleMaterialIndices = solverImplementation.collisionMaterials,
                collisionMaterials = ObiColliderWorld.GetInstance().collisionMaterials.AsNativeArray<BurstCollisionMaterial>(),

                simplices = solverImplementation.simplices,
                simplexCounts = solverImplementation.simplexCounts,

                deltas = solverImplementation.positionDeltas,
                counts = solverImplementation.positionConstraintCounts,
                orientationDeltas = solverImplementation.orientationDeltas,
                orientationCounts = solverImplementation.orientationConstraintCounts,
                contacts = ((BurstSolverImpl)constraints.solver).particleContacts,

                batchData = batchData,
                substepTime = substepTime,
            };

            int batchCount = batchData.isLast ? batchData.workItemCount : 1;
            return projectConstraints.Schedule(batchData.workItemCount, batchCount, inputDeps);
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
            if (!((BurstSolverImpl)constraints.solver).particleContacts.IsCreated)
                return inputDeps;

            var parameters = solverAbstraction.GetConstraintParameters(m_ConstraintType);

            var applyConstraints = new ApplyBatchedCollisionConstraintsBatchJob()
            {
                contacts = ((BurstSolverImpl)constraints.solver).particleContacts,

                simplices = solverImplementation.simplices,
                simplexCounts = solverImplementation.simplexCounts,

                positions = solverImplementation.positions,
                deltas = solverImplementation.positionDeltas,
                counts = solverImplementation.positionConstraintCounts,
                orientations = solverImplementation.orientations,
                orientationDeltas = solverImplementation.orientationDeltas,
                orientationCounts = solverImplementation.orientationConstraintCounts,
                constraintParameters = parameters,
                batchData = batchData
            };

            int batchCount = batchData.isLast ? batchData.workItemCount : 1;
            return applyConstraints.Schedule(batchData.workItemCount, batchCount, inputDeps);
        }

        [BurstCompile]
        public struct ParticleFrictionConstraintsBatchJob : IJobParallelFor
        {

            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float4> prevPositions;
            [ReadOnly] public NativeArray<quaternion> orientations;
            [ReadOnly] public NativeArray<quaternion> prevOrientations;

            [ReadOnly] public NativeArray<float> invMasses;
            [ReadOnly] public NativeArray<float4> invInertiaTensors;
            [ReadOnly] public NativeArray<float4> radii;
            [ReadOnly] public NativeArray<int> particleMaterialIndices;
            [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;

            // simplex arrays:
            [ReadOnly] public NativeList<int> simplices;
            [ReadOnly] public SimplexCounts simplexCounts;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> counts;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<quaternion> orientationDeltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> orientationCounts;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<BurstContact> contacts;

            [ReadOnly] public BatchData batchData;
            [ReadOnly] public float substepTime;

            public void Execute(int workItemIndex)
            {
                int start, end;
                batchData.GetConstraintRange(workItemIndex, out start, out end);

                for (int i = start; i < end; ++i)
                {
                    var contact = contacts[i];

                    int simplexStartA = simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSizeA);
                    int simplexStartB = simplexCounts.GetSimplexStartAndSize(contact.bodyB, out int simplexSizeB);

                    // Combine collision materials:
                    BurstCollisionMaterial material = CombineCollisionMaterials(simplices[simplexStartA], simplices[simplexStartB]);

                    float4 prevPositionA = float4.zero;
                    float4 linearVelocityA = float4.zero;
                    float4 angularVelocityA = float4.zero;
                    float4 invInertiaTensorA = float4.zero;
                    quaternion orientationA = new quaternion(0, 0, 0, 0);
                    float simplexRadiusA = 0;

                    float4 prevPositionB = float4.zero;
                    float4 linearVelocityB = float4.zero;
                    float4 angularVelocityB = float4.zero;
                    float4 invInertiaTensorB = float4.zero;
                    quaternion orientationB = new quaternion(0, 0, 0, 0);
                    float simplexRadiusB = 0;

                    for (int j = 0; j < simplexSizeA; ++j)
                    {
                        int particleIndex = simplices[simplexStartA + j];
                        prevPositionA += prevPositions[particleIndex] * contact.pointA[j];
                        linearVelocityA += BurstIntegration.DifferentiateLinear(positions[particleIndex], prevPositions[particleIndex], substepTime) * contact.pointA[j];
                        angularVelocityA += BurstIntegration.DifferentiateAngular(orientations[particleIndex], prevOrientations[particleIndex], substepTime) * contact.pointA[j];
                        invInertiaTensorA += invInertiaTensors[particleIndex] * contact.pointA[j];
                        orientationA.value += orientations[particleIndex].value * contact.pointA[j];
                        simplexRadiusA += BurstMath.EllipsoidRadius(contact.normal, prevOrientations[particleIndex], radii[particleIndex].xyz) * contact.pointA[j];
                    }
                    for (int j = 0; j < simplexSizeB; ++j)
                    {
                        int particleIndex = simplices[simplexStartB + j];
                        prevPositionB += prevPositions[particleIndex] * contact.pointB[j];
                        linearVelocityB += BurstIntegration.DifferentiateLinear(positions[particleIndex], prevPositions[particleIndex], substepTime) * contact.pointB[j];
                        angularVelocityB += BurstIntegration.DifferentiateAngular(orientations[particleIndex], prevOrientations[particleIndex], substepTime) * contact.pointB[j];
                        invInertiaTensorB += invInertiaTensors[particleIndex] * contact.pointB[j];
                        orientationB.value += orientations[particleIndex].value * contact.pointB[j];
                        simplexRadiusB += BurstMath.EllipsoidRadius(contact.normal, prevOrientations[particleIndex], radii[particleIndex].xyz) * contact.pointB[j];
                    }

                    float4 rA = float4.zero, rB = float4.zero;

                    // Consider angular velocities if rolling contacts are enabled:
                    if (material.rollingContacts > 0)
                    {
                        rA = -contact.normal * simplexRadiusA;
                        rB = contact.normal * simplexRadiusB;

                        linearVelocityA += new float4(math.cross(angularVelocityA.xyz, rA.xyz), 0);
                        linearVelocityB += new float4(math.cross(angularVelocityB.xyz, rB.xyz), 0);
                    }

                    // Calculate relative velocity:
                    float4 relativeVelocity = linearVelocityA - linearVelocityB;

                    // Calculate friction impulses (in the tangent and bitangent ddirections):
                    float2 impulses = contact.SolveFriction(relativeVelocity, material.staticFriction, material.dynamicFriction, substepTime);

                    // Apply friction impulses to both particles:
                    if (math.abs(impulses.x) > BurstMath.epsilon || math.abs(impulses.y) > BurstMath.epsilon)
                    {
                        float4 tangentImpulse = impulses.x * contact.tangent;
                        float4 bitangentImpulse = impulses.y * contact.bitangent;
                        float4 totalImpulse = tangentImpulse + bitangentImpulse;

                        float baryScale = BurstMath.BaryScale(contact.pointA);
                        for (int j = 0; j < simplexSizeA; ++j)
                        {
                            int particleIndex = simplices[simplexStartA + j];
                            deltas[particleIndex] += (tangentImpulse * contact.tangentInvMassA + bitangentImpulse * contact.bitangentInvMassA) * substepTime * contact.pointA[j] * baryScale;
                            counts[particleIndex]++;
                        }

                        baryScale = BurstMath.BaryScale(contact.pointB);
                        for (int j = 0; j < simplexSizeB; ++j)
                        {
                            int particleIndex = simplices[simplexStartB + j];
                            deltas[particleIndex] -= (tangentImpulse * contact.tangentInvMassB + bitangentImpulse * contact.bitangentInvMassB) * substepTime * contact.pointB[j] * baryScale;
                            counts[particleIndex]++;
                        }

                        // Rolling contacts:
                        if (material.rollingContacts > 0)
                        {
                            // Calculate angular velocity deltas due to friction impulse:
                            float4x4 solverInertiaA = BurstMath.TransformInertiaTensor(invInertiaTensorA, orientationA);
                            float4x4 solverInertiaB = BurstMath.TransformInertiaTensor(invInertiaTensorB, orientationB);

                            float4 angVelDeltaA = math.mul(solverInertiaA, new float4(math.cross(rA.xyz, totalImpulse.xyz), 0));
                            float4 angVelDeltaB = -math.mul(solverInertiaB, new float4(math.cross(rB.xyz, totalImpulse.xyz), 0));

                            // Final angular velocities, after adding the deltas:
                            angularVelocityA += angVelDeltaA;
                            angularVelocityB += angVelDeltaB;

                            // Calculate weights (inverse masses):
                            float invMassA = math.length(math.mul(solverInertiaA, math.normalizesafe(angularVelocityA)));
                            float invMassB = math.length(math.mul(solverInertiaB, math.normalizesafe(angularVelocityB)));

                            // Calculate rolling axis and angular velocity deltas:
                            float4 rollAxis = float4.zero;
                            float rollingImpulse = contact.SolveRollingFriction(angularVelocityA, angularVelocityB, material.rollingFriction, invMassA, invMassB, ref rollAxis);
                            angVelDeltaA += rollAxis * rollingImpulse * invMassA;
                            angVelDeltaB -= rollAxis * rollingImpulse * invMassB;

                            // Apply orientation deltas to particles:
                            quaternion orientationDeltaA = BurstIntegration.AngularVelocityToSpinQuaternion(orientationA, angVelDeltaA, substepTime);
                            quaternion orientationDeltaB = BurstIntegration.AngularVelocityToSpinQuaternion(orientationB, angVelDeltaB, substepTime);

                            for (int j = 0; j < simplexSizeA; ++j)
                            {
                                int particleIndex = simplices[simplexStartA + j];
                                quaternion qA = orientationDeltas[particleIndex];
                                qA.value += orientationDeltaA.value;
                                orientationDeltas[particleIndex] = qA;
                                orientationCounts[particleIndex]++;
                            }

                            for (int j = 0; j < simplexSizeB; ++j)
                            {
                                int particleIndex = simplices[simplexStartB+ j];
                                quaternion qB = orientationDeltas[particleIndex];
                                qB.value += orientationDeltaB.value;
                                orientationDeltas[particleIndex] = qB;
                                orientationCounts[particleIndex]++;
                            }
                        }
                    }

                    contacts[i] = contact;
                }
            }

            private BurstCollisionMaterial CombineCollisionMaterials(int entityA, int entityB)
            {
                // Combine collision materials:
                int aMaterialIndex = particleMaterialIndices[entityA];
                int bMaterialIndex = particleMaterialIndices[entityB];

                if (aMaterialIndex >= 0 && bMaterialIndex >= 0)
                    return BurstCollisionMaterial.CombineWith(collisionMaterials[aMaterialIndex], collisionMaterials[bMaterialIndex]);
                else if (aMaterialIndex >= 0)
                    return collisionMaterials[aMaterialIndex];
                else if (bMaterialIndex >= 0)
                    return collisionMaterials[bMaterialIndex];

                return new BurstCollisionMaterial();
            }
        }


    }
}
#endif