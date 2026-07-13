using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirageXR
{
    public class SidebarResizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform [] checkmarks;
        [SerializeField] private BoxCollider [] checkmarkColliders;
        [SerializeField] private float targetPanelWidth = 174f;
        [SerializeField] private float targetCheckmarkWidth = 174f;
        [SerializeField] private float resizeSpeed = 5f;

        private float _initialPanelWidth;
        private float _initialCheckmarkWidth;
        private Coroutine _resizeCoroutine;

        private void Start()
        {
            if (panel == null)
            {
                panel = GetComponent<RectTransform>();
            }
            _initialPanelWidth = panel.sizeDelta.x;
            if (checkmarks.Length > 0)
            {
                _initialCheckmarkWidth = checkmarks[0].sizeDelta.x;
            }
            ResizeCheckmarkColliders();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartResizing(targetPanelWidth, targetCheckmarkWidth);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartResizing(_initialPanelWidth, _initialCheckmarkWidth);
        }

        private void StartResizing(float targetPanelWidth, float targetCheckmarkWidth)
        {
            if (_resizeCoroutine != null)
            {
                StopCoroutine(_resizeCoroutine);
            }
            _resizeCoroutine = StartCoroutine(ResizeElements(targetPanelWidth, targetCheckmarkWidth));
        }

        private IEnumerator ResizeElements(float targetPanelWidth, float targetCheckmarkWidth)
        {
            var panelResizing = true;
            var checkmarksResizing = true;

            while (panelResizing || checkmarksResizing)
            {
                var newPanelWidth = Mathf.Lerp(panel.sizeDelta.x, targetPanelWidth, Time.deltaTime * resizeSpeed);
                panel.sizeDelta = new Vector2(newPanelWidth, panel.sizeDelta.y);
                panelResizing = !Mathf.Approximately(panel.sizeDelta.x, targetPanelWidth);
                
                checkmarksResizing = false;
                for (var i = 0; i < checkmarks.Length; i++)
                {
                    var checkmark = checkmarks[i];
                    var newCheckmarkWidth = Mathf.Lerp(checkmark.sizeDelta.x, targetCheckmarkWidth, Time.deltaTime * resizeSpeed);
                    checkmark.sizeDelta = new Vector2(newCheckmarkWidth, checkmark.sizeDelta.y);
                    ResizeCheckmarkCollider(i, checkmark);
                    if (!Mathf.Approximately(checkmark.sizeDelta.x, targetCheckmarkWidth))
                        checkmarksResizing = true;
                }
                yield return null;
            }
        }

        private void ResizeCheckmarkColliders()
        {
            for (var i = 0; i < checkmarks.Length; i++)
            {
                ResizeCheckmarkCollider(i, checkmarks[i]);
            }
        }

        private void ResizeCheckmarkCollider(int colliderIndex, RectTransform checkmark)
        {
            var boxCollider = checkmarkColliders[colliderIndex];
            var rect = checkmark.rect;
            boxCollider.size = new Vector3(rect.width, rect.height, boxCollider.size.z);
            boxCollider.center = new Vector3(rect.center.x, rect.center.y, boxCollider.center.z);
        }
    }
}
