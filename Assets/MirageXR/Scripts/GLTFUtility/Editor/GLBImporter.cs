
using UnityEngine;

namespace Siccity.GLTFUtility {
	//[ScriptedImporter(1, "glb")] 
	public class GLBImporter : GLTFImporter {

		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
			// Load asset
			AnimationClip[] animations;
			if (importSettings == null) importSettings = new ImportSettings();
			GameObject root = Importer.LoadFromFile(ctx.assetPath, importSettings, out animations, Format.GLB);
			// Save asset
			GLTFAssetUtility.SaveToAsset(root, animations, ctx, importSettings);
		}
	}
}