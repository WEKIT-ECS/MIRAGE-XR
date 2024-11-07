using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirageXR
{
    public class SidebarResizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform [] checkmarks;
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
                foreach (var checkmark in checkmarks)
                {
                    var newCheckmarkWidth = Mathf.Lerp(checkmark.sizeDelta.x, targetCheckmarkWidth, Time.deltaTime * resizeSpeed);
                    checkmark.sizeDelta = new Vector2(newCheckmarkWidth, checkmark.sizeDelta.y);
                    if (!Mathf.Approximately(checkmark.sizeDelta.x, targetCheckmarkWidth))
                        checkmarksResizing = true;
                }
                yield return null;
            }
        }
    }
}
