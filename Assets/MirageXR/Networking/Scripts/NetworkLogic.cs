#if PHOTON_FUSION
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class NetworkLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
	{
		public void PlayerJoined(PlayerRef player)
		{
			Debug.Log("Player joined: " + player);
		}

		public void PlayerLeft(PlayerRef player)
		{
			Debug.Log("Player left: " + player);
		}
	}
}
#endif