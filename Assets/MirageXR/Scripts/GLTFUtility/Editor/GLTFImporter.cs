using UnityEditor;

using UnityEngine;

namespace Siccity.GLTFUtility {
	//[ScriptedImporter(1, "gltf")]
	public class GLTFImporter : UnityEditor.AssetImporters.ScriptedImporter {

		public ImportSettings importSettings;

		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
			// Load asset
			AnimationClip[] animations;
			if (importSettings == null) importSettings = new ImportSettings();
			GameObject root = Importer.LoadFromFile(ctx.assetPath, importSettings, out animations, Format.GLTF);

			// Save asset
			GLTFAssetUtility.SaveToAsset(root, animations, ctx, importSettings);
		}
	}
}