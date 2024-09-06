﻿using UnityEngine;

namespace MirageXR
{
    public class PlaceBehaviour : MonoBehaviour
    {
        public LearningExperienceEngine.Place Place { get; set; }

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnClearAll += Delete;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnClearAll -= Delete;
        }

        private void Delete()
        {
            Destroy(gameObject);
        }
    }
}