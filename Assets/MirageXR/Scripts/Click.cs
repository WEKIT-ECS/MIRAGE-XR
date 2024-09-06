using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Universal Click.
    /// TODO: This is a really weird way of making the audiosource play. replace??
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Click : MonoBehaviour
    {

        void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnClick += DoClick;
        }

        void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnClick -= DoClick;
        }

        void DoClick()
        {
            GetComponent<AudioSource>().Play();
        }
    }
}

