using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class SharingManager : MonoBehaviour
	{
		[SerializeField] private GameObject _connectionManagerPrefab;

		private GameObject _connectionManagerInstance;

		public void Initialization()
		{
#if FUSION2
			_connectionManagerInstance = Instantiate(_connectionManagerPrefab);
			_connectionManagerInstance.transform.parent = transform;
#else
			Debug.LogWarning("Photon Fusion not installed. Collaborative features will not be activated.");
#endif
		}
	}
}
