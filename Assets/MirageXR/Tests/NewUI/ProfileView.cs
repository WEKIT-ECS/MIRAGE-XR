using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileView : PopupBase
{
    [SerializeField] private MoodleServersView _moodlePrefab;
    [SerializeField] private Button _selectServer;
    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
    
    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        
        _selectServer.onClick.AddListener(ShowServerPanel);
    }

    private void ShowServerPanel()
    {
        PopupsViewer.Instance.Show(_moodlePrefab);
    }
}
