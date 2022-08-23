using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
using TMPro;

public class SearchView : PopupBase
{

    [SerializeField] private Transform _listTransform;
    [SerializeField] private ActivityListItem_v2 _smallItemPrefab;
    [SerializeField] private ActivityListItem_v2 _bigItemPrefab;
    [SerializeField] private TMP_InputField _inputFieldSearch;


    private List<SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();

    private ActivityListView_v2 activityListView;

    private void Start()
    {
        activityListView = ConnectedObject.GetComponent<ActivityListView_v2>();
        _content = activityListView.content;
        //_items = activityListView.items;

        _inputFieldSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);

        EventManager.OnActivityStarted += Close;

        UpdateListView();
    }


    public async void UpdateListView()
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
            var enable = string.IsNullOrEmpty(text) || item.activityName.ToLower().Contains(text.ToLower());
            item.gameObject.SetActive(enable);
        }
    }


    

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }
}
