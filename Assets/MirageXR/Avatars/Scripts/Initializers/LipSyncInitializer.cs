using uLipSync;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// Sets up the blend shape driver to prepare for the lip sync
	/// Note that this does not add the uLipSync component as this is supposed to be added together with an audio source.
	/// For the networked variant, this is done via a prefab
	/// </summary>
	public class LipSyncInitializer : AvatarInitializer
	{
		/// <summary>
		/// Runs somewhere between the local setup and the networking setup
		/// </summary>
		public override int Priority => -2;

		/// <summary>
		/// Adds a uLipSyncBlendShape component as the blend shape driver for lip sync.
		/// It then sets up the blend shape mapping.
		/// </summary>
		/// <param name="avatar">The avatar object which should be initialized</param>
		public override void InitializeAvatar(GameObject avatar)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
			uLipSyncBlendShape blendShapeController = avatar.AddComponent<uLipSyncBlendShape>();
			blendShapeController.skinnedMeshRenderer = skinnedMeshRenderer;
			blendShapeController.AddBlendShape("A", "viseme_aa");
			blendShapeController.AddBlendShape("I", "viseme_I");
			blendShapeController.AddBlendShape("U", "viseme_U");
			blendShapeController.AddBlendShape("E", "viseme_E");
			blendShapeController.AddBlendShape("O", "viseme_O");
			blendShapeController.AddBlendShape("-", "");
			blendShapeController.AddBlendShape("S", "viseme_SS");
		}
	}
}
