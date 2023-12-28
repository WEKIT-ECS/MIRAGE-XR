#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace Obi
{


    public class BurstColliderWorld : MonoBehaviour, IColliderWorldImpl
    {
        struct MovingCollider
        {
            public BurstCellSpan oldSpan;
            public BurstCellSpan newSpan;
            public int entity;
        }

        private int refCount = 0;
        private int colliderCount = 0;

        private NativeMultilevelGrid<int> grid;
        private NativeQueue<MovingCollider> movingColliders;
        public NativeQueue<BurstContact> colliderContactQueue;

        public ObiNativeCellSpanList cellSpans;

        public int referenceCount { get { return refCount; } }

        public void Awake()
        {

            this.grid = new NativeMultilevelGrid<int>(1000, Allocator.Persistent);
            this.movingColliders = new NativeQueue<MovingCollider>(Allocator.Persistent);
            this.colliderContactQueue = new NativeQueue<BurstContact>(Allocator.Persistent);

            this.cellSpans = new ObiNativeCellSpanList();

            ObiColliderWorld.GetInstance().RegisterImplementation(this);
        }

        public void OnDestroy()
        {
            ObiColliderWorld.GetInstance().UnregisterImplementation(this);

            grid.Dispose();
            movingColliders.Dispose();
            colliderContactQueue.Dispose();
            cellSpans.Dispose();
        }

        public void IncreaseReferenceCount()
        {
            refCount++;
        }
        public void DecreaseReferenceCount()
        {
            if (--refCount <= 0 && gameObject != null)
                DestroyImmediate(gameObject);  
        }

        public void SetColliders(ObiNativeColliderShapeList shapes, ObiNativeAabbList bounds, ObiNativeAffineTransformList transforms, int count)
        {
            colliderCount = count;

            // insert new empty cellspans at the end if needed:
            while (colliderCount > cellSpans.count)
                cellSpans.Add(new CellSpan(new VInt4(10000), new VInt4(10000)));
        }

        public void SetRigidbodies(ObiNativeRigidbodyList rigidbody)
        {
        }

        public void SetCollisionMaterials(ObiNativeCollisionMaterialList materials)
        {

        }

        public void SetTriangleMeshData(ObiNativeTriangleMeshHeaderList headers, ObiNativeBIHNodeList nodes, ObiNativeTriangleList triangles, ObiNativeVector3List vertices)
        {
        }

        public void SetEdgeMeshData(ObiNativeEdgeMeshHeaderList headers, ObiNativeBIHNodeList nodes, ObiNativeEdgeList edges, ObiNativeVector2List vertices)
        {
        }

        public void SetDistanceFieldData(ObiNativeDistanceFieldHeaderList headers, ObiNativeDFNodeList nodes) { }
        public void SetHeightFieldData(ObiNativeHeightFieldHeaderList headers, ObiNativeFloatList samples) { }

        public void UpdateWorld(float deltaTime)
        {
            var world = ObiColliderWorld.GetInstance();

            var identifyMoving = new IdentifyMovingColliders
            {
                movingColliders = movingColliders.AsParallelWriter(),
                shapes = world.colliderShapes.AsNativeArray<BurstColliderShape>(cellSpans.count),
                rigidbodies = world.rigidbodies.AsNativeArray<BurstRigidbody>(),
                collisionMaterials = world.collisionMaterials.AsNativeArray<BurstCollisionMaterial>(),
                bounds = world.colliderAabbs.AsNativeArray<BurstAabb>(cellSpans.count),
                cellIndices = cellSpans.AsNativeArray<BurstCellSpan>(),
                colliderCount = colliderCount,
                dt = deltaTime
            };
            JobHandle movingHandle = identifyMoving.Schedule(cellSpans.count, 128);

            var updateMoving = new UpdateMovingColliders
            {
                movingColliders = movingColliders,
                grid = grid,
                colliderCount = colliderCount
            };

            updateMoving.Schedule(movingHandle).Complete();

            // remove tail from the current spans array:
            if (colliderCount < cellSpans.count)
                cellSpans.count -= cellSpans.count - colliderCount;
        }

        [BurstCompile]
        struct IdentifyMovingColliders : IJobParallelFor
        {
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeQueue<MovingCollider>.ParallelWriter movingColliders;

            [ReadOnly] public NativeArray<BurstColliderShape> shapes;
            [ReadOnly] public NativeArray<BurstRigidbody> rigidbodies;
            [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;
            public NativeArray<BurstAabb> bounds;

            public NativeArray<BurstCellSpan> cellIndices;
            [ReadOnly] public int colliderCount;
            [ReadOnly] public float dt;

            // Iterate over all colliders and store those whose cell span has changed.
            public void Execute(int i)
            {
                BurstAabb velocityBounds = bounds[i];

                int rb = shapes[i].rigidbodyIndex;

                // Expand bounds by rigidbody's linear velocity
                // (check against out of bounds rigidbody access, can happen when a destroyed collider references a rigidbody that has just been destroyed too)
                if (rb >= 0 && rb < rigidbodies.Length)
                    velocityBounds.Sweep(rigidbodies[rb].velocity * dt);

                // Expand bounds by collision material's stick distance:
                if (shapes[i].materialIndex >= 0) 
                    velocityBounds.Expand(collisionMaterials[shapes[i].materialIndex].stickDistance);

                float size = velocityBounds.AverageAxisLength();
                int level = NativeMultilevelGrid<int>.GridLevelForSize(size);
                float cellSize = NativeMultilevelGrid<int>.CellSizeOfLevel(level);

                // get new collider bounds cell coordinates:
                BurstCellSpan newSpan = new BurstCellSpan(new int4(GridHash.Quantize(velocityBounds.min.xyz, cellSize), level),
                                                          new int4(GridHash.Quantize(velocityBounds.max.xyz, cellSize), level));

                // if the collider is 2D, project it to the z = 0 cells.
                if (shapes[i].is2D != 0)
                {
                    newSpan.min[2] = 0;
                    newSpan.max[2] = 0;
                }

                // if the collider is at the tail (removed), we will only remove it from its current cellspan.
                // if the new cellspan and the current one are different, we must remove it from its current cellspan and add it to its new one.
                if (i >= colliderCount || cellIndices[i] != newSpan)
                {
                    // Add the collider to the list of moving colliders:
                    movingColliders.Enqueue(new MovingCollider()
                    {
                        oldSpan = cellIndices[i],
                        newSpan = newSpan,
                        entity = i
                    });

                    // Update previous coords:
                    cellIndices[i] = newSpan;
                }

            }
        }

        [BurstCompile]
        struct UpdateMovingColliders : IJob
        {
            public NativeQueue<MovingCollider> movingColliders;
            public NativeMultilevelGrid<int> grid;
            [ReadOnly] public int colliderCount;

            public void Execute()
            {
                while (movingColliders.Count > 0)
                {
                    MovingCollider movingCollider = movingColliders.Dequeue();

                    // remove from old cells:
                    grid.RemoveFromCells(movingCollider.oldSpan, movingCollider.entity);

                    // insert in new cells, as long as the index is below the amount of colliders.
                    // otherwise, the collider is at the "tail" and there's no need to add it back.
                    if (movingCollider.entity < colliderCount)
                        grid.AddToCells(movingCollider.newSpan, movingCollider.entity);
                }

                // remove all empty cells from the grid:
                grid.RemoveEmpty();
            }
        }

        [BurstCompile]
        unsafe struct GenerateContactsJob : IJobParallelFor
        {
            //collider grid:
            [ReadOnly] public NativeMultilevelGrid<int> colliderGrid;

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<int> gridLevels;

            // particle arrays:
            [ReadOnly] public NativeArray<float4> velocities;
            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<quaternion> orientations;
            [ReadOnly] public NativeArray<float> invMasses;
            [ReadOnly] public NativeArray<float4> radii;
            [ReadOnly] public NativeArray<int> filters;

            // simplex arrays:
            [ReadOnly] public NativeList<int> simplices;
            [ReadOnly] public SimplexCounts simplexCounts;
            [ReadOnly] public NativeArray<BurstAabb> simplexBounds;

            // collider arrays:
            [ReadOnly] public NativeArray<BurstAffineTransform> transforms;
            [ReadOnly] public NativeArray<BurstColliderShape> shapes;
            [ReadOnly] public NativeArray<BurstCollisionMaterial> collisionMaterials;
            [ReadOnly] public NativeArray<BurstRigidbody> rigidbodies;
            [ReadOnly] public NativeArray<BurstAabb> bounds;

            // distance field data:
            [ReadOnly] public NativeArray<DistanceFieldHeader> distanceFieldHeaders;
            [ReadOnly] public NativeArray<BurstDFNode> distanceFieldNodes;

            // triangle mesh data:
            [ReadOnly] public NativeArray<TriangleMeshHeader> triangleMeshHeaders;
            [ReadOnly] public NativeArray<BIHNode> bihNodes;
            [ReadOnly] public NativeArray<Triangle> triangles;
            [ReadOnly] public NativeArray<float3> vertices;

            // edge mesh data:
            [ReadOnly] public NativeArray<EdgeMeshHeader> edgeMeshHeaders;
            [ReadOnly] public NativeArray<BIHNode> edgeBihNodes;
            [ReadOnly] public NativeArray<Edge> edges;
            [ReadOnly] public NativeArray<float2> edgeVertices;

            // height field data:
            [ReadOnly] public NativeArray<HeightFieldHeader> heightFieldHeaders;
            [ReadOnly] public NativeArray<float> heightFieldSamples;

            // output contacts queue:
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeQueue<BurstContact>.ParallelWriter contactsQueue;

            // auxiliar data:
            [ReadOnly] public BurstAffineTransform solverToWorld;
            [ReadOnly] public BurstAffineTransform worldToSolver;
            [ReadOnly] public float deltaTime;
            [ReadOnly] public Oni.SolverParameters parameters;

            public void Execute(int i)
            {
                int simplexStart = simplexCounts.GetSimplexStartAndSize(i, out int simplexSize);
                BurstAabb simplexBoundsSS = simplexBounds[i];

                // get all colliders overlapped by the cell bounds, in all grid levels:
                BurstAabb simplexBoundsWS = simplexBoundsSS.Transformed(solverToWorld);
                NativeList<int> candidates = new NativeList<int>(16,Allocator.Temp);

                // max size of the particle bounds in cells:
                int3 maxSize = new int3(10);
                bool is2D = parameters.mode == Oni.SolverParameters.Mode.Mode2D;

                for (int l = 0; l < gridLevels.Length; ++l)
                {
                    float cellSize = NativeMultilevelGrid<int>.CellSizeOfLevel(gridLevels[l]);

                    int3 minCell = GridHash.Quantize(simplexBoundsWS.min.xyz, cellSize);
                    int3 maxCell = GridHash.Quantize(simplexBoundsWS.max.xyz, cellSize);
                    maxCell = minCell + math.min(maxCell - minCell, maxSize);

                    for (int x = minCell[0]; x <= maxCell[0]; ++x)
                    {
                        for (int y = minCell[1]; y <= maxCell[1]; ++y)
                        {
                            // for 2D mode, project each cell at z == 0 and check them too. This way we ensure 2D colliders
                            // (which are inserted in cells with z == 0) are accounted for in the broadphase.
                            if (is2D)
                            {
                                if (colliderGrid.TryGetCellIndex(new int4(x, y, 0, gridLevels[l]), out int cellIndex))
                                {
                                    var colliderCell = colliderGrid.usedCells[cellIndex];
                                    candidates.AddRange(colliderCell.ContentsPointer, colliderCell.Length);
                                }
                            }

                            for (int z = minCell[2]; z <= maxCell[2]; ++z)
                            {
                                if (colliderGrid.TryGetCellIndex(new int4(x, y, z, gridLevels[l]), out int cellIndex))
                                {
                                    var colliderCell = colliderGrid.usedCells[cellIndex];
                                    candidates.AddRange(colliderCell.ContentsPointer, colliderCell.Length);
                                }
                            }
                        }
                    }
                }

                if (candidates.Length > 0)
                {
                    // make sure each candidate collider only shows up once in the array:
                    NativeArray<int> uniqueCandidates = candidates.AsArray();
                    uniqueCandidates.Sort();
                    int uniqueCount = uniqueCandidates.Unique();

                    // iterate over candidate colliders, generating contacts for each one
                    for (int k = 0; k < uniqueCount; ++k)
                    {
                        int c = uniqueCandidates[k];
                        if (c < shapes.Length)
                        {
                            BurstColliderShape shape = shapes[c];
                            BurstAabb colliderBoundsWS = bounds[c];
                            int rb = shape.rigidbodyIndex;

                            // Expand bounds by rigidbody's linear velocity:
                            if (rb >= 0)
                                colliderBoundsWS.Sweep(rigidbodies[rb].velocity * deltaTime);

                            // Expand bounds by collision material's stick distance:
                            if (shape.materialIndex >= 0)
                                colliderBoundsWS.Expand(collisionMaterials[shape.materialIndex].stickDistance);

                            // check if any simplex particle and the collider should collide:
                            bool shouldCollide = false;
                            var colliderCategory = shape.filter & ObiUtils.FilterCategoryBitmask;
                            var colliderMask = (shape.filter & ObiUtils.FilterMaskBitmask) >> 16;
                            for (int j = 0; j < simplexSize; ++j)
                            {
                                var simplexCategory = filters[simplices[simplexStart + j]] & ObiUtils.FilterCategoryBitmask;
                                var simplexMask =    (filters[simplices[simplexStart + j]] & ObiUtils.FilterMaskBitmask) >> 16;
                                shouldCollide |= (simplexCategory & colliderMask) != 0 && (simplexMask & colliderCategory) != 0;
                            }

                            if (shouldCollide && simplexBoundsWS.IntersectsAabb(in colliderBoundsWS, is2D))
                            {
                                // generate contacts for the collider:
                                BurstAffineTransform colliderToSolver = worldToSolver * transforms[c];
                                GenerateContacts(in shape, in colliderToSolver, c, rb, i, simplexStart, simplexSize, simplexBoundsSS);
                            }
                        }
                    }
                }
            }

            private void GenerateContacts(in BurstColliderShape shape,
                                          in BurstAffineTransform colliderToSolver,
                                          int colliderIndex,
                                          int rigidbodyIndex,
                                          int simplexIndex,
                                          int simplexStart,
                                          int simplexSize,
                                          in BurstAabb simplexBoundsSS)
            {
                float4x4 solverToCollider;
                BurstAabb simplexBoundsCS;

                switch (shape.type)
                {
                    case ColliderShape.ShapeType.Sphere:
                        BurstSphere sphereShape = new BurstSphere() { colliderToSolver = colliderToSolver, shape = shape, dt = deltaTime };
                        sphereShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsSS,
                                             simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                        break;
                    case ColliderShape.ShapeType.Box:
                        BurstBox boxShape = new BurstBox() { colliderToSolver = colliderToSolver, shape = shape, dt = deltaTime };
                        boxShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsSS,
                                          simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                        break;
                    case ColliderShape.ShapeType.Capsule:
                        BurstCapsule capsuleShape = new BurstCapsule(){colliderToSolver = colliderToSolver,shape = shape, dt = deltaTime };
                        capsuleShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsSS,
                                              simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);
                        break;
                    case ColliderShape.ShapeType.SignedDistanceField:

                        if (shape.dataIndex < 0) return;

                        BurstDistanceField distanceFieldShape = new BurstDistanceField()
                        {
                            colliderToSolver = colliderToSolver,
                            solverToWorld = solverToWorld,
                            shape = shape,
                            distanceFieldHeaders = distanceFieldHeaders,
                            dfNodes = distanceFieldNodes,
                            dt = deltaTime,
                            collisionMargin = parameters.collisionMargin
                        };

                        distanceFieldShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsSS,
                                                    simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);

                        break;
                    case ColliderShape.ShapeType.Heightmap:

                        if (shape.dataIndex < 0) return;

                        // invert a full matrix here to accurately represent collider bounds scale.
                        solverToCollider = math.inverse(float4x4.TRS(colliderToSolver.translation.xyz, colliderToSolver.rotation, colliderToSolver.scale.xyz));
                        simplexBoundsCS = simplexBoundsSS.Transformed(solverToCollider);

                        BurstHeightField heightmapShape = new BurstHeightField()
                        {
                            colliderToSolver = colliderToSolver,
                            solverToWorld = solverToWorld,
                            shape = shape,
                            header = heightFieldHeaders[shape.dataIndex],
                            heightFieldSamples = heightFieldSamples,
                            collisionMargin = parameters.collisionMargin,
                            dt = deltaTime
                        };

                        heightmapShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsCS,
                                                    simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);

                        break;
                    case ColliderShape.ShapeType.TriangleMesh:

                        if (shape.dataIndex < 0) return;

                        // invert a full matrix here to accurately represent collider bounds scale.
                        solverToCollider = math.inverse(float4x4.TRS(colliderToSolver.translation.xyz, colliderToSolver.rotation, colliderToSolver.scale.xyz));
                        simplexBoundsCS = simplexBoundsSS.Transformed(solverToCollider);

                        BurstTriangleMesh triangleMeshShape = new BurstTriangleMesh()
                        {
                            colliderToSolver = colliderToSolver,
                            solverToWorld = solverToWorld,
                            shape = shape,
                            header = triangleMeshHeaders[shape.dataIndex],
                            bihNodes = bihNodes,
                            triangles = triangles,
                            vertices = vertices,
                            collisionMargin = parameters.collisionMargin,
                            dt = deltaTime
                        };

                        triangleMeshShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsCS,
                                                    simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);

                        break;
                    case ColliderShape.ShapeType.EdgeMesh:

                        if (shape.dataIndex < 0) return;

                        // invert a full matrix here to accurately represent collider bounds scale.
                        solverToCollider = math.inverse(float4x4.TRS(colliderToSolver.translation.xyz, colliderToSolver.rotation, colliderToSolver.scale.xyz));
                        simplexBoundsCS = simplexBoundsSS.Transformed(solverToCollider);

                        BurstEdgeMesh edgeMeshShape = new BurstEdgeMesh()
                        {
                            colliderToSolver = colliderToSolver,
                            shape = shape,
                            header = edgeMeshHeaders[shape.dataIndex],
                            edgeBihNodes = edgeBihNodes,
                            edges = edges,
                            vertices = edgeVertices,
                            dt = deltaTime
                        };

                        edgeMeshShape.Contacts(colliderIndex, rigidbodyIndex, rigidbodies, positions, orientations, velocities, radii, simplices, in simplexBoundsCS,
                                                    simplexIndex, simplexStart, simplexSize, contactsQueue, parameters.surfaceCollisionIterations, parameters.surfaceCollisionTolerance);

                        break;
                }
            }

        }


        public JobHandle GenerateContacts(BurstSolverImpl solver, float deltaTime, JobHandle inputDeps)
        {
            var world = ObiColliderWorld.GetInstance();

            var generateColliderContactsJob = new GenerateContactsJob
            {
                colliderGrid = grid,
                gridLevels = grid.populatedLevels.GetKeyArray(Allocator.TempJob),

                positions = solver.positions,
                orientations = solver.orientations,
                velocities = solver.velocities,
                invMasses = solver.invMasses,
                radii = solver.principalRadii,
                filters = solver.filters,

                simplices = solver.simplices,
                simplexCounts = solver.simplexCounts,
                simplexBounds = solver.simplexBounds,

                transforms = world.colliderTransforms.AsNativeArray<BurstAffineTransform>(),
                shapes = world.colliderShapes.AsNativeArray<BurstColliderShape>(),
                rigidbodies = world.rigidbodies.AsNativeArray<BurstRigidbody>(),
                collisionMaterials = world.collisionMaterials.AsNativeArray<BurstCollisionMaterial>(),
                bounds = world.colliderAabbs.AsNativeArray<BurstAabb>(),

                distanceFieldHeaders = world.distanceFieldContainer.headers.AsNativeArray<DistanceFieldHeader>(),
                distanceFieldNodes = world.distanceFieldContainer.dfNodes.AsNativeArray<BurstDFNode>(),

                triangleMeshHeaders = world.triangleMeshContainer.headers.AsNativeArray<TriangleMeshHeader>(),
                bihNodes = world.triangleMeshContainer.bihNodes.AsNativeArray<BIHNode>(),
                triangles = world.triangleMeshContainer.triangles.AsNativeArray<Triangle>(),
                vertices = world.triangleMeshContainer.vertices.AsNativeArray<float3>(),

                edgeMeshHeaders = world.edgeMeshContainer.headers.AsNativeArray<EdgeMeshHeader>(),
                edgeBihNodes = world.edgeMeshContainer.bihNodes.AsNativeArray<BIHNode>(),
                edges = world.edgeMeshContainer.edges.AsNativeArray<Edge>(),
                edgeVertices = world.edgeMeshContainer.vertices.AsNativeArray<float2>(),

                heightFieldHeaders = world.heightFieldContainer.headers.AsNativeArray<HeightFieldHeader>(),
                heightFieldSamples = world.heightFieldContainer.samples.AsNativeArray<float>(),

                contactsQueue = colliderContactQueue.AsParallelWriter(),
                solverToWorld = solver.solverToWorld,
                worldToSolver = solver.worldToSolver,
                deltaTime = deltaTime,
                parameters = solver.abstraction.parameters
            };

            return generateColliderContactsJob.Schedule(solver.simplexCounts.simplexCount, 16, inputDeps);

        }

    }
}
#endif