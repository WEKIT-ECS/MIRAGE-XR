using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationView : PopupBase
{
    [SerializeField] private GameObject _footer;
    [SerializeField] private Image _targetRed;
    [SerializeField] private Image _targetBlue;
    [SerializeField] private TextMeshProUGUI _textDone;
    [SerializeField] private Button _btnClose;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        canBeClosedByOutTap = false;
        _btnClose.onClick.AddListener(CloseBtnClicked);
    }

    private void CloseBtnClicked()
    {
        Close();
        // show bottom panel and new activity screen
        RootView_v2.Instance.bottomPanel.gameObject.SetActive(true);
        RootView_v2.Instance.newActivityPanel.SetActive(true);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
}
