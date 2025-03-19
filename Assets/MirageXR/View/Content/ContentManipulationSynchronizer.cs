using UnityEngine;

namespace MirageXR.View
{
    [RequireComponent(typeof(ContentView))]
    public class ContentManipulationSynchronizer : MonoBehaviour
    {
        private ContentView _contentView;

        private void Awake()
        {
            _contentView = GetComponent<ContentView>();
        }

        private void Start()
        {
            _contentView.OnManipulationStartedEvent.AddListener(OnManipulationStarted);
            _contentView.OnManipulationEndedEvent.AddListener(OnManipulationEnded);
        }

        private void OnManipulationStarted(Transform target) { }

        private void OnManipulationEnded(Transform target)
        {
            RootObject.Instance.LEE.ContentManager.UpdateContent(_contentView.GetContent());
        }
    }
}
