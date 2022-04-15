using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class GazeSpinner : MonoBehaviour
    {
        [SerializeField] private Image fillCircle;
        [SerializeField] private Transform particleContainer;
        private float progress = 0f;

        public float Duration
        {
            get; set;
        }

        public int stepNumber
        {
            get; set;
        }

        private void Start()
        {
            StartCoroutine(Spin());
        }

        IEnumerator Spin()
        {
            yield return new WaitForSeconds(0.1f);

            var startTime = Time.time;

            while (Time.time - startTime < Duration)
            {
                progress += 1.0f / Duration * Time.deltaTime;
                fillCircle.fillAmount = progress;
                particleContainer.rotation = Quaternion.Euler(new Vector3(0f, 0f, -progress * 360));
                yield return null;
            }

            var activityManager = ActivityManager.Instance;
            if (activityManager.ActiveAction != null)
            {
                activityManager.ActiveAction.isCompleted = true;
            }

            activityManager.ActivateActionByIndex(stepNumber);
            TaskStationDetailMenu.Instance.SelectedButton = null;
        }
    }
}
