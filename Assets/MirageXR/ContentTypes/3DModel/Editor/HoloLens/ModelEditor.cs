using UnityEngine;

namespace MirageXR
{
    public class ModelEditor : MonoBehaviour
    {
        private Action action;
        private ToggleObject annotationToEdit;
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


        public void Open(Action action, ToggleObject annotation)
        {
            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;
        }


        public void Create(string modelName)
        {
            modelName = ZipUtilities.CheckFileForIllegalCharacters(modelName);

            if (annotationToEdit != null)
            {
                annotationToEdit.predicate = "3d:" + modelName;
                EventManager.DeactivateObject(annotationToEdit);
            }
            else
            {
                var workplaceManager = RootObject.Instance.workplaceManager;
                var detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
                var originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                    annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                annotationToEdit = RootObject.Instance.augmentationManager.AddAugmentation(action, offset);
                annotationToEdit.option = modelName;
                annotationToEdit.predicate = "3d:" + modelName;
                annotationToEdit.url = "3d:" + modelName;
            }

            EventManager.ActivateObject(annotationToEdit);
            EventManager.NotifyActionModified(action);
            
            RootObject.Instance.activityManager.SaveData();

            Close();
        }
    }
}
