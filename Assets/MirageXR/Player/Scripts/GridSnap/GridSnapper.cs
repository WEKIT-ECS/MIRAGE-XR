using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// MonoBehaviour to be attached to objects that can be manipulated.
/// Requires an instance (singleton) of the <see cref="GridGenerator"/> class in the scene.
/// </summary>
[RequireComponent(typeof(ObjectManipulator))]
public class GridSnapper : MonoBehaviour
{
    /* Maybe init with default material and grab object reference automatically.
    * Sometimes the reference object mesh might not be at the same hierarchy, that's
    * why it's currently assigned manually.
    */
    [SerializeField]
    private GameObject ghostObjectReference;
    [SerializeField]
    private Material ghostMaterial;
    private GameObject ghostObject;
    private ObjectManipulator objectManipulator;

    private bool isManipulating;
    private Vector3 lastSnapped;

    private void Start()
    {
        objectManipulator = GetComponent<ObjectManipulator>();
        objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
        objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);

        CreateGhost();
    }

    private void Update()
    {
        // Only do something if manipulation is underway.
        if (!isManipulating)
        {
            return;
        }

        var snapped = GetSnappedPosition();

        // Only recalculate snapped position and rerender grid if snap position changes.
        if (snapped != lastSnapped)
        {
            ghostObject.transform.position = snapped;
            GridGenerator.Instance.RenderGrid(snapped);
            lastSnapped = snapped;
        }

        // For now always apply rotation. Would need to be changes when rotation snapping is implemented.
        ghostObject.transform.rotation = transform.rotation;
    }

    /// <summary>
    /// Listener that activates ghost and grid visualization upon manipulation start.
    /// </summary>
    private void OnManipulationStarted(ManipulationEventData data)
    {
        isManipulating = true;
        ghostObject.SetActive(true);
        ghostObject.transform.position = transform.position;
        GridGenerator.Instance.RenderGrid(lastSnapped);
    }

    /// <summary>
    /// Listener that deactivates ghost and grid visualization upon manipulation start.
    /// Also snaps the manipulated object to the nearest grid point.
    /// </summary>
    private void OnManipulationEnded(ManipulationEventData data)
    {
        isManipulating = false;
        ghostObject.SetActive(false);
        transform.position = GetSnappedPosition();
        GridGenerator.Instance.ParticleSystem.Clear();
    }

    /// <summary>
    /// Calculates the closest grid point to the current position.
    /// </summary>
    /// <returns>Closest grid point</returns>
    private Vector3 GetSnappedPosition()
    {
        Vector3 currentPosition = transform.position;
        var snapIncrement = GridGenerator.Instance.snapIncrement;

        Vector3 snappedPosition = new Vector3(
            Mathf.Round(currentPosition.x / snapIncrement) * snapIncrement,
            Mathf.Round(currentPosition.y / snapIncrement) * snapIncrement,
            Mathf.Round(currentPosition.z / snapIncrement) * snapIncrement
        );

        return snappedPosition;
    }

    /// <summary>
    /// Calculates the closest rotation angle to the current rotation.
    /// </summary>
    /// <returns>Snapped target rotation</returns>
    private Quaternion GetSnappedRotation()
    {
        // Insert rotation snapping
        return Quaternion.identity;
    }

    /// <summary>
    /// Creates the ghost instance of the manipulated object.
    /// </summary> 
    private void CreateGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }

        ghostObject = Instantiate(ghostObjectReference);
        var renderer = ghostObject.GetComponent<Renderer>();
        renderer.material = ghostMaterial;
        ghostObject.SetActive(false);
    }
}