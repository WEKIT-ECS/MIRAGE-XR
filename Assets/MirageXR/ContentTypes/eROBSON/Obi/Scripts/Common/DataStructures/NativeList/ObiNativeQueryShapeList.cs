using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeQueryShapeList : ObiNativeList<QueryShape>
    {

        public ObiNativeQueryShapeList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (int i = 0; i < capacity; ++i)
                this[i] = new QueryShape();
        }

    }
}

