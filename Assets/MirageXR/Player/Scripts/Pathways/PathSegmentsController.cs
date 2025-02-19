using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace MirageXR
{
    public class PathSegmentsController : MonoBehaviour
    {
        // the extend of the curved corner which must be deducted from the vertical and horizontal distance
        private const float CURVE_EXTEND = 0.25f;

        [SerializeField] private Transform horizontalPart;
        [SerializeField] private Transform curvePart;
        [SerializeField] private Transform verticalPart;
        [SerializeField] private Transform lowerDisplay;
        [SerializeField] private Transform upperDisplay;

        [SerializeField] private Transform calibrationButton;
        [SerializeField] private Interactable calibrationStartButton;
        [SerializeField] private CalibrationFlow calibrationFlowPrefab;

        public Transform startTransform;
        public Transform endTransform;

        public float startOffset;
        public float endOffset;

        private CalibrationFlow _calibrationFlow;

        public bool IsVisible
        {
            get; set;
        }

        private void Awake()
        {
            IsVisible = gameObject.activeSelf;
        }

        private void Start()
        {
            if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
            {
                lowerDisplay.gameObject.SetActive(false);

                if (calibrationButton)
                {
                    calibrationButton.gameObject.SetActive(false);
                }

                if (calibrationStartButton)
                {
                    calibrationStartButton.gameObject.SetActive(false);
                }
            }

            if (calibrationStartButton)
            {
                calibrationStartButton.OnClick.AddListener(StartCalibrationFlow);
            }
        }

        private void StartCalibrationFlow()
        {
            if (!LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.IsReady || _calibrationFlow)
            {
                return;
            }

            _calibrationFlow = Instantiate(calibrationFlowPrefab);
            _calibrationFlow.Initialization(() => _calibrationFlow = null);
        }

        private void Update()
        {
            var startPosition = startTransform.position;
            var endPosition = endTransform.position;

            var projectedStart = new Vector3(startPosition.x, 0, startPosition.z);
            var projectedEnd = new Vector3(endPosition.x, 0, endPosition.z);

            var horizontalDistance = Vector3.Distance(projectedStart, projectedEnd);
            var verticalDistance = endPosition.y - startPosition.y;

            // curve's origin is always underneath the end
            transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);

            var lengthHorizontalPart = Mathf.Max(0, horizontalDistance - startOffset - CURVE_EXTEND);
            horizontalPart.localScale = new Vector3(1, 1, lengthHorizontalPart);
            lowerDisplay.localPosition = new Vector3(lowerDisplay.localPosition.x, 0.1f, horizontalDistance - startOffset);

            if (calibrationStartButton)
            {
                calibrationStartButton.transform.localPosition = new Vector3(calibrationStartButton.transform.localPosition.x, 0.1f, horizontalDistance - startOffset);
            }

            if (calibrationButton && calibrationButton.gameObject.activeInHierarchy)
            {
                calibrationButton.localPosition = new Vector3(calibrationButton.localPosition.x, 0, horizontalDistance - startOffset);
            }

            var verticalOnlyMode = lengthHorizontalPart == 0;
            curvePart.gameObject.SetActive(!verticalOnlyMode && IsVisible);

            float lengthVertical;
            if (verticalOnlyMode)
            {
                // if we only show the vertical bar, it should cover the entire height
                lengthVertical = Mathf.Max(0, verticalDistance - endOffset);
                verticalPart.localPosition = Vector3.zero;
            }
            else
            {
                lengthVertical = Mathf.Max(0, verticalDistance - endOffset - CURVE_EXTEND);
                verticalPart.localPosition = new Vector3(0, CURVE_EXTEND, 0);
            }
            verticalPart.localScale = new Vector3(1, lengthVertical, 1);
            upperDisplay.localPosition = new Vector3(upperDisplay.localPosition.x, verticalDistance - endOffset, 0);

            // set the rotation: the curve should be rotated towards the start and must be upright
            var forwardVector = projectedStart - projectedEnd;
            if (forwardVector != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardVector, Vector3.up);
            }
        }
    }
}