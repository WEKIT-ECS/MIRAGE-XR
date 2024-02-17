using UnityEngine;
using Obi;

public class RopeBetweenTwoPoints : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public ObiSolver solver;

    void Start()
    {
        // Generate rope synchronously:
        Generate();
    }

    void Generate()
    {
        if (start != null && end != null)
        {
            // Adjust our transform:
            transform.position = (start.position + end.position) / 2;
            transform.rotation = Quaternion.FromToRotation(Vector3.right, end.position - start.position);

            // Calculate control point positions and tangent vector:
            Vector3 startPositionLS = transform.InverseTransformPoint(start.position);
            Vector3 endPositionLS = transform.InverseTransformPoint(end.position);
            Vector3 tangentLS = (endPositionLS - startPositionLS).normalized;

            // Create the blueprint: 
            var blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();

            // Build the rope path:
            int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
            blueprint.path.AddControlPoint(startPositionLS, -tangentLS, tangentLS, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "start");
            blueprint.path.AddControlPoint(endPositionLS, -tangentLS, tangentLS, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "end");
            blueprint.path.FlushEvents();

            // Generate particles/constraints:
            blueprint.GenerateImmediate();

            // Add rope actor / renderer / attachment components:
            var rope = gameObject.AddComponent<ObiRope>();
            var ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
            var attachment1 = gameObject.AddComponent<ObiParticleAttachment>();
            var attachment2 = gameObject.AddComponent<ObiParticleAttachment>();

            // Load the default rope section for rendering:
            ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");

            // Set the blueprint:
            rope.ropeBlueprint = blueprint;

            // Attach both ends:
            attachment1.target = start;
            attachment2.target = end;
            attachment1.particleGroup = blueprint.groups[0];
            attachment2.particleGroup = blueprint.groups[1];

            // Parent the actor under a solver to start the simulation:
            transform.SetParent(solver.transform);
        }
    }
}
