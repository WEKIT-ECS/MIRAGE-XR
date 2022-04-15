using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class PlayerExitHandler : MonoBehaviour
    {
        void OnEnable()
        {
            EventManager.OnPlayerExit += HandleExit;
        }

        void OnDisable()
        {
            EventManager.OnPlayerExit -= HandleExit;
        }

        void HandleExit()
        {
            Destroy(gameObject);
        }
    }
}