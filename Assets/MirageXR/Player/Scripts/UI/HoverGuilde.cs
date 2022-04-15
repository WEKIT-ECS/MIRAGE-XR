using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverGuilde : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Text _guildText;
    private string _message;

    public void SetMessage(string message) {
        _message = message;
    }

    public void SetGuildText(Text guildText) {
        _guildText = guildText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GuideMessage(_message);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _guildText.transform.parent.gameObject.SetActive(false);
    }


    private void GuideMessage(string message)
    {
        _guildText.text = message;
        _guildText.transform.parent.gameObject.SetActive(true);
    }

}
