#if (OBI_ONI_SUPPORTED)
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Obi
{
    public class OniShapeMatchingConstraintsBatchImpl : OniConstraintsBatchImpl, IShapeMatchingConstraintsBatchImpl
    {
        public OniShapeMatchingConstraintsBatchImpl(OniShapeMatchingConstraintsImpl constraints) : base(constraints, Oni.ConstraintType.ShapeMatching)
        {
        }

        public void SetShapeMatchingConstraints(ObiNativeIntList particleIndices,
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
                                                 int count)
        {
            Oni.SetShapeMatchingConstraints(oniBatch, particleIndices.GetIntPtr(), firstIndex.GetIntPtr(), numIndices.GetIntPtr(), explicitGroup.GetIntPtr(),
                                            shapeMaterialParameters.GetIntPtr(),restComs.GetIntPtr(), coms.GetIntPtr(), orientations.GetIntPtr(), linearTransforms.GetIntPtr(),
                                            plasticDeformations.GetIntPtr(), count);
        }

        public void CalculateRestShapeMatching()
        {
            Oni.RecalculateInertiaTensors(((OniSolverImpl)this.constraints.solver).oniSolver);
            Oni.CalculateRestShapeMatching(((OniSolverImpl)this.constraints.solver).oniSolver, oniBatch);
        }
    }
}
#endif