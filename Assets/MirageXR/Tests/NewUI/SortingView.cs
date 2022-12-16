using MirageXR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SortingView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _toggleSmallCards;
    [SerializeField] private Toggle _toggleBigCards;

    [SerializeField] private Toggle _toggleShowAll;
    [SerializeField] private Toggle _toggleMyAssignments;
    [SerializeField] private Toggle _toggleMyActivities;
    [SerializeField] private Toggle _toggleByDate;
    [SerializeField] private Toggle _toggleByRelevance;

    private ActivityListView_v2 _parentView;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _btnClose.onClick.AddListener(Close);

        SetToggles();

        _toggleShowAll.onValueChanged.AddListener(ShowAll);
        _toggleMyAssignments.onValueChanged.AddListener(ShowMyAssignments);
        _toggleMyActivities.onValueChanged.AddListener(ShowMyActivities);
        _toggleByDate.onValueChanged.AddListener(SortByDate);
        _toggleByRelevance.onValueChanged.AddListener(SortByRelevance);


        _toggleSmallCards.onValueChanged.AddListener(ShowSmallCard);
        _toggleBigCards.onValueChanged.AddListener(ShowBigCard);
    }

    private void SetToggles()
    {
        _toggleSmallCards.isOn = !DBManager.showBigCards;
        _toggleBigCards.isOn = DBManager.showBigCards;

        switch (DBManager.currentShowby)
        {
            case DBManager.ShowBy.ALL:
                _toggleShowAll.isOn = true;
                _toggleMyAssignments.isOn = false;
                _toggleMyActivities.isOn = false;
                break;
            case DBManager.ShowBy.MYACTIVITIES:
                _toggleShowAll.isOn = false;
                _toggleMyActivities.isOn = true;
                _toggleMyAssignments.isOn = false;
                break;
            case DBManager.ShowBy.MYASSIGNMENTS:
                _toggleShowAll.isOn = false;
                _toggleMyAssignments.isOn = true;
                _toggleMyActivities.isOn = false;
                break;
        }

        switch (DBManager.currentSortby)
        {
            case DBManager.SortBy.DATE:
                _toggleByRelevance.isOn = false;
                _toggleByDate.isOn = true;

                break;
            case DBManager.SortBy.RELAVEANCE:
                _toggleByRelevance.isOn = true;
                _toggleByDate.isOn = false;

                break;
        }

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
        Close();
    }

    private void ShowBigCard(bool value)
    {
        DBManager.showBigCards = true;
        _parentView.UpdateView();
        Close();
    }

    private void ShowAll(bool value)
    {
        DBManager.currentShowby = DBManager.ShowBy.ALL;
        _toggleMyAssignments.isOn = false;
        _toggleMyActivities.isOn = false;

        _parentView.OnShowByChanged();
        Close();
    }

    private void ShowMyAssignments(bool value)
    {
        DBManager.currentShowby = DBManager.ShowBy.MYASSIGNMENTS;
        _toggleShowAll.isOn = false;
        _toggleMyActivities.isOn = false;

        _parentView.OnShowByChanged();
        Close();
    }

    private void ShowMyActivities(bool value)
    {
        DBManager.currentShowby = DBManager.ShowBy.MYACTIVITIES;
        _toggleShowAll.isOn = false;
        _toggleMyAssignments.isOn = false;

        _parentView.OnShowByChanged();
        Close();
    }

    private void SortByDate(bool value)
    {
        DBManager.currentSortby = DBManager.SortBy.DATE;
        _toggleByRelevance.isOn = false;

        _parentView.OnSortbyChanged();
        Close();
    }

    private void SortByRelevance(bool value)
    {
        DBManager.currentSortby = DBManager.SortBy.RELAVEANCE;
        _toggleByDate.isOn = false;

        _parentView.OnSortbyChanged();
        Close();
    }


}
