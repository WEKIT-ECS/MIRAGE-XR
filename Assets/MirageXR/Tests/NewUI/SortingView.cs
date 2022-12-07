using System;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class SortingView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _toggleSmallCards;
    [SerializeField] private Toggle _toggleBigCards;

    private ActivityListView_v2 _parentView;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

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
        try
        {
            _parentView = (ActivityListView_v2)args[0];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void ShowSmallCard(bool value)
    {
        DBManager.showBigCards = false;
        _parentView.UpdateView();
    }

    private void ShowBigCard(bool value)
    {
        DBManager.showBigCards = true;
        _parentView.UpdateView();
    }

}
