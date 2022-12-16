using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using MirageXR;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ActivityListView_v2 : BaseView
{
    private const float HIDED_SIZE = 80f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    [SerializeField] private Button _btnFilter;
    [SerializeField] private Transform _listTransform;
    [SerializeField] private ActivityListItem_v2 _smallItemPrefab;
    [SerializeField] private ActivityListItem_v2 _bigItemPrefab;
    [SerializeField] private SortingView _sortingPrefab;

    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private GameObject _backToActivity;
    [SerializeField] private Button _btnBackToActivity;
    [SerializeField] private Button _btnRestartActivity;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;

    private List<SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();
    private bool _interactable = true;
    private Vector2 _panelSize;

    public List<SessionContainer> content => _content;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _btnFilter.onClick.AddListener(OnByDateClick);
        _btnBackToActivity.onClick.AddListener(OnBackToActivityButton);
        _btnRestartActivity.onClick.AddListener(OnRestartActivityButton);
        _backToActivity.gameObject.SetActive(false);
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

        _panelSize = _panel.sizeDelta;

        EventManager.OnActivitySaved += FetchAndUpdateView;
        EventManager.OnActivityStarted += ShowBackButtons;

        FetchAndUpdateView();
    }

    private void OnDestroy()
    {
        EventManager.OnActivitySaved -= FetchAndUpdateView;
        EventManager.OnActivityStarted -= ShowBackButtons;
    }

    private static async Task<List<SessionContainer>> FetchContent()
    {
        var dictionary = new Dictionary<string, SessionContainer>();

        var localList = await LocalFiles.GetDownloadedActivities();
        localList.ForEach(t =>
        {
            if (dictionary.ContainsKey(t.id))
            {
                dictionary[t.id].Activity = t;
            }
            else
            {
                dictionary.Add(t.id, new SessionContainer { Activity = t });
            }
        });

        var remoteList = await RootObject.Instance.moodleManager.GetArlemList();
        remoteList?.ForEach(t =>
        {
            if (dictionary.ContainsKey(t.sessionid))
            {
                dictionary[t.sessionid].Session = t;
            }
            else
            {
                dictionary.Add(t.sessionid, new SessionContainer { Session = t });
            }
        });

        return dictionary.Values.ToList();
    }

    public void ShowBackButtons()
    {
        _backToActivity.SetActive(true);
    }

    public void HideBackButtons()
    {
        _backToActivity.SetActive(false);
    }

    public async void FetchAndUpdateView()
    {
        _content = await FetchContent();
        UpdateView();
    }

    public void UpdateView()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }
#endif
        _items.ForEach(item => Destroy(item.gameObject));
        _items.Clear();

        var prefab = !DBManager.showBigCards ? _smallItemPrefab : _bigItemPrefab;
        _content.ForEach(content =>
        {
            var item = Instantiate(prefab, _listTransform);
            item.Init(content);
            _items.Add(item);
        });
    }

    private void OnByDateClick()
    {
        PopupsViewer.Instance.Show(_sortingPrefab, this);
    }

    private void OnBackToActivityButton()
    {
        RootView_v2.Instance.OnActivityLoaded();
    }

    private void OnRestartActivityButton()
    {
        RestartActivityAsync().AsAsyncVoid();
    }

    private async Task RestartActivityAsync()
    {
        LoadView.Instance.Show();
        RootView_v2.Instance.OnActivityLoaded();
        await RootObject.Instance.activityManager.ActivateFirstAction();
        LoadView.Instance.Hide();
    }

    private async void OnNewActivityChanged()
    {
        LoadView.Instance.Show();
        await RootObject.Instance.editorSceneService.LoadEditorAsync();
        await RootObject.Instance.activityManager.CreateNewActivity();
        LoadView.Instance.Hide();
    }

    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            _panel.DOSizeDelta(new Vector2(_panelSize.x, -_panel.rect.height + HIDED_SIZE), HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
            rootView.bottomPanelView.Hide();
        }
        else
        {
            _panel.DOSizeDelta(_panelSize, 0.5f);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
            rootView.bottomPanelView.Show();
        }
    }
}