using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{

    public class TimelineObjectBehaviour : MonoBehaviour
    {

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