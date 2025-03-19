using UnityEngine;
namespace MirageXR.View
{
    [RequireComponent(typeof(ContentView))]
    public class StepViewManipulationSynchronizer : MonoBehaviour
    {
        private StepView _stepView;

        private void Awake()
        {
            _stepView = GetComponent<StepView>();
        }

        private void Start()
        {
            _stepView.OnManipulationStartedEvent.AddListener(OnManipulationStarted);
            _stepView.OnManipulationEndedEvent.AddListener(OnManipulationEnded);
        }

        private void OnManipulationStarted(Transform target) { }

        private void OnManipulationEnded(Transform target)
        {
            RootObject.Instance.LEE.StepManager.UpdateStep(_stepView.Step);
        }
    }
}
