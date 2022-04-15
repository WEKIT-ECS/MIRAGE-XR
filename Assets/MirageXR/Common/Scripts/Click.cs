using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Universal Click. Should receive a Nobel prize for this...
    /// </summary>
    public class Click : MonoBehaviour
    {

        void OnEnable()
        {
            EventManager.OnClick += DoClick;
        }

        void OnDisable()
        {
            EventManager.OnClick -= DoClick;
        }

        void DoClick()
        {
            GetComponent<AudioSource>().Play();
        }
    }
}

