using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private Transform _headTarget;
        [SerializeField] private Transform _bodyTarget;
        [SerializeField] private Transform _elbowHint;
		[SerializeField] private bool _isLeftHand;

		private Vector3 _currentElbowHintPosition;

		private void Update()
		{
			_currentElbowHintPosition = Vector3.Lerp(_headTarget.position, _bodyTarget.position, 0.8f);
			_currentElbowHintPosition -= _bodyTarget.forward;
			float sideFactor = _isLeftHand ? -1f : 1f;
			_currentElbowHintPosition += sideFactor * _bodyTarget.right * 0.8f;
			_elbowHint.position = _currentElbowHintPosition;
		}
	}
}
