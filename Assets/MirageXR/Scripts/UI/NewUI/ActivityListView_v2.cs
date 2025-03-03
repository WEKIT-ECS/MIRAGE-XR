using DG.Tweening;
using MirageXR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ActivityListView_v2 : BaseView
{
    private const float HIDED_SIZE = 80f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    private const string TUTORIAL_ACTIVITY_ID = "session-2023-02-24_11-18-29";

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
    [SerializeField] private TMP_Text _txtShowby;
    [SerializeField] private TMP_Text _txtSortby;

    private List<LearningExperienceEngine.SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();
    private bool _interactable = true;
    private static bool _orderByRelavance = false;
    private Vector2 _panelSize;

    public List<LearningExperienceEngine.SessionContainer> content => _content;

    //private List<Activity> _activities;
    
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

        LearningExperienceEngine.EventManager.OnStartActivity += ShowBackButtons;

        //RootObject.Instance.ActivityManager.OnActivitiesFetched += OnActivitiesFetched;

        FetchAndUpdateView();
    }

    /*private void OnActivitiesFetched(List<Activity> activities)
    {
        _activities = activities;
        UpdateView();
    }*/

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnStartActivity -= ShowBackButtons;
    }

    private static async Task<List<LearningExperienceEngine.SessionContainer>> FetchContent()
    {
        var dictionary = new Dictionary<string, LearningExperienceEngine.SessionContainer>();

        if (_orderByRelavance)
        {
            var activityList = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.GetArlemList();
            return OrderByRelavance(activityList).Values.ToList();
        }

        var localList = await LearningExperienceEngine.LocalFiles.GetDownloadedActivities();
        localList.ForEach(t =>
        {
            if (dictionary.ContainsKey(t.id))
            {
                dictionary[t.id].Activity = t;
            }
            else
            {
                dictionary.Add(t.id, new LearningExperienceEngine.SessionContainer { Activity = t });
            }
        });

        var remoteList = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.GetArlemList();
        remoteList?.ForEach(t =>
        {
            if (dictionary.ContainsKey(t.sessionid))
            {
                dictionary[t.sessionid].Session = t;
            }
            else
            {
                dictionary.Add(t.sessionid, new LearningExperienceEngine.SessionContainer { Session = t });
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
        LoadView.Instance?.Show(true);
        //await RootObject.Instance.ActivityManager.FetchActivitiesAsync();
        _content = await FetchContent();
        UpdateView();
        LoadView.Instance?.Hide();
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

        var prefab = !LearningExperienceEngine.UserSettings.showBigCards ? _smallItemPrefab : _bigItemPrefab;
        _content.ForEach(content =>
        {
            var item = Instantiate(prefab, _listTransform);
            item.Init(content);
            _items.Add(item);
        });

        /*if (_activities != null)
        {
            foreach (var activity in _activities)
            {
                var item = Instantiate(prefab, _listTransform);
                item.Init(activity);
                _items.Add(item);
            */
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
        await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivateFirstAction();
        LoadView.Instance.Hide();
    }

    private async void OnNewActivityChanged()
    {
        LoadView.Instance.Show();
        await RootObject.Instance.EditorSceneService.LoadEditorAsync();
        await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.CreateNewActivity();
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

    public void OnShowByChanged()
    {
        foreach (var item in _items)
        {
            switch (LearningExperienceEngine.UserSettings.currentShowby)
            {
                case LearningExperienceEngine.UserSettings.ShowBy.ALL:
                    item.gameObject.SetActive(true);
                    _txtShowby.text = "Show All";
                    break;
                case LearningExperienceEngine.UserSettings.ShowBy.MYACTIVITIES:
                    item.gameObject.SetActive(item.GetComponent<ActivityListItem_v2>().userIsAuthor);
                    _txtShowby.text = "My Activities";
                    break;
                case LearningExperienceEngine.UserSettings.ShowBy.MYASSIGNMENTS:
                    item.gameObject.SetActive(item.GetComponent<ActivityListItem_v2>().userIsEnroled);
                    _txtShowby.text = "My Assignments";
                    break;
            }
        }
    }

    public void OnSortbyChanged()
    {
        switch (LearningExperienceEngine.UserSettings.currentSortby)
        {
            case LearningExperienceEngine.UserSettings.SortBy.DATE:
                _orderByRelavance = false;
                _txtSortby.text = "By Date";
                FetchAndUpdateView();
                break;
            case LearningExperienceEngine.UserSettings.SortBy.RELEVEANCE:
                _orderByRelavance = true;
                _txtSortby.text = "By Relevence";
                FetchAndUpdateView();
                break;
        }

        LearningExperienceEngine.UserSettings.currentShowby = LearningExperienceEngine.UserSettings.ShowBy.ALL;

        OnShowByChanged();
    }

    private static Dictionary<string, LearningExperienceEngine.SessionContainer> OrderByRelavance(List<LearningExperienceEngine.Session> activityList)
    {
        var dictionary = new Dictionary<string, LearningExperienceEngine.SessionContainer>();

        var sessionContainersByDate = new List<KeyValuePair<DateTime, LearningExperienceEngine.SessionContainer>>();

        foreach (var activity in activityList)
        {
            LearningExperienceEngine.SessionContainer sessionContainer = new LearningExperienceEngine.SessionContainer { Session = activity };

            if (sessionContainer.hasDeadline)
            {
                if (DateTime.TryParse(sessionContainer.Session.deadline, out var date))
                {
                    sessionContainersByDate.Add(new KeyValuePair<DateTime, LearningExperienceEngine.SessionContainer>(date, sessionContainer));
                }
                else
                {
                    Debug.LogError("Cannot convert date");
                }
            }
        }

        List<KeyValuePair<DateTime, LearningExperienceEngine.SessionContainer>> sortedDateList = sessionContainersByDate.OrderBy(d => d.Value).ToList();

        foreach (var keypair in sortedDateList)
        {
            keypair.Deconstruct(out var date, out var sessionContatiner);
            dictionary.Add(sessionContatiner.Session.sessionid, sessionContatiner);
        }

        foreach (var activity in activityList)
        {
            if (!dictionary.ContainsKey(activity.sessionid))
            {
                LearningExperienceEngine.SessionContainer sessionContainer = new LearningExperienceEngine.SessionContainer { Session = activity };

                if (sessionContainer.userIsOwner)
                {
                    dictionary.Add(activity.sessionid, sessionContainer);
                }
            }
        }

        foreach (var activity in activityList)
        {
            if (!dictionary.ContainsKey(activity.sessionid))
            {
                dictionary.Add(activity.sessionid, new LearningExperienceEngine.SessionContainer { Session = activity });
            }
        }

        return dictionary;
    }

    public async Task CreateTutorialActivity()
    {
        await DeleteTutorialActivity();
#if UNITY_ANDROID || UNITY_EDITOR
        await MoveTutorialActivityToLocalFilesAndroidAsync();
#elif UNITY_IOS
        await MoveTutorialActivityToLocalFilesIOS();
#endif
    }

    private async Task DeleteTutorialActivity()
    {
        var tutorialActivity = await TryGetTutorialFromLocalFiles();

        if (tutorialActivity != null)
        {
            LearningExperienceEngine.LocalFiles.TryDeleteActivity(tutorialActivity.id);
            UpdateView();
        }
    }

    private async Task MoveTutorialActivityToLocalFilesAndroidAsync()
    {
        var zipPath = Path.Combine(Application.streamingAssetsPath, "TutorialActivity.zip");
        Debug.Log($"[ActivityListView_v2] Loading tutorial zip from location '{zipPath}'");

        using var www = UnityWebRequest.Get(zipPath);
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            MoveAndUnpackTutorialZipFileAndroid(www).AsAsyncVoid();
        }
    }

    private async Task MoveAndUnpackTutorialZipFileAndroid(UnityWebRequest www)
    {
        var savePath = Path.Combine(Application.persistentDataPath, "TutorialActivity.zip");
        Debug.Log($"[ActivityListView_v2] Copying file to location '{savePath}'");

        // clear out any previous download attempts
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        var stream = new FileStream(savePath, FileMode.OpenOrCreate);
        await stream.WriteAsync(www.downloadHandler.data);
        await LearningExperienceEngine.ZipUtilities.ExtractZipFileAsync(stream, Application.persistentDataPath);
        await stream.DisposeAsync();
        File.Delete(savePath);

        CreateTutorialActivityCard().AsAsyncVoid();
    }

    private async Task MoveTutorialActivityToLocalFilesIOS() // was void
    {
        var stream = new FileStream(Path.Combine(Application.streamingAssetsPath, "TutorialActivity.zip"), FileMode.Open);
        await LearningExperienceEngine.ZipUtilities.ExtractZipFileAsync(stream, Application.persistentDataPath);
        await stream.DisposeAsync();
        CreateTutorialActivityCard().AsAsyncVoid();
    }

    private async Task CreateTutorialActivityCard()
    {
        var t = await TryGetTutorialFromLocalFiles();

        if (t != null)
        {
            var sessionContainer = new LearningExperienceEngine.SessionContainer { Activity = t };

            _content.Insert(0, sessionContainer);
            UpdateView();
        }
    }

    private async Task<LearningExperienceEngine.Activity> TryGetTutorialFromLocalFiles()
    {
        var filePath = Path.Combine(Application.persistentDataPath, $"{TUTORIAL_ACTIVITY_ID}.json");
        var activity = await LearningExperienceEngine.LocalFiles.ReadActivityAsync(filePath);
        return activity;
    }
}
