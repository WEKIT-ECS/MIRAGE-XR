using i5.Toolkit.Core.VerboseLogging;
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
            AppLog.LogTrace($"Getting downloaded activities");
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
                        AppLog.LogWarning($"Could not parse {fileInfos[i].Name}");
                    }
                    else
                    {
                        result.Add(activity);
                    }
                }
                catch (Exception e)
                {
                    AppLog.LogError($"Exception occurred when reading {fileInfos[i].Name} : {e}");
                }
            }

            AppLog.LogDebug($"Found {result.Count} stored and parseable activities in local storage");
            return result;
        }

        public static async Task<Activity> ReadActivityAsync(string filePath)
        {
            AppLog.LogTrace($"Reading activity at {filePath}");
            try
            {
                using (var stream = File.OpenText(filePath))
                {
                    var text = await stream.ReadToEndAsync();
                    Activity result = JsonUtility.FromJson<Activity>(text);
                    if (result == null)
                    {
                        AppLog.LogWarning($"Could not parse activity {filePath}");
                    }
                    else
                    {
                        AppLog.LogInfo($"Successfully read and parsed {filePath}");
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                AppLog.LogError($"Error while reading activity {filePath}: {e}");
                return null;
            }
        }

        public static bool TryDeleteActivity(string id)
        {
            AppLog.LogTrace($"Deleting activity with id {id}...");
            const string activityFormat = "{0}-activity.json";
            const string workplaceFormat = "{0}-workplace.json";

            var path = Path.Combine(Application.persistentDataPath, id);
            if (Directory.Exists(path))
            {
                var directoryInfo = new DirectoryInfo(path);
                foreach (var file in directoryInfo.GetFiles()) file.Delete();
                foreach (var dir in directoryInfo.GetDirectories()) dir.Delete(true);
                Directory.Delete(path);
                AppLog.LogTrace($"Deleted directory {directoryInfo.Name}");
            }

            var activityFile = string.Format(activityFormat, path);
            if (File.Exists(string.Format(activityFile)))
            {
                File.Delete(activityFile);
                AppLog.LogTrace($"Deleted activity file at {activityFile}");
            }

            var workplaceFile = string.Format(workplaceFormat, path);
            if (File.Exists(workplaceFile))
            {
                File.Delete(workplaceFile);
                AppLog.LogTrace($"Deleted workplace file at {workplaceFile}");
            }

            AppLog.LogInfo($"Deleted activity with id {id} from local storage");
            return true;
        }

        public static string GetActivityJsonFilename(string fileIdentifier, bool fullPath = false)
        {
            foreach (var suffixes in _possibleSuffixes)
            {
                var fileName = fileIdentifier + suffixes;
                var activityFileName = Path.Combine(Application.persistentDataPath, fileName);
                if (File.Exists(activityFileName))
                {
                    AppLog.LogDebug($"Identified activity json filename based on {fileIdentifier} as {activityFileName}");
                    return fullPath ? activityFileName : fileName;
                }
            }

            AppLog.LogWarning($"Could not identify json filename for activity based on {fileIdentifier}");
            return null;
        }

        public static void SaveLoginDetails(string key, string username, string password)
        {
            AppLog.LogTrace("Saving the login details...");
            const string configFileName = "config.info";
            const char splitChar = '|';

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            var encryptedPassword = Encryption.EncriptDecrypt(password);
            var loginInfo = $"{key}{username}{splitChar}{encryptedPassword}";

            try
            {
                if (File.Exists(path))
                {
                    AppLog.LogTrace($"Appending login details to existing config file at {path}");
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
                    AppLog.LogTrace($"Writing login details to fresh config file at {path} as it does not exist yet");
                    File.WriteAllText(path, loginInfo);
                }
                AppLog.LogInfo("Saved login details");
            }
            catch (IOException e)
            {
                AppLog.LogError($"Error while saving login details: {e}");
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
            AppLog.LogTrace("Trying to get password from config file");
            const string configFileName = "config.info";
            const char splitChar = '|';
            password = null;
            username = null;

            try
            {
                var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path))
            {
                AppLog.LogTrace($"Could not retrieve password from config file since there is no config file at {path}");
                return false;
            }

                var infos = File.ReadAllLines(path);
                var pword = (from info in infos where info.StartsWith(key) select info.Replace(key, string.Empty)).FirstOrDefault();

            if (string.IsNullOrEmpty(pword))
            {
                AppLog.LogTrace("Could not retrieve password from config file since it is not stored in the file");
                return false;
            }

                var array = pword.Split(splitChar);

            if (array.Length < 2)
            {
                AppLog.LogWarning("Could not retrieve password since the stored encrypted value seems to be malformed");
                return false;
            }

                username = array[0];
                password = Encryption.EncriptDecrypt(array[1]);
                AppLog.LogDebug("Successfully retrieved stored password from config file");
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
            AppLog.LogTrace($"Removing the stored key {key} from the config file");
            const string configFileName = "config.info";

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path))
            {
                AppLog.LogTrace($"Config file does not exist at {path}, so there is no need to remove the key");
                return;
            }

            var sb = new StringBuilder();
            var configFileInfo = File.ReadAllLines(path);
            foreach (var line in configFileInfo)
            {
                if (!line.StartsWith(key)) sb.AppendLine(line);
            }

            File.WriteAllText(path, sb.ToString());
            AppLog.LogDebug($"Removed key {key} from config file");
        }

        public static bool TryToGetUsernameAndPassword(out string username, out string password)
        {
            AppLog.LogTrace("Trying to get username and password from config file");
            const string moodleKey = "moodle";
            const string configFileName = "config.info";
            const char splitChar = '|';

            username = null;
            password = null;
            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path))
            {
                AppLog.LogTrace($"Could not get username and password from config file since the config file does not exist at {path}");
                return false;
            }

            var infos = File.ReadAllLines(path);
            var userPass = (from info in infos where info.StartsWith(moodleKey) select info.Replace(moodleKey, string.Empty)).FirstOrDefault();

            if (string.IsNullOrEmpty(userPass))
            {
                AppLog.LogTrace("Could not get username and password from config file since they are not stored in the file");
                return false;
            }

            var array = userPass.Split(splitChar);

            if (array.Length < 2)
            {
                AppLog.LogWarning("Could not get username and password from config file since the stored value seems to be malformed");
                return false;
            }

            username = array[0];
            password = Encryption.EncriptDecrypt(array[1]);
            AppLog.LogInfo("Successfully retrieved stored username and password from config file");
            return true;
        }

        public static void SaveUsernameAndPassword(string username, string password)
        {
            AppLog.LogTrace("Saving the username and password in the config file...");
            const string moodleKey = "moodle";
            const string configFileName = "config.info";
            const char splitChar = '|';

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            var encryptedPassword = Encryption.EncriptDecrypt(password);
            var loginInfo = $"{moodleKey}{username}{splitChar}{encryptedPassword}";

            try
            {
                if (File.Exists(path))
                {
                    AppLog.LogTrace("Appending key-value pairs to existing config file...");
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
                    AppLog.LogTrace("Writing key-value pairs to fresh config file as it did not yet exist");
                    File.WriteAllText(path, loginInfo);
                }
                AppLog.LogInfo("Successfully saved username and password to config file");
            }
            catch (IOException e)
            {
                AppLog.LogError($"An error occurred while saving the username and password to the config file: {e}");
            }
        }

        public static void RemoveUsernameAndPassword()
        {
            AppLog.LogTrace("Removing username and password from config file...");
            const string moodleKey = "moodle";
            const string configFileName = "config.info";

            var path = Path.Combine(Application.persistentDataPath, configFileName);

            if (!File.Exists(path))
            {
                AppLog.LogTrace($"No need to remove username and password as the config file does not exist at {path}");
                return;
            }

            try
            {
                var sb = new StringBuilder();
                var configFileInfo = File.ReadAllLines(path);
                foreach (var line in configFileInfo)
                {
                    if (!line.StartsWith(moodleKey)) sb.AppendLine(line);
                }

                File.WriteAllText(path, sb.ToString());
                AppLog.LogInfo("Successfully deleted username and password from config file");
            }
            catch (Exception e)
            {
                AppLog.LogError($"An error occurred while deleting the username and password: {e}");
            }
        }

        private static bool IsActivityFile(FileSystemInfo file)
        {
            return _possibleSuffixes.Any(t => file.Name.ToLower().EndsWith(t));
        }
    }
}