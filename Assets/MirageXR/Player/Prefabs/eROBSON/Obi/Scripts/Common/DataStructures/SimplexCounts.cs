using UnityEngine;
using System.Collections;


namespace Obi
{
    public struct SimplexCounts
    {
        public int pointCount;
        public int edgeCount;
        public int triangleCount;

        public int simplexCount
        {
            get { return pointCount + edgeCount + triangleCount; }
        }

        public SimplexCounts(int pointCount, int edgeCount, int triangleCount)
        {
            this.pointCount = pointCount;
            this.edgeCount = edgeCount;
            this.triangleCount = triangleCount;
        }

        public int GetSimplexStartAndSize(int index, out int size)
        {
            if (index < pointCount)
            {
                size = 1;
                return index;
            }
            else if (index < pointCount + edgeCount)
            {
                size = 2;
                return pointCount + (index - pointCount) * 2;
            }
            else if (index < simplexCount)
            {
                size = 3;
                int triStart = pointCount + edgeCount * 2;
                return triStart + (index - pointCount - edgeCount) * 3;
            }
            size = 0;
            return 0;
        }
    }
}
