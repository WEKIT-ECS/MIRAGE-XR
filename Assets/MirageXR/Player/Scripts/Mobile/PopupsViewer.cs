using System.Collections.Generic;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PopupsViewer : MonoBehaviour
{
    public static PopupsViewer Instance { get; private set; }

    [SerializeField] private Button _btnBackground;

    private readonly Stack<PopupBase> _stack = new Stack<PopupBase>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{Instance.GetType().FullName} must only be a single copy!");
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        _btnBackground.onClick.AddListener(OnOutTap);
    }

    public PopupBase Show(PopupBase popupPrefab, params object[] args)
    {
        var popup = Instantiate(popupPrefab, transform);
        _stack.Push(popup);
        popup.gameObject.SetActive(false);
        popup.Initialization(OnClose, args);
        UpdateView();

        return popup;
    }

    private void UpdateView()
    {
        if (_stack.Count == 0)
        {
            _btnBackground.gameObject.SetActive(false);
            return;
        }

        var popup = _stack.Peek();
        if (popup.isMarkedToDelete)
        {
            popup.Close();
            UpdateView();
        }
        else
        {
            ShowPopup(popup);
        }
    }

    private void ShowPopup(PopupBase popup)
    {
        popup.gameObject.SetActive(true);
        popup.transform.SetAsLastSibling();
        var lastSiblingIndex = popup.transform.GetSiblingIndex();
        _btnBackground.transform.SetSiblingIndex(lastSiblingIndex - 1);
        _btnBackground.gameObject.SetActive(popup.showBackground);
    }

    private void OnOutTap()
    {
        if (_stack.Count == 0)
        {
            return;
        }

        var popup = _stack.Peek();
        if (popup.canBeClosedByOutTap)
        {
            popup.Close();
        }

        EventManager.NotifyOnTutorialPopupCloseClicked();
    }

    private void OnClose(PopupBase popup)
    {
        if (_stack.Count <= 0)
        {
            Debug.LogError("Stack is empty!");
            return;
        }

        if (_stack.Peek() == popup)
        {
            Destroy(_stack.Pop().gameObject);
            UpdateView();
        }
    }
}
