using LearningExperienceEngine;
using UnityEngine;

namespace MirageXR
{
    public class GlyphEditor : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private LearningExperienceEngine.StepTrigger stepTrigger;
        [SerializeField] private GlyphListItem _glyphListItemPrefab;
        [SerializeField] private ActionObject[] _actionObjects;


        private Transform _annotationStartingPoint;
        private LearningExperienceEngine.Action _action;
        private LearningExperienceEngine.ToggleObject _annotationToEdit;

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

            GenerateActionList();

            if (_annotationToEdit != null)
            {
                var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == _annotationToEdit.poi);
                var duration = trigger != null ? trigger.duration : 1;
                var stepNumber = trigger != null ? trigger.value : "1";
                stepTrigger.Initiate(_annotationToEdit, duration, stepNumber);
            }

        }

        private void GenerateActionList()
        {
            foreach (var actionObject in _actionObjects)
            {
                var item = Instantiate(_glyphListItemPrefab, _contentContainer);
                item.Init(actionObject, Create);
            }
        }


        public void Create(string iconName, Sprite sprite)
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

            // change predicate on all steps
            activityManager.ActionsOfTypeAction.ForEach(a =>
            {
                var anno = a.enter.activates.Find(t => t.poi == _annotationToEdit.poi);
                if (anno != null)
                {
                    anno.predicate = "act:" + iconName;
                }
            });


            stepTrigger.MyPoi = _annotationToEdit;
            stepTrigger.SetupTrigger();


            LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(_action);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();

            Close();
        }
    }

}
