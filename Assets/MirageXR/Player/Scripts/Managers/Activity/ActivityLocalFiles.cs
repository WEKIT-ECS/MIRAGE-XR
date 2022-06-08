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
            var fileName = string.Format(JSON_NAME_FORMAT, activity.id);
            var recFilePath = Path.Combine(Application.persistentDataPath, fileName);
            var json = ActivityParser.Serialize(activity);
            File.WriteAllText(recFilePath, json);
            RootObject.Instance.workplaceManager.SaveWorkplace();
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