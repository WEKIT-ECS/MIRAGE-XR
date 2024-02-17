using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FixedPort : MonoBehaviour
{

    [SerializeField] private Transform bitObject;
    [SerializeField] private Toggle lockToggle;

    public bool IsLock => lockToggle.isOn;

    public bool Moving => _moving;

    private Vector3 _tempPosition;

    private Port _myPort;
    private ObjectManipulator _objectManipulator;
    private bool _moving;

    private bool _circuitDataLoaded;

    private eROBSONItems _eROBSONItems;

    public IEnumerator LetConnect(float snappingDuration, Vector3 detectedPortPosition)
    {
        transform.position = detectedPortPosition;
        _tempPosition = transform.position;
        var manipulationStatus = _objectManipulator.enabled;
        _objectManipulator.enabled = false;
        yield return new WaitForSeconds(snappingDuration);
        _objectManipulator.enabled = manipulationStatus;
    }


    private IEnumerator Start()
    {
        while (!ErobsonItemManager.Instance || !ErobsonItemManager.Instance.CircuitParsed)
        {
            yield return null;
        }

        bitObject.TryGetComponent(out _eROBSONItems);

        _tempPosition = transform.position;
        _myPort = GetComponentInChildren<Port>();
        _circuitDataLoaded = true;

        //If object manipulator not exit
        if (!TryGetComponent(out _objectManipulator))
        {
            yield break;
        }

        _objectManipulator.OnManipulationStarted.AddListener(OnMovingStart);
        _objectManipulator.OnManipulationEnded.AddListener(OnMovingStop);

        lockToggle.onValueChanged.AddListener(OnLockToggled);

        //The port is locked at start
        OnLockToggled(true);
    }

    private void OnLockToggled(bool locked)
    {
        _objectManipulator.enabled = !locked;
    }


    private void OnMovingStart(ManipulationEventData data)
    {
        _moving = _eROBSONItems && !_eROBSONItems.BitIsLocked;
    }

    private void OnMovingStop(ManipulationEventData data)
    {
        _tempPosition = transform.position;
        _moving = false;
        ErobsonItemManager.Instance.SaveJson();
    }

    private void Update()
    {
        if (!lockToggle || lockToggle.isOn || !_objectManipulator || !_objectManipulator.enabled || !_eROBSONItems)
        {
            return;
        }

        //if the main bit is moving do not store movable part position
        if (!_eROBSONItems.IsMoving && !_moving)
        {
            _tempPosition = transform.position;
        }
    }

    private void LateUpdate()
    {
        if (_eROBSONItems && _eROBSONItems.BitIsLocked && !IsLock)
        {
            lockToggle.isOn = true;
        }


        if (_objectManipulator && !_objectManipulator.enabled)
        {
            return;
        }

        if (!_circuitDataLoaded || _moving)
        {
            return;
        }

        // Only update the position if the port is locked
        if (IsLock)
        {
            transform.position = _tempPosition;
        }
    }
}