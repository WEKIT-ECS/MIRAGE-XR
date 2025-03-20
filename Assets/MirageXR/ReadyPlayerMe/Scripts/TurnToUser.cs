using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class TurnToUser : MonoBehaviour
    {
		// Reference to the main camera's Transform.
		private Transform _camera;

		// Flattened position of the camera (y-coordinate set to 0).
		private Vector3 _flattenedCameraPos;
		// Flattened position of this GameObject (y-coordinate set to 0).
		private Vector3 _flattenedPos;

		[field: SerializeField]
		public Quaternion RotationOffset { get; set; }
		[field: SerializeField]
		public float RotationSpeed { get; set; } = 3f;

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

			Quaternion targetRotation = RotationOffset * Quaternion.LookRotation(_flattenedPos - _flattenedCameraPos, Vector3.up);

			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				RotationSpeed * Time.deltaTime);
		}
	}
}
