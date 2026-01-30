using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEditor.XR.Interaction.Toolkit.ProjectValidation;
using UnityEngine;

namespace UnityEditor.XR.Interaction.Toolkit.Samples.VisionOS.Editor
{
    /// <summary>
    /// Unity Editor class which registers Project Validation rules for the visionOS sample,
    /// checking that other required samples and packages are installed.
    /// </summary>
    static class VisionOSSampleProjectValidation
    {
        const string k_SampleDisplayName = "visionOS Volume Demo";
        const string k_Category = "XR Interaction Toolkit";
        const string k_StarterAssetsSampleName = "Starter Assets";
        const string k_ProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";
        const string k_XRIPackageName = "com.unity.xr.interaction.toolkit";
        const string k_ShaderGraphPackageName = "com.unity.shadergraph";
        const string k_PolySpatialXRPackageName = "com.unity.polyspatial.xr";
        const string k_PolySpatialVisionOSPackageName = "com.unity.polyspatial.visionos";

        static readonly PackageVersion s_RecommendedVisionOSPackageVersion = new PackageVersion("1.1.6");

        static readonly BuildTargetGroup[] s_BuildTargetGroups =
            ((BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup))).Distinct().ToArray();

        static readonly List<BuildValidationRule> s_BuildValidationRules = new List<BuildValidationRule>
        {
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_PolySpatialXRPackageAddRequest == null || s_PolySpatialXRPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] PolySpatial XR ({k_PolySpatialXRPackageName}) package must be at version {s_RecommendedVisionOSPackageVersion} or higher to use the sample.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_PolySpatialXRPackageName) >= s_RecommendedVisionOSPackageVersion,
#if UNITY_2022_3_19_OR_NEWER
                FixIt = () =>
                {
                    if (s_PolySpatialXRPackageAddRequest == null || s_PolySpatialXRPackageAddRequest.IsCompleted)
                        s_PolySpatialXRPackageAddRequest = InstallOrUpdatePackage(k_PolySpatialXRPackageName);
                },
                FixItAutomatic = true,
#else
                FixItAutomatic = false,
#endif
                Error = true,
                HelpText = "This package requires Unity 2022.3.19f1 or newer.",
            },
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_PolySpatialVisionOSPackageAddRequest == null || s_PolySpatialVisionOSPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] PolySpatial visionOS ({k_PolySpatialVisionOSPackageName}) package must be at version {s_RecommendedVisionOSPackageVersion} or higher to use the sample.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_PolySpatialVisionOSPackageName) >= s_RecommendedVisionOSPackageVersion,
#if UNITY_2022_3_19_OR_NEWER
                FixIt = () =>
                {
                    if (s_PolySpatialVisionOSPackageAddRequest == null || s_PolySpatialVisionOSPackageAddRequest.IsCompleted)
                        s_PolySpatialVisionOSPackageAddRequest = InstallOrUpdatePackage(k_PolySpatialVisionOSPackageName);
                },
                FixItAutomatic = true,
#else
                FixItAutomatic = false,
#endif
                Error = true,
                HelpText = "This package requires Unity 2022.3.19f1 or newer.",
            },
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_ShaderGraphPackageAddRequest == null || s_ShaderGraphPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] Shader Graph ({k_ShaderGraphPackageName}) package must be installed for materials used in this sample.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.IsPackageInstalled(k_ShaderGraphPackageName),
                FixIt = () =>
                {
                    s_ShaderGraphPackageAddRequest = Client.Add(k_ShaderGraphPackageName);
                    if (s_ShaderGraphPackageAddRequest.Error != null)
                    {
                        Debug.LogError($"Package installation error: {s_ShaderGraphPackageAddRequest.Error}: {s_ShaderGraphPackageAddRequest.Error.message}");
                    }
                },
                FixItAutomatic = true,
                Error = false,
            },
            new BuildValidationRule
            {
                Message = $"[{k_SampleDisplayName}] {k_StarterAssetsSampleName} sample from XR Interaction Toolkit ({k_XRIPackageName}) package must be imported or updated to use this sample. {GetImportSampleVersionMessage(k_Category, k_StarterAssetsSampleName, ProjectValidationUtility.minimumXRIStarterAssetsSampleVersion)}",
                Category = k_Category,
                CheckPredicate = () => ProjectValidationUtility.SampleImportMeetsMinimumVersion(k_Category, k_StarterAssetsSampleName, ProjectValidationUtility.minimumXRIStarterAssetsSampleVersion),
                FixIt = () =>
                {
                    if (TryFindSample(k_XRIPackageName, string.Empty, k_StarterAssetsSampleName, out var sample))
                    {
                        sample.Import(Sample.ImportOptions.OverridePreviousImports);
                    }
                },
                FixItAutomatic = true,
                Error = !ProjectValidationUtility.HasSampleImported(k_Category, k_StarterAssetsSampleName),
            },
        };

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        static AddRequest s_PolySpatialXRPackageAddRequest;
        static AddRequest s_PolySpatialVisionOSPackageAddRequest;
#pragma warning restore CS0649
        static AddRequest s_ShaderGraphPackageAddRequest;

        [InitializeOnLoadMethod]
        static void RegisterProjectValidationRules()
        {
            foreach (var buildTargetGroup in s_BuildTargetGroups)
            {
                BuildValidator.AddRules(buildTargetGroup, s_BuildValidationRules);
            }

            // Delay evaluating conditions for issues to give time for Package Manager and UPM cache to fully initialize.
            EditorApplication.delayCall += ShowWindowIfIssuesExist;
        }

        static void ShowWindowIfIssuesExist()
        {
            foreach (var validation in s_BuildValidationRules)
            {
                if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
                {
                    ShowWindow();
                    return;
                }
            }
        }

        internal static void ShowWindow()
        {
            // Delay opening the window since sometimes other settings in the player settings provider redirect to the
            // project validation window causing serialized objects to be nullified.
            EditorApplication.delayCall += () =>
            {
                SettingsService.OpenProjectSettings(k_ProjectValidationSettingsPath);
            };
        }

        static bool TryFindSample(string packageName, string packageVersion, string sampleDisplayName, out Sample sample)
        {
            sample = default;

            if (!PackageVersionUtility.IsPackageInstalled(packageName))
                return false;

            IEnumerable<Sample> packageSamples;
            try
            {
                packageSamples = Sample.FindByPackage(packageName, packageVersion);
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule. Exception: {e}");
                return false;
            }

            if (packageSamples == null)
            {
                Debug.LogWarning($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
                return false;
            }

            foreach (var packageSample in packageSamples)
            {
                if (packageSample.displayName == sampleDisplayName)
                {
                    sample = packageSample;
                    return true;
                }
            }

            Debug.LogWarning($"Couldn't find {sampleDisplayName} sample in the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
            return false;
        }

        static string ToString(string packageName, string packageVersion)
        {
            return string.IsNullOrEmpty(packageVersion) ? packageName : $"{packageName}@{packageVersion}";
        }

        static AddRequest InstallOrUpdatePackage(string packageName)
        {
            // Set a 3-second timeout for request to avoid editor lockup
            var currentTime = DateTime.Now;
            var endTime = currentTime + TimeSpan.FromSeconds(3);

            var request = Client.Search(packageName);
            if (request.Status == StatusCode.InProgress)
            {
                Debug.Log($"Searching for ({packageName}) in Unity Package Registry.");
                while (request.Status == StatusCode.InProgress && currentTime < endTime)
                    currentTime = DateTime.Now;
            }

            var addRequestString = packageName;
            if (request.Status == StatusCode.Success && request.Result.Length > 0)
            {
                var versions = request.Result[0].versions;
#if UNITY_2022_2_OR_NEWER
                var recommendedVersion = new PackageVersion(versions.recommended);
#else
                var recommendedVersion = new PackageVersion(versions.verified);
#endif
                var latestCompatible = new PackageVersion(versions.latestCompatible);
                if (recommendedVersion < s_RecommendedVisionOSPackageVersion && s_RecommendedVisionOSPackageVersion <= latestCompatible)
                    addRequestString = $"{packageName}@{s_RecommendedVisionOSPackageVersion}";
            }

            var addRequest = Client.Add(addRequestString);
            if (addRequest.Error != null)
            {
                Debug.LogError($"Package installation error: {addRequest.Error}: {addRequest.Error.message}");
            }

            return addRequest;
        }

        static string GetImportSampleVersionMessage(string packageFolderName, string sampleDisplayName, PackageVersion version)
        {
            if (ProjectValidationUtility.SampleImportMeetsMinimumVersion(packageFolderName, sampleDisplayName, version) || !ProjectValidationUtility.HasSampleImported(packageFolderName, sampleDisplayName))
                return string.Empty;

            return $"An older version of {sampleDisplayName} has been found. This may cause errors.";
        }
    }
}
