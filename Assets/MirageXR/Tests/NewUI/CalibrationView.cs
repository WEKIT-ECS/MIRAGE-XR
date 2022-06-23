using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationView : MonoBehaviour
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
        _btnClose.onClick.AddListener(Close);
    }

    private void Close()
    {
        RootView_v2.Instance.OnHomeClick(true);
        RootView_v2.Instance.bottomPanel.gameObject.SetActive(true);
    }

}
