using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
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

        private ObjectManipulator _startObjectManipulator;
        private ObjectManipulator _endObjectManipulator;


        private bool _pointsMoving;

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeToggle;
        }


        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeToggle;
        }

        void Start()
        {
            _startObjectManipulator = startPoint.GetComponentInParent<ObjectManipulator>();
            _endObjectManipulator = endPoint.GetComponentInParent<ObjectManipulator>();

            var myPoiManipulator = transform.parent.GetComponent<ObjectManipulator>();

            _startObjectManipulator.OnManipulationStarted.AddListener(delegate { OnMeasuringStart(); });
            _endObjectManipulator.OnManipulationStarted.AddListener(delegate { OnMeasuringStart(); });
            myPoiManipulator.OnManipulationStarted.AddListener(delegate { OnMeasuringStart(); });

            _startObjectManipulator.OnManipulationEnded.AddListener(delegate { OnMeasuringEnd(); });
            _endObjectManipulator.OnManipulationEnded.AddListener(delegate { OnMeasuringEnd(); });
            myPoiManipulator.OnManipulationEnded.AddListener(delegate { OnMeasuringEnd(); });


            UpdateMeasuring();
        }


        private void OnEditModeToggle(bool editMode)
        {
            //comment out if measuring should be disabled in playmode
            //startObjectManipulator.enabled = editMode;
            //endObjectManipulator.enabled = editMode;
        }


        private void Update()
        {
            if (_pointsMoving)
                UpdateMeasuring();
        }


        private void UpdateMeasuring()
        {
            var distanceAtoB = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);

            //show in meter if it is longer than 10 meter
            var scalability = distanceAtoB >= 10 ? 1 : 100;
            measureText.text = (distanceAtoB * scalability).ToString("0.00") + (scalability == 1 ? "m" : "cm");

            var numberContainer = measureText.transform.parent;

            //if larger than 25cm
            if (distanceAtoB >= 0.25f)
            {
                numberContainer.position = Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f);
                lineFromA.SetPosition(0, startPoint.transform.position);
                lineFromA.SetPosition(1, aToCenterConnectPoint.position);

                //enable line b
                lineFromB.enabled = true;
                lineFromB.SetPosition(0, endPoint.transform.position);
                lineFromB.SetPosition(1, bToCenterConnectPoint.position);

                var relativePos = numberContainer.transform.position - endPoint.transform.position;
                var rotation = Quaternion.LookRotation(relativePos);
                numberContainer.rotation = rotation * Quaternion.Euler(0, 90, 0);
            }
            else
            {
                numberContainer.position = Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f) + new Vector3(0, 0.1f, 0);
                lineFromA.SetPosition(0, startPoint.transform.position);
                lineFromA.SetPosition(1, endPoint.transform.position);

                //disable line b
                lineFromB.enabled = false;

                numberContainer.rotation = Quaternion.identity;
            }

            var colliderCenter = GetComponent<BoxCollider>().center;
            GetComponent<BoxCollider>().center = numberContainer.transform.localPosition;

        }



        private void OnMeasuringStart()
        {
            _pointsMoving = true;
        }


        private void OnMeasuringEnd()
        {
            _pointsMoving = false;
            LearningExperienceEngine.EventManager.NotifyOnCompletedMeasuring(measureText.text, "MeasureTool");
        }

    }

}
