using MirageXR;
using UnityEngine;

public class SetStartingPosition : MonoBehaviour
{
    [SerializeField] private Vector3 offsetFromActivitySeceltor;

    private Transform _userViewport;

    private void Awake()
    {
        _userViewport = GameObject.FindGameObjectWithTag("UserViewport").transform;
    }

    private void OnEnable()
    {
        EventManager.OnPlayerReset += SetPosition;
        EventManager.OnWorkplaceLoaded += SetPosition;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerReset -= SetPosition;
        EventManager.OnWorkplaceLoaded -= SetPosition;
    }

    private void Start()
    {
        SetPosition();
    }

    public void Call()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        // if an offset is set for this window
        if (offsetFromActivitySeceltor == Vector3.zero)
        {
            transform.position = _userViewport.position;
        }
        else
        {
            transform.position = _userViewport.position + offsetFromActivitySeceltor;
        }

        var rotation = _userViewport.eulerAngles;
        rotation.z = 0;
        transform.eulerAngles = rotation;
    }
}
