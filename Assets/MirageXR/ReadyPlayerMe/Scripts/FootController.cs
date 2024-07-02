using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class FootController : MonoBehaviour
    {
		[SerializeField] private Transform _headTarget;
        [SerializeField] private Transform _kneeHint;
        [SerializeField] private Transform _bodyTarget;

		private static FloorManagerWithFallback _floorManager => RootObject.Instance.floorManagerWithRaycastFallback;

        private Vector3 _currentFootTargetPosition, _currentKneeHintPosition;

		private void Update()
        {
            PlaceFootTarget();
            PlaceKneeHint();
        }

        private void PlaceFootTarget()
        {
			_currentFootTargetPosition = transform.position;
			_currentFootTargetPosition.y = _floorManager.GetFloorHeight(_headTarget.position);
			transform.position = _currentFootTargetPosition;
		}

        private void PlaceKneeHint()
        {
            _currentKneeHintPosition = Vector3.Lerp(_bodyTarget.position, _currentFootTargetPosition, 0.5f);
            _currentKneeHintPosition += _bodyTarget.forward * 0.5f;
            _kneeHint.position = _currentKneeHintPosition;
        }
    }
}
