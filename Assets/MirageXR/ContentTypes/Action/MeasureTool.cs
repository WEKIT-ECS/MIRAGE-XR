using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class MeasureTool : MonoBehaviour
    {
        [SerializeField] private GameObject startPoint;
        [SerializeField] private GameObject endPoint;

        [SerializeField] private Transform aToCenterConnectPoint;
        [SerializeField] private Transform bToCenterConnectPoint;

        [SerializeField] private Text measureText;

        [SerializeField] private LineRenderer lineFromA;
        [SerializeField] private LineRenderer lineFromB;

        private Vector3 _lastStartPos;
        private Vector3 _lastEndPos;

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeToggle;
        }

        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeToggle;
        }

        private void Start()
        {
            if (startPoint != null) _lastStartPos = startPoint.transform.position;
            if (endPoint != null) _lastEndPos = endPoint.transform.position;
            UpdateMeasuring();
        }

        private void OnEditModeToggle(bool editMode)
        {
        }

        private void Update()
        {
            if (startPoint == null || endPoint == null) return;

            if (startPoint.transform.position != _lastStartPos || endPoint.transform.position != _lastEndPos)
            {
                _lastStartPos = startPoint.transform.position;
                _lastEndPos = endPoint.transform.position;
                UpdateMeasuring();
            }
        }

        private void UpdateMeasuring()
        {
            if (startPoint == null || endPoint == null || measureText == null) return;

            var distanceAtoB = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);

            // show in meter if it is longer than 10 meter
            var scalability = distanceAtoB >= 10 ? 1 : 100;
            measureText.text = (distanceAtoB * scalability).ToString("0.00") + (scalability == 1 ? "m" : "cm");

            var numberContainer = measureText.transform.parent;
            if (numberContainer == null) return;

            // if larger than 25cm
            if (distanceAtoB >= 0.25f)
            {
                numberContainer.position = Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f);
                if (lineFromA != null && aToCenterConnectPoint != null)
                {
                    lineFromA.SetPosition(0, startPoint.transform.position);
                    lineFromA.SetPosition(1, aToCenterConnectPoint.position);
                }

                if (lineFromB != null && bToCenterConnectPoint != null)
                {
                    lineFromB.enabled = true;
                    lineFromB.SetPosition(0, endPoint.transform.position);
                    lineFromB.SetPosition(1, bToCenterConnectPoint.position);
                }

                var relativePos = numberContainer.transform.position - endPoint.transform.position;
                if (relativePos != Vector3.zero)
                {
                    var rotation = Quaternion.LookRotation(relativePos);
                    numberContainer.rotation = rotation * Quaternion.Euler(0, 90, 0);
                }
            }
            else
            {
                numberContainer.position = Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f) + new Vector3(0, 0.1f, 0);
                if (lineFromA != null)
                {
                    lineFromA.SetPosition(0, startPoint.transform.position);
                    lineFromA.SetPosition(1, endPoint.transform.position);
                }

                if (lineFromB != null)
                {
                    lineFromB.enabled = false;
                }

                numberContainer.rotation = Quaternion.identity;
            }

            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.center = numberContainer.transform.localPosition;
            }
        }
    }
}
