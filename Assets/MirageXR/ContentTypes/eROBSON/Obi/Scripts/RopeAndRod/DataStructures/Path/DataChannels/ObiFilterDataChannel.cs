using System;

namespace Obi
{
    [Serializable]
    public class ObiPhaseDataChannel : ObiPathDataChannelIdentity<int>
    {
        public ObiPhaseDataChannel() : base(new ObiConstantInterpolator()) { }
    }
}