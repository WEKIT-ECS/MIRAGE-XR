using UnityEngine;

namespace MirageXR
{
    public class VFXEditor : MonoBehaviour
    {
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private VfxListItem _vfxListItemPrefab;
        [SerializeField] private VfxObject[] _vfxObjects;
        
        private Action _action;
        private ToggleObject _annotationToEdit;
        private Transform _annotationStartingPoint;

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            _annotationStartingPoint = startingPoint;
        }

        public void Close()
        {
            _action = null;
            _annotationToEdit = null;
            gameObject.SetActive(false);

            Destroy(gameObject);
        }

        public void Open(Action action, ToggleObject annotation)
        {
            gameObject.SetActive(true);
            _action = action;
            _annotationToEdit = annotation;

            GenerateVFXList();
        }

        private void OnDisable()
        {
            foreach (var icon in _contentContainer.GetComponentsInChildren<RectTransform>())
            {
                if(icon.gameObject != _contentContainer.gameObject)
                    Destroy(icon.gameObject);
            }
        }

        private void GenerateVFXList()
        {
            foreach (var vfxObject in _vfxObjects)
            {
                var item = Instantiate(_vfxListItemPrefab, _contentContainer);
                item.Init(vfxObject, Create);
            }
        }

        private void Create(string iconName)
        {
            if (_annotationToEdit != null)
            {
                EventManager.DeactivateObject(_annotationToEdit);
            }
            else
            {
                Detectable detectable = WorkplaceManager.Instance.GetDetectable(WorkplaceManager.Instance.GetPlaceFromTaskStationId(_action.id));
                GameObject originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(_annotationStartingPoint.transform.position,
                    _annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                _annotationToEdit = ActivityManager.Instance.AddAnnotation(_action, offset);
            }

            _annotationToEdit.predicate = "vfx:" + iconName;
            EventManager.ActivateObject(_annotationToEdit);
            EventManager.NotifyActionModified(_action);

            Close();
        }
    }
}
