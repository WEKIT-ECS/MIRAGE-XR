using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class I3ButtonController : MonoBehaviour
{
    private readonly int _pressed = Animator.StringToHash("Pressed");

    [SerializeField] private BitsBehaviourController bitBehaviourController;
    [SerializeField] private Button pressButton;
    [SerializeField] private Animator anim;
    [SerializeField] private Renderer renderer;

    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material pressedMaterial;


    private bool _isPressed;


    private void Start()
    {
        pressButton.onClick.AddListener(OnButtonPressed);
    }


    private void OnButtonPressed()
    {
        _isPressed = !_isPressed;
        if(bitBehaviourController)
        {
            bitBehaviourController.BitActivatingToggle();
        }
        ToggleAnimation();
        ToggleColor();
    }


    private void ToggleAnimation()
    {
        if (!anim)
        {
            return;
        }

        anim.SetBool(_pressed, _isPressed);
    }


    private void ToggleColor()
    {
        renderer.material = _isPressed ? pressedMaterial : defaultMaterial;
    }
}
