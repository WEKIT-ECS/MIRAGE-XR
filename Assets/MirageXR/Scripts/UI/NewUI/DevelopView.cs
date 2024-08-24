using System;
using System.Collections.Generic;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevelopView : PopupBase
{
	[SerializeField] private Button _btnClose;
	[SerializeField] private Toggle _devModeToggle;

	private bool _isShownDevelopModeMessage;

	public override void Initialization(Action<PopupBase> onClose, params object[] args)
	{
		base.Initialization(onClose, args);

		_devModeToggle.isOn = LearningExperienceEngine.DBManager.developMode;

		_devModeToggle.onValueChanged.AddListener(OnDevModeToggleValueChanged);

		_btnClose.onClick.AddListener(Close);
	}

	protected override bool TryToGetArguments(params object[] args)
	{
		return true;
	}

	private void OnDevModeToggleValueChanged(bool value)
	{
		LearningExperienceEngine.DBManager.developMode = value;

		if (_isShownDevelopModeMessage)
		{
			return;
		}

		var valueString = value ? "enabled" : "disabled";
		Toast.Instance.Show($"Developer mode has been {valueString}.Restart the application for it take effect.");
		_isShownDevelopModeMessage = true;
	}
}
