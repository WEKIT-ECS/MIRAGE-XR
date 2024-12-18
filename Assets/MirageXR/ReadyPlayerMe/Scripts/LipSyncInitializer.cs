using System.Collections;
using System.Collections.Generic;
using uLipSync;
using UnityEngine;

namespace MirageXR
{
	public class LipSyncInitializer : AvatarInitializer
	{
		public override int Priority => -2;

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
