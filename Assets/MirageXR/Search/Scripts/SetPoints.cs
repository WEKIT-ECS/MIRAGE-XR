using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPoints : MonoBehaviour
{

    public Transform targetTransform;
    [SerializeField] private float originOffset = 0.1f;
    [SerializeField] private float targetOffset = 0.1f;

    [Space]
    [SerializeField] private bool autoSetTarget = false;
    [SerializeField] private string targetObjectName = "cursorTarget";

    private Vector3 _originPoint, _targetPoint, _middlePoint, _directionVector;
    private LineRenderer _lineRenderer;

    // Use this for initialization
    void Start()
    {
        _lineRenderer = transform.GetComponent<LineRenderer>();

        if (autoSetTarget)
        {
            targetTransform = GameObject.Find(targetObjectName).transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _originPoint = transform.position;
        _targetPoint = targetTransform.position;
        _middlePoint = (_originPoint + _targetPoint) / 2;

        _directionVector = (_targetPoint - _originPoint).normalized;

        _lineRenderer.SetPosition(0, _originPoint + _directionVector * originOffset);
        _lineRenderer.SetPosition(1, _middlePoint);
        _lineRenderer.SetPosition(2, _targetPoint - _directionVector * targetOffset);
    }
}
