using System;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class SortingView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _toggleSmallCards;
    [SerializeField] private Toggle _toggleBigCards;
    
    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);

        _btnClose.onClick.AddListener(Close);

        if (DBManager.showBigCards)
        {
            _toggleSmallCards.isOn = false;
            _toggleBigCards.isOn = true;
        }
        else
        {
            _toggleBigCards.isOn = false;
            _toggleSmallCards.isOn = true;
        }

        _toggleSmallCards.onValueChanged.AddListener(ShowSmallCard);
        _toggleBigCards.onValueChanged.AddListener(ShowBigCard);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void ShowSmallCard(bool value)
    {
        DBManager.showBigCards = false;
        ActivityListView_v2.Instance.UpdateView();
    }

    private void ShowBigCard(bool value)
    {
        DBManager.showBigCards = true;
        ActivityListView_v2.Instance.UpdateView();
    }

}
