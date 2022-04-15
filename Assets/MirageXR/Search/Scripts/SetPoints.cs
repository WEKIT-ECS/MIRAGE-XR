using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPoints : MonoBehaviour {

	public Transform targetTransform;
	public float originOffset = 0.1f;
	public float targetOffset = 0.1f;

	[Space]
	public bool autoSetTarget = false;
	public string targetObjectName = "cursorTarget";

	Vector3 originPoint, targetPoint, middlePoint, directionVector;
	LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
		lineRenderer = transform.GetComponent<LineRenderer> ();

		if (autoSetTarget) {
			targetTransform = GameObject.Find (targetObjectName).transform;
		}
	}
	
	// Update is called once per frame
	void Update () {		
		originPoint = transform.position;
		targetPoint = targetTransform.position;
		middlePoint = (originPoint + targetPoint) / 2;

		directionVector = (targetPoint -originPoint).normalized;

		lineRenderer.SetPosition (0, originPoint + directionVector * originOffset);
		lineRenderer.SetPosition (1, middlePoint);
		lineRenderer.SetPosition (2, targetPoint - directionVector * targetOffset);
	}
}
