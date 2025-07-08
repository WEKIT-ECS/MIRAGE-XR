using System.IO;
using ReadyPlayerMe.Core.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ReadyPlayerMe.Samples.WebGLSample.Editor
{
    public class WebGLBuildPreprocessor : IPreprocessBuildWithReport
    {
        private const string TITLE_TEXT = "Build Warning";
        private const string IMPORT_BUTTON_TEXT = "Import and Build";
        private const string CONTINUE_BUTTON_TEXT = "Build without Template";
        private const string DONT_ASK_BUTTON_TEXT = "Don't ask again";
        private const string WARNING_TEXT =
            @"It looks like you are building for WebGL without the RpmWebGLTemplate. Would you like to import it now before building?";
        private const string DONT_ASK_AGAIN_PREF = "RPM_DONT_ASK_AGAIN_WEBGL_TEMPLATE_WARNING";
        
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_WEBGL
            if (ProjectPrefs.GetBool(DONT_ASK_AGAIN_PREF) || Application.isBatchMode || (WebGLPackageImporter.IsTemplateImported() &&
                WebGLPackageImporter.IsWebHelperImported()))
            {
                return;
            }
            ShowPopup();
#endif
        }

        public static void ShowPopup()
        {
            var buttonOption = EditorUtility.DisplayDialogComplex(TITLE_TEXT,
                WARNING_TEXT,
                IMPORT_BUTTON_TEXT,
                CONTINUE_BUTTON_TEXT,
                DONT_ASK_BUTTON_TEXT);

            switch (buttonOption)
            {
                case 0:
                    WebGLPackageImporter.ImportPackage();
                    AssetDatabase.Refresh();
                    WebGLPackageImporter.SetWebGLTemplate();
                    break;
                case 2:
                    ProjectPrefs.SetBool(DONT_ASK_AGAIN_PREF, true);
                    break;
                default:
                    break;
            }
        }
    }
}
