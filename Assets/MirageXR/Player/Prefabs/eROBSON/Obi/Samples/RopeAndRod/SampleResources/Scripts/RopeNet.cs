using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

/**
 * Sample component that procedurally generates a net using rigidbodies and ropes.
 */
public class RopeNet : MonoBehaviour
{
    public Material material;

    public Vector2Int resolution = new Vector2Int(5, 5);
    public Vector2 size = new Vector2(0.5f, 0.5f);
    public float nodeSize = 0.2f;

    void Awake()
    {
        // create an object containing both the solver and the updater:
        GameObject solverObject = new GameObject("solver", typeof(ObiSolver), typeof(ObiFixedUpdater));
        ObiSolver solver = solverObject.GetComponent<ObiSolver>();
        ObiFixedUpdater updater = solverObject.GetComponent<ObiFixedUpdater>();
        updater.substeps = 2;

        // adjust solver settings:
        solver.particleCollisionConstraintParameters.enabled = false;
        solver.distanceConstraintParameters.iterations = 8;
        solver.pinConstraintParameters.iterations = 4;
        solver.parameters.sleepThreshold = 0.001f;
        solver.PushSolverParameters();

        // add the solver to the updater:
        updater.solvers.Add(solver);

        // create the net (ropes + rigidbodies)
        CreateNet(solver);
    }

    private void CreateNet(ObiSolver solver)
    {
        ObiCollider[,] nodes = new ObiCollider[resolution.x + 1, resolution.y + 1];

        for (int x = 0; x <= resolution.x; ++x)
        {
            for (int y = 0; y <= resolution.y; ++y)
            {
                GameObject rb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rb.AddComponent<Rigidbody>();
                rb.transform.position = new Vector3(x, y, 0) * size;
                rb.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);

                nodes[x, y] = rb.AddComponent<ObiCollider>();
                nodes[x, y].Filter = 1;
            }
        }

        nodes[0, resolution.y].GetComponent<Rigidbody>().isKinematic = true;
        nodes[resolution.x, resolution.y].GetComponent<Rigidbody>().isKinematic = true;

        for (int x = 0; x <= resolution.x; ++x)
        {
            for (int y = 0; y <= resolution.y; ++y)
            {
                Vector3 pos = new Vector3(x, y, 0) * size;
                if (x < resolution.x)
                {
                    Vector3 offset = new Vector3(nodeSize * 0.5f, 0, 0);
                    var rope = CreateRope(pos + offset, pos + new Vector3(size.x, 0, 0) - offset);
                    rope.transform.parent = solver.transform;

                    PinRope(rope, nodes[x, y], nodes[x + 1, y], new Vector3(0.5f, 0, 0), -new Vector3(0.5f, 0, 0));
                }

                if (y < resolution.y)
                {
                    Vector3 offset = new Vector3(0, nodeSize * 0.5f, 0);
                    var rope = CreateRope(pos + offset, pos + new Vector3(0, size.y, 0) - offset);
                    rope.transform.parent = solver.transform;

                    PinRope(rope, nodes[x, y], nodes[x, y + 1], new Vector3(0, 0.5f, 0), -new Vector3(0, 0.5f, 0));
                }
            }
        }
    }

    private void PinRope(ObiRope rope, ObiCollider bodyA, ObiCollider bodyB, Vector3 offsetA, Vector3 offsetB)
    {
        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        var pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(rope.solverIndices[0], bodyA, offsetA, Quaternion.identity, 0, 999, float.PositiveInfinity);
        batch.AddConstraint(rope.solverIndices[rope.activeParticleCount - 1], bodyB, offsetB, Quaternion.identity, 0, 999, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
    }

    // Creates a rope between two points in world space:
    private ObiRope CreateRope(Vector3 pointA, Vector3 pointB)
    {
        // Create a rope
        var ropeObject = new GameObject("solver", typeof(ObiRope), typeof(ObiRopeLineRenderer));
        var rope = ropeObject.GetComponent<ObiRope>();
        var ropeRenderer = ropeObject.GetComponent<ObiRopeLineRenderer>();
        rope.GetComponent<MeshRenderer>().material = material;
        rope.GetComponent<ObiPathSmoother>().decimation = 0.1f;
        ropeRenderer.uvScale = new Vector2(1, 5);

        // Setup a blueprint for the rope:
        var blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.15f;
        blueprint.thickness = 0.02f;
        blueprint.pooledParticles = 0;

        // convert both points to the rope's local space:
        pointA = rope.transform.InverseTransformPoint(pointA);
        pointB = rope.transform.InverseTransformPoint(pointB);

        // Procedurally generate the rope path (a simple straight line):
        Vector3 direction = (pointB - pointA) * 0.25f;
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(pointA, -direction, direction, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "A");
        blueprint.path.AddControlPoint(pointB, -direction, direction, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "B");
        blueprint.path.FlushEvents();

        rope.ropeBlueprint = blueprint;
        return rope;
    }
}
