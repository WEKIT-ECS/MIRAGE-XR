#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;

namespace Obi
{
    public class BurstColliderFrictionConstraintsBatch : BurstConstraintsBatchImpl, IColliderFrictionConstraintsBatchImpl
    {
        public BurstColliderFrictionConstraintsBatch(BurstColliderFrictionConstraints constraints)
        {
            m_Constraints = constraints;
            m_ConstraintType = Oni.ConstraintType.Friction;
        }

        public override JobHandle Initialize(JobHandle inputDeps, float substepTime)
        {
            return inputDeps;
        }

        public override JobHandle Evaluate(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            if (!((BurstSolverImpl)constraints.solver).colliderContacts.IsCreated)
                return inputDeps;

            var projectConstraints = new FrictionConstraintsBatchJob()
            {
                positions = solverImplementation.positions,
                prevPositions = solverImplementation.prevPositions,
                orientations = solverImplementation.orientations,
                prevOrientations = solverImplementation.prevOrientations,

                invMasses = solverImplementation.invMasses,
                invInertiaTensors = solverImplementation.invInertiaTensors,
                radii = solverImplementation.principalRadii,
                particleMaterialIndices = solverImplementation.collisionMaterials,

                simplices = solverImplementation.simplices,
                simplexCounts = solverImplementation.simplexCounts,

                shapes = ObiColliderWorld.GetInstance().colliderShapes.AsNativeArray<BurstColliderShape>(),
                transforms = ObiColliderWorld.GetInstance().colliderTransforms.AsNativeArray<BurstAffineTransform>(),
                collisionMaterials = ObiColliderWorld.GetInstance().collisionMaterials.AsNativeArray<BurstCollisionMaterial>(),
                rigidbodies = ObiColliderWorld.GetInstance().rigidbodies.AsNativeArray<BurstRigidbody>(),
                rigidbodyLinearDeltas = solverImplementation.abstraction.rigidbodyLinearDeltas.AsNativeArray<float4>(),
                rigidbodyAngularDeltas = solverImplementation.abstraction.rigidbodyAngularDeltas.AsNativeArray<float4>(),

                deltas = solverImplementation.positionDeltas,
                counts = solverImplementation.positionConstraintCounts,
                orientationDeltas = solverImplementation.orientationDeltas,
                orientationCounts = solverImplementation.orientationConstraintCounts,

                contacts = ((BurstSolverImpl)constraints.solver).colliderContacts,
                inertialFrame = ((BurstSolverImpl)constraints.solver).inertialFrame,
                substeps = substeps,
                stepTime = stepTime,
                substepTime = substepTime
            };
            return projectConstraints.Schedule(inputDeps);
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
            if (!((BurstSolverImpl)constraints.solver).colliderContacts.IsCreated)
                return inputDeps;

            var parameters = solverAbstraction.GetConstraintParameters(m_ConstraintType);

            var applyConstraints = new ApplyCollisionConstraintsBatchJob()
            {
                contacts = ((BurstSolverImpl)constraints.solver).colliderContacts,

                simplices = solverImplementation.simplices,
                simplexCounts = solverImplementation.simplexCounts,

                positions = solverImplementation.positions,
                deltas = solverImplementation.positionDeltas,
                counts = solverImplementation.positionConstraintCounts,
                orientations = solverImplementation.orientations,
                orientationDeltas = solverImplementation.orientationDeltas,
                orientationCounts = solverImplementation.orientationConstraintCounts,
                constraintParameters = parameters
            };

            return applyConstraints.Schedule(inputDeps);
        }

        [BurstCompile]
        public struct FrictionConstraintsBatchJob : IJob
        {
            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float4> prevPositions;
            [ReadOnly] public NativeArray<quaternion> orientations;
            [ReadOnly] public NativeArray<quaternion> prevOrientations;

            [ReadOnly] public NativeArray<float> invMasses;
            [ReadOnly] public NativeArray<float4> invInertiaTensors;
            [ReadOnly] public NativeArray<float4> radii;
            [ReadOnly] public NativeArray<int> particleMaterialIndices;

            // simplex arrays:
            [ReadOnly] public NativeList<int> simplices;
            [ReadOnly] public SimplexCounts simplexCounts;

            [ReadOnly] public NativeArray<BurstColliderShape> shapes;
            [ReadOnly] public NativeArray<BurstAffineTransform> transforms;
            [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;
            [ReadOnly] public NativeArray<BurstRigidbody> rigidbodies;
            public NativeArray<float4> rigidbodyLinearDeltas;
            public NativeArray<float4> rigidbodyAngularDeltas;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> counts;

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<quaternion> orientationDeltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> orientationCounts;

            public NativeArray<BurstContact> contacts;
            [ReadOnly] public BurstInertialFrame inertialFrame;
            [ReadOnly] public float stepTime;
            [ReadOnly] public float substepTime;
            [ReadOnly] public int substeps;

            public void Execute()
            {
                for (int i = 0; i < contacts.Length; ++i)
                {
                    var contact = contacts[i];

                    // Get the indices of the particle and collider involved in this contact:
                    int simplexStart = simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);
                    int colliderIndex = contact.bodyB;

                    // Skip contacts involving triggers:
                    if (shapes[colliderIndex].flags > 0)
                        continue;

                    // Get the rigidbody index (might be < 0, in that case there's no rigidbody present)
                    int rigidbodyIndex = shapes[colliderIndex].rigidbodyIndex;

                    // Combine collision materials (use material from first particle in simplex)
                    BurstCollisionMaterial material = CombineCollisionMaterials(simplices[simplexStart], colliderIndex);

                    // Calculate relative velocity:
                    float4 rA = float4.zero, rB = float4.zero;

                    float4 prevPositionA = float4.zero;
                    float4 linearVelocityA = float4.zero;
                    float4 angularVelocityA = float4.zero;
                    float4 invInertiaTensorA = float4.zero;
                    quaternion orientationA = new quaternion(0, 0, 0, 0);
                    float simplexRadiusA = 0;

                    for (int j = 0; j < simplexSize; ++j)
                    {
                        int particleIndex = simplices[simplexStart + j];
                        prevPositionA    += prevPositions[particleIndex] * contact.pointA[j];
                        linearVelocityA  += BurstIntegration.DifferentiateLinear(positions[particleIndex],prevPositions[particleIndex], substepTime) * contact.pointA[j];
                        angularVelocityA += BurstIntegration.DifferentiateAngular(orientations[particleIndex], prevOrientations[particleIndex], substepTime) * contact.pointA[j];
                        invInertiaTensorA += invInertiaTensors[particleIndex] * contact.pointA[j];
                        orientationA.value += orientations[particleIndex].value * contact.pointA[j];
                        simplexRadiusA += BurstMath.EllipsoidRadius(contact.normal, prevOrientations[particleIndex], radii[particleIndex].xyz) * contact.pointA[j];
                    }

                    float4 relativeVelocity = linearVelocityA;

                    // Add particle angular velocity if rolling contacts are enabled:
                    if (material.rollingContacts > 0)
                    {
                        rA = -contact.normal * simplexRadiusA;
                        relativeVelocity += new float4(math.cross(angularVelocityA.xyz, rA.xyz), 0);
                    }

                    // Subtract rigidbody velocity:
                    if (rigidbodyIndex >= 0)
                    {
                        // Note: unlike rA, that is expressed in solver space, rB is expressed in world space.
                        rB = inertialFrame.frame.TransformPoint(contact.pointB) - rigidbodies[rigidbodyIndex].com;
                        relativeVelocity -= BurstMath.GetRigidbodyVelocityAtPoint(rigidbodyIndex, contact.pointB, rigidbodies, rigidbodyLinearDeltas, rigidbodyAngularDeltas, inertialFrame.frame);
                    }

                    // Determine impulse magnitude:
                    float2 impulses = contact.SolveFriction(relativeVelocity, material.staticFriction, material.dynamicFriction, stepTime);

                    if (math.abs(impulses.x) > BurstMath.epsilon || math.abs(impulses.y) > BurstMath.epsilon)
                    {
                        float4 tangentImpulse   = impulses.x * contact.tangent;
                        float4 bitangentImpulse = impulses.y * contact.bitangent;
                        float4 totalImpulse = tangentImpulse + bitangentImpulse;

                        float baryScale = BurstMath.BaryScale(contact.pointA);
                        for (int j = 0; j < simplexSize; ++j)
                        {
                            int particleIndex = simplices[simplexStart + j];
                            //(tangentImpulse * contact.tangentInvMassA + bitangentImpulse * contact.bitangentInvMassA) * dt;
                            deltas[particleIndex] += (tangentImpulse * contact.tangentInvMassA + bitangentImpulse * contact.bitangentInvMassA) * substepTime * contact.pointA[j] * baryScale; 
                            counts[particleIndex]++;
                        }

                        if (rigidbodyIndex >= 0)
                        {
                            BurstMath.ApplyImpulse(rigidbodyIndex, -totalImpulse, contact.pointB, rigidbodies, rigidbodyLinearDeltas, rigidbodyAngularDeltas, inertialFrame.frame);
                        }

                        // Rolling contacts:
                        if (material.rollingContacts > 0)
                        {
                            // Calculate angular velocity deltas due to friction impulse:
                            float4x4 solverInertiaA = BurstMath.TransformInertiaTensor(invInertiaTensorA, orientationA);

                            float4 angVelDeltaA = math.mul(solverInertiaA, new float4(math.cross(rA.xyz, totalImpulse.xyz), 0));
                            float4 angVelDeltaB = float4.zero;

                            // Final angular velocities, after adding the deltas:
                            angularVelocityA += angVelDeltaA;
                            float4 angularVelocityB = float4.zero;

                            // Calculate weights (inverse masses):
                            float invMassA = math.length(math.mul(solverInertiaA, math.normalizesafe(angularVelocityA)));
                            float invMassB = 0;

                            if (rigidbodyIndex >= 0)
                            {
                                angVelDeltaB = math.mul(-rigidbodies[rigidbodyIndex].inverseInertiaTensor, new float4(math.cross(rB.xyz, totalImpulse.xyz), 0));
                                angularVelocityB = rigidbodies[rigidbodyIndex].angularVelocity + angVelDeltaB;
                                invMassB = math.length(math.mul(rigidbodies[rigidbodyIndex].inverseInertiaTensor, math.normalizesafe(angularVelocityB)));
                            }

                            // Calculate rolling axis and angular velocity deltas:
                            float4 rollAxis = float4.zero;
                            float rollingImpulse = contact.SolveRollingFriction(angularVelocityA, angularVelocityB, material.rollingFriction, invMassA, invMassB, ref rollAxis);
                            angVelDeltaA += rollAxis * rollingImpulse * invMassA;
                            angVelDeltaB -= rollAxis * rollingImpulse * invMassB;

                            // Apply orientation delta to particles:
                            quaternion orientationDelta = BurstIntegration.AngularVelocityToSpinQuaternion(orientationA, angVelDeltaA, substepTime);

                            for (int j = 0; j < simplexSize; ++j)
                            {
                                int particleIndex = simplices[simplexStart + j];
                                quaternion qA = orientationDeltas[particleIndex];
                                qA.value += orientationDelta.value;
                                orientationDeltas[particleIndex] = qA;
                                orientationCounts[particleIndex]++;
                            }

                            // Apply angular velocity delta to rigidbody:
                            if (rigidbodyIndex >= 0)
                            {
                                float4 angularDelta = rigidbodyAngularDeltas[rigidbodyIndex];
                                angularDelta += angVelDeltaB;
                                rigidbodyAngularDeltas[rigidbodyIndex] = angularDelta;
                            }
                        }
                    }

                    contacts[i] = contact;
                }
            }

            private BurstCollisionMaterial CombineCollisionMaterials(int entityA, int entityB)
            {
                // Combine collision materials:
                int particleMaterialIndex = particleMaterialIndices[entityA];
                int colliderMaterialIndex = shapes[entityB].materialIndex;

                if (colliderMaterialIndex >= 0 && particleMaterialIndex >= 0)
                    return BurstCollisionMaterial.CombineWith(collisionMaterials[particleMaterialIndex], collisionMaterials[colliderMaterialIndex]);
                else if (particleMaterialIndex >= 0)
                    return collisionMaterials[particleMaterialIndex];
                else if (colliderMaterialIndex >= 0)
                    return collisionMaterials[colliderMaterialIndex];

                return new BurstCollisionMaterial();
            }

        }


    }
}
#endif