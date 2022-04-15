using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the aura placement
/// Follows a target object and places the aura underneath its
/// </summary>
public class UIOrigin : MonoBehaviour
{
    [Tooltip("Layer mask that filters for objects belonging to the floor, e.g. the spatial mapping")]
    [SerializeField] private LayerMask floorLayer;

    // the object that the placement should follow
    private Transform followTarget;

    private float heightOffset = 1.75f;
    private Vector3 currentPosition;

    private List<float> raycastResults = new List<float>();

    /// <summary>
    /// Singleton instance of the placement script
    /// </summary>
    public static UIOrigin Instance;

    public float CurrentFloorYPosition() { return currentPosition.y; }

    // Sets the main camera as the target that the aura should follow
    private void Awake()
    {
        Instance = this;

        // Attach mixed reality camera as the follow target.
        followTarget = GameObject.FindWithTag("MainCamera").transform;
    }

    // initializes the currentPosition vector
    private void Start()
    {
        currentPosition = Vector3.zero;
        currentPosition.y = followTarget.position.y - heightOffset;
    }

    // Updates the position of the aura
    // Calculates the floor height and puts the aura onto the floor
    private void Update()
    {
        currentPosition.x = followTarget.position.x;
        currentPosition.z = followTarget.position.z;

        currentPosition.y = GetFloorHeight(followTarget.position, 0.75f, 6) + 0.03f;

        transform.position = currentPosition;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(followTarget.forward, Vector3.up).normalized, Vector3.up);
    }

    // Calculates the floor height
    // Casts a given number of rays in a circular shape around the user to find the floor heights
    // It finds the lowest found point, spans a 20cm zone above this and then takes the highest raycast hit within this zone
    // This should prevent the aura from jumping onto furniture, e.g. if the user sits on a chair
    // It should also lower the probability that the aura is placed underneath the floor, e.g. in environments with steps
    private float GetFloorHeight(Vector3 centerPos, float radius, int numberOfRaycasts)
    {
        float minimumValue = centerPos.y;

        raycastResults.Clear();

        // create rays circular around a center position with a given radius

        for (int i = 0; i < numberOfRaycasts; i++)
        {
            float radians = 2 * Mathf.PI / numberOfRaycasts * i;

            float vertical = Mathf.Sin(radians);
            float horizontal = Mathf.Cos(radians);

            Vector3 spawnDir = new Vector3(horizontal, centerPos.y, vertical);

            Vector3 raycastOrigin = centerPos + spawnDir * radius;

            Ray ray = new Ray(raycastOrigin, Vector3.down);
            Debug.DrawLine(raycastOrigin, raycastOrigin + 2f * ray.direction);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f, floorLayer))
            {
                raycastResults.Add(hit.point.y);
                minimumValue = Mathf.Min(minimumValue, hit.point.y);
            }
        }

        float floorHeight;

        // if we did not find the floor with any raycast, we just take a default height were we assume the floor to be
        if (raycastResults.Count == 0)
        {
            floorHeight = centerPos.y - heightOffset;
        }
        // otherwise: take the lowest point, span a 20cm zone above it and take the highest floor hit in this zone
        else
        {
            floorHeight = minimumValue;
            float threshold = minimumValue + 0.2f;
            for (int i = 0; i < raycastResults.Count; i++)
            {
                if (raycastResults[i] <= threshold)
                {
                    floorHeight = Mathf.Max(floorHeight, raycastResults[i]);
                }
            }
        }

        return floorHeight;
    }
}
