using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class DialogWindow : MonoBehaviour
{
    protected const int MAX_CELLS_IN_ROW = 3; 
    
    public static DialogWindow Instance { get; private set; }

    protected class DialogContent
    {
        public string title => _title;
        public string message => _message;
        public DialogButtonContent[] buttonContents => _buttonContents;

        private readonly string _title;
        private readonly string _message;
        private readonly DialogButtonContent[] _buttonContents;

        public DialogContent(string title, string message, DialogButtonContent[] buttonContents)
        {
            _title = title;
            _message = message;
            _buttonContents = buttonContents;
        }
    }
    
    [SerializeField] protected Image _background;
    [SerializeField] protected GameObject _window;
    [SerializeField] protected RectTransform _buttonsTransform;
    [SerializeField] protected Button _dialogButtonPrefab;

    protected abstract string _titleText { get; set; }
    protected abstract string _messageText { get; set; }
    protected abstract GameObject _titleGameObject { get; }

    protected abstract void SetupWindowPosition();
    protected abstract void InstantiateDialogButton(DialogButtonContent content);
    
    protected bool _isActive;
    
    protected readonly Queue<DialogContent> _queue = new Queue<DialogContent>();
    
    protected void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{Instance.GetType().FullName} must only be a single copy!");
            return;
        }
        
        Instance = this;
    }

    protected void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _isActive = false;
        _background.gameObject.SetActive(false);
        _window.SetActive(false);
    }
    
    public void Show(string message, params DialogButtonContent[] buttonContents)
    {
        Show(null, message, buttonContents);
    }
    
    public void Show(string title, string message, params DialogButtonContent[] buttonContents)
    {
        _queue.Enqueue(new DialogContent(title, message, buttonContents));

        if (!_isActive)
        {
            ViewDialog(_queue.Dequeue());
        }
    }

    public void Hide()
    {
        if (_queue.Count > 0)
        {
                ViewDialog(_queue.Dequeue());
        }
        else
        {
            _isActive = false;
            _background.gameObject.SetActive(false);
            _window.SetActive(false);
        }
    }

    protected void ViewDialog(DialogContent dialogContent)
    {
        foreach (Transform obj in _buttonsTransform) Destroy(obj.gameObject);
        _titleGameObject.SetActive(false);
        
        if (!string.IsNullOrEmpty(dialogContent.title))
        {
            _titleText = dialogContent.title;
            _titleGameObject.SetActive(true);
        }
        
        _messageText = dialogContent.message;

        foreach (var content in dialogContent.buttonContents)
        {
            InstantiateDialogButton(content);
        }

        _background.gameObject.SetActive(true);
        _window.SetActive(true);
        _isActive = true;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_window.transform);
        UpdateCellWidth();

        SetupWindowPosition();
    }

    protected void UpdateCellWidth()
    {
        var grid = _buttonsTransform.GetComponent<GridLayoutGroup>();
        var width = _buttonsTransform.rect.width;
        var cellWidth = (width - grid.padding.horizontal - (grid.spacing.x * (MAX_CELLS_IN_ROW - 1))) / MAX_CELLS_IN_ROW;
        grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
    }
}
