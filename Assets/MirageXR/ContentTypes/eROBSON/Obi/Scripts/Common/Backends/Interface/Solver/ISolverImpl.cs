﻿using UnityEngine;

namespace Obi
{
    public interface ISolverImpl
    {
        #region Lifecycle
        void Destroy();
        #endregion

        #region Inertial Frame
        void InitializeFrame(Vector4 translation, Vector4 scale, Quaternion rotation);
        void UpdateFrame(Vector4 translation, Vector4 scale, Quaternion rotation, float deltaTime);
        void ApplyFrame(float worldLinearInertiaScale, float worldAngularInertiaScale, float deltaTime);
        #endregion

        #region Particles
        void ParticleCountChanged(ObiSolver solver);
        void SetActiveParticles(int[] indices, int num);
        void InterpolateDiffuseProperties(ObiNativeVector4List properties, ObiNativeVector4List diffusePositions, ObiNativeVector4List diffuseProperties, ObiNativeIntList neighbourCount, int diffuseCount);
        #endregion

        #region Rigidbodies
        void SetRigidbodyArrays(ObiSolver solver);
        #endregion

        #region Constraints
        IConstraintsBatchImpl CreateConstraintsBatch(Oni.ConstraintType type);
        void DestroyConstraintsBatch(IConstraintsBatchImpl batch);
        int GetConstraintCount(Oni.ConstraintType type);
        void GetCollisionContacts(Oni.Contact[] contacts, int count);
        void GetParticleCollisionContacts(Oni.Contact[] contacts, int count);
        void SetConstraintGroupParameters(Oni.ConstraintType type, ref Oni.ConstraintParameters parameters);
        #endregion

        #region Update
        IObiJobHandle CollisionDetection(float stepTime);
        IObiJobHandle Substep(float stepTime, float substepTime, int substeps);
        void ApplyInterpolation(ObiNativeVector4List startPositions, ObiNativeQuaternionList startOrientations, float stepTime, float unsimulatedTime);
        #endregion

        #region Simplices
        int GetDeformableTriangleCount();
        void SetDeformableTriangles(int[] indices, int num, int destOffset);
        int RemoveDeformableTriangles(int num, int sourceOffset);

        void SetSimplices(int[] simplices, SimplexCounts counts);
        #endregion

        #region Utils
        void SetParameters(Oni.SolverParameters parameters);
        void GetBounds(ref Vector3 min, ref Vector3 max);
        void ResetForces();
        int GetParticleGridSize();
        void GetParticleGrid(ObiNativeAabbList cells);
        void SpatialQuery(ObiNativeQueryShapeList shapes, ObiNativeAffineTransformList transforms, ObiNativeQueryResultList results);
        void ReleaseJobHandles();
        #endregion
    }
}
