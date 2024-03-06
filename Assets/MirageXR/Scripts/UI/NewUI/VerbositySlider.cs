using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class VerbositySlider : MonoBehaviour
	{
		[SerializeField] private Slider _slider;
		[SerializeField] private Transform[] _labelContainers;

		private TextMeshProUGUI[] _labels;
		private Image[] _labelIcons;

		private readonly Color inactiveColor = new Color(0.6862745f, 0.6862745f, 0.6862745f); // #AFAFAF
		private readonly Color activeColor = Color.white;

		private int _logLevel;

		private void Awake()
		{
			_labels = new TextMeshProUGUI[_labelContainers.Length];
			_labelIcons = new Image[_labelContainers.Length];

			for (int i = 0; i < _labelContainers.Length; i++)
			{
				_labels[i] = _labelContainers[i].GetComponentInChildren<TextMeshProUGUI>();
				_labelIcons[i] = _labelContainers[i].GetComponentInChildren<Image>();
			}
		}

		private void OnEnable()
		{
			_logLevel = (int)AppLog.MinimumLogLevel;
			AdjustUI();
		}

		public void OnSliderValueChanged()
		{
			_logLevel = (int)_slider.value;
			AppLog.MinimumLogLevel = (LogLevel)_logLevel;
			AppLog.LogInfo("Changed log level to " + _logLevel, this);
			AdjustUI();
		}

		private void AdjustUI()
		{
			for (int i = 0; i < _labels.Length; i++)
			{
				if (i <= _logLevel)
				{
					_labels[i].color = activeColor;
					_labelIcons[i].color = activeColor;
				}
				else
				{
					_labels[i].color = inactiveColor;
					_labelIcons[i].color = inactiveColor;
				}
			}
		}
	}
}