using UnityEngine;
using System;

namespace Obi
{
    public interface ISkinConstraintsUser
    {
        bool skinConstraintsEnabled
        {
            get;
            set;
        }

        Vector3 GetSkinRadiiBackstop(ObiSkinConstraintsBatch batch, int constraintIndex);
        float GetSkinCompliance(ObiSkinConstraintsBatch batch, int constraintIndex);
    }

    [Serializable]
    public class ObiSkinConstraintsData : ObiConstraints<ObiSkinConstraintsBatch>
    {
        public override ObiSkinConstraintsBatch CreateBatch(ObiSkinConstraintsBatch source = null)
        {
            return new ObiSkinConstraintsBatch();
        }
    }
}
