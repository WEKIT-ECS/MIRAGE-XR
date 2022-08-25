using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MirageXR;
public class MoodleServersView : PopupBase
{

    [SerializeField] private Button _btnWEKIT;
    [SerializeField] private Button _btnARETE;
    [SerializeField] private Button _btnSave;
    [SerializeField] private ExtendedInputField _inputFieldMoodleAddress;

    public void Start()
    {
        _inputFieldMoodleAddress.SetValidator(IsValidUrl);

        _btnWEKIT.onClick.AddListener(OnClickWEKIT);
        _btnARETE.onClick.AddListener(OnClickARETE);
        _btnSave.onClick.AddListener(OnClickSave);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
        
    }

    private void OnClickWEKIT() {
        ChangeServerDomain(DBManager.WEKIT_URL);
    }

    private void OnClickARETE()
    {
        ChangeServerDomain(DBManager.ARETE_URL);
    }

    private void OnClickSave()
    {
        if (!_inputFieldMoodleAddress.Validate()) return;

        ChangeServerDomain(_inputFieldMoodleAddress.text);
    }


    private void ChangeServerDomain(string domain)
    {
        if (DBManager.domain != domain)
        {
            DBManager.domain = domain;
            DBManager.LogOut();
            RootView_v2.Instance.activityListView_V2.UpdateListView();
        }


        EventManager.NotifyMoodleDomainChanged();
        Close();
    }


    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

}
