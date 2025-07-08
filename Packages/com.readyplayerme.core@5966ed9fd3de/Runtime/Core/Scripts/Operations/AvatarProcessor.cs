using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for making processing the avatar after it has been loaded into a GameObject.
    /// </summary>
    public class AvatarProcessor : IOperation<AvatarContext>
    {
        private const string TAG = nameof(AvatarProcessor);

        public int Timeout { get; set; }

        /// <summary>
        /// An <see cref="Action" /> callback that can be used to subscribe to <see cref="WebRequestDispatcherExtension" />
        /// <c>ProgressChanged</c> events.
        /// </summary>
        public Action<float> ProgressChanged { get; set; }

        /// <summary>
        /// Executes the operation to process the avatar <see cref="GameObject" />.
        /// </summary>
        /// <param name="context">A container for all the data related to the Avatar model.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>The updated <c>AvatarContext</c>.</returns>
        public Task<AvatarContext> Execute(AvatarContext context, CancellationToken token)
        {
            if (context.Data is GameObject)
            {
                context = ProcessAvatarGameObject(context);
                ProcessAvatar(context.Data as GameObject, context.Metadata, context.AvatarConfig);
                ProgressChanged?.Invoke(1);
                return Task.FromResult(context);
            }

            throw new CustomException(FailureType.AvatarProcessError, $"Avatar postprocess failed. {context.Data} is either null or is not of type GameObject");
        }

        /// <summary>
        /// Replaces the instance of the avatar GameObject if it already exists and sets the name of the GameObject.
        /// </summary>
        /// <param name="context">A container for all the data related to the Avatar model.</param>
        /// <returns>The <see cref="AvatarContext" />.</returns>
        private AvatarContext ProcessAvatarGameObject(AvatarContext context)
        {

            ((Object) context.Data).name = context.AvatarUri.Guid;

            return context;
        }

        /// <summary>
        /// This method triggers GameObject changes and setup for the new avatar GameObject.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="avatarMetadata"></param>
        /// <param name="avatarConfig"></param>
        public void ProcessAvatar(GameObject avatar, AvatarMetadata avatarMetadata, AvatarConfig avatarConfig = null)
        {
            SDKLogger.Log(TAG, PROCESSING_AVATAR);

            try
            {
                if (avatar.transform.Find(BONE_HALF_BODY_ROOT))
                {
                    RemoveHalfBodyRoot(avatar);
                }

                if (!avatar.transform.Find(BONE_ARMATURE))
                {
                    AddArmatureBone(avatar);
                }

                if (avatarMetadata.BodyType == BodyType.FullBody || avatarMetadata.BodyType == BodyType.FullBodyXR)
                {
                    var animator = avatar.GetComponent<Animator>();
                    if (animator == null)
                    {
                        animator = avatar.AddComponent<Animator>();
                    }
                    AvatarAnimationHelper.SetupAnimator(avatarMetadata, animator);
                }

                RenameChildMeshes(avatar, avatarConfig);
            }
            catch (Exception e)
            {
                var message = $"Avatar postprocess failed. {e.Message}";
                SDKLogger.Log(TAG, message);
                throw new CustomException(FailureType.AvatarProcessError, message);
            }
        }

        #region Setup Armature and Animations

        // Bone names
        private const string BONE_HIPS = "Hips";
        private const string BONE_ARMATURE = "Armature";
        private const string BONE_HALF_BODY_ROOT = "AvatarRoot";

        /// <summary>
        /// Removes the roo bone to ensure the correct skeleton hierarchy.
        /// </summary>
        /// <param name="avatar">The <see cref="GameObject" /> to update.</param>
        private void RemoveHalfBodyRoot(GameObject avatar)
        {
            var root = avatar.transform.Find(BONE_HALF_BODY_ROOT);
            for (var i = root.childCount - 1; i >= 0; --i)
            {
                root.GetChild(i).transform.SetParent(avatar.transform);
            }
            Object.DestroyImmediate(root.gameObject);
        }

        /// <summary>
        /// Adds the root armature bone to ensure the correct skeleton hierarchy.
        /// </summary>
        /// <param name="avatar">The <see cref="GameObject" /> to update.</param>
        private void AddArmatureBone(GameObject avatar)
        {
            SDKLogger.Log(TAG, ADDING_ARMATURE_BONE);

            var armature = new GameObject();
            armature.name = BONE_ARMATURE;
            armature.transform.parent = avatar.transform;

            var hips = avatar.transform.Find(BONE_HIPS);
            if (hips) hips.parent = armature.transform;
        }

        #endregion

        #region Set Component Names

        // Prefix to remove from names for correction
        private const string PREFIX = "Wolf3D_";

        // Default prefixes
        private const string AVATAR_PREFIX = "Avatar";
        private const string RENDERER_PREFIX = "Renderer";
        private const string MATERIAL_PREFIX = "Material";
        private const string SKINNED_MESH_PREFIX = "SkinnedMesh";
        private const string PROCESSING_AVATAR = "Processing avatar.";
        private const string ADDING_ARMATURE_BONE = "Adding armature bone";

        // Shader properties by TextureChannel
        private static readonly Dictionary<TextureChannel, string> ShaderProperties = new Dictionary<TextureChannel, string>
        {
            { TextureChannel.BaseColor, "baseColorTexture" },
            { TextureChannel.Normal, "normalTexture" },
            { TextureChannel.Emissive, "emissiveTexture" },
            { TextureChannel.Occlusion, "occlusionTexture" },
            { TextureChannel.MetallicRoughness, "metallicRoughnessTexture" }
        };

        /// <summary>
        /// Rename avatar assets.
        /// </summary>
        /// <param name="avatar">The <see cref="GameObject" /> to update.</param>
        /// <param name="avatarConfig"></param>
        /// <remarks>Naming convention is 'Avatar_Type_Name'. This makes it easier to view them in profiler</remarks>
        private void RenameChildMeshes(GameObject avatar, AvatarConfig avatarConfig = null)
        {
            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var renderer in renderers)
            {
                var assetName = renderer.name.Replace(PREFIX, "");

                renderer.name = $"{RENDERER_PREFIX}_{assetName}";
                renderer.sharedMaterial.name = $"{MATERIAL_PREFIX}_{assetName}";

                if (avatarConfig != null && avatarConfig.Shader != null)
                {
                    OverrideShader(renderer, avatarConfig);
                }

                SetMeshName(renderer, assetName);
            }
        }

        private void OverrideShader(Renderer renderer, AvatarConfig avatarConfig)
        {
            var sourceMaterial = renderer.sharedMaterial;
            var targetMaterial = new Material(avatarConfig.Shader);
            ShaderMaterialOverrideHelper.ApplyOverrides(sourceMaterial, targetMaterial, avatarConfig.ShaderPropertyMapping.ToArray());
            renderer.sharedMaterial = targetMaterial;
        }

        /// <summary>
        /// Set the name of the <see cref="SkinnedMeshRenderer" />.
        /// </summary>
        /// <param name="renderer">SkinMeshRenderer to update.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <remarks>Naming convention is 'SkinnedMesh_AssetName'. This makes it easier to view in profiler</remarks>
        private void SetMeshName(SkinnedMeshRenderer renderer, string assetName)
        {
            renderer.sharedMesh.name = $"{SKINNED_MESH_PREFIX}_{assetName}";
            renderer.updateWhenOffscreen = true;
        }

        #endregion
    }
}
