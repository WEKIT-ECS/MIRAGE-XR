using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.UI;
using System;

public class XRButtonTrigger : XRBaseInteractable
{
    [SerializeField] private XRBaseInteractable m_Interactable;

    [SerializeField] private Button button;
        
    void Awake()
    {
        m_Interactable.selectEntered.AddListener(OnSelectExited);
    }

    private void OnSelectExited(SelectEnterEventArgs arg0)
    {
        button.onClick.Invoke();
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // If the interactor is a poke interactor, invoke button onClick envent.
        //if (args.interactorObject is IPokeStateDataProvider)
        //{
            button.onClick.Invoke();
        //}
            
    }
}
