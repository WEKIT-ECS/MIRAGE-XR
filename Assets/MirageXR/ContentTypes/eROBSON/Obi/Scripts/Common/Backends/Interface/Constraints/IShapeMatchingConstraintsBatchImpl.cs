namespace Obi
{
    public interface IShapeMatchingConstraintsBatchImpl : IConstraintsBatchImpl
    {
        void SetShapeMatchingConstraints(ObiNativeIntList particleIndices,
                                         ObiNativeIntList firstIndex,
                                         ObiNativeIntList numIndices,
                                         ObiNativeIntList explicitGroup,
                                         ObiNativeFloatList shapeMaterialParameters,
                                         ObiNativeVector4List restComs,
                                         ObiNativeVector4List coms,
                                         ObiNativeQuaternionList orientations,
                                         ObiNativeMatrix4x4List linearTransforms,
                                         ObiNativeMatrix4x4List plasticDeformations,
                                         ObiNativeFloatList lambdas,
                                         int count);

        void CalculateRestShapeMatching();
    }
}
