using UnityEngine;

namespace Obi
{
    public struct QueryShape
    {
        public enum QueryType
        {
            Sphere = 0,
            Box = 1,
            Ray = 2,
        }

        public Vector4 center;      /**<  box: center of the box in solver space.
                                          sphere: center of the sphere in solver space,.
                                          ray: start of the ray in solver space.*/

        public Vector4 size;     /**<     box: size of the box in each axis.
                                          sphere: radius of sphere (x,y,z),
                                          ray: end of the line segment in solver space.*/
        public QueryType type;
        public float contactOffset;
        public float maxDistance;          // minimum distance around the shape to look for.
        public int filter;

        public QueryShape(QueryType type, Vector3 center, Vector3 size, float contactOffset, float distance, int filter)
        {
            this.type = type;
            this.center = center;
            this.size = size;
            this.contactOffset = contactOffset;
            this.maxDistance = distance;
            this.filter = filter;
        }
    }
}
