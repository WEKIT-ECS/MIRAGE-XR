using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiNativeMatrix4x4List : ObiNativeList<Matrix4x4>
    {
        public ObiNativeMatrix4x4List(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (int i = 0; i < capacity; ++i)
                this[i] = Matrix4x4.identity;
        }
    }
}

