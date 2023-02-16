#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Mathematics;

namespace Obi
{
    public struct BurstAabb
    {
        public float4 min;
        public float4 max;

        public float4 size
        {
            get { return max - min; }
        }

        public float4 center
        {
            get { return min + (max - min) * 0.5f; }
        }

        public BurstAabb(float4 min, float4 max)
        {
            this.min = min;
            this.max = max;
        }

        public BurstAabb(float4 v1, float4 v2, float4 v3, float margin)
        {
            min = math.min(math.min(v1, v2), v3) - new float4(margin, margin, margin, 0);
            max = math.max(math.max(v1, v2), v3) + new float4(margin, margin, margin, 0);
        }

        public BurstAabb(float2 v1, float2 v2, float margin)
        {
            min = new float4(math.min(v1, v2) - new float2(margin, margin),0,0);
            max = new float4(math.max(v1, v2) + new float2(margin, margin),0,0);
        }

        public BurstAabb(float4 previousPosition, float4 position, float radius)
        {
            min = math.min(position - radius, previousPosition - radius);
            max = math.max(position + radius, previousPosition + radius);
        }

        public float AverageAxisLength()
        {
            float4 d = max - min;
            return (d.x + d.y + d.z) * 0.33f;
        }

        public float MaxAxisLength()
        {
            return math.cmax(max - min);
        }

        public void EncapsulateParticle(float4 position, float radius)
        {
            min = math.min(min, position - radius);
            max = math.max(max, position + radius);
        }

        public void EncapsulateParticle(float4 previousPosition, float4 position, float radius)
        {
            min = math.min(math.min(min, position - radius), previousPosition - radius);
            max = math.max(math.max(max, position + radius), previousPosition + radius);
        }

        public void EncapsulateBounds(in BurstAabb bounds)
        {
            min = math.min(min,bounds.min);
            max = math.max(max,bounds.max);
        }

        public void Expand(float4 amount)
        {
            min -= amount;
            max += amount;
        }

        public void Sweep(float4 velocity)
        {
            min = math.min(min, min + velocity);
            max = math.max(max, max + velocity);
        }

        public void Transform(in BurstAffineTransform transform)
        {
            Transform(float4x4.TRS(transform.translation.xyz, transform.rotation, transform.scale.xyz));
        }

        public void Transform(in float4x4 transform)
        {
            float3 xa = transform.c0.xyz * min.x;
            float3 xb = transform.c0.xyz * max.x;

            float3 ya = transform.c1.xyz * min.y;
            float3 yb = transform.c1.xyz * max.y;

            float3 za = transform.c2.xyz * min.z;
            float3 zb = transform.c2.xyz * max.z;

            min = new float4(math.min(xa, xb) + math.min(ya, yb) + math.min(za, zb) + transform.c3.xyz, 0);
            max = new float4(math.max(xa, xb) + math.max(ya, yb) + math.max(za, zb) + transform.c3.xyz, 0);
        }

        public BurstAabb Transformed(in BurstAffineTransform transform)
        {
            var cpy = this;
            cpy.Transform(transform);
            return cpy;
        }

        public BurstAabb Transformed(in float4x4 transform)
        {
            var cpy = this;
            cpy.Transform(transform);
            return cpy;
        }

        public bool IntersectsAabb(in BurstAabb bounds, bool in2D = false)
        {
            if (in2D)
            return (min[0] <= bounds.max[0] && max[0] >= bounds.min[0]) &&
                   (min[1] <= bounds.max[1] && max[1] >= bounds.min[1]);

            return (min[0] <= bounds.max[0] && max[0] >= bounds.min[0]) &&
                   (min[1] <= bounds.max[1] && max[1] >= bounds.min[1]) &&
                   (min[2] <= bounds.max[2] && max[2] >= bounds.min[2]);
        }

        public bool IntersectsRay(float4 origin, float4 inv_dir, bool in2D = false) 
        {
            float4 t1 = (min - origin) * inv_dir;
            float4 t2 = (max - origin) * inv_dir;

            float4 tmin1 = math.min(t1,t2);
            float4 tmax1 = math.max(t1,t2);

            float tmin, tmax;
        
            if (in2D) 
            {
                tmin = math.cmax(tmin1.xy);
                tmax = math.cmin(tmax1.xy);
            }
            else
            {
                tmin = math.cmax(tmin1.xyz);
                tmax = math.cmin(tmax1.xyz);
            }
        
            return tmax >= math.max(0, tmin) && tmin <= 1;
        }
    }
}
#endif