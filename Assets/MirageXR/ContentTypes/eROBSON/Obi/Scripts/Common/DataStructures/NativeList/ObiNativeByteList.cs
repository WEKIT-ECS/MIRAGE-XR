using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeByteList : ObiNativeList<byte>
    {

        public ObiNativeByteList() { }
        public ObiNativeByteList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (int i = 0; i < capacity; ++i)
                this[i] = 0;
        }

    }
}

