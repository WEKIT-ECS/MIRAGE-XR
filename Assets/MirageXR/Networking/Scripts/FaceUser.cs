using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// A component that makes a GameObject face the user by aligning its y-axis rotation towards the camera.
	/// </summary>
	public class FaceUser : MonoBehaviour
    {
		// Reference to the main camera's Transform.
		private Transform _camera;

		// Flattened position of the camera (y-coordinate set to 0).
		private Vector3 _flattenedCameraPos;
		// Flattened position of this GameObject (y-coordinate set to 0).
		private Vector3 _flattenedPos;

		// Called when the script instance is being loaded.
		// Initializes the reference to the main camera's Transform.
		private void Start()
		{
            _camera = Camera.main.transform;
		}

		// Called every frame. Rotates the GameObject to face the user.
		private void Update()
        {
            _flattenedCameraPos = new Vector3(_camera.position.x, 0, _camera.position.z);
            _flattenedPos = new Vector3(transform.position.x, 0, transform.position.z);

            transform.rotation = Quaternion.LookRotation(_flattenedPos - _flattenedCameraPos, Vector3.up);
        }
    }
}
