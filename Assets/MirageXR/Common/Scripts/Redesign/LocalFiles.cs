using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public static class LocalFiles
    {
        private static readonly string[] _possibleSuffixes = { "-activity.json", "_activity.json", "activity.json" };

        public static async Task<List<Activity>> GetDownloadedActivities()
        {
            var result = new List<Activity>();
            var fileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles().ToList();
            for (var i = fileInfos.Count - 1; i >= 0; i--)
            {
                if (!IsActivityFile(fileInfos[i])) continue;

                try
                {
                    var filePath = Path.Combine(Application.persistentDataPath, fileInfos[i].Name);
                    var activity = await ReadActivityAsync(filePath);

                    if (activity == null)
                    {
                        Debug.LogWarning($"Could not parse {fileInfos[i].Name}");
                    }
                    else
                    {
                        result.Add(activity);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Exception occurred when reading {fileInfos[i].Name} : {e}");
                }
            }

            return result;
        }

        public static async Task<Activity> ReadActivityAsync(string filePath)
        {
            try
            {
                using (var stream = File.OpenText(filePath))
                {
                    var text = await stream.ReadToEndAsync();
                    return JsonUtility.FromJson<Activity>(text);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static bool TryDeleteActivity(string id)
        {
            const string activityFormat = "{0}-activity.json";
            const string workplaceFormat = "{0}-workplace.json";

            var path = Path.Combine(Application.persistentDataPath, id);
            if (Directory.Exists(path))
            {
                var directoryInfo = new DirectoryInfo(path);
                foreach (var file in directoryInfo.GetFiles()) file.Delete();
                foreach (var dir in directoryInfo.GetDirectories()) dir.Delete(true);
                Directory.Delete(path);
            }

            var activityFile = string.Format(activityFormat, path);
            if (File.Exists(string.Format(activityFile)))
                File.Delete(activityFile);

            var workplaceFile = string.Format(workplaceFormat, path);
            if (File.Exists(workplaceFile))
                File.Delete(workplaceFile);

            return true;
        }

        public static string GetActivityJsonFilename(string fileIdentifier, bool fullPath = false)
        {
            foreach (var suffixes in _possibleSuffixes)
            {
                var fileName = fileIdentifier + suffixes;
                var activityFileName = Path.Combine(Application.persistentDataPath, fileName);
                if (File.Exists(activityFileName)) return fullPath ? activityFileName : fileName;
            }

            return null;
        }

        public static void SaveLoginDetails(string key, string username, string password)
        {
            const string configFileName = "config.info";
            const char splitChar = '|';

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            var encryptedPassword = Encryption.EncriptDecrypt(password);
            var loginInfo = $"{key}{username}{splitChar}{encryptedPassword}";

            if (File.Exists(path))
            {
                var sb = new StringBuilder();
                var configFileInfo = File.ReadAllLines(path);
                foreach (var line in configFileInfo)
                {
                    if (!line.StartsWith(key)) sb.AppendLine(line);
                }

                sb.AppendLine(loginInfo);
                File.WriteAllText(path, sb.ToString());
            }
            else
            {
                File.WriteAllText(path, loginInfo);
            }
        }

        /// <summary>
        /// Generic function that retrieves login info from the locally generated file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool TryGetPassword(string key, out string username, out string password) // TODO: Replace with a json file
        {
            const string configFileName = "config.info";
            const char splitChar = '|';
            password = null;
            username = null;

            try
            {
                var path = Path.Combine(Application.persistentDataPath, configFileName);

                if (!File.Exists(path)) return false;

                var infos = File.ReadAllLines(path);
                var pword = (from info in infos where info.StartsWith(key) select info.Replace(key, string.Empty)).FirstOrDefault();

                if (string.IsNullOrEmpty(pword))
                {
                    return false;
                }

                var array = pword.Split(splitChar);

                if (array.Length < 2)
                {
                    return false;
                }

                username = array[0];
                password = Encryption.EncriptDecrypt(array[1]);
                return true;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return false;
            }
        }

        public static void RemoveKey(string key)
        {
            const string configFileName = "config.info";

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path)) return;

            var sb = new StringBuilder();
            var configFileInfo = File.ReadAllLines(path);
            foreach (var line in configFileInfo)
            {
                if (!line.StartsWith(key)) sb.AppendLine(line);
            }

            File.WriteAllText(path, sb.ToString());
        }

        public static bool TryToGetUsernameAndPassword(out string username, out string password)
        {
            const string moodleKey = "moodle";
            const string configFileName = "config.info";
            const char splitChar = '|';

            username = null;
            password = null;
            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path)) return false;

            var infos = File.ReadAllLines(path);
            var userPass = (from info in infos where info.StartsWith(moodleKey) select info.Replace(moodleKey, string.Empty)).FirstOrDefault();

            if (string.IsNullOrEmpty(userPass)) return false;

            var array = userPass.Split(splitChar);

            if (array.Length < 2) return false;

            username = array[0];
            password = Encryption.EncriptDecrypt(array[1]);
            return true;
        }

        public static void SaveUsernameAndPassword(string username, string password)
        {
            const string moodleKey = "moodle";
            const string configFileName = "config.info";
            const char splitChar = '|';

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            var encryptedPassword = Encryption.EncriptDecrypt(password);
            var loginInfo = $"{moodleKey}{username}{splitChar}{encryptedPassword}";

            if (File.Exists(path))
            {
                var sb = new StringBuilder();
                var configFileInfo = File.ReadAllLines(path);
                foreach (var line in configFileInfo)
                {
                    if (!line.StartsWith(moodleKey)) sb.AppendLine(line);
                }

                sb.AppendLine(loginInfo);
                File.WriteAllText(path, sb.ToString());
            }
            else
            {
                File.WriteAllText(path, loginInfo);
            }
        }

        public static void RemoveUsernameAndPassword()
        {
            const string moodleKey = "moodle";
            const string configFileName = "config.info";

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path)) return;

            var sb = new StringBuilder();
            var configFileInfo = File.ReadAllLines(path);
            foreach (var line in configFileInfo)
            {
                if (!line.StartsWith(moodleKey)) sb.AppendLine(line);
            }

            File.WriteAllText(path, sb.ToString());
        }

        private static bool IsActivityFile(FileSystemInfo file)
        {
            return _possibleSuffixes.Any(t => file.Name.ToLower().EndsWith(t));
        }
    }
}