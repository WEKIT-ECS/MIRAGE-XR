using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Responsible for communicating between the views and the data model.
    /// </summary>
    public class ContentAugmentationController : MonoBehaviour
    {
        // Start: add listeners
        void Start()
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
        /// <param name="contentAugmentation">reference to the the content augmentation object</param>
        void OnAugmentationDeleted (LearningExperienceEngine.ToggleObject contentAugmentation )
        {
            LearningExperienceEngine.EventManager.DeactivateObject(contentAugmentation);
        }

        void OnAugmentationRestored(LearningExperienceEngine.ToggleObject restoreObject)
        {
            LearningExperienceEngine.EventManager.ActivateObject(restoreObject);
        }

    }
}
