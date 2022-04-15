using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserViewportController : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private float offset = 0.015f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f, mask.value))
        {
            transform.position = hit.point - offset * cam.transform.forward;
        }
        else
        {
            transform.position = cam.transform.position + cam.transform.forward;
        }
    }
}
