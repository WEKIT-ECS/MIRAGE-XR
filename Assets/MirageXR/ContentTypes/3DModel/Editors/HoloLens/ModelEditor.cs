using LearningExperienceEngine;
using UnityEngine;

namespace MirageXR
{
    public class ModelEditor : MonoBehaviour
    {
        private LearningExperienceEngine.Action action;
        private LearningExperienceEngine.ToggleObject annotationToEdit;
        private Transform annotationStartingPoint;

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            annotationStartingPoint = startingPoint;
        }

        public void Close()
        {
            action = null;
            annotationToEdit = null;
            // gameObject.SetActive(false);
            Destroy(gameObject);
        }


        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
        {
            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;
        }


        public void Create(string modelName)
        {
            modelName = LearningExperienceEngine.ZipUtilities.RemoveIllegalCharacters(modelName);

            if (annotationToEdit != null)
            {
                annotationToEdit.predicate = "3d:" + modelName;
                LearningExperienceEngine.EventManager.DeactivateObject(annotationToEdit);
            }
            else
            {
                var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
                var detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
                var originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                    annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(action, offset);
                annotationToEdit.option = modelName;
                annotationToEdit.predicate = "3d:" + modelName;
                annotationToEdit.url = "3d:" + modelName;
            }

            LearningExperienceEngine.EventManager.ActivateObject(annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(action);

            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();

            Close();
        }
    }
}
