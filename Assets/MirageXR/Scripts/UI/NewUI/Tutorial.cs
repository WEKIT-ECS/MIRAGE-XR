using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using Coffee.UIExtensions;
using UnityEngine.UI;
using static UnityEditor.Progress;

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

    [SerializeField] private Image _maskingImage;
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
    private TutorialItem _currentTutorialItem;

    public bool IsActivated => _isActivated;

    protected void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        Debug.LogDebug("Hiding tutorial because of Init");
        Hide();
    }

    public void Show(Queue<TutorialModel> queue)
    {
        _maskingImage.enabled = true;
        _unmaskPanel.gameObject.SetActive(true);
        _background.SetActive(true);
        _panel.gameObject.SetActive(true);
        _isActivated = true;
        _queue = queue;
        _currentTutorialItem = null;
        Debug.LogDebug("Showing Tutorial, status: " + this.gameObject.activeSelf);
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
        _maskingImage.enabled = false;
        _isActivated = false;
        Debug.LogDebug("Hiding Tutorial, status: " + this.gameObject.activeSelf);
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
            Debug.LogDebug("Hiding because queue count is 0");
            Hide();
        }
    }

    private async Task ShowItem(TutorialModel model)
    {
        if (model.HasId)
        {
            Debug.LogDebug("New show item: " + model.id);
            var item = await FindTutorialItem(model.id);
            if (item)
            {
                await Task.Yield();
                if (item.Delay > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(item.Delay));
                }

                _unmaskPanel.fitTarget = (RectTransform)item.transform;
                _unmaskPanel.fitOnLateUpdate = true;
                _unmaskPanel.gameObject.SetActive(true);
                MarkTarget(item);
                _currentTutorialItem = item;
            }
            else
            {
                Debug.LogError($"Can't find TutorialModel with id = '{model.id}'");
                model.id = null;
            }
        }
        else
        {
            // This is in case the last model was targeted, so that it does not leave behind a mark
            if (_unmaskPanel.fitTarget != null)
            {
                _unmaskPanel.gameObject.SetActive(false);
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
        Debug.LogDebug("Hiding tutorial because of Skip");
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
            item.Button.onClick.AddListener(OnButtonClicked);
            Debug.LogDebug("Marked button: " + item.Id);
        }

        if (item.Toggle)
        {
            item.Toggle.onValueChanged.AddListener(OnToggleValueChanged);
            //_toggleIsFirstPass = true;
            Debug.LogDebug("Marked toggle: " + item.Id);
        }

        if (item.InputField)
        {
            item.InputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            Debug.LogDebug("Marked input field: " + item.Id);
        }
    }

    private void OnButtonClicked()
    {
        if (_currentTutorialItem.Button)
        {
            _currentTutorialItem.Button.onClick.RemoveListener(OnButtonClicked);
        }

        OnFinishedTargetInteraction();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (_currentTutorialItem.Toggle)
        {
            /*
            if (_toggleIsFirstPass)
            {
                _toggleIsFirstPass = false;
                _currentTutorialItem.Toggle.isOn = true;
            }
            else
            {
                return;
            }*/

            _currentTutorialItem.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        OnFinishedTargetInteraction();
    }
    private void OnInputFieldValueChanged(string value)
    {
        if (_currentTutorialItem.InputField)
        {
            _currentTutorialItem.InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
        }

        OnFinishedTargetInteraction();
    }

    private void OnFinishedTargetInteraction()
    {
        if (_lastMessageView)
        {
            Destroy(_lastMessageView.gameObject);
            _lastMessageView = null;
        }

        Next();
    }
}
