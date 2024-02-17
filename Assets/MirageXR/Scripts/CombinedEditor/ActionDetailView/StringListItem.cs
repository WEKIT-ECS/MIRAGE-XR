using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StringListItem : MonoBehaviour
{
    [SerializeField] Text textLabel;

    private string textContent;

    public string TextContent
    {
        get => textContent;
        set
        {
            textContent = value;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        textLabel.text = textContent;
    }
}
