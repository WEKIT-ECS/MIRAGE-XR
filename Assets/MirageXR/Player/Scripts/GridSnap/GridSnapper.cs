using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using i5.Toolkit.Core.OpenIDConnectClient;

public class GridSnapper : MonoBehaviour
{
    private ObjectManipulator objectManipulator;

    public GameObject ghostObjectReference;
    public Material ghostMaterial;
    private GameObject ghostObject;

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
        if (!isManipulating)
            return;

        var snapped = GetSnappedPosition();

        if (snapped != lastSnapped)
        {
            ghostObject.transform.position = snapped;
            GridGenerator.Instance.RenderGrid(snapped);
            lastSnapped = snapped;
        }

        ghostObject.transform.rotation = transform.rotation;
    }

    private void OnManipulationStarted(ManipulationEventData data)
    {
        isManipulating = true;
        ghostObject.SetActive(true);
        ghostObject.transform.position = transform.position;
        Debug.Log("Start man");
        GridGenerator.Instance.RenderGrid(lastSnapped);
    }

    private void OnManipulationEnded(ManipulationEventData data)
    {
        isManipulating = false;
        ghostObject.SetActive(false);
        Debug.Log("End man");
        transform.position = GetSnappedPosition();
        GridGenerator.Instance.ParticleSystem.Clear();
    }

    private Vector3 GetSnappedPosition()
    {
        Debug.Log("SNAP");
        Vector3 currentPosition = transform.position;
        var snapIncrement = GridGenerator.Instance.snapIncrement;

        Vector3 snappedPosition = new Vector3(
            Mathf.Round(currentPosition.x / snapIncrement) * snapIncrement,
            Mathf.Round(currentPosition.y / snapIncrement) * snapIncrement,
            Mathf.Round(currentPosition.z / snapIncrement) * snapIncrement
        );

        return snappedPosition;
    }

    private void CreateGhost()
    {
        ghostObject = Instantiate(ghostObjectReference);
        var renderer = ghostObject.GetComponent<Renderer>();
        renderer.material = ghostMaterial;
    }
}