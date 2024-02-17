using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Service which makes the MRTK's WorldAnchorManager accessible
    /// </summary>
    public class WorldAnchorService : IService
    {
        /// <summary>
        /// The instance of the manager
        /// </summary>
        private GameObject managerInstanceObj;

        public WorldAnchorManager Manager { get; private set; }

        /// <summary>
        /// Called by the ServiceManager to initialize the service
        /// Since WorldAnchorManager is a MonoBehaviour, it creates a manager instance on a new GameObject
        /// </summary>
        /// <param name="owner">The ServiceManager which owns this service</param>
        public void Initialize(IServiceManager owner)
        {
            managerInstanceObj = ObjectPool<GameObject>.RequestResource(() => { return new GameObject(); });
            managerInstanceObj.name = "World Anchor Manager";
            managerInstanceObj.transform.parent = owner.Runner.transform;
            Manager = managerInstanceObj.AddComponent<WorldAnchorManager>();
        }

        /// <summary>
        /// Called by teh ServiceManager to clean the service up at the end of the runtime
        /// Cleans up the created GameObject so that it can be returned to the ObjectPool
        /// </summary>
        public void Cleanup()
        {
            managerInstanceObj.name = "GameObject";
            managerInstanceObj.transform.parent = null;
            GameObject.Destroy(Manager);
            ObjectPool<GameObject>.ReleaseResource(managerInstanceObj);
            managerInstanceObj = null;
        }
    }
}