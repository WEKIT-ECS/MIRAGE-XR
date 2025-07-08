using UnityEditor;

namespace ReadyPlayerMe.Core.Editor
{
    public class ModelAssetProcessor : AssetPostprocessor
    {
        private const string ANIMATION_ASSET_PATH = "Assets/Ready Player Me/Animations";

        private void OnPreprocessModel()
        {
            var modelImporter = assetImporter as ModelImporter;
            UpdateAnimationFileSettings(modelImporter);
        }

        private void UpdateAnimationFileSettings(ModelImporter modelImporter)
        {
            void SetModelImportData()
            {
                if (modelImporter is null) return;
                modelImporter.useFileScale = false;
                modelImporter.animationType = ModelImporterAnimationType.Human;
            }

            if (assetPath.Contains(ANIMATION_ASSET_PATH))
            {
                SetModelImportData();
            }
        }
    }
}
