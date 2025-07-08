using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    public static class ProjectPrefs
    {
        public const string FIRST_TIME_SETUP_DONE = "first-time-setup-guide";

        public static bool GetBool(string key)
        {
            return EditorPrefs.GetBool($"{Application.dataPath}{key}");
        }

        public static void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool($"{Application.dataPath}{key}", value);
        }
    }
}
