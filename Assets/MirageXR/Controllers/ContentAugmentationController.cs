using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Responsible for communicating between the views and the data model.
    /// </summary>
    public class ContentAugmentationController : MonoBehaviour
    {
        // Start: add listeners
        private void Start()
        {
            LearningExperienceEngine.EventManager.OnAugmentationDeleted += OnAugmentationDeleted;
            LearningExperienceEngine.EventManager.OnAugmentationRestored += OnAugmentationRestored;
        }

        // on Destroy: remove listeners
        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnAugmentationDeleted -= OnAugmentationDeleted;
            LearningExperienceEngine.EventManager.OnAugmentationRestored -= OnAugmentationRestored;
        }

        /// <summary>
        /// Gets called when a content augmentation has been deleted from the data model
        /// </summary>
        /// <param name="contentAugmentation">reference to the content augmentation object</param>
        private void OnAugmentationDeleted (LearningExperienceEngine.ToggleObject contentAugmentation )
        {
            LearningExperienceEngine.EventManager.DeactivateObject(contentAugmentation);
        }

        private void OnAugmentationRestored(LearningExperienceEngine.ToggleObject restoreObject)
        {
            LearningExperienceEngine.EventManager.ActivateObject(restoreObject);
        }

    }
}
