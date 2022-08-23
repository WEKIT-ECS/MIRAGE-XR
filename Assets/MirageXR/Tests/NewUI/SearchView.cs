using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
using TMPro;
using UnityEngine.UI;

public class SearchView : PopupBase
{

    [SerializeField] private Transform _listTransform;
    [SerializeField] private ActivityListItem_v2 _smallItemPrefab;
    [SerializeField] private ActivityListItem_v2 _bigItemPrefab;
    [SerializeField] private TMP_InputField _inputFieldSearch;
    [SerializeField] private Button _allButton;
    [SerializeField] private Button _titleButton;
    [SerializeField] private Button _authorButton;

    private List<SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();

    private ActivityListView_v2 activityListView;
    private int searchType;

    private void Start()
    {
        activityListView = ConnectedObject.GetComponent<ActivityListView_v2>();
        _content = activityListView.content;
        //_items = activityListView.items;

        _inputFieldSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);

        _allButton.onClick.AddListener(OnAllClick);
        _titleButton.onClick.AddListener(OnTitleClick);
        _authorButton.onClick.AddListener(OnAuthorClick);

        EventManager.OnActivityStarted += Close;

        searchType = 0;

        UpdateListView();
    }


    public void OnAllClick() {
        searchType = 0;

        OnInputFieldSearchChanged(_inputFieldSearch.text);
    }

    public void OnTitleClick()
    {
        searchType = 1;

        OnInputFieldSearchChanged(_inputFieldSearch.text);
    }

    public void OnAuthorClick()
    {
        searchType = 2;

        OnInputFieldSearchChanged(_inputFieldSearch.text);
    }

    private async void UpdateListView()
    {      
        _items.ForEach(item => Destroy(item.gameObject));
        _items.Clear();

        ActivityListItem_v2 prefab;
        if (!DBManager.showBigCards)
        {
            prefab = _smallItemPrefab;
        }
        else
        {
            prefab = _bigItemPrefab;
        }
        _content.ForEach(content =>
        {
            var item = Instantiate(prefab, _listTransform);
            item.Init(content);
            _items.Add(item);
        });
    }

    private void OnInputFieldSearchChanged(string text)
    {
        foreach (var item in _items)
        {
            var author = string.IsNullOrEmpty(text) || item.activityAuthor.ToLower().Contains(text.ToLower());
            var title = string.IsNullOrEmpty(text) || item.activityName.ToLower().Contains(text.ToLower());

            switch (searchType)
            {
                case 0:
                    if (title || author)
                    {
                        item.gameObject.SetActive(true);
                    }
                    else
                    {
                        item.gameObject.SetActive(false);
                    }
                    break;
                case 1:
                    item.gameObject.SetActive(title);
                    break;
                case 2:
                    item.gameObject.SetActive(author);
                    break;
            }                 
        }
    }


    

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
}
