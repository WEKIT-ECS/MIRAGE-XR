using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;

public class SetStartingPosition : MonoBehaviour
{

    private Transform _userViewport;
    [SerializeField] private Vector3 offsetFromActivitySeceltor;


    private void Awake()
    {
        _userViewport = GameObject.FindGameObjectWithTag("UserViewport").transform;
    }

    private void OnEnable()
    {
        EventManager.OnPlayerReset += SetPosition;
        EventManager.OnWorkplaceParsed += SetPosition;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerReset -= SetPosition;
        EventManager.OnWorkplaceParsed -= SetPosition;
    }

    // Use this for initialization
    void Start()
    {
        SetPosition();
    }

    public void Call()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        //if an offset is set for this window
        if (offsetFromActivitySeceltor == Vector3.zero)
            transform.position = _userViewport.position;
        else
            transform.position = _userViewport.position + offsetFromActivitySeceltor;


        var rotation = _userViewport.eulerAngles;
        rotation.z = 0;
        transform.eulerAngles = rotation;
    }
}
