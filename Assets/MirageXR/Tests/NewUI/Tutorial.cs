using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using MirageXR;
using UnityEngine;

public class TutorialModel
{
    public bool hasId => !string.IsNullOrEmpty(id);

    public bool hasMessage => !string.IsNullOrEmpty(message);

    public string id;
    public string message;
    public Vector2 relativePosition = new Vector2(0.5f, 0.5f); // from 0.0f to 1.0f
}

public class Tutorial : MonoBehaviour
{
    private const float AnimationFadeTime = 0.1f;

    [SerializeField] private CanvasGroup _backgroundCanvasGroup;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TutorialMessageView _tutorialMessageViewPrefab;
    [SerializeField] private RectTransform[] _searchRoots;

    private Queue<TutorialModel> _queue;

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
        await Task.Yield();
        if (_queue.Count != 0)
        {
            ShowItem(_queue.Dequeue());
        }
        else
        {
            Hide();
        }
    }

    private void ShowItem(TutorialModel model)
    {
        if (model.hasId)
        {
            var item = FindTutorialItem(model.id);
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
            ShowMessage(model);
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

    private TutorialItem FindTutorialItem(string id)
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

        return null;
    }

    private TutorialItem CopyTutorialItem(TutorialItem item)
    {
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
    }

    private void OnCopyClicked(TutorialItem item, TutorialItem copy)
    {
        item.button.onClick.Invoke();
        Destroy(copy.gameObject);
        Next();
    }

    private async Task ShowAnimation()
    {
        _backgroundCanvasGroup.alpha = 0.0f;
        await _backgroundCanvasGroup.DOFade(1.0f, AnimationFadeTime).AsyncWaitForCompletion();
    }

    private async Task HideAnimation()
    {
        _backgroundCanvasGroup.alpha = 1.0f;
        await _backgroundCanvasGroup.DOFade(0, AnimationFadeTime).AsyncWaitForCompletion();
    }
}
