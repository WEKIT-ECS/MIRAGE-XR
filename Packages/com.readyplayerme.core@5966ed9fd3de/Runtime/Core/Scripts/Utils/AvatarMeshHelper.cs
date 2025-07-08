using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ReadyPlayerMe.Core
{
    public static class AvatarMeshHelper
    {
        private static readonly string[] headMeshNames =
        {
            "Renderer_Head",
            "Renderer_Hair",
            "Renderer_Beard",
            "Renderer_Teeth",
            "Renderer_Glasses",
            "Renderer_EyeLeft",
            "Renderer_EyeRight",
            "Renderer_Headwear",
            "Renderer_Facewear"
        };

        [Obsolete("Use TransferMesh(GameObject source, GameObject target) instead.")]
        public static void TransferMesh(GameObject source, SkinnedMeshRenderer[] targetMeshes, Animator targetAnimator)
        {
            TransferMesh(source, targetMeshes[0].transform.parent.gameObject);
        }

        /// <summary>
        ///     Transfers the mesh and material data from the source to the target avatar.
        /// </summary>
        /// <param name="source">Imported Ready Player Me avatar.</param>
        /// <param name="target">Photon Network Player Character.</param>
        public static void TransferMesh(GameObject source, GameObject target)
        {
            // store the relevant data of the source (downloaded) avatar
            var rendererDict = new Dictionary<string, SkinnedMeshRenderer>();

            var sourceRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
            var targetRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var renderer in sourceRenderers)
            {
                rendererDict.Add(renderer.name, renderer);
            }

            var meshWithBones = targetRenderers.DefaultIfEmpty(null).FirstOrDefault((renderer) => renderer.bones.Length != 0);

            // transfer the data to the target skinning mesh renderers
            foreach (var renderer in targetRenderers)
            {
                if (rendererDict.TryGetValue(renderer.name, out var sourceRenderer))
                {
                    renderer.sharedMesh = sourceRenderer.sharedMesh;
                    renderer.sharedMaterial = sourceRenderer.sharedMaterial;

                    if (renderer.bones.Length == 0 && meshWithBones != null)
                    {
                        renderer.bones = meshWithBones.bones;
                        continue;
                    }
                    // transfer the bone data
                    foreach (var targetBone in renderer.bones)
                    {
                        foreach (var sourceBone in sourceRenderer.bones)
                        {
                            if (sourceBone.name == targetBone.name)
                            {
                                targetBone.position = sourceBone.position;
                                targetBone.rotation = sourceBone.rotation;
                                targetBone.localScale = sourceBone.localScale;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    renderer.sharedMesh = null;
                    renderer.material = null;
                }
            }

            // transfer the animation avatar
            var sourceAnimator = source.GetComponentInChildren<Animator>();
            var targetAnimator = target.GetComponentInChildren<Animator>();
            targetAnimator.avatar = sourceAnimator.avatar;
        }

        /// <summary>
        ///     Returns the meshes of the head of the avatar, such as glasses, hair, teeth, etc.
        /// </summary>
        /// <param name="avatar">A Ready Player Me Avatar.</param>
        /// <returns>Head meshes in a GameObject Array.</returns>
        public static GameObject[] GetHeadMeshes(GameObject avatar)
        {
            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            var headMeshes = new List<GameObject>();

            foreach (var renderer in renderers)
            {
                if (headMeshNames.Contains(renderer.name))
                {
                    headMeshes.Add(renderer.gameObject);
                }
            }

            return headMeshes.ToArray();
        }
    }
}
