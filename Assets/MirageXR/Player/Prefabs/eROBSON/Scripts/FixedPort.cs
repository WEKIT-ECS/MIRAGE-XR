using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class FixedPort : MonoBehaviour
{
    [SerializeField] private Transform bitObject;

    private Vector3 _tempPosition;

    private Transform _myTransform;
    private Port _myPort;
    private ObjectManipulator _objectManipulator;
    private bool _moving;

    private bool _circuitDataLoaded;

    private IEnumerator Start()
    {
        while (!ErobsonItemManager.Instance || !ErobsonItemManager.Instance.CircuitParsed)
        {
            yield return null;
        }

        _myTransform = transform;
        _tempPosition = _myTransform.position;
        _myPort = GetComponentInChildren<Port>();
        _circuitDataLoaded = true;

        //If object manipulator not exit
        if (!TryGetComponent(out _objectManipulator))
        {
            yield break;
        }

        _objectManipulator.OnManipulationStarted.AddListener(OnMovingStart);
        _objectManipulator.OnManipulationEnded.AddListener(OnMovingStop);
    }


    private void OnMovingStart(ManipulationEventData data)
    {
        _moving = true;
    }

    private void OnMovingStop(ManipulationEventData data)
    {
        _moving = false;
        //_myTransform.position = _tempPosition;
        ErobsonItemManager.Instance.SaveJson();
    }

    private void Update()
    {
        if (!_circuitDataLoaded)
        {
            return;
        }

        if (!bitObject.GetComponent<eROBSONItems>().IsMoving)
        {
            _tempPosition = _myTransform.position;
        }

        if (!_myPort)
        {
            return;
        }
        _myPort.PortIsMovingSeparately = _moving;
    }



    private void LateUpdate()
    {
        if (!_circuitDataLoaded || _moving)
        {
            return;
        }

        _myTransform.position = _tempPosition;
    }
}
