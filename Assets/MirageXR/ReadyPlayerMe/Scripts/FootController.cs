using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class FootController : MonoBehaviour
    {
		[SerializeField] private Transform _bodyTransform;

		private static FloorManagerWithFallback _floorManager => RootObject.Instance.floorManagerWithRaycastFallback;

        private Vector3 _currentPosition;

        private void Update()
        {
            _currentPosition = transform.position;
            _currentPosition.y = _floorManager.GetFloorHeight(_bodyTransform.position);
            transform.position = _currentPosition;
        }
    }
}
