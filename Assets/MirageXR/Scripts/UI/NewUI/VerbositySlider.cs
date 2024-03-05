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

		private TextMeshPro[] _labels;
		private Image[] _labelIcons;

		private int _logLevel;

		private void Awake()
		{
			_labels = new TextMeshPro[_labelContainers.Length];
			_labelIcons = new Image[_labelContainers.Length];

			for (int i=0;i< _labelContainers.Length;i++)
			{
				_labels[i] = _labelContainers[i].GetComponentInChildren<TextMeshPro>();
				_labelIcons[i] = _labelContainers[i].GetComponentInChildren<Image>();
			}
		}

		public void OnSliderValueChanged()
		{
			_logLevel = (int)_slider.value;
		}

		private void AdjustUI()
		{
			for (int i=0;i<_labelContainers.Length;i++)
			{
				if (i < _logLevel)
				{
				}
				else
				{
				}
			}
		}
	}
}