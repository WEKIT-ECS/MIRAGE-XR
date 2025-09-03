using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class DraggableBottomPopup : MonoBehaviour
	{
		[SerializeField] private TMP_Text _titleLabel;


		[field: SerializeField] public GameObject Content { get; private set; }

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

		public void OpenDialog()
		{
			gameObject.SetActive(true);
		}

		public void CloseDialog()
		{
			gameObject.SetActive(false);
		}
	}
}
