using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickCounter : MonoBehaviour, IPointerClickHandler
{
    private const float WAIT_TIME = 2f;

    [Serializable] public class UnityEventInt : UnityEvent<int> { }

    [SerializeField] private int _clickAmount = 5;
    [SerializeField] private UnityEventInt _onClickAmountReached;

    public UnityEventInt onClickAmountReached => _onClickAmountReached;

    private Coroutine _resetCoroutine;
    private int _count;

    private void OnDisable()
    {
        StopResetCoroutine();
        ResetCount();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopResetCoroutine();

        _count++;
        if (_count >= _clickAmount)
        {
            onClickAmountReached.Invoke(_clickAmount);
            ResetCount();
        }
        else
        {
            StartResetCoroutine();
        }
    }

    private void StartResetCoroutine()
    {
        StartCoroutine(WaitAndResetEnumerator());
    }

    private void StopResetCoroutine()
    {
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }
    }

    private IEnumerator WaitAndResetEnumerator()
    {
        yield return new WaitForSeconds(WAIT_TIME);
        ResetCount();
    }

    private void ResetCount()
    {
        _count = 0;
    }
}
