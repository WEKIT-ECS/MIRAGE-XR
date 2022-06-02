using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public static class ReferenceLoader
    {
        /// <summary>
        /// Get the gameobject prefab reference from addressable 
        /// </summary>
        /// <param name="prefabName">The prefab name in addressable</param>
        /// <returns>The prefab gameobject</returns>
        public static async Task<T> GetAssetReferenceAsync<T>(string prefabName) where T : Object
        {
            var operation = Addressables.LoadAssetAsync<T>(prefabName);
            await operation.Task;

            if (operation.Status != AsyncOperationStatus.Failed) return operation.Result;
            
            Debug.LogError(operation.OperationException);
            return null;
        }
    }
}
