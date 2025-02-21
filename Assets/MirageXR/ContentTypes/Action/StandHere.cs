using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class StandHere : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private GameObject eyeLens;
        [SerializeField] private GameObject lookingPoint;

        private bool _triggerActivated;
        private bool notificationDisplayed;


        private void Start()
        {
            InvokeRepeating(nameof(CheckPlayerBehaviour), 0f, 0.1f);
        }

        private void CheckPlayerBehaviour()
        {
            if (_triggerActivated) return;

            var screenPoint = Camera.main.WorldToViewportPoint(lookingPoint.transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0.3 && screenPoint.x < 0.6 && screenPoint.y > 0.3 && screenPoint.y < 0.6;

            var playerDistanceToEyeLens = Vector3.Distance(Camera.main.transform.position, eyeLens.transform.position);

            if (onScreen && playerDistanceToEyeLens < 0.15f && !_triggerActivated)
            {

                var activeAction = activityManager.ActiveAction;
                if (activityManager.ActionsOfTypeAction.IndexOf(activeAction) == activityManager.ActionsOfTypeAction.Count - 1 && !notificationDisplayed)
                {
                    /// give the info and close
                    DialogWindow.Instance.Show("Info!",
                    "This is the last step. The trigger is disabled!\n Add a new step and try again.",
                    new DialogButtonContent("Ok"));
                    notificationDisplayed = true;
                }
                else
                {
                    ActionListMenu.Instance.NextAction();
                    _triggerActivated = true;
                }

            }

        }

    }

}
