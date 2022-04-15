using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirageXR
{
    public class ToolTipCaster : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private string tooltipText;

        private ToolipsController toolipsController;
        private const string tooltipPrefabPath = "Prefabs/UI/Hololens/VoiceCommandTooltip";

        private GameObject _tooltipObject;


        public void SetTooltipText(string text)
        {
            tooltipText = text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Only on Hololens
            if (!PlatformManager.Instance.WorldSpaceUi) return;

            CreateTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Only on Hololens
            if (!PlatformManager.Instance.WorldSpaceUi) return;

            DestroyToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Only on Hololens
            if (!PlatformManager.Instance.WorldSpaceUi) return;

            DestroyToolTip();
        }


        private void CreateTooltip()
        {
            if (_tooltipObject != null) return;

            _tooltipObject = Instantiate(Resources.Load<GameObject>(tooltipPrefabPath));
            _tooltipObject.transform.SetParent(transform);
            var _tooltipObjectHeigth = _tooltipObject.GetComponent<RectTransform>().rect.height * 1.1f;
            _tooltipObject.transform.localPosition = Vector3.zero - new Vector3(0, _tooltipObjectHeigth, 0);
            toolipsController = _tooltipObject.GetComponent<ToolipsController>();
            toolipsController.SetTipText(tooltipText);
        }

        private void DestroyToolTip()
        {
            if(_tooltipObject)
                Destroy(_tooltipObject);
        }


    }

}
