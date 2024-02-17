#if (OBI_ONI_SUPPORTED)
using System;

namespace Obi
{
    public class OniJobHandle : IObiJobHandle
    {
        private IntPtr pointer = IntPtr.Zero;

        public OniJobHandle SetPointer(IntPtr newPtr)
        {
            pointer = newPtr;
            return this;
        }

        public void Complete()
        {
            Oni.Complete(pointer);
        }

        public void Release()
        {
            pointer = IntPtr.Zero;
        }
    }
}
#endif
