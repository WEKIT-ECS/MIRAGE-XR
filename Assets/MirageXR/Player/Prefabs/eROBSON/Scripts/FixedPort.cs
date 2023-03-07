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

    private Transform _myTransform;
    private Port _myPort;
    private ObjectManipulator _objectManipulator;
    private bool _moving;

    private bool _circuitDataLoaded;

    public IEnumerator LetConnect(float snappingDuration, Vector3 detectedPortPosition)
    {
        transform.position = detectedPortPosition;
        _tempPosition = _myTransform.position;
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
        _moving = true;
    }

    private void OnMovingStop(ManipulationEventData data)
    {
        _tempPosition = _myTransform.position;
        _moving = false;
        ErobsonItemManager.Instance.SaveJson();
    }

    private void Update()
    {
        if (!lockToggle || lockToggle.isOn || !_objectManipulator || !_objectManipulator.enabled)
        {
            return;
        }

        //if the main bit is moving do not store movable part position
        if (!bitObject.GetComponent<eROBSONItems>().IsMoving && !_moving)
        {
            _tempPosition = _myTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (_objectManipulator && !_objectManipulator.enabled)
        {
            return;
        }

        if (!_circuitDataLoaded || _moving)
        {
            return;
        }

        _myTransform.position = _tempPosition;
    }
}