using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera _camera;


    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (!_camera)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(_camera.transform.forward);
    }
}