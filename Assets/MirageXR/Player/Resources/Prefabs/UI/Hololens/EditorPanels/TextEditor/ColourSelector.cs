using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColourSelector : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private UnityEvent _onColourSelected = new UnityEvent();

    public UnityEvent onColourSelected => _onColourSelected;

    public Color _selectedColour = Color.white;

    public void ColourSelected(Image image)
    {
        _selectedColour = image.color;
        _onColourSelected.Invoke();
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
