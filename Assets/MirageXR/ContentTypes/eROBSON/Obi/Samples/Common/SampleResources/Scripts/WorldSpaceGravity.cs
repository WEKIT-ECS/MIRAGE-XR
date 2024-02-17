using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class WorldSpaceGravity : MonoBehaviour
{

    ObiSolver solver;
    public Vector3 worldGravity = new Vector3(0,-9.81f,0);

    void Awake()
    {
        solver = GetComponent<ObiSolver>();
    }

    void Update()
    {
        solver.parameters.gravity = transform.InverseTransformDirection(worldGravity);
        solver.PushSolverParameters();
    }
}
