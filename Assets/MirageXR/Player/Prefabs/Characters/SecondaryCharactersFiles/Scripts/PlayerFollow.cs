using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform PlayerTransform;
    public Vector3 lookOffset;
    public float smoothTime = 0.1f;

    private Vector3 _cameraOffset, _newPos, _lookPos, velo, velo2;

    private void Start ()
    {
        _cameraOffset = transform.position - PlayerTransform.position;
        _newPos = PlayerTransform.position + _cameraOffset;
        _lookPos = PlayerTransform.position + lookOffset;
    }
	
	private void LateUpdate ()
    {
        _newPos = Vector3.SmoothDamp(_newPos, PlayerTransform.position + _cameraOffset, ref velo, smoothTime);
        _lookPos = Vector3.SmoothDamp(_lookPos, PlayerTransform.position + lookOffset, ref velo2, smoothTime);

        transform.position = _newPos;
        transform.LookAt(_lookPos);
	}
}
