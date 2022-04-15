using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirageXR
{
    public class PathSegmentsController : MonoBehaviour
    {
        [SerializeField] private Transform horizontalPart;
        [SerializeField] private Transform curvePart;
        [SerializeField] private Transform verticalPart;
        [SerializeField] private Transform lowerDisplay;
        [SerializeField] private Transform upperDisplay;

        public Transform calibrationButton;

        public Transform startTransform;
        public Transform endTransform;

        public float startOffset;
        public float endOffset;

        public bool IsVisible
        {
            get; set;
        }

        // the extend of the curved corner which must be deducted from the vertical and horizontal distance
        private const float curveExtend = 0.25f;

        private void Awake()
        {
            IsVisible = gameObject.activeSelf;
        }

        private void Update()
        {

            Vector3 projectedStart = new Vector3(startTransform.position.x, 0, startTransform.position.z);
            Vector3 projectedEnd = new Vector3(endTransform.position.x, 0, endTransform.position.z);

            float horizontalDistance = Vector3.Distance(projectedStart, projectedEnd);
            float verticalDistance = endTransform.position.y - startTransform.position.y;

            // curve's origin is always underneath the end
            transform.position = new Vector3(
                endTransform.position.x,
                startTransform.position.y,
                endTransform.position.z);

            float lengthHorizontalPart = Mathf.Max(0, horizontalDistance - startOffset - curveExtend);
            horizontalPart.localScale = new Vector3(1, 1, lengthHorizontalPart);

            lowerDisplay.localPosition = new Vector3(
                lowerDisplay.localPosition.x,
                0.1f,
                horizontalDistance - startOffset);

            if (calibrationButton)
            {
                calibrationButton.localPosition = new Vector3(
                    calibrationButton.localPosition.x,
                    0,
                    horizontalDistance - startOffset);
            }


            bool verticalOnlyMode = lengthHorizontalPart == 0;
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
                lengthVertical = Mathf.Max(0, verticalDistance - endOffset - curveExtend);
                verticalPart.localPosition = new Vector3(0, curveExtend, 0);
            }
            verticalPart.localScale = new Vector3(1, lengthVertical, 1);

            upperDisplay.localPosition = new Vector3(
                upperDisplay.localPosition.x,
                verticalDistance - endOffset,
                0);

            // set the rotation: the curve should be rotated towards the start and must be upright
            Vector3 forwardVector = projectedStart - projectedEnd;
            if (forwardVector != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardVector, Vector3.up);
            }
        }
    }
}