using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Unity.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using System.Collections.Generic;
using System.Linq;

namespace MirageXR
{

    /// <summary>
    /// This class adds new menu items to the editor for compiling MirageXR for specific platforms.
    /// </summary>
    public class LocalBuildPipeline
    {

        /// <summary>
        /// List of scenes to include in the build
        /// </summary>
        private static List<string> mScenePaths = EditorBuildSettings.scenes.Select(l => l.path).ToList();

        /// <summary>
        /// Configure plugin: activate, see https://docs.unity3d.com/Packages/com.unity.xr.management@4.4/manual/EndUser.html#example-configure-plug-ins-per-build-target
        /// </summary>
        /// <param name="buildTargetGroup"></param>
        /// <param name="pluginName">String, should be "Unity.XR.Oculus.OculusLoader" or "UnityEngine.XR.OpenXR.OpenXRLoader"</param>
        public static void EnablePlugin(BuildTargetGroup buildTargetGroup, string pluginName)
        {
            var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            if (buildTargetSettings != null)
            {
                Debug.Log($"Successfully retrieved XR General Settings Per Build Target for {buildTargetSettings.name}");
            }
            else
            {
                Debug.Log($"Could not retrieve XR General Settings Per Build Target");
            }

            var pluginSettings = buildTargetSettings.AssignedSettings;
            if (pluginSettings != null)
            {
                Debug.Log($"Successfully retrieved assigned plugin settings {pluginSettings.name}");
            }
            else
            {
                Debug.Log($"Could not retrieve assigned plugin settings");
            }

            var success = XRPackageMetadataStore.AssignLoader(pluginSettings, pluginName, buildTargetGroup);
            if (success)
            {
                Debug.Log($"XR Plug-in Management: Enabled {pluginName} plugin on {buildTargetGroup}");
            }
            else
            {
                Debug.Log($"XR Plug-in Management: enabling failed for {pluginName} plugin on {buildTargetGroup}");
            }

            // from https://forum.unity.com/threads/editor-programmatically-set-the-vr-system-in-xr-plugin-management.972285/ :
            //XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            //EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);

        }

        /// <summary>
        /// Deactivate plugin
        /// </summary>
        /// <param name="buildTargetGroup"></param>
        /// <param name="pluginName">String, should be "Unity.XR.Oculus.OculusLoader" or "UnityEngine.XR.OpenXR.OpenXRLoader"</param>
        public static void DisablePlugin(BuildTargetGroup buildTargetGroup, string pluginName)
        {
            var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            var pluginSettings = buildTargetSettings.AssignedSettings;

            var success = XRPackageMetadataStore.RemoveLoader(pluginSettings, pluginName, buildTargetGroup);
            if (success)
            {
                Debug.Log($"XR Plug-in Management: Disabled {pluginName} plugin on {buildTargetGroup}");
            }
        }

        /// <summary>
        /// Build menu entry for 'Quest 3'
        /// </summary>
        [MenuItem("MirageXR/Build/Quest 3")]
        static void BuildQuest()
        {
            Debug.Log("starting a 'Quest 3' build...");

            // change build target if needed
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log($"Switching build target to Android");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            else
            {
                Debug.Log($"Didn't need to switch build target, as it was already set to Android");
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            //  Basic build settings
            buildPlayerOptions.scenes = mScenePaths.ToArray(); // from EditorBuildSettings
            buildPlayerOptions.locationPathName = "Builds/quest3/mirageXR.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            //  Texture compression ASTC
            MobileTextureSubtarget oldEditorUserBuildSettings = EditorUserBuildSettings.androidBuildSubtarget; // temp
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            //  Gradle
            AndroidBuildSystem oldAndroidBuildSystem = EditorUserBuildSettings.androidBuildSystem; // temp
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            // if (!Lightmapping.Bake()) {
            //      Debug.LogError("Unable to bake lightmaps");
            //}

            // if (!StaticOcclusionCulling.Compute()) {
            //      Debug.LogError("Unable to bake occlusion culling maps");
            // }

            // https://exyte.com/blog/optimization-techniques-for-porting-unity-steamvr-games-to-oculus-quest
            // FindObjectOfType<OculusQuestOptimizer>()?.Optimize();

            // AR core should be disabled!
            // DisablePlugin(BuildTargetGroup.Android, "UnityEngine.XR.ARcore.OpenXRLoader");

            EnablePlugin(BuildTargetGroup.Android, "UnityEngine.XR.OpenXR.OpenXRLoader");

            // TODO: URP pipeline asset renderer: disable postprocessing
            // TODO: URP pipeline asset: disable terrain holes; disable HDR; shadows max distance: 2.5

            Material oldSkybox = RenderSettings.skybox;
            RenderSettings.skybox = Resources.Load<Material>("Assets/MirageXR/Automation/Black"); // requires alpha enabled material!

            AssetDatabase.SaveAssets();

            // Build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            // Report
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }

            // Restore previous values from temp
            EditorUserBuildSettings.androidBuildSystem = oldAndroidBuildSystem;
            EditorUserBuildSettings.androidBuildSubtarget = oldEditorUserBuildSettings;
            RenderSettings.skybox = oldSkybox;

        } // methodBuildQuest()

    } // class LocalBuildPipeline()

} // namespace