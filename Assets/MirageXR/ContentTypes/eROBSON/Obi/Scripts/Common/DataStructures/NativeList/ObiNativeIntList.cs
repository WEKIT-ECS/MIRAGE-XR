﻿using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiNativeIntList : ObiNativeList<int>
    {
        public ObiNativeIntList() { }
        public ObiNativeIntList(int capacity = 8, int alignment = 16) : base(capacity,alignment)
        { 
            for (int i = 0; i < capacity; ++i)
                this[i] = 0;
        }

    }
}

