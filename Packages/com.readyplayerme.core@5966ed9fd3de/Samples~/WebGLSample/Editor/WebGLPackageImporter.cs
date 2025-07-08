using System.IO;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Samples.WebGLSample.Editor
{
    public static class WebGLPackageImporter
    {
        private const string TAG = nameof(WebGLPackageImporter);
        private const string WINDOW_TITLE = "RPM WebGL Sample";
        private const string DESCRIPTION =
            "The RPM WebGL Sample includes a WebGL template and a WebGLHelper library that is used for WebGL builds. For these to work they need to be imported into specific folder locations. Would you like to import them automatically now?";
        private const string IMPORT_BUTTON_TEXT = "Import";
        private const string DONT_IMPORT_BUTTON_TEXT = "Don't import";
        private const string DONT_ASK_BUTTON_TEXT = "Don't ask again";
        private const string RPM_WEBGL_SCREEN_SHOWN_KEY = "rpm-webgl-screen-shown";
        private const string WEBGL_PACKAGE = "RpmWebGLPackage";
        private const string DONT_ASK_AGAIN_PREF = "rpm-webgl-package-importer";
        private const string TEMPLATE_FOLDER = "Assets/WebGLTemplates/RPMTemplate/TemplateData";
        private static string unityPackagePath;
        private const string RPM_WEB_HELPER = "RpmWebGLHelper";

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            if (CanShowWebGLScreen())
            {
                ShowWebGLPopup();
            }
        }

        private static bool CanShowWebGLScreen()
        {
            return !ProjectPrefs.GetBool(RPM_WEBGL_SCREEN_SHOWN_KEY) && !ProjectPrefs.GetBool(DONT_ASK_AGAIN_PREF) && !IsTemplateImported();
        }

        public static void ShowWebGLPopup()
        {
            ProjectPrefs.SetBool(RPM_WEBGL_SCREEN_SHOWN_KEY, true);
            var shouldUpdate = EditorUtility.DisplayDialogComplex(WINDOW_TITLE,
                DESCRIPTION,
                IMPORT_BUTTON_TEXT,
                DONT_IMPORT_BUTTON_TEXT,
                DONT_ASK_BUTTON_TEXT);

            switch (shouldUpdate)
            {
                case 0:
                    ImportPackage();
                    SetWebGLTemplate();
                    break;
                case 2:
                    ProjectPrefs.SetBool(DONT_ASK_AGAIN_PREF, true);
                    break;
                default:
                    break;
            }
        }

        public static void ImportPackage()
        {
            unityPackagePath = GetRelativeAssetPath();
            if (!string.IsNullOrEmpty(unityPackagePath))
            {
                AssetDatabase.ImportPackage(unityPackagePath, false);
            }
        }

        public static bool IsTemplateImported()
        {
            return AssetDatabase.IsValidFolder(TEMPLATE_FOLDER);
        }

        private static string GetRelativeAssetPath()
        {
            var webglPackageGuid = AssetDatabase.FindAssets(WEBGL_PACKAGE);
            if (webglPackageGuid == null || webglPackageGuid.Length < 1) return string.Empty;
            return AssetDatabase.GUIDToAssetPath(webglPackageGuid[0]);
        }

        public static void SetWebGLTemplate()
        {
            PlayerSettings.WebGL.template = "PROJECT:RPMTemplate";
            SDKLogger.Log(TAG, "Updated player settings to use RPMTemplate");
        }

        public static bool IsWebHelperImported()
        {
            var guids = AssetDatabase.FindAssets(RPM_WEB_HELPER);
            return guids != null && guids.Length > 0;
        }
    }
}
