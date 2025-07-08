using GLTFast;
using GLTFast.Logging;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible instantiating Avatar model as a GameObject.
    /// </summary>
    public class GltFastGameObjectInstantiator : GameObjectInstantiator
    {
        public GltFastGameObjectInstantiator(
            IGltfReadable gltf,
            Transform parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
        )
            : base(gltf, parent, logger, settings)
        {
        }

        /// <inheritdoc />
        public override void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        )
        {
            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }

            GameObject meshGo;
            if (primitiveNumeration == 0)
            {
                // Use Node GameObject for first Primitive
                meshGo = m_Nodes[nodeIndex];
                // Ready Player Me - Parent mesh to Avatar root game object
                meshGo.transform.SetParent(m_Parent.transform);
            }
            else
            {
                meshGo = new GameObject(meshName);
                meshGo.transform.SetParent(m_Nodes[nodeIndex].transform, false);
                meshGo.layer = m_Settings.Layer;
            }

            Renderer renderer;

            var hasMorphTargets = meshResult.mesh.blendShapeCount > 0;
            if (joints == null && !hasMorphTargets)
            {
                var mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = meshResult.mesh;
                var mr = meshGo.AddComponent<MeshRenderer>();
                renderer = mr;
            }
            else
            {
                var smr = meshGo.AddComponent<SkinnedMeshRenderer>();
                smr.updateWhenOffscreen = m_Settings.SkinUpdateWhenOffscreen;
                if (joints != null)
                {
                    var bones = new Transform[joints.Length];
                    for (var j = 0; j < bones.Length; j++)
                    {
                        var jointIndex = joints[j];
                        bones[j] = m_Nodes[jointIndex].transform;
                    }
                    smr.bones = bones;
                    if (rootJoint.HasValue)
                    {
                        smr.rootBone = m_Nodes[rootJoint.Value].transform;
                    }
                }
                smr.sharedMesh = meshResult.mesh;
                if (morphTargetWeights != null)
                {
                    for (var i = 0; i < morphTargetWeights.Length; i++)
                    {
                        var weight = morphTargetWeights[i];
                        smr.SetBlendShapeWeight(i, weight);
                    }
                }
                renderer = smr;
            }

            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < materials.Length; index++)
            {
                var material = m_Gltf.GetMaterial(meshResult.materialIndices[index]) ?? m_Gltf.GetDefaultMaterial();
                materials[index] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }
}
