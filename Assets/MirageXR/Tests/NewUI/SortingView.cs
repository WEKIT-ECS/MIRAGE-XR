using System;
using System.Collections;
using System.Collections.Generic;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class SortingView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _toggleSmallCards;
    [SerializeField] private Toggle _toggleBigCards;

    private const string SELECTED_CARD_KEY = "cardSize";
    public static string SELECTED_CARD_SIZE
    {
        get => PlayerPrefs.GetString(SELECTED_CARD_KEY, "small");
        set => PlayerPrefs.SetString(SELECTED_CARD_KEY, value);
    }

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        
        _btnClose.onClick.AddListener(Close);
        //_toggleSmallCards.isOn = true;
        //_toggleBigCards.isOn = false;
        
        _toggleSmallCards.onValueChanged.AddListener(ShowSmallCard);
        _toggleBigCards.onValueChanged.AddListener(ShowBigCard);
    }
    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void ShowSmallCard(bool value)
    {
        SELECTED_CARD_SIZE = "small";
        ActivityListView_v2.Instance.UpdateListView();
    }
    
    private void ShowBigCard(bool value)
    {
        SELECTED_CARD_SIZE = "big";
        ActivityListView_v2.Instance.UpdateListView();
    }
    
}
