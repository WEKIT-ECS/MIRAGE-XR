using System;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    public static class ActivityLocalFiles
    {
        private const string JSON_NAME_FORMAT = "{0}-activity.json";

        public static void CleanUp(Activity activity)
        {
            var newDirectoryPath = Path.Combine(Application.persistentDataPath, activity.id);
            if (Directory.Exists(newDirectoryPath))
            {
                Directory.Delete(newDirectoryPath, true);
            }
        }

        public static void SaveData(Activity activity)
        {
            try
            {
                var fileName = string.Format(JSON_NAME_FORMAT, activity.id);
                var recFilePath = Path.Combine(Application.persistentDataPath, fileName);
                var json = ActivityParser.Serialize(activity);
                File.WriteAllText(recFilePath, json);
                EventManager.ActivitySaved();
                RootObject.Instance.WorkplaceManager.SaveWorkplace();
            }
            catch (Exception e)
            {
                Debug.LogError("Error when saving activity: " + e);
            }
        }

        public static void MoveData(string oldActivityId, string activityId)
        {
            if (string.IsNullOrEmpty(oldActivityId) || string.IsNullOrEmpty(activityId))
            {
                throw new ArgumentException();
            }

            var oldPath = Path.Combine(Application.persistentDataPath, oldActivityId);
            var path = Path.Combine(Application.persistentDataPath, activityId);

            if (!Directory.Exists(oldPath))
            {
                Directory.CreateDirectory(oldPath);
            }

            Utilities.CopyEntireFolder(oldPath, path);
        }
    }
}