using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    public static class PrefabHelper
    {
        private const string TAG = nameof(PrefabHelper);
        public static void TransferPrefabByGuid(string guid, string newPath)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"Prefab with guid {guid} not found.");
                return;
            }
            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(newPath, typeof(GameObject));
        }
        
        public static GameObject CreateAvatarPrefab(AvatarMetadata avatarMetadata, string path, string prefabPath = null, AvatarConfig avatarConfig = null)
        {
            var modelFilePath = $"{path}.glb";
            AssetDatabase.Refresh();
            var avatarSource = AssetDatabase.LoadAssetAtPath(modelFilePath, typeof(GameObject));
            var newAvatar = (GameObject) PrefabUtility.InstantiatePrefab(avatarSource);
            var avatarProcessor = new AvatarProcessor();
            PrefabUtility.UnpackPrefabInstance(newAvatar, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            avatarProcessor.ProcessAvatar(newAvatar, avatarMetadata, avatarConfig);
            var avatarData = newAvatar.AddComponent<AvatarData>();
            avatarData.AvatarMetadata = avatarMetadata;
            avatarData.AvatarId = newAvatar.name;
            CreatePrefab(newAvatar, prefabPath ?? $"{path}.prefab");
            return newAvatar;
        }
        
        public static void CreatePrefab(GameObject source, string path)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(source, path, InteractionMode.AutomatedAction, out var success);
            PrefabUtility.ApplyObjectOverride(source, path, InteractionMode.AutomatedAction);

            SDKLogger.Log(TAG, success ? $"Prefab created successfully at path: {path}" : "Prefab creation failed");

            AssetDatabase.Refresh();
            EditorUtility.SetDirty(source);
        }
    }
}
