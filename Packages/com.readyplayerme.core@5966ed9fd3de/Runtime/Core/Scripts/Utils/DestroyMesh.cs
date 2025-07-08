using UnityEngine;

namespace ReadyPlayerMe
{
    /// <summary>
    /// This class is responsible for destroying all SkinnedMeshRenderer components and their associated resources (meshes, materials, and textures)
    /// when the GameObject is destroyed to prevent memory leaks.
    /// </summary>
    public class DestroyMesh : MonoBehaviour
    {
        private SkinnedMeshRenderer[] meshes;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the meshes array by finding all SkinnedMeshRenderer components in the child objects of the current GameObject.
        /// </summary>
        private void Awake()
        {
            meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        /// <summary>
        /// Called when the GameObject is destroyed.
        /// Destroys all associated SkinnedMeshRenderer meshes, materials, and textures to ensure proper memory management.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var mesh in meshes)
            {
                var materials = mesh.sharedMaterials;

                foreach (var material in materials)
                {
                    if (material == null) continue;

                    foreach (var property in material.GetTexturePropertyNames())
                    {
                        var texture = material.GetTexture(property);

                        if (texture == null) continue;

                        Destroy(texture);
                    }

                    Destroy(material);
                }

                Destroy(mesh.sharedMesh);
            }
        }
    }
}
