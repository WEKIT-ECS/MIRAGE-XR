using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Callbacks;

namespace MirageXR
{
    public class IconBuildPostprocessor
    {
        
        /// <summary>
        /// Finds the three images that make up the vision os icon and correctly sets up the visionOS icon for the xcode project
        /// </summary>
        #if UNITY_VISIONOS || VISION_OS
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string buildPath) {
            if (target != BuildTarget.VisionOS) {return;}
            const string iconDir = "Assets/MirageXR/Logo/VisionOS";

            var iconGuidFront = AssetDatabase.FindAssets("AppIcon-1024Front@2x", new[] { iconDir })[0];
            var iconGuidMiddle = AssetDatabase.FindAssets("AppIcon-1024Middle@2x", new[] { iconDir })[0];
            var iconGuidBack = AssetDatabase.FindAssets("AppIcon-1024Back@2x", new[] { iconDir })[0];

            VisionOSIconCreation.SetupVisionOSIcon(
                buildPath: buildPath,
                version: 1,
                author: "Xcode", 
                Tuple.Create("Front", iconGuidFront),
                Tuple.Create("Middle", iconGuidMiddle),
                Tuple.Create("Back", iconGuidBack)
            );
        }
        #endif
    }
}