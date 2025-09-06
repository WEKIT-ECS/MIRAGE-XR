using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MirageXR
{
	public class DragHandle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		private DraggableBottomPopup _bottomPopup;

		private RectTransform _dialogTransform;
		private Vector2 pointerStartPosScreen;
		private Vector2 dialogStartAnchored;

		[SerializeField] private float _overshootResistance = 0.5f;

		private void Awake()
		{
			_bottomPopup = GetComponentInParent<DraggableBottomPopup>();
			_dialogTransform = _bottomPopup.Dialog;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_dialogTransform.DOKill();
			pointerStartPosScreen = eventData.position;
			dialogStartAnchored = _dialogTransform.anchoredPosition;
		}

		public void OnDrag(PointerEventData eventData)
		{
			float deltaY = eventData.position.y - pointerStartPosScreen.y;
			float newY = dialogStartAnchored.y + deltaY;
			if (newY > 0f)
			{
				// amount dragged past the limit
				float overshoot = newY;
				// apply resistance
				overshoot = Mathf.Pow(overshoot, _overshootResistance);

				newY = overshoot;
			}
			_dialogTransform.anchoredPosition = new Vector2(dialogStartAnchored.x, newY);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			float currentY = _dialogTransform.anchoredPosition.y;
			float distanceFromClosed = _dialogTransform.sizeDelta.y + currentY - _bottomPopup.DialogHeader.sizeDelta.y;

			if (distanceFromClosed <= _bottomPopup.DialogContent.sizeDelta.y / 2f)
			{
				_bottomPopup.CloseDialog();
			}
			else
			{
				_bottomPopup.OpenDialog(false);
			}
		}
	}
}
