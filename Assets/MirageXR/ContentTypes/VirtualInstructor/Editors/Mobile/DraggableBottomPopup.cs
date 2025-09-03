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


		[field: SerializeField] public GameObject Content { get; private set; }

		[SerializeField] private RectTransform _dialogTransform;

		private RectTransform _contentTransform;

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

		private void Awake()
		{
			_contentTransform = Content.GetComponent<RectTransform>();
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

		public void OpenDialog()
		{
			gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_dialogTransform);
			_dialogTransform.anchoredPosition = new Vector2(_dialogTransform.anchoredPosition.x, -_dialogTransform.sizeDelta.y);
			_dialogTransform.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
		}

		public void CloseDialogImmediately()
		{
			gameObject.SetActive(false);
		}

		public void CloseDialog()
		{
			_dialogTransform.DOAnchorPosY(-_dialogTransform.sizeDelta.y, 0.5f)
				.OnComplete(() => CloseDialogImmediately());
		}
	}
}
