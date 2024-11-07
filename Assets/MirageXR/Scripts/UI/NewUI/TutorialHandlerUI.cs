using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using Coffee.UIExtensions;
using UnityEngine.UI;

/// <summary>
/// Defines the functionality of the UI-based tutorial system.
/// Only functional in UI-space, cannot be used in world-space.
/// </summary>
public class TutorialHandlerUI : MonoBehaviour
{
    private const int MAX_TRY_COUNT = 40;
    private const int WAIT_IN_MILLISECONDS = 250;

    [SerializeField] private Image _maskingImage;
    [SerializeField] private Unmask _unmaskPanel;
    [SerializeField] private GameObject _background;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TutorialMessageView _tutorialMessageViewPrefab;
    [SerializeField] private RectTransform[] _searchRoots;

    private Queue<TutorialStepModelUI> _queue;
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

    /// <summary>
    /// Starts tutorial sequence, beginning with first element of queue.
    /// Sets up prefab elements for showing.
    /// </summary>
    /// <param name="queue">Queue of models to be shown in order.</param>
    public void Show(Queue<TutorialStepModelUI> queue)
    {
        _maskingImage.enabled = true;
        _unmaskPanel.gameObject.SetActive(true);
        _background.SetActive(true);
        _panel.gameObject.SetActive(true);
        _isActivated = true;
        _queue = queue;
        _currentTutorialItem = null;
        Debug.LogDebug("Showing TutorialHandlerUI, status: " + this.gameObject.activeSelf);
        Next();
    }

    /// <summary>
    /// Hides active tutorial as well as cleaning up.
    /// </summary>
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
        Debug.LogDebug("Hiding TutorialHandlerUI, status: " + this.gameObject.activeSelf);
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
            Debug.LogDebug("Hiding TutorialHandlerUI because queue count is 0");
            Hide();
            TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.UI_FINISHED_QUEUE);
        }
    }

    /// <summary>
    /// Shows individual tutorial steps, based on given tutorial model.
    /// </summary>
    /// <param name="model">Information for showing the step.</param>
    private async Task ShowItem(TutorialStepModelUI model)
    {
        if (model.HasId)
        {
            Debug.LogDebug("New show item: " + model.Id);
            var item = await FindTutorialItem(model.Id);
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
                _unmaskPanel.gameObject.SetActive(false);
                Debug.LogError($"Can't find TutorialStepModelUI with id = '{model.Id}'");
                model.Id = null;
            }
        }
        else
        {
            _unmaskPanel.gameObject.SetActive(false);
            // This is in case the last model was targeted, so that it does not leave behind a mark
            if (_unmaskPanel.fitTarget != null)
            {
                Debug.LogDebug("Disabling unmask panel.");
                _unmaskPanel.gameObject.SetActive(false);
            }
        }

        if (model.HasMessage)
        {
            _lastMessageView = ShowMessage(model);
        }
    }

    /// <summary>
    /// Shows the textual part of the tutorial step, which guides the user.
    /// </summary>
    /// <param name="model">Model which holds the message to be shown.</param>
    /// <returns>The view that this class uses.</returns>
    private TutorialMessageView ShowMessage(TutorialStepModelUI model)
    {
        var tutorialMessageView = Instantiate(_tutorialMessageViewPrefab, _panel);
        tutorialMessageView.Initialization(model, OnMessageViewNextClicked, OnMessageViewExitClicked);
        return tutorialMessageView;
    }

    /// <summary>
    /// Handles what happens if the "Got it" button is clicked.
    /// </summary>
    /// <param name="model">The model that was shown.</param>
    private void OnMessageViewNextClicked(TutorialStepModelUI model)
    {
        if (model.HasId)
        {
            if (_currentTutorialItem.Button)
            {
                _currentTutorialItem.Button.onClick.RemoveListener(OnButtonClicked);
            }

            if (_currentTutorialItem.Toggle)
            {
                _currentTutorialItem.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (_currentTutorialItem.InputField)
            {
                _currentTutorialItem.InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            }
        }
        Hide();

        Next();
    }

    private void OnMessageViewExitClicked(TutorialStepModelUI model)
    {
        if (model.HasId)
        {
            if (_currentTutorialItem.Button)
            {
                _currentTutorialItem.Button.onClick.RemoveListener(OnButtonClicked);
            }

            if (_currentTutorialItem.Toggle)
            {
                _currentTutorialItem.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (_currentTutorialItem.InputField)
            {
                _currentTutorialItem.InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            }
        }
        Hide();

        TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.UI_GOT_IT);
        Debug.LogDebug("Hiding tutorial because of Exit");
    }

    /// <summary>
    /// Searches for the needed UI item. The item needs to have a TutorialScript component
    /// with the given id.
    /// </summary>
    /// <param name="id">The id given inn the TutorialScript component.</param>
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

    /// <summary>
    /// Sets up listeners for the given UI item.
    /// </summary>
    /// <param name="item">Model for the needed UI item.</param>
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
            Debug.LogDebug("Removed listener from button: " + _currentTutorialItem.Id);
        }

        OnFinishedTargetInteraction();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (_currentTutorialItem.Toggle)
        {
            _currentTutorialItem.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            Debug.LogDebug("Removed listener from toggle: " + _currentTutorialItem.Id);
        }

        OnFinishedTargetInteraction();
    }
    private void OnInputFieldValueChanged(string value)
    {
        if (_currentTutorialItem.InputField)
        {
            _currentTutorialItem.InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            Debug.LogDebug("Removed listener from input field: " + _currentTutorialItem.Id);
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

        Debug.LogDebug("Finisihed interaction with: " + _currentTutorialItem.Id);
        Next();
    }
}
