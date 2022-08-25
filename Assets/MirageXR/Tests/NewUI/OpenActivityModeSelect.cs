using System;
using System.Text.RegularExpressions;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenActivityModeSelect : PopupBase
{

    [SerializeField] private Toggle _editToggle;
    [SerializeField] private Toggle _viewToggle;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;


    private void Start()
    {
        _editToggle.onValueChanged.AddListener(OnEditToggle);
        _viewToggle.onValueChanged.AddListener(OnViewToggle);

        _openButton.onClick.AddListener(OnOpen);
        _closeButton.onClick.AddListener(Close);
    }

    private void OnEditToggle(bool value) 
    {
        _viewToggle.enabled = !value;
    }

    private void OnViewToggle(bool value)
    {
        _editToggle.enabled = !value;
    }

    private void OnOpen()
    {
        ActivityListItem_v2 activityItem = ConnectedObject.transform.GetComponent<ActivityListItem_v2>();

        activityItem.OpenActivity(_editToggle.enabled);

        Close();
    }


    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

}
