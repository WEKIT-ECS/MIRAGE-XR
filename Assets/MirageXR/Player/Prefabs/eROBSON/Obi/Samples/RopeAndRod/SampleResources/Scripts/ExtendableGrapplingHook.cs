using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

/**
 * Sample component that shows how to use Obi Rope to create a grappling hook for a 2.5D game.
 * 95% of the code is the grappling hook logic (user input, scene raycasting, launching, attaching the hook, etc) and parameter setup,
 * to show how to use Obi completely at runtime. This might not be practical for real-world scenarios,
 * but illustrates how to do it.
 *
 * Note that the choice of using actual rope simulation for grapple dynamics is debatable. Usually
 * a simple spring works better both in terms of performance and controllability. 
 *
 * If complex interaction is required with the scene, a purely geometry-based approach (ala Worms ninja rope) can
 * be the right choice under certain circumstances.
 */
public class ExtendableGrapplingHook : MonoBehaviour
{

    public ObiSolver solver;
    public ObiCollider character;

    public Material material;
    public ObiRopeSection section;

    [Range(0, 1)]
    public float hookResolution = 0.5f;
    public float hookExtendRetractSpeed = 2;
    public float hookShootSpeed = 30;
    public int particlePoolSize = 100;

    private ObiRope rope;
    private ObiRopeBlueprint blueprint;
    private ObiRopeExtrudedRenderer ropeRenderer;

    private ObiRopeCursor cursor;

    private RaycastHit hookAttachment;

    void Awake()
    {

        // Create both the rope and the solver:	
        rope = gameObject.AddComponent<ObiRope>();
        ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = section;
        ropeRenderer.uvScale = new Vector2(1, 4);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        rope.GetComponent<MeshRenderer>().material = material;

        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;
        blueprint.pooledParticles = particlePoolSize;

        // Tweak rope parameters:
        rope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        cursor = rope.gameObject.AddComponent<ObiRopeCursor>();
        cursor.cursorMu = 0;
        cursor.direction = true;
    }

    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }

    /**
	 * Raycast against the scene to see if we can attach the hook to something.
	 */
    private void LaunchHook()
    {

        // Get the mouse position in the scene, in the same XY plane as this object:
        Vector3 mouse = Input.mousePosition;
        mouse.z = transform.position.z - Camera.main.transform.position.z;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        // Get a ray from the character to the mouse:
        Ray ray = new Ray(transform.position, mouseInScene - transform.position);

        // Raycast to see what we hit:
        if (Physics.Raycast(ray, out hookAttachment))
        {
            // We actually hit something, so attach the hook!
            StartCoroutine(AttachHook());
        }

    }

    private IEnumerator AttachHook()
    {
        yield return null;

        // Clear pin constraints:
        var pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();

        Vector3 localHit = rope.transform.InverseTransformPoint(hookAttachment.point);

        // Procedurally generate the rope path (just a short segment, as we will extend it over time):
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        // Generate the particle representation of the rope (wait until it has finished):
        yield return blueprint.Generate();
        
        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        rope.ropeBlueprint = blueprint;

        // wait one frame:
        yield return null;

        rope.GetComponent<MeshRenderer>().enabled = true;

        // set masses to zero, as we're going to override positions while we extend the rope:
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 0;

        float currentLength = 0;

        // while the last particle hasn't reached the hit, extend the rope:
        while (true)
        {
            // calculate rope origin in solver space:
            Vector3 origin = solver.transform.InverseTransformPoint(rope.transform.position);

            // update direction and distance to hook point:
            Vector3 direction = hookAttachment.point - origin;
            float distance = direction.magnitude;
            direction.Normalize();

            // increase length:
            currentLength += hookShootSpeed * Time.deltaTime;

            // if we have reached the desired length, break the loop:
            if (currentLength >= distance)
            {
                cursor.ChangeLength(distance);
                break;
            }

            // change rope length (clamp to distance between rope origin and hook to avoid overshoot)
            cursor.ChangeLength(Mathf.Min(distance, currentLength));

            // iterate over all particles in sequential order, placing them in a straight line while
            // respecting element length:
            float length = 0;
            for (int i = 0; i < rope.elements.Count; ++i)
            {
                solver.positions[rope.elements[i].particle1] = origin + direction * length;
                solver.positions[rope.elements[i].particle2] = origin + direction * (length + rope.elements[i].restLength);
                length += rope.elements[i].restLength;
            }

            // wait one frame:
            yield return null;
        }

        // restore masses so that the simulation takes over now that the rope is in place:
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 10; // 1/0.1 = 10

        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(rope.elements[0].particle1, character, transform.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(rope.elements[rope.elements.Count-1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);

        rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }

    private void DetachHook()
    {
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        rope.ropeBlueprint = null;
        rope.GetComponent<MeshRenderer>().enabled = false;
    }


    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (!rope.isLoaded)
                LaunchHook();
            else
                DetachHook();
        }

        if (rope.isLoaded)
        {
            if (Input.GetKey(KeyCode.W))
            {
                cursor.ChangeLength(rope.restLength - hookExtendRetractSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                cursor.ChangeLength(rope.restLength + hookExtendRetractSpeed * Time.deltaTime);
            }
        }
    }
}
