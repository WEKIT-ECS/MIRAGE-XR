using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// A simple helper script to implement a long button press.
/// </summary>
public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _holdTime;
    private bool _pointerDown;
    private float _pointerDownTimer;
    public UnityEvent _onLongClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    private void Update()
    {
        if (_pointerDown)
        {
            _pointerDownTimer += Time.deltaTime;
            if (_pointerDownTimer >= _holdTime)
            {
                _onLongClick?.Invoke();
                Reset();
            }
        }
    }

    private void Reset()
    {
        _pointerDown = false;
        _pointerDownTimer = 0f;
    }
}
