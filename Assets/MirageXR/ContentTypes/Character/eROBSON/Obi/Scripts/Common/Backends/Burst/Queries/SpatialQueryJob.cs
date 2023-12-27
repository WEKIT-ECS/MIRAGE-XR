#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace Obi
{

    [BurstCompile]
    unsafe struct SpatialQueryJob : IJobParallelFor
    {
        //collider grid:
        [ReadOnly] public NativeMultilevelGrid<int> grid;

        // particle arrays:
        [ReadOnly] public NativeArray<float4> positions;
        [ReadOnly] public NativeArray<quaternion> orientations;
        [ReadOnly] public NativeArray<float4> radii;
        [ReadOnly] public NativeArray<int> filters;

        // simplex arrays:
        [ReadOnly] public NativeList<int> simplices;
        [ReadOnly] public SimplexCounts simplexCounts;

        // query arrays:
        [ReadOnly] public NativeArray<BurstQueryShape> shapes;
        [ReadOnly] public NativeArray<BurstAffineTransform> transforms;

        // output contacts queue:
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeQueue<BurstQueryResult>.ParallelWriter results;

        // auxiliar data:
        [ReadOnly] public BurstAffineTransform worldToSolver;
        [ReadOnly] public Oni.SolverParameters parameters;

        // execute for each query shape:
        public void Execute(int i)
        {
            var shapeToSolver = worldToSolver * transforms[i];

            // calculate solver-space aabb of query shape:
            BurstAabb queryBoundsSS = CalculateShapeAABB(shapes[i]).Transformed(shapeToSolver);

            var shapeCategory = shapes[i].filter & ObiUtils.FilterCategoryBitmask;
            var shapeMask = (shapes[i].filter & ObiUtils.FilterMaskBitmask) >> 16;

            bool is2D = parameters.mode == Oni.SolverParameters.Mode.Mode2D;

            // iterate over all occupied cells:
            for (int c = 0; c < grid.usedCells.Length; ++c)
            {
                var cell = grid.usedCells[c];

                // calculate thickedned grid bounds:
                float size = NativeMultilevelGrid<int>.CellSizeOfLevel(cell.Coords.w);
                float4 cellPos = (float4)cell.Coords * size;
                BurstAabb cellBounds = new BurstAabb(cellPos - new float4(size), cellPos + new float4(2 * size));

                // if cell and query bounds intersect:
                if (cellBounds.IntersectsAabb(in queryBoundsSS, is2D))
                {
                    // iterate over cell contents:
                    for (int k = 0; k < cell.Length; ++k)
                    {
                        int simplexStart = simplexCounts.GetSimplexStartAndSize(cell[k], out int simplexSize);

                        // check if any simplex particle and the query shape should collide:
                        bool shouldCollide = false;
                        for (int j = 0; j < simplexSize; ++j)
                        {
                            var simplexCategory = filters[simplices[simplexStart + j]] & ObiUtils.FilterCategoryBitmask;
                            var simplexMask = (filters[simplices[simplexStart + j]] & ObiUtils.FilterMaskBitmask) >> 16;
                            shouldCollide |= (simplexCategory & shapeMask) != 0 && (simplexMask & shapeCategory) != 0;
                        }

                        if (shouldCollide)
                            Query(shapes[i], shapeToSolver, i, cell[k], simplexStart, simplexSize);
                    }
                }
            }
        }

        private BurstAabb CalculateShapeAABB(in BurstQueryShape shape)
        {
            float offset = shape.contactOffset + shape.distance;
            switch (shape.type)
            {
                case QueryShape.QueryType.Sphere:
                    return new BurstAabb(shape.center, shape.center, shape.size.x + offset);
                case QueryShape.QueryType.Box:
                    return new BurstAabb(shape.center - shape.size*0.5f - offset, shape.center + shape.size * 0.5f + offset);
                case QueryShape.QueryType.Ray:
                    return new BurstAabb(shape.center, shape.size, offset);
            }
            return new BurstAabb();
        }

        private void Query(in BurstQueryShape shape,
                           in BurstAffineTransform shapeToSolver,
                           int shapeIndex,
                           int simplexIndex,
                           int simplexStart,
                           int simplexSize)
        {
            switch (shape.type)
            {
                case QueryShape.QueryType.Sphere:
                    BurstSphereQuery sphereShape = new BurstSphereQuery() { colliderToSolver = shapeToSolver, shape = shape};
                    sphereShape.Query(shapeIndex, positions, orientations, radii, simplices,
                                      simplexIndex, simplexStart, simplexSize, results, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                    break;
                case QueryShape.QueryType.Box:
                    BurstBoxQuery boxShape = new BurstBoxQuery() { colliderToSolver = shapeToSolver, shape = shape};
                    boxShape.Query(shapeIndex, positions, orientations, radii, simplices,
                                      simplexIndex, simplexStart, simplexSize, results, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                    break;
                case QueryShape.QueryType.Ray:
                    BurstRay rayShape = new BurstRay() { colliderToSolver = shapeToSolver, shape = shape };
                    rayShape.Query(shapeIndex, positions, orientations, radii, simplices,
                                      simplexIndex, simplexStart, simplexSize, results, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                    break;
            }
        }
    }

    [BurstCompile]
    public struct CalculateQueryDistances : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4> prevPositions;
        [ReadOnly] public NativeArray<quaternion> prevOrientations;
        [ReadOnly] public NativeArray<float4> radii;

        // simplex arrays:
        [ReadOnly] public NativeList<int> simplices;
        [ReadOnly] public SimplexCounts simplexCounts;

        public NativeArray<BurstQueryResult> queryResults;

        public void Execute(int i)
        {
            var result = queryResults[i];

            int simplexStart = simplexCounts.GetSimplexStartAndSize(result.simplexIndex, out int simplexSize);

            float4 simplexPrevPosition = float4.zero;
            float simplexRadius = 0;

            for (int j = 0; j < simplexSize; ++j)
            {
                int particleIndex = simplices[simplexStart + j];
                simplexPrevPosition += prevPositions[particleIndex] * result.simplexBary[j];
                simplexRadius += BurstMath.EllipsoidRadius(result.normal, prevOrientations[particleIndex], radii[particleIndex].xyz) * result.simplexBary[j];
            }

            // update contact distance
            result.distance = math.dot(simplexPrevPosition - result.queryPoint, result.normal) - simplexRadius;
            queryResults[i] = result;
        }
    }
}
#endif