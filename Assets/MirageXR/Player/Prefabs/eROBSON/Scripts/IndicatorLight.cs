using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorLight : MonoBehaviour
{

    [SerializeField] private Color offColor;
    [SerializeField] private Color onColor;

    private Material mat;


    private void Start()
    {
        var renderer = GetComponent<Renderer>();

        if(renderer)
            mat = renderer.material;
    }

    public void SwitchLight(bool status)
    {
        if (!mat)
            return;

        if (status)
        {
            mat.color = onColor;
        }
        else
        {
            mat.color = offColor;
        }
    }
}
