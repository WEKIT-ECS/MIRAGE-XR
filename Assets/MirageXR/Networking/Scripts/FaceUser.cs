using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class FaceUser : MonoBehaviour
    {
        private Transform _camera;

        private Vector3 _flattenedCameraPos;
        private Vector3 _flattenedPos;

		private void Start()
		{
            _camera = Camera.main.transform;
		}

		// Update is called once per frame
		void Update()
        {
            _flattenedCameraPos = new Vector3(_camera.position.x, 0, _camera.position.z);
            _flattenedPos = new Vector3(transform.position.x, 0, transform.position.z);

            transform.rotation = Quaternion.LookRotation(_flattenedPos - _flattenedCameraPos, Vector3.up);
        }
    }
}
