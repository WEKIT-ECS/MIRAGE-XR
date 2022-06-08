using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Editor
{
    public class AddressablesBuildPreprocessor : IPreprocessBuildWithReport
    {
        private const string ADDRESSABLES_ERROR_MESSAGE = "The contents of the player must be built before the project is built. This can be done from the Addressables window in the Build->Build Player Content menu command.";
        private const string ADDRESSABLES_DIALOG_TITLE = "Addressables";
        private const string ADDRESSABLES_DIALOG_MESSAGE = "The contents of the player must be built before the project is built. Build the project?";
        private const string ADDRESSABLES_SETTINGS_FILENAME = "settings.json";
        private const int DIALOG_SPAWN_TIME = 2;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!CheckAddressableContent())
            {
                ShowDialogAfter(DIALOG_SPAWN_TIME);
                throw new BuildFailedException(ADDRESSABLES_ERROR_MESSAGE);
            }
        }

        private static bool CheckAddressableContent()
        {
            var path = Path.Combine(Addressables.BuildPath, ADDRESSABLES_SETTINGS_FILENAME);
            return File.Exists(path);
        }

        private static async void ShowDialogAfter(int seconds)
        {
            await Task.Delay(seconds * 1000);
            ShowDialog();
        }

        private static void ShowDialog()
        {
            var result = EditorUtility.DisplayDialog(ADDRESSABLES_DIALOG_TITLE, ADDRESSABLES_DIALOG_MESSAGE, "Ok", "Cancel");
            if (result)
            {
                AddressableAssetSettings.BuildPlayerContent();
            }
        }
    }
}