using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// A simple helper script to implement a long button press.
/// </summary>
public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float _holdTime;
    [SerializeField] private UnityEvent  _onLongClick = new UnityEvent();
    [SerializeField] private UnityEvent<float> _onHoldProgressChanged = new UnityEvent<float>();
    [SerializeField] public Color holdColor;

    public UnityEvent onLongClick => _onLongClick;
    public UnityEvent<float> onHoldProgressChanged => _onHoldProgressChanged;

    private bool _pointerDown;
    private float _pointerDownTimer;
    private bool _longPressTriggered;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
        _longPressTriggered = false;
        _onHoldProgressChanged?.Invoke(0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_longPressTriggered)
        {
            _pointerDown = false;
            _pointerDownTimer = 0f;
            _onHoldProgressChanged?.Invoke(1f);
            return;
        }

        ResetPointer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _longPressTriggered = false;
        ResetPointer();
    }

    public bool ConsumeLongPress()
    {
        if (!_longPressTriggered)
        {
            return false;
        }

        _longPressTriggered = false;
        _onLongClick?.Invoke();
        return true;
    }

    private void Update()
    {
        if (!_pointerDown)
        {
            return;
        }

        if (_longPressTriggered)
        {
            _onHoldProgressChanged?.Invoke(1f);
            return;
        }

        _pointerDownTimer += Time.deltaTime;
        var holdProgress = _holdTime > 0f ? Mathf.Clamp01(_pointerDownTimer / _holdTime) : 1f;
        _onHoldProgressChanged?.Invoke(holdProgress);
        if (_pointerDownTimer < _holdTime)
        {
            return;
        }

        _longPressTriggered = true;
        _onHoldProgressChanged?.Invoke(1f);
    }

    private void OnDisable()
    {
        _longPressTriggered = false;
        ResetPointer();
    }

    private void ResetPointer()
    {
        _pointerDown = false;
        _pointerDownTimer = 0f;
        _onHoldProgressChanged?.Invoke(0f);
    }
}
