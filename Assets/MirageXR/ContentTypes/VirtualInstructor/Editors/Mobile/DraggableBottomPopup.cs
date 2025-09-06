using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class DraggableBottomPopup : MonoBehaviour
	{
		[SerializeField] private TMP_Text _titleLabel;
		[field: SerializeField] public RectTransform Dialog { get; private set; }

		[field: SerializeField] public RectTransform DialogHeader { get; private set; }

		[field: SerializeField] public RectTransform DialogContent { get; private set; }

		private string _title;
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				if (gameObject.activeSelf)
				{
					UpdateUI();
				}
			}
		}

		private void UpdateUI()
		{
			if (_titleLabel != null)
			{
				_titleLabel.text = _title;
			}
		}

		private void OnEnable()
		{
			UpdateUI();
		}

		public void OpenDialog(bool slideInFromBottom = true)
		{
			gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(Dialog);
			if (slideInFromBottom)
			{
				Dialog.anchoredPosition = new Vector2(Dialog.anchoredPosition.x, -Dialog.sizeDelta.y);
			}
			Dialog.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
		}

		public void CloseDialogImmediately()
		{
			gameObject.SetActive(false);
		}

		public void CloseDialog()
		{
			Dialog.DOAnchorPosY(-Dialog.sizeDelta.y, 0.5f)
				.OnComplete(() => CloseDialogImmediately());
		}
	}
}
