using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared
{
    // Interface for an object spawning the user prefab. Should be place on the NetworkRunner gameobject for automation scripts
    public interface IUserSpawner
    {
        public NetworkObject UserPrefab { get; set; }

    }
}
