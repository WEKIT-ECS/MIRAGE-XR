using DG.Tweening;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    private List<SessionContainer> _content;
    private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();
    private bool _interactable = true;
    private static bool _orderByRelavance = false;
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

        EventManager.OnActivityStarted += ShowBackButtons;

        FetchAndUpdateView();
    }

    private void OnDestroy()
    {
        EventManager.OnActivityStarted -= ShowBackButtons;
    }

    private static async Task<List<SessionContainer>> FetchContent()
    {
        var dictionary = new Dictionary<string, SessionContainer>();

        if (_orderByRelavance)
        {
            var activityList = await RootObject.Instance.moodleManager.GetArlemList();
            return OrderByRelavance(activityList).Values.ToList();
        }

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
        LoadView.Instance?.Show(true);
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

    public void OnShowByChanged()
    {
        foreach (var item in _items)
        {
            switch (DBManager.currentShowby)
            {
                case DBManager.ShowBy.ALL:
                    item.gameObject.SetActive(true);
                    _txtShowby.text = "Show All";
                    break;
                case DBManager.ShowBy.MYACTIVITIES:
                    item.gameObject.SetActive(item.GetComponent<ActivityListItem_v2>().userIsAuthor);
                    _txtShowby.text = "My Activities";
                    break;
                case DBManager.ShowBy.MYASSIGNMENTS:
                    item.gameObject.SetActive(item.GetComponent<ActivityListItem_v2>().userIsEnroled);
                    _txtShowby.text = "My Assignments";
                    break;
            }
        }
    }

    public void OnSortbyChanged()
    {
        switch (DBManager.currentSortby)
        {
            case DBManager.SortBy.DATE:
                _orderByRelavance = false;
                _txtSortby.text = "By Date";
                FetchAndUpdateView();
                break;
            case DBManager.SortBy.RELEVEANCE:
                _orderByRelavance = true;
                _txtSortby.text = "By Relevence";
                FetchAndUpdateView();
                break;
        }

        DBManager.currentShowby = DBManager.ShowBy.ALL;

        OnShowByChanged();
    }

    private static Dictionary<string, SessionContainer> OrderByRelavance(List<Session> activityList)
    {
        var dictionary = new Dictionary<string, SessionContainer>();

        var sessionContainersByDate = new List<KeyValuePair<DateTime, SessionContainer>>();

        foreach (var activity in activityList)
        {
            SessionContainer sessionContainer = new SessionContainer { Session = activity };

            if (sessionContainer.hasDeadline)
            {
                if (DateTime.TryParse(sessionContainer.Session.deadline, out var date))
                {
                    sessionContainersByDate.Add(new KeyValuePair<DateTime, SessionContainer>(date, sessionContainer));
                }
                else
                {
                    Debug.LogError("Cannot convert date");
                }
            }
        }

        List<KeyValuePair<DateTime, SessionContainer>> sortedDateList = sessionContainersByDate.OrderBy(d => d.Value).ToList();

        foreach (var keypair in sortedDateList)
        {
            keypair.Deconstruct(out var date, out var sessionContatiner);
            dictionary.Add(sessionContatiner.Session.sessionid, sessionContatiner);
        }

        foreach (var activity in activityList)
        {
            if (!dictionary.ContainsKey(activity.sessionid))
            {
                SessionContainer sessionContainer = new SessionContainer { Session = activity };

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
                dictionary.Add(activity.sessionid, new SessionContainer { Session = activity });
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
        MoveTutorialActivityToLocalFilesIOS();
#endif
    }

    public async Task DeleteTutorialActivity()
    {
        var tutorialActivity = await TryGetTutorialFromLocalFiles();

        if (tutorialActivity != null)
        {
            LocalFiles.TryDeleteActivity(tutorialActivity.id);

            UpdateView();
        }
    }

    /*private IEnumerator MoveTutorialActivityToLocalFilesAndroid()
    {
        string zipPath = Path.Combine(Application.streamingAssetsPath, "TutorialActivity.zip");
#if UNITY_EDITOR
        zipPath = "file://" + zipPath;
#endif
        Debug.Log("[ActivtyListView_v2] Loading tutorial zip from location '" + zipPath + "'");
        using (UnityWebRequest www = UnityWebRequest.Get(zipPath))
        {
            yield return www.SendWebRequest();

            MoveAndUnpackTutorialZipFileAndroid(www);
        }
    }*/

    private async Task MoveTutorialActivityToLocalFilesAndroidAsync()
    {
        string zipPath = Path.Combine(Application.streamingAssetsPath, "TutorialActivity.zip");
        Debug.Log("[ActivityListView_v2] Loading tutorial zip from location '" + zipPath + "'");

        using (UnityWebRequest www = UnityWebRequest.Get(zipPath))
        {
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
                MoveAndUnpackTutorialZipFileAndroid(www);
            }
        }
    }

    private async void MoveAndUnpackTutorialZipFileAndroid(UnityWebRequest www)
    {
        string savePath = Path.Combine(Application.persistentDataPath, "TutorialActivity.zip");
        Debug.Log("[ActivtyListView_v2] Copying file to location '" + savePath.ToString() + "'");

        // clear out any previous download attempts
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        var stream = new FileStream(savePath, FileMode.OpenOrCreate);

        await stream.WriteAsync(www.downloadHandler.data);

        await ZipUtilities.ExtractZipFileAsync(stream, Application.persistentDataPath);

        stream.Close();

        File.Delete(savePath);

        CreateTutorialActivityCard();
    }

    private async void MoveTutorialActivityToLocalFilesIOS()
    {
        var stream = new FileStream(Path.Combine(Application.streamingAssetsPath, "TutorialActivity.zip"), FileMode.Open);

        await ZipUtilities.ExtractZipFileAsync(stream, Application.persistentDataPath);

        stream.Close();

        CreateTutorialActivityCard();
    }

    private async void CreateTutorialActivityCard()
    {
        var t = await TryGetTutorialFromLocalFiles();

        if (t != null)
        {
            var sessionContatiner = new SessionContainer { Activity = t };

            _content.Insert(0, sessionContatiner);
            UpdateView();
        }
    }

    private async Task<Activity> TryGetTutorialFromLocalFiles()
    {
        var filePath = Path.Combine(Application.persistentDataPath, "session-2023-02-24_11-18-29-activity.json");
        var activity = await LocalFiles.ReadActivityAsync(filePath);
        return activity;
    }
}
