using UnityEngine;
using System.Collections;
using System;

namespace Obi
{

    public interface IStretchShearConstraintsUser
    {
        bool stretchShearConstraintsEnabled
        {
            get;
            set;
        }

        Vector3 GetStretchShearCompliance(ObiStretchShearConstraintsBatch batch, int constraintIndex);
    }

    [Serializable]
    public class ObiStretchShearConstraintsData : ObiConstraints<ObiStretchShearConstraintsBatch>
    {

        public override ObiStretchShearConstraintsBatch CreateBatch(ObiStretchShearConstraintsBatch source = null)
        {
            return new ObiStretchShearConstraintsBatch();
        }
    }
}
