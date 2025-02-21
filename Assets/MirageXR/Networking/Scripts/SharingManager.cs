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
			Instantiate(_connectionManagerPrefab);
#else
			Debug.LogWarning("Photon Fusion not installed. Collaborative features will not be activated.");
#endif
		}
	}
}
