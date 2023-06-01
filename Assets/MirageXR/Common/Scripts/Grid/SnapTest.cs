using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using UnityEngine;

public class SnapTest : MonoBehaviour
{
    [SerializeField] private ObjectManipulator _manipulator;

    private void Start()
    {
        _manipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
    }

    private void OnManipulationEnded(ManipulationEventData eventData)
    {
        var position = RootObject.Instance.gridManager.GetSnapPosition(transform.position);
        transform.position = position;
    }
}
