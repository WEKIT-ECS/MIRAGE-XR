using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEventInputEventData : UnityEvent<InputEventData> {}

public class GlobalClickListener : MonoBehaviour, IMixedRealityInputHandler
{
    private const float TIME_FOR_CLICK = 1.5f;
    private float _clickTime;

    [SerializeField] private UnityEventInputEventData _onClickEvent = new UnityEventInputEventData();

    public UnityEventInputEventData onClickEvent => _onClickEvent;

    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
    }

    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
    }

    public void OnInputUp(InputEventData eventData)
    {
        var delta = Time.time - _clickTime;
        if (delta < TIME_FOR_CLICK)
        {
            onClickEvent?.Invoke(eventData);
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        _clickTime = Time.time;
    }
}