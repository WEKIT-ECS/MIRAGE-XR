#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;

namespace Obi
{
    public struct ContactProvider : IConstraintProvider
    {
        public NativeArray<BurstContact> contacts;
        public NativeArray<BurstContact> sortedContacts;
        public NativeArray<int> simplices;
        public SimplexCounts simplexCounts;

        public int GetConstraintCount()
        {
            return contacts.Length;
        }

        public int GetParticleCount(int constraintIndex)
        {
            simplexCounts.GetSimplexStartAndSize(contacts[constraintIndex].bodyA, out int simplexSizeA);
            simplexCounts.GetSimplexStartAndSize(contacts[constraintIndex].bodyB, out int simplexSizeB);
            return simplexSizeA + simplexSizeB;
        }
        public int GetParticle(int constraintIndex, int index)
        {
            int simplexStartA = simplexCounts.GetSimplexStartAndSize(contacts[constraintIndex].bodyA, out int simplexSizeA);
            int simplexStartB = simplexCounts.GetSimplexStartAndSize(contacts[constraintIndex].bodyB, out int simplexSizeB);
            if (index < simplexSizeA)
                return simplices[simplexStartA + index];
            else
                return simplices[simplexStartB + index - simplexSizeA];
        }

        public void WriteSortedConstraint(int constraintIndex, int sortedIndex)
        {
            sortedContacts[sortedIndex] = contacts[constraintIndex];
        }
    }

}
#endif