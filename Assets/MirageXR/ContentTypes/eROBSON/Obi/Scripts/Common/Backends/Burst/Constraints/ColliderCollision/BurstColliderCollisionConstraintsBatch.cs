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
    public class BurstColliderCollisionConstraintsBatch : BurstConstraintsBatchImpl, IColliderCollisionConstraintsBatchImpl
    {
        public BurstColliderCollisionConstraintsBatch(BurstColliderCollisionConstraints constraints)
        {
            m_Constraints = constraints;
            m_ConstraintType = Oni.ConstraintType.Collision;
        }

        public override JobHandle Initialize(JobHandle inputDeps, float substepTime)
        {
            var updateContacts = new UpdateContactsJob()
            {
                prevPositions = solverImplementation.prevPositions,
                prevOrientations = solverImplementation.prevOrientations,
                velocities = solverImplementation.velocities,
                radii = solverImplementation.principalRadii,
                invMasses = solverImplementation.invMasses,
                invInertiaTensors = solverImplementation.invInertiaTensors,
                particleMaterialIndices = solverImplementation.collisionMaterials,
                collisionMaterials = ObiColliderWorld.GetInstance().collisionMaterials.AsNativeArray<BurstCollisionMaterial>(),

                simplices = solverImplementation.simplices,
                simplexCounts = solverImplementation.simplexCounts,

                shapes = ObiColliderWorld.GetInstance().colliderShapes.AsNativeArray<BurstColliderShape>(),
                transforms = ObiColliderWorld.GetInstance().colliderTransforms.AsNativeArray<BurstAffineTransform>(),
                rigidbodies = ObiColliderWorld.GetInstance().rigidbodies.AsNativeArray<BurstRigidbody>(),
                rigidbodyLinearDeltas = solverImplementation.abstraction.rigidbodyLinearDeltas.AsNativeArray<float4>(),
                rigidbodyAngularDeltas = solverImplementation.abstraction.rigidbodyAngularDeltas.AsNativeArray<float4>(),

                contacts = ((BurstSolverImpl)constraints.solver).colliderContacts,
                inertialFrame = ((BurstSolverImpl)constraints.solver).inertialFrame
            };
            return updateContacts.Schedule(((BurstSolverImpl)constraints.solver).colliderContacts.Length, 128, inputDeps);
        }

        public override JobHandle Evaluate(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            var parameters = solverAbstraction.GetConstraintParameters(m_ConstraintType);

            var projectConstraints = new CollisionConstraintsBatchJob()
            {
                positions = solverImplementation.positions,
                prevPositions = solverImplementation.prevPositions,
                orientations = solverImplementation.orientations,
                prevOrientations = solverImplementation.prevOrientations,
                invMasses = solverImplementation.invMasses,
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

                deltas = solverAbstraction.positionDeltas.AsNativeArray<float4>(),
                counts = solverAbstraction.positionConstraintCounts.AsNativeArray<int>(),

                contacts = ((BurstSolverImpl)constraints.solver).colliderContacts,
                inertialFrame = ((BurstSolverImpl)constraints.solver).inertialFrame,
                constraintParameters = parameters,
                solverParameters = solverAbstraction.parameters,
                substeps = substeps,
                stepTime = stepTime,
                substepTime = substepTime
            };
            return projectConstraints.Schedule(inputDeps);
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
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

        /**
         * Updates contact data (such as contact distance) at the beginning of each substep. This is
         * necessary because contacts are generalted only once at the beginning of each step, not every substep.
         */
        [BurstCompile]
        public struct UpdateContactsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float4> prevPositions;
            [ReadOnly] public NativeArray<quaternion> prevOrientations;
            [ReadOnly] public NativeArray<float4> velocities;
            [ReadOnly] public NativeArray<float4> radii;
            [ReadOnly] public NativeArray<float> invMasses;
            [ReadOnly] public NativeArray<float4> invInertiaTensors;

            [ReadOnly] public NativeArray<int> particleMaterialIndices;
            [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;

            // simplex arrays:
            [ReadOnly] public NativeList<int> simplices;
            [ReadOnly] public SimplexCounts simplexCounts;

            [ReadOnly] public NativeArray<BurstColliderShape> shapes;
            [ReadOnly] public NativeArray<BurstAffineTransform> transforms;
            [ReadOnly] public NativeArray<BurstRigidbody> rigidbodies;
            [ReadOnly] public NativeArray<float4> rigidbodyLinearDeltas;
            [ReadOnly] public NativeArray<float4> rigidbodyAngularDeltas;

            public NativeArray<BurstContact> contacts;
            [ReadOnly] public BurstInertialFrame inertialFrame;

            public void Execute(int i)
            {
                var contact = contacts[i];

                int simplexStart = simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);

                // get the material from the first particle in the simplex:
                int aMaterialIndex = particleMaterialIndices[simplices[simplexStart]];
                bool rollingContacts = aMaterialIndex >= 0 ? collisionMaterials[aMaterialIndex].rollingContacts > 0 : false;

                float4 relativeVelocity = float4.zero;
                float4 simplexPrevPosition = float4.zero;
                quaternion simplexPrevOrientation = new quaternion(0, 0, 0, 0);
                float simplexInvMass = 0;
                float4 simplexInvInertia = float4.zero;
                float simplexRadius = 0;

                for (int j = 0; j < simplexSize; ++j)
                {
                    int particleIndex = simplices[simplexStart + j];
                    relativeVelocity += velocities[particleIndex]    * contact.pointA[j];
                    simplexPrevPosition += prevPositions[particleIndex] * contact.pointA[j];
                    simplexPrevOrientation.value += prevOrientations[particleIndex].value * contact.pointA[j];
                    simplexInvMass += invMasses[particleIndex] * contact.pointA[j];
                    simplexInvInertia += invInertiaTensors[particleIndex] * contact.pointA[j];
                    simplexRadius += BurstMath.EllipsoidRadius(contact.normal, prevOrientations[particleIndex], radii[particleIndex].xyz) * contact.pointA[j];
                }

                // if there's a rigidbody present, subtract its velocity from the relative velocity:
                int rigidbodyIndex = shapes[contact.bodyB].rigidbodyIndex;
                if (rigidbodyIndex >= 0)
                {
                    relativeVelocity -= BurstMath.GetRigidbodyVelocityAtPoint(rigidbodyIndex, contact.pointB, rigidbodies, rigidbodyLinearDeltas, rigidbodyAngularDeltas, inertialFrame.frame);

                    int bMaterialIndex = shapes[contact.bodyB].materialIndex;
                    rollingContacts |= bMaterialIndex >= 0 ? collisionMaterials[bMaterialIndex].rollingContacts > 0 : false;
                }

                // update contact distance
                contact.distance = math.dot(simplexPrevPosition - contact.pointB, contact.normal) - simplexRadius;

                // calculate contact point in A's surface:
                float4 contactPoint = contact.pointB + contact.normal * contact.distance;

                // update contact orthonormal basis:
                contact.CalculateBasis(relativeVelocity);

                // calculate A's contact mass.
                contact.CalculateContactMassesA(simplexInvMass, simplexInvInertia, simplexPrevPosition, simplexPrevOrientation, contactPoint, rollingContacts);

                // calculate B's contact mass.
                if (rigidbodyIndex >= 0)
                    contact.CalculateContactMassesB(rigidbodies[rigidbodyIndex], inertialFrame.frame);

                contacts[i] = contact;
            }
        }

        [BurstCompile]
        public struct CollisionConstraintsBatchJob : IJob
        {
            [ReadOnly] public NativeArray<float4> prevPositions;
            [ReadOnly] public NativeArray<quaternion> orientations;
            [ReadOnly] public NativeArray<quaternion> prevOrientations;
            [ReadOnly] public NativeArray<float> invMasses;
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

            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> positions;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<float4> deltas;
            [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction] public NativeArray<int> counts;

            public NativeArray<BurstContact> contacts;
            [ReadOnly] public BurstInertialFrame inertialFrame;
            [ReadOnly] public Oni.ConstraintParameters constraintParameters;
            [ReadOnly] public Oni.SolverParameters solverParameters;
            [ReadOnly] public float stepTime;
            [ReadOnly] public float substepTime;
            [ReadOnly] public int substeps;

            public void Execute()
            {
                for (int i = 0; i < contacts.Length; ++i)
                {
                    var contact = contacts[i];

                    int simplexStart = simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);
                    int colliderIndex = contact.bodyB;

                    // Skip contacts involving triggers:
                    if (shapes[colliderIndex].flags > 0)
                        continue;

                    // Get the rigidbody index (might be < 0, in that case there's no rigidbody present)
                    int rigidbodyIndex = shapes[colliderIndex].rigidbodyIndex;

                    // Combine collision materials (use material from first particle in simplex)
                    BurstCollisionMaterial material = CombineCollisionMaterials(simplices[simplexStart], colliderIndex);

                    // Get relative velocity at contact point.
                    // As we do not consider true ellipses for collision detection, particle contact points are never off-axis.
                    // So particle angular velocity does not contribute to normal impulses, and we can skip it.
                    float4 simplexPosition = float4.zero;
                    float4 simplexPrevPosition = float4.zero;
                    float simplexRadius = 0;

                    for (int j = 0; j < simplexSize; ++j)
                    {
                        int particleIndex = simplices[simplexStart + j];
                        simplexPosition += positions[particleIndex] * contact.pointA[j];
                        simplexPrevPosition += prevPositions[particleIndex] * contact.pointA[j];
                        simplexRadius += BurstMath.EllipsoidRadius(contact.normal, orientations[particleIndex], radii[particleIndex].xyz) * contact.pointA[j];
                    }

                    // project position to the end of the full step:
                    float4 posA = math.lerp(simplexPrevPosition, simplexPosition, substeps);
                    posA += -contact.normal * simplexRadius;

                    float4 posB = contact.pointB;

                    if (rigidbodyIndex >= 0)
                        posB += BurstMath.GetRigidbodyVelocityAtPoint(rigidbodyIndex, contact.pointB, rigidbodies, rigidbodyLinearDeltas, rigidbodyAngularDeltas, inertialFrame.frame) * stepTime;

                    // adhesion:
                    float lambda = contact.SolveAdhesion(posA, posB, material.stickDistance, material.stickiness, stepTime);

                    // depenetration:
                    lambda += contact.SolvePenetration(posA, posB, solverParameters.maxDepenetration * stepTime);

                    // Apply normal impulse to both simplex and rigidbody:
                    if (math.abs(lambda) > BurstMath.epsilon)
                    {
                        float4 delta = lambda * contact.normal * BurstMath.BaryScale(contact.pointA) / substeps;
                        for (int j = 0; j < simplexSize; ++j)
                        {
                            int particleIndex = simplices[simplexStart + j];
                            deltas[particleIndex] += delta * invMasses[particleIndex] * contact.pointA[j];
                            counts[particleIndex]++;
                        }

                        // Apply position deltas immediately, if using sequential evaluation:
                        if (constraintParameters.evaluationOrder == Oni.ConstraintParameters.EvaluationOrder.Sequential)
                        {
                            for (int j = 0; j < simplexSize; ++j)
                            {
                                int particleIndex = simplices[simplexStart + j];
                                BurstConstraintsBatchImpl.ApplyPositionDelta(particleIndex, constraintParameters.SORFactor, ref positions, ref deltas, ref counts);
                            }
                        }

                        if (rigidbodyIndex >= 0)
                        {
                            BurstMath.ApplyImpulse(rigidbodyIndex, -lambda / stepTime * contact.normal, contact.pointB, rigidbodies, rigidbodyLinearDeltas, rigidbodyAngularDeltas, inertialFrame.frame);
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
