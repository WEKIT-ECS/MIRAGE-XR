using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ReadyPlayerMe.Core.Editor
{
    public enum RenderPipeline { Standard, URP, HDRP }
    public static class GraphicsSettingsUtility
    {
        private const string TAG = nameof(GraphicsSettingsUtility);
        private const string PRELOADED_SHADER_PROPERTY = "m_PreloadedShaders";
        private const string GRAPHICS_SETTING_PATH = "ProjectSettings/GraphicsSettings.asset";

        private const string SHADER_VARIANT_ASSETS_FOLDER = "Assets/Ready Player Me/Core/Runtime/Core/Shaders";
        private const string SHADER_VARIANT_PACKAGES_FOLDER = "Packages/com.readyplayerme.core/Runtime/Core/Shaders";

        private const string SHADER_VARIANTS_STANDARD = "glTFastShaderVariants";
        private const string SHADER_VARIANTS_URP = "glTFastShaderVariantsURP";
        private const string SHADER_VARIANTS_HDRP = "glTFastShaderVariantsHDRP";

        private const string HDRP_TYPE_NAME = "HDRenderPipelineAsset";
        private const string URP_TYPE_NAME = "UniversalRenderPipelineAsset";
        private const string SHADER_SESSION_CHECK = "SHADER_SESSION_CHECK";
        private const string VARIANTS_FOUND_LOG = "glTFast shader variants found in Graphics Settings->Preloaded Shaders";
        private const string SHADER_VARIANTS_EXTENSION = ".shadervariants";

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            if (SessionState.GetBool(SHADER_SESSION_CHECK, false)) return;
            SessionState.SetBool(SHADER_SESSION_CHECK, true);

            EditorApplication.update += CheckAndUpdatePreloadShaders;
        }

        private static void CheckAndUpdatePreloadShaders()
        {
            EditorApplication.update -= CheckAndUpdatePreloadShaders;
            AddPreloadShaderVariants(true);
        }

        public static void AddPreloadShaderVariants(bool checkForMissingVariants = false)
        {
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>(GRAPHICS_SETTING_PATH);
            var serializedGraphicsObject = new SerializedObject(graphicsSettings);
            SerializedProperty shaderPreloadArray = serializedGraphicsObject.FindProperty(PRELOADED_SHADER_PROPERTY);
            AssetDatabase.Refresh();

            var newArrayIndex = shaderPreloadArray.arraySize;
            var shaderVariants = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(GetTargetShaderPath());
            if (shaderVariants == null)
            {
                Debug.LogWarning($"Shader variants not found at {GetTargetShaderPath()}");
                return;
            }
            if (checkForMissingVariants)
            {
                var shadersMissing = true;
                var serializedVariants = new SerializedObject(shaderVariants);

                foreach (SerializedProperty shaderInclude in shaderPreloadArray)
                {
                    if (shaderInclude.objectReferenceValue.name == serializedVariants.targetObject.name)
                    {
                        SDKLogger.Log(TAG, VARIANTS_FOUND_LOG);
                        shadersMissing = false;
                        break;
                    }
                }
                if (!shadersMissing) return;
            }

            shaderPreloadArray.InsertArrayElementAtIndex(newArrayIndex);
            SerializedProperty shaderInArray = shaderPreloadArray.GetArrayElementAtIndex(newArrayIndex);
            shaderInArray.objectReferenceValue = shaderVariants;

            serializedGraphicsObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public static bool IsMissingVariants()
        {
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>(GRAPHICS_SETTING_PATH);
            var serializedGraphicsObject = new SerializedObject(graphicsSettings);

            SerializedProperty shaderPreloadArray = serializedGraphicsObject.FindProperty(PRELOADED_SHADER_PROPERTY);

            var shaderVariants = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(GetTargetShaderPath());
            var shadersMissing = true;
            var serializedVariants = new SerializedObject(shaderVariants);

            foreach (SerializedProperty shaderInclude in shaderPreloadArray)
            {
                if (shaderInclude.objectReferenceValue.name == serializedVariants.targetObject.name)
                {
                    SDKLogger.Log(TAG, VARIANTS_FOUND_LOG);

                    shadersMissing = false;
                    break;
                }
            }
            return shadersMissing;
        }

        private static string GetTargetShaderPath()
        {
            var shaderFolderPath = SHADER_VARIANT_PACKAGES_FOLDER;
            if (!Directory.Exists(shaderFolderPath))
            {
                shaderFolderPath = SHADER_VARIANT_ASSETS_FOLDER;
            }
            return GetCurrentRenderPipeline() switch
            {
                RenderPipeline.URP => $"{shaderFolderPath}/{SHADER_VARIANTS_URP}{SHADER_VARIANTS_EXTENSION}",
                RenderPipeline.HDRP => $"{shaderFolderPath}/{SHADER_VARIANTS_HDRP}{SHADER_VARIANTS_EXTENSION}",
                _ => $"{shaderFolderPath}/{SHADER_VARIANTS_STANDARD}{SHADER_VARIANTS_EXTENSION}"
            };
        }

        private static RenderPipeline GetCurrentRenderPipeline()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                var renderPipelineType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (renderPipelineType.Contains(HDRP_TYPE_NAME))
                {
                    return RenderPipeline.HDRP;
                }
                if (renderPipelineType.Contains(URP_TYPE_NAME))
                {
                    return RenderPipeline.URP;
                }
            }
            return RenderPipeline.Standard;
        }
    }
}
