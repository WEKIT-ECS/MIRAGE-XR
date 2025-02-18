using LearningExperienceEngine;
using UnityEngine;

namespace MirageXR
{
    public class VFXEditor : MonoBehaviour
    {
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private VfxListItem _vfxListItemPrefab;
        [SerializeField] private VfxObject[] _vfxObjects;

        private LearningExperienceEngine.Action _action;
        private LearningExperienceEngine.ToggleObject _annotationToEdit;
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

        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
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
                if (icon.gameObject != _contentContainer.gameObject)
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
                LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);
            }
            else
            {
                var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
                Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_action.id));
                GameObject originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(_annotationStartingPoint.transform.position,
                    _annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                _annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
            }

            _annotationToEdit.predicate = "effect:" + iconName;
            LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(_action);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
            Close();
        }
    }
}
