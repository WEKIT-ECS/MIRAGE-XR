using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
	public class ChangeableSettingsPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _currentValueLabel;
		[SerializeField] private TMP_Text _titleLabel;
		[SerializeField] private TMP_Text _descriptionLabel;

		public UnityEvent OnClick;
		public UnityEvent<string> OnValueChanged;

		private string _currentValue;
		private Button _button;

		public string CurrentValue
		{
			get => _currentValue;
			set
			{
				if (_currentValue != value)
				{
					_currentValue = value;
					_currentValueLabel.text = _currentValue;
					OnValueChanged.Invoke(value);
				}
			}
		}

		public string Title
		{
			get => _titleLabel.text;
			set
			{
				_titleLabel.text = value;
			}
		}

		public string Description
		{
			get => _descriptionLabel.text;
			set
			{
				_descriptionLabel.text = value;
			}
		}

		private void OnEnable()
		{
			if (_button == null)
			{
				_button = GetComponent<Button>();
			}
			_button.onClick.AddListener(PanelClicked);
		}

		private void OnDisable()
		{
			_button.onClick.RemoveListener(PanelClicked);
		}

		private void PanelClicked()
		{
			OnClick.Invoke();
		}
	}
}