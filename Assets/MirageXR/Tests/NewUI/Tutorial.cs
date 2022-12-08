using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class TutorialModel
{
    public enum MessagePosition
    {
        Top,
        Middle,
        Bottom
    }

    public bool hasId => !string.IsNullOrEmpty(id);

    public bool hasMessage => !string.IsNullOrEmpty(message);

    public string id;
    public string message;
    public MessagePosition position = MessagePosition.Middle;
}

public class Tutorial : MonoBehaviour
{
    private const int MAX_TRY_COUNT = 40;
    private const int WAIT_IN_MILLISECONDS = 250;

    [SerializeField] private CanvasGroup _backgroundCanvasGroup;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TutorialMessageView _tutorialMessageViewPrefab;
    [SerializeField] private RectTransform[] _searchRoots;

    private Queue<TutorialModel> _queue;
    private TutorialMessageView _lastMessageView;

    protected void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        Hide();
    }

    public void Show(Queue<TutorialModel> queue)
    {
        _backgroundCanvasGroup.gameObject.SetActive(true);
        _panel.gameObject.SetActive(true);
        _queue = queue;
        Next();
    }

    public void Hide()
    {
        _backgroundCanvasGroup.gameObject.SetActive(false);
        _panel.gameObject.SetActive(false);
    }

    private void Next()
    {
        NextAsync().AsAsyncVoid();
    }

    private async Task NextAsync()
    {
        if (_queue.Count != 0)
        {
            await ShowItem(_queue.Dequeue());
        }
        else
        {
            Hide();
        }
    }

    private async Task ShowItem(TutorialModel model, int tryCount = 0)
    {
        if (model.hasId)
        {
            var item = await FindTutorialItem(model.id);
            if (item)
            {
                var copy = CopyTutorialItem(item);
                SetUpCopy(item, copy);
            }
            else
            {
                Debug.LogError($"Can't find TutorialModel with id = '{model.id}'");
                model.id = null;
            }
        }

        if (model.hasMessage)
        {
            _lastMessageView = ShowMessage(model);
        }
    }

    private TutorialMessageView ShowMessage(TutorialModel model)
    {
        var tutorialMessageView = Instantiate(_tutorialMessageViewPrefab, _panel);
        tutorialMessageView.Initialization(model, OnMessageViewButtonClicked);
        return tutorialMessageView;
    }

    private void OnMessageViewButtonClicked(TutorialModel model)
    {
        if (!model.hasId)
        {
            Next();
        }
    }

    private async Task<TutorialItem> FindTutorialItem(string id)
    {
        for (int i = 0; i < MAX_TRY_COUNT; i++)
        {
            foreach (var searchRoot in _searchRoots)
            {
                var items = searchRoot.GetComponentsInChildren<TutorialItem>();
                if (items == null)
                {
                    continue;
                }

                var item = items.FirstOrDefault(t => t.id == id);
                if (item)
                {
                    return item;
                }
            }

            await Task.Delay(WAIT_IN_MILLISECONDS);
        }

        return null;
    }

    private TutorialItem CopyTutorialItem(TutorialItem item)
    {
        if (item.isPartOfScrollView)
        {
            item.ScrollToTop();
        }

        var copyItem = Instantiate(item, _panel, true);

        return copyItem;
    }

    private void SetUpCopy(TutorialItem item, TutorialItem copy)
    {
        if (item.button)
        {
            copy.button.onClick.RemoveAllListeners();
            copy.button.onClick.AddListener(() => OnCopyClicked(item, copy));
        }

        if (item.toggle)
        {
            copy.toggle.onValueChanged.RemoveAllListeners();
            copy.toggle.onValueChanged.AddListener(value => OnCopyClicked(item, copy));
        }

        copy.StartTracking(item.transform);
    }

    private void OnCopyClicked(TutorialItem item, TutorialItem copy)
    {
        item.button.onClick.Invoke();
        copy.StopTracking();
        Destroy(copy.gameObject);
        if (_lastMessageView)
        {
            Destroy(_lastMessageView.gameObject);
            _lastMessageView = null;
        }

        Next();
    }
}
