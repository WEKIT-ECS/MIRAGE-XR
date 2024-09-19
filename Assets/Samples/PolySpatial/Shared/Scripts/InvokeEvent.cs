using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace PolySpatial.Samples
{
    public class InvokeEvent : MonoBehaviour
    {
        [SerializeField]
        SpatialUIButton m_Button;

        [SerializeField]
        UnityEvent onClick;

//not wrong this
        // [SerializeField]
        // public UnityEngine.UI.Image image;

        // void Start()
        // {
        //     image.enabled = false;
        //     image.enabled = true;
        // }

        void OnEnable()
        {
            if (m_Button)
            {
                m_Button.WasPressed += WasPressed;
            }
        }

        void OnDisable()
        {
            if (m_Button)
            {
                m_Button.WasPressed -= WasPressed;
            }
        }

        void WasPressed(string buttonText, MeshRenderer meshrenderer)
        {
            Debug.Log("Event invoked");
            onClick.Invoke();
        }
    }
}
