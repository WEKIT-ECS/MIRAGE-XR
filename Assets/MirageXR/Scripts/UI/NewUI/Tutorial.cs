using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using Coffee.UIExtensions;

public class TutorialModel
{
    public enum MessagePosition
    {
        Top,
        Middle,
        Bottom
    }

    public string id;
    public string message;
    public MessagePosition position = MessagePosition.Middle;
    public string btnText = "Cancel";

    public bool HasId => !string.IsNullOrEmpty(id);

    public bool HasMessage => !string.IsNullOrEmpty(message);
}

public class Tutorial : MonoBehaviour
{
    private const int MAX_TRY_COUNT = 40;
    private const int WAIT_IN_MILLISECONDS = 250;

    [SerializeField] private Unmask _unmaskPanel;
    [SerializeField] private GameObject _background;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TutorialMessageView _tutorialMessageViewPrefab;
    [SerializeField] private RectTransform[] _searchRoots;

    private Queue<TutorialModel> _queue;
    private TutorialMessageView _lastMessageView;
    private TutorialItem _lastCopy;

    private bool _toggleIsFirstPass;
    private bool _isActivated;

    public bool isActivated => _isActivated;

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
        this.gameObject.SetActive(true);
        _unmaskPanel.gameObject.SetActive(true);
        _background.SetActive(true);
        _panel.gameObject.SetActive(true);
        _isActivated = true;
        _queue = queue;
        Next();
    }

    public void Hide()
    {
        if (_lastMessageView)
        {
            Destroy(_lastMessageView.gameObject);
            _lastMessageView = null;
        }

        if (_lastCopy)
        {
            Destroy(_lastCopy.gameObject);
            _lastCopy = null;
        }

        _background.SetActive(false);
        _panel.gameObject.SetActive(false);
        _unmaskPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
        _isActivated = false;
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

    private async Task ShowItem(TutorialModel model)
    {
        if (model.HasId)
        {
            var item = await FindTutorialItem(model.id);
            if (item)
            {
                await Task.Yield();
                if (item.Delay > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(item.Delay));
                }

                _unmaskPanel.fitTarget = (RectTransform) item.transform;
                _unmaskPanel.fitOnLateUpdate = true;
                MarkTarget(item);

                //_lastCopy = CopyTutorialItem(item);
                //SetUpTargetCopy(item, _lastCopy);
            }
            else
            {
                Debug.LogError($"Can't find TutorialModel with id = '{model.id}'");
                model.id = null;
            }
        }

        if (model.HasMessage)
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
        Hide();
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

                var item = items.FirstOrDefault(t => t.Id == id);
                if (item)
                {
                    return item;
                }
            }

            await Task.Delay(WAIT_IN_MILLISECONDS);
        }

        return null;
    }


    private void MarkTarget(TutorialItem item)
    {
        if (item.Button)
        {
            item.Button.onClick.AddListener(() => OnTargetClicked(item));
        }

        if (item.Toggle)
        {
            item.Toggle.onValueChanged.AddListener(value => OnTargetClicked(item));
            //_toggleIsFirstPass = true;
        }

        if (item.InputField)
        {
            //TODO: Add for input field
        }
    }

    private void OnTargetClicked(TutorialItem item)
    {
        if (item.Button)
        {
            item.Button.onClick.RemoveListener(() => OnTargetClicked(item));
        }

        if (item.Toggle)
        {
            /*
            if (_toggleIsFirstPass)
            {
                _toggleIsFirstPass = false;
                item.Toggle.isOn = true;
            }
            else
            {
                return;
            }*/

            item.Toggle.onValueChanged.RemoveListener(value => OnTargetClicked(item));
        }

        if (item.InputField)
        {
            //TODO: Add removal
        }

        if (_lastMessageView)
        {
            Destroy(_lastMessageView.gameObject);
            _lastMessageView = null;
        }

        Next();
    }
}
