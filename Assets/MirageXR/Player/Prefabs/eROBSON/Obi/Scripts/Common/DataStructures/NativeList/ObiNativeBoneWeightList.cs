using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiNativeBoneWeightList : ObiNativeList<BoneWeight1>
    {
        public ObiNativeBoneWeightList() { }
        public ObiNativeBoneWeightList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (int i = 0; i < capacity; ++i)
                this[i] = new BoneWeight1();
        }

    }
}

