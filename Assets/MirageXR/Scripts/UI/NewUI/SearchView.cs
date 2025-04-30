using System;
using System.Collections.Generic;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchView : PopupBase
{
    [SerializeField] private Transform _listTransform;
    [SerializeField] private ActivityListItem_v2 _smallItemPrefab;
    [SerializeField] private ActivityListItem_v2 _bigItemPrefab;
    [SerializeField] private TMP_InputField _inputFieldSearch;
    [SerializeField] private Button _clearSearchBtn;
    [SerializeField] private Button _backHomeBtn;
    [SerializeField] private Toggle _allToggle;
    [SerializeField] private Toggle _titleToggle;
    [SerializeField] private Toggle _authorToggle;
    [SerializeField] private GameObject _textNoResults;

    private List<LearningExperienceEngine.SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();

    private ActivityListView_v2 _activityListView;

    private enum SearchType
    {
        All,
        Title,
        Author
    }

    private SearchType _selectedSearchType;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        if (_activityListView)
        {
            _content = _activityListView.content;
            _inputFieldSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);

            _backHomeBtn.onClick.AddListener(Close);
            _clearSearchBtn.onClick.AddListener(ClearSearchField);
            _allToggle.onValueChanged.AddListener(OnAllClick);
            _titleToggle.onValueChanged.AddListener(OnTitleClick);
            _authorToggle.onValueChanged.AddListener(OnAuthorClick);


            _selectedSearchType = SearchType.All;

            UpdateListView();
        }

        _clearSearchBtn.gameObject.SetActive(false);
        _textNoResults.SetActive(false);
        LearningExperienceEngine.EventManager.OnStartActivity += Close;
    }

    private void ClearSearchField()
    {
       _inputFieldSearch.text = string.Empty;
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnStartActivity -= Close;
    }

    private void OnAllClick(bool isOn)
    {
        if (isOn)
        {
            _selectedSearchType = SearchType.All;
            OnInputFieldSearchChanged(_inputFieldSearch.text);
        }
    }

    private void OnTitleClick(bool isOn)
    {
        if (isOn)
        {
            _selectedSearchType = SearchType.Title;
            OnInputFieldSearchChanged(_inputFieldSearch.text);
        }
    }

    private void OnAuthorClick(bool isOn)
    {
        if (isOn)
        {
            _selectedSearchType = SearchType.Author;
            OnInputFieldSearchChanged(_inputFieldSearch.text);
        }
    }

    private void UpdateListView()
    {
        /*_items.ForEach(item => Destroy(item.gameObject)); //TODO
        _items.Clear();

        var prefab = !LearningExperienceEngine.UserSettings.showBigCards ? _smallItemPrefab : _bigItemPrefab;

        _content.ForEach(content =>
        {
            var item = Instantiate(prefab, _listTransform);
            item.Init(content);
            _items.Add(item);
            item.gameObject.SetActive(false);
        });*/
    }

    private void OnInputFieldSearchChanged(string text)
    {
        var itemsCount = 0;
        var isTextEmpty = string.IsNullOrEmpty(text);

        foreach (var item in _items)
        {
            var author = isTextEmpty || item.activityAuthor.ToLower().Contains(text.ToLower());
            var title = isTextEmpty || item.activityName.ToLower().Contains(text.ToLower());
            var itemActive = false;

            switch (_selectedSearchType)
            {
                case SearchType.All:
                    itemActive = title || author;
                    break;
                case SearchType.Title:
                    itemActive = title;
                    break;
                case SearchType.Author:
                    itemActive = author;
                    break;
            }

            item.gameObject.SetActive(itemActive);

            if (itemActive)
            {
                itemsCount++;
            }

            if (isTextEmpty)
            {
                item.gameObject.SetActive(false);
            }
        }

        _clearSearchBtn.gameObject.SetActive(!isTextEmpty);
        _textNoResults.SetActive(isTextEmpty ? false : itemsCount <= 0);
    }


    protected override bool TryToGetArguments(params object[] args)
    {
        if (args is { Length: 1 } && args[0] is ActivityListView_v2 obj)
        {
            _activityListView = obj;
            return true;
        }

        return false;
    }

    public void CloseAndReturnToHomePage()
    {
        Close();
        RootView_v2.Instance.ReturnToHomePage();
    }
}
