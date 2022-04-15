using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MirageXR;

public class Tappable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsSelected { get; set; }

    private void OnEnable()
    {
        EventManager.OnTap += Tap;
    }

    private void OnDisable()
    {
        EventManager.OnTap -= Tap;
    }

    private void Tap()
    {
        if(IsSelected)
            GetComponent<Button>().onClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsSelected = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsSelected = false;
    }
}
