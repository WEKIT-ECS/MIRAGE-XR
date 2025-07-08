using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReadyPlayerMe.Core.Editor
{
    public class FBXAnimationExtractor : UnityEditor.Editor
    {
        private const string FBX_FILE_SUFFIX = ".fbx";
        private const string PREVIEW_ANIM_PREFIX = "__preview__";
        private const string ASSET_ANIM_SUFFIX = ".anim";

        [MenuItem("Assets/Extract Animations", false, 9999)]
        private static string[] ExtractAnimations()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var directoryName = Path.GetDirectoryName(assetPath);

            var assetGUIDs = Selection.assetGUIDs;
            var paths = new string[assetGUIDs.Length];
            for (var i = 0; i < assetGUIDs.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                LoadAssetsAndExtractClips(paths[i], directoryName);
            }
            if (paths.Length < 1)
            {
                return null;
            }
            return paths.Where(path => path.Contains(FBX_FILE_SUFFIX)).ToArray();
            ;
        }

        private static void LoadAssetsAndExtractClips(string path, string directoryName)
        {
            Object[] clips = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object clip in clips)
            {
                if (clip != null && clip is AnimationClip animationClip && !animationClip.name.StartsWith(PREVIEW_ANIM_PREFIX))
                {
                    TryCreateAsset(animationClip, directoryName);
                }
            }
        }

        private static void TryCreateAsset(AnimationClip clip, string directoryName)
        {
            try
            {
                var temp = new AnimationClip();
                EditorUtility.CopySerialized(clip, temp);
                var validatedName = string.Join("_", clip.name.Split(Path.GetInvalidFileNameChars()));
                AssetDatabase.CreateAsset(temp, GetUniqueName($"{directoryName}/{validatedName}"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create animation asset with exception: {e}");
                throw;
            }

        }

        private static string GetUniqueName(string baseAssetPath)
        {
            var nameUnique = false;
            var suffix = "";
            var uniqueIndex = 0;
            while (!nameUnique)
            {
                if (File.Exists($"{baseAssetPath}{suffix}{ASSET_ANIM_SUFFIX}"))
                {
                    suffix = $"_{uniqueIndex}";
                    uniqueIndex++;
                    continue;
                }
                nameUnique = true;
            }

            return $"{baseAssetPath}{suffix}{ASSET_ANIM_SUFFIX}";
        }

        [MenuItem("Assets/Extract Animations and Delete File", false, 9999)]
        private static void ExtractAnimationsAndDeleteFile()
        {
            var animationFilePaths = ExtractAnimations();
            if (animationFilePaths == null || animationFilePaths.Length < 1)
            {
                return;
            }
            if (EditorUtility.DisplayDialog("File Deletion Warning", $"Are you sure you want to delete {animationFilePaths.Length} .fbx files?",
                    "Okay", "Cancel"))
            {
                DeleteFbxFiles(animationFilePaths);
            }
        }

        private static void DeleteFbxFiles(string[] animationFilePaths)
        {
            foreach (var animationFilePath in animationFilePaths)
            {
                AssetDatabase.DeleteAsset(animationFilePath);
            }
        }

        [MenuItem("Assets/Extract Animations", true), MenuItem("Assets/Extract Animations and Delete File", true)]
        private static bool ExtractAnimationsValidation()
        {
            var activeObject = Selection.activeObject;
            if (!activeObject) return false;

            var assetPath = AssetDatabase.GetAssetPath(activeObject);
            var isFbx = Path.GetExtension(assetPath).ToLower() == FBX_FILE_SUFFIX;
            var isGameObject = activeObject is GameObject;

            return isGameObject && isFbx;
        }
    }
}
