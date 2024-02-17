using UnityEngine;
using System;

namespace Obi
{

    public interface IBendTwistConstraintsUser
    {
        bool bendTwistConstraintsEnabled
        {
            get;
            set;
        }

        Vector3 GetBendTwistCompliance(ObiBendTwistConstraintsBatch batch, int constraintIndex);
        Vector2 GetBendTwistPlasticity(ObiBendTwistConstraintsBatch batch, int constraintIndex);
    }

    [Serializable]
    public class ObiBendTwistConstraintsData : ObiConstraints<ObiBendTwistConstraintsBatch>
    {

        public override ObiBendTwistConstraintsBatch CreateBatch(ObiBendTwistConstraintsBatch source = null)
        {
            return new ObiBendTwistConstraintsBatch();
        }
    }
}
