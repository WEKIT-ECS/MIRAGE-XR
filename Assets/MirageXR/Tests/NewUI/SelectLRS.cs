using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MirageXR;
public class SelectLRS : PopupBase
{

    [SerializeField] private Button _btnWEKIT;
    [SerializeField] private Button _btnARETE;

    public void Start()
    {
        _btnWEKIT.onClick.AddListener(OnClickWEKIT);
        _btnARETE.onClick.AddListener(OnClickARETE);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
        
    }

    private void OnClickWEKIT() {
        ChangeRecordStore(DBManager.LearningRecordStores.WEKIT);
    }

    private void OnClickARETE()
    {
        ChangeRecordStore(DBManager.LearningRecordStores.ARETE);
    }

    private void ChangeRecordStore(DBManager.LearningRecordStores selectedLearningRecordStore)
    {
        EventManager.NotifyxAPIChanged(selectedLearningRecordStore);

        DBManager.publicCurrentLearningRecordStore = selectedLearningRecordStore;

        Close();
    }



}