using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class contains a number of different extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        private const string TAG = nameof(ExtensionMethods);

        /// <summary>
        /// Implements a <see cref="CustomException" /> for the <paramref name="token" />.
        /// </summary>
        /// <param name="token">The <see cref="CancellationToken" />.</param>
        public static void ThrowCustomExceptionIfCancellationRequested(this CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new CustomException(FailureType.OperationCancelled, OPERATION_WAS_CANCELLED);
            }
        }

        /// <summary>
        /// Saves the avatar metadata to a local file.
        /// </summary>
        /// <param name="metadata">The metadata to save.</param>
        /// <param name="guid">The avatar guid (identifier).</param>
        /// <param name="path">The path to save the file.</param>
        /// <param name="saveInProject">If true it will save in the project folder instead of the persistant data path.</param>
        public static void SaveToFile(this AvatarMetadata metadata, string guid, string path)
        {
            DirectoryUtility.ValidateAvatarSaveDirectory(guid);
            var json = JsonConvert.SerializeObject(metadata);
            File.WriteAllText(path, json);
        }

        #region Get Picker

        // All possible names of objects with head mesh
        private static readonly string[] HeadMeshNameFilter = { "Renderer_Head", "Renderer_Avatar", "Renderer_Head_Custom" };

        private const string BEARD_MESH_NAME_FILTER = "Renderer_Beard";
        private const string TEETH_MESH_NAME_FILTER = "Renderer_Teeth";
        private const string OPERATION_WAS_CANCELLED = "Operation was cancelled";
        private const string COROUTINE_RUNNER = "[CoroutineRunner]";

        /// <summary>
        /// This method extends <c>GameObject</c> to simplify getting the Ready Player Me avatar's <c>SkinnedMeshRenderer</c>.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject" /> to search for a <see cref="SkinnedMeshRenderer" />.</param>
        /// <param name="meshType">Determines the <see cref="MeshType" /> to search for.</param>
        /// <param name="ignoreNullMesh">If true it will filter our <see cref="SkinnedMeshRenderers"/> with NO mesh data.</param>
        /// <returns>The <see cref="SkinnedMeshRenderer" /> if found.</returns>
        public static SkinnedMeshRenderer GetMeshRenderer(this GameObject gameObject, MeshType meshType, bool ignoreNullMesh = false)
        {
            SkinnedMeshRenderer mesh;
            var children = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            if (ignoreNullMesh)
            {
                children = children.Where(renderer => renderer.sharedMesh != null).ToList();
            }
            if (children.Count == 0)
            {

                SDKLogger.AvatarLoaderLogger.Log(TAG, $"No SkinnedMeshRenderer found on the Game Object {gameObject.name}.");
                return null;
            }

            switch (meshType)
            {
                case MeshType.BeardMesh:
                    mesh = children.FirstOrDefault(child => BEARD_MESH_NAME_FILTER == child.name);
                    break;
                case MeshType.TeethMesh:
                    mesh = children.FirstOrDefault(child => TEETH_MESH_NAME_FILTER == child.name);
                    break;
                case MeshType.HeadMesh:
                    mesh = children.FirstOrDefault(child => HeadMeshNameFilter.Contains(child.name));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meshType), meshType, null);
            }

            if (mesh != null) return mesh;

            SDKLogger.AvatarLoaderLogger.Log(TAG, $"Mesh type {meshType} not found on the Game Object {gameObject.name}.");

            return null;
        }

        #endregion
    }
}
