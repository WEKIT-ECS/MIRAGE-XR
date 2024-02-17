using UnityEngine;
using System.Collections;

namespace Obi
{
    public interface IBendConstraintsBatchImpl : IConstraintsBatchImpl
    {
        void SetBendConstraints(ObiNativeIntList particleIndices, ObiNativeFloatList restBends, ObiNativeVector2List bendingStiffnesses, ObiNativeVector2List plasticity, ObiNativeFloatList lambdas, int count);
    }
}
