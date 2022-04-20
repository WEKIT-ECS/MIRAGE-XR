using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirageXR
{
    public static class ReferenceLoader
    {
        /// <summary>
        /// Get the gameobject prefab reference from addressable 
        /// </summary>
        /// <param name="prefabName">The prefab name in addressable</param>
        /// <returns>The prefab gameobject</returns>
        public static async Task<GameObject> GetAssetReference(string prefabName)
        {
            var prefabInAddressable = Addressables.LoadAssetAsync<GameObject>(prefabName);
            await prefabInAddressable.Task;

            //if the prefab reference has been found successfully
            if (prefabInAddressable.Status == AsyncOperationStatus.Succeeded)
            {
                return prefabInAddressable.Result;
            }
            else
            {
                return null;
            }

        }

    }

}
