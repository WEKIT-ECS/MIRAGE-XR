#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Jobs;

namespace Obi
{
    public class BurstJobHandle : IObiJobHandle
    {
        private JobHandle handle = new JobHandle();

        public BurstJobHandle SetHandle(JobHandle newHandle)
        {
            handle = newHandle;
            return this;
        }

        public void Complete()
        {
            handle.Complete();
        }

        public void Release()
        {
            handle = new JobHandle();
        }
    }
}
#endif

