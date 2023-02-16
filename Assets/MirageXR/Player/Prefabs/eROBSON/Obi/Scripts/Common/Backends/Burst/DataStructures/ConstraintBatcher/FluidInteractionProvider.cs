#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;

namespace Obi
{
    public struct FluidInteractionProvider : IConstraintProvider
    {
        public NativeArray<FluidInteraction> interactions;
        public NativeArray<FluidInteraction> sortedInteractions;

        public int GetConstraintCount()
        {
            return interactions.Length;
        }

        public int GetParticleCount(int constraintIndex)
        {
            return 2;
        }
        public int GetParticle(int constraintIndex, int index)
        {
            return interactions[constraintIndex].GetParticle(index);
        }

        public void WriteSortedConstraint(int constraintIndex, int sortedIndex)
        {
            sortedInteractions[sortedIndex] = interactions[constraintIndex];
        }
    }

}
#endif