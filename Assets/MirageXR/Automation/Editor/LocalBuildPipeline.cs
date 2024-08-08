using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Unity.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;

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
        public static string[] Scenes = { "Assets/MirageXR/Scenes/Start.unity", "Assets/MirageXR/Scenes/Player.unity", "Assets/MirageXR/Scenes/ActivitySelection.unity" };

        /// <summary>
        /// Configure plugin: activate, see https://docs.unity3d.com/Packages/com.unity.xr.management@4.4/manual/EndUser.html#example-configure-plug-ins-per-build-target
        /// </summary>
        /// <param name="buildTargetGroup"></param>
        /// <param name="pluginName">String, should be "Unity.XR.Oculus.OculusLoader" or "UnityEngine.XR.OpenXR.OpenXRLoader"</param>
        public static void EnablePlugin(BuildTargetGroup buildTargetGroup, string pluginName)
        {
            var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            if (buildTargetSettings != null) {
                Debug.Log($"Successfully retrieved XR General Settings Per Build Target for {buildTargetSettings.name}");
            } else
            {
                Debug.Log($"Could not retrieve XR General Settings Per Build Target");
            }

            var pluginSettings = buildTargetSettings.AssignedSettings;
            if (pluginSettings != null) {
                Debug.Log($"Successfully retrieved assigned plugin settings {pluginSettings.name}");
            } else
            {
                Debug.Log($"Could not retrieve assigned plugin settings");
            }

            var success = XRPackageMetadataStore.AssignLoader(pluginSettings, pluginName, buildTargetGroup);
            if (success)
            {
                Debug.Log($"XR Plug-in Management: Enabled {pluginName} plugin on {buildTargetGroup}");
            } else
            {
                Debug.Log($"XR Plug-in Management: enabling failed for {pluginName} plugin on {buildTargetGroup}");
            }
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

        [MenuItem("MirageXR/Build/Quest 3 (release)")]
		static void BuildQuest()
		{
			Debug.Log("starting: Quest 3 build...");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            buildPlayerOptions.scenes = Scenes;
            buildPlayerOptions.locationPathName = "Build/Android/mirageXR.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            // explicitly change build target (needed?)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // see https://forum.unity.com/threads/editor-programmatically-set-the-vr-system-in-xr-plugin-management.972285/
            //XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            //EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            //XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            //XRPackageMetadataStore.AssignLoader(settings.Manager, "Unity.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);

            EnablePlugin(BuildTargetGroup.Android, "UnityEngine.XR.OpenXR.OpenXRLoader");
            
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }

        }

	}
}
