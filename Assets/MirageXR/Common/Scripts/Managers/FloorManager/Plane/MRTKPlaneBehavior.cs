using UnityEngine;

public class MRTKPlaneBehavior : MonoBehaviour, IPlaneBehaviour
{
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
