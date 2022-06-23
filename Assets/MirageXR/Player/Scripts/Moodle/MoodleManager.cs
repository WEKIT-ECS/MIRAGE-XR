using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public class MoodleManager
    {
        private const long MAX_FILE_SIZE_FOR_MEMORY = 150 * 1024 * 1024; // 150 mb
        private static ActivityManager activityManager => RootObject.Instance.activityManager;

        private GameObject _progressText;   //TODO: remove ui logic 

        public string GetProgressText
        {
            get => _progressText.GetComponent<Text>().text;
            set => _progressText.GetComponent<Text>().text = value;
        }

        private void Update()    //TODO: remove ui logic 
        {
            if (!DBManager.LoggedIn && _progressText)
                _progressText.GetComponent<Text>().text = string.Empty;
        }

        public async Task<bool> Login(string username, string password)
        {
            var (result, response) = await Network.LoginRequestAsync(username, password, DBManager.domain);
            result = result && response.StartsWith("succeed") && response.Contains(",");
            if (result)
            {
                DBManager.token = response.Split(',')[1];
                DBManager.username = username;
                await GetUserId();
                await GetUserMail();
            }

            return result;
        }

        /// <summary>
        /// Zip files and send them to upload as a zip file
        /// </summary>
        public async Task<(bool, string)> UploadFile(string filepath, string recordingID, int updateFile) //TODO: split it to two methods based on 'updateFile' value
        {
            _progressText = Object.FindObjectOfType<ActionListMenu>().uploadProgressText;

            if (_progressText)
            {
                _progressText.GetComponent<Text>().text = "Compressing";
                _progressText.SetActive(true);
            }

            var file = await CompressRecord(filepath, activityManager.SessionId);
            if (_progressText) _progressText.GetComponent<Text>().text = "Uploading";
            return await StartUploading($"{recordingID}.zip", file, updateFile);
        }

        /// <summary>
        /// Upload The File
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="file"></param>
        /// <param updateMode="int"></param> 0 == check if sessionid exist , 1 = exist and update , 2 = exist and clone
        /// <returns></returns>
        private async Task<(bool, string)> StartUploading(string filename, byte[] file, int updateMode)
        {
            const string thumbnailName = "thumbnail.jpg";

            if (!DBManager.LoggedIn)
            {
                Debug.Log("You are not logged in");
                return (false, "Error: You are not logged in");
            }

            byte[] thumbnail = null;
            var thumbnailExist = File.Exists(Path.Combine(activityManager.ActivityPath, thumbnailName));
            if (thumbnailExist) thumbnail = File.ReadAllBytes(Path.Combine(activityManager.ActivityPath, thumbnailName));

            var activityJson = File.ReadAllText(activityManager.ActivityPath + "-activity.json");
            var workplaceJson = File.ReadAllText(activityManager.ActivityPath + "-workplace.json");

            var (result, response) = await Network.UploadRequestAsync(DBManager.token, DBManager.userid, activityManager.SessionId,
                DBManager.publicUploadPrivacy, filename, activityManager.Activity.name, file, thumbnailName, thumbnail, DBManager.domain, activityJson, workplaceJson, updateMode);

            if (result && !response.Contains("Error"))
            {
                if (response.EndsWith("Saved."))
                    Debug.Log(response);
                else
                    Debug.LogError(response);

                Maggie.Speak("The upload is completed.");
                if (_progressText) _progressText.GetComponent<Text>().text = "Done";
                return (true, response);
            }

            // The file handling response should be displayed as Log, not LogError
            if (response.Contains("File exist"))
                Debug.Log($"Error on uploading: {response}");
            else
            {
                Maggie.Speak("Uploading ARLEM failed. Check your system administrator.");
                Debug.LogError($"Error on uploading: {response}");
            }

            if (_progressText) _progressText.GetComponent<Text>().text = "Error!";

            var activityEditor = Object.FindObjectOfType<ActivityEditor>();
            // show update confirmation panel if file exist
            if (activityEditor && response == "Error: File exist, update")
            {
                Object.FindObjectOfType<ActivityEditor>().ShowUploadWarningPanel();
            }
            if (activityEditor && response == "Error: File exist, clone")
            {
                Object.FindObjectOfType<ActivityEditor>().ShowCloneWarningPanel();
            }

            return (false, response);
        }

        /// <summary>
        /// Ask moodle about the user id of the current logged user
        /// </summary>
        public async Task<string> GetUserId()
        {
            var requestValue = "userid";
            var function = "core_user_get_users";
            var parametersValueFormat = "criteria[0][key]=username&criteria[0][value]=" + DBManager.username;
            var (result, response) = await Network.GetCustomDataFromAPIRequestAsync(DBManager.token, DBManager.domain, requestValue, function, parametersValueFormat);
            if (!result)
            {
                Debug.LogError($"Can't get UserId, error: {response}");
                return null;
            }
            DBManager.userid = Regex.Replace(response, "[^0-9]+", string.Empty); // only numbers

            return DBManager.userid;
        }

        public async Task<string> GetUserMail()
        {
            var requestValue = "mail";
            var function = "core_user_get_users";
            var parametersValueFormat = "criteria[0][key]=username&criteria[0][value]=" + DBManager.username;
            var (result, response) = await Network.GetCustomDataFromAPIRequestAsync(DBManager.token, DBManager.domain, requestValue, function, parametersValueFormat);
            if (!result)
            {
                Debug.LogError($"Can't get Usermail, error: {response}");
                return null;
            }

            DBManager.usermail = response;
            return response;
        }

        /// <summary>
        /// Get the list of all arlems on Moodle and make the sessions
        /// </summary>
        /// <returns></returns>
        public async Task<List<Session>> GetArlemList()
        {
            const string httpsPrefix = "https://";
            const string httpPrefix = "http://";

            var serverUrl = DBManager.domain;
            var response = await GetArlemListJson(serverUrl);
            if (response == null && !DBManager.domain.StartsWith(httpsPrefix))
            {
                serverUrl = DBManager.domain.StartsWith(httpPrefix) ? serverUrl.Replace(httpPrefix, httpsPrefix) : httpsPrefix + serverUrl;
                response = await GetArlemListJson(serverUrl);
                if (response != null)
                {
                    DBManager.domain = serverUrl;
                }
            }

            Debug.Log(response);

            return ParseArlemListJson(response);
        }

        private static async Task<string> GetArlemListJson(string serverUrl)
        {
            const string responseValue = "arlemlist";

            var (result, response) = await Network.GetCustomDataFromDBRequestAsync(DBManager.userid, serverUrl, responseValue, DBManager.token);

            if (!result || response.StartsWith("Error"))
            {
                Debug.LogError($"Network error\nmessage: {response}");
                return null;
            }

            return response;
        }

        private static List<Session> ParseArlemListJson(string json)
        {
            const string emptyJson = "[]";

            try
            {
                var arlemList = new List<Session>();

                if (json == emptyJson)
                {
                    Debug.Log("Probably there is no public activity on the server.");
                    return arlemList;
                }

                var parsed = JObject.Parse(json);
                foreach (var pair in parsed)
                {
                    if (pair.Value != null)
                    {
                        var arlem = pair.Value.ToObject<Session>();
                        arlemList.Add(arlem);
                    }
                }

                return arlemList;
            }
            catch (Exception e)
            {
                Debug.LogError($"ParseArlemListJson error\nmessage: {e}");
                return null;
            }
        }

        public async Task<bool> DeleteArlem(string itemID, string sessionID)
        {
            var (result, response) = await Network.GetCustomDataFromDBRequestAsync(DBManager.userid, DBManager.domain, "deleteArlem", DBManager.token, itemID, sessionID);

            var value = result && !response.StartsWith("Error");
            if (value)
            {
                Debug.Log(sessionID + " is deleted from server");
            }
            else
            {
                Debug.LogError(response);
            }

            return value;
        }

        /// <summary>
        /// Increase the views of the activity on server by 1
        /// </summary>
        /// <param name="itemID"></param>
        public async Task UpdateViewsOfActivity(string itemID)
        {
            var (result, response) = await Network.GetCustomDataFromDBRequestAsync(DBManager.userid, DBManager.domain, "updateViews", DBManager.token, itemID);

            // Return null if some error happened
            if (!result || response.StartsWith("Error"))
            {
                Debug.LogError(response);
            }
            else
            {
                Debug.Log(" Views column of the activity is increased");
            }
        }

        /// <summary>
        /// Zip the worksshop files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recordingId"></param>
        /// <returns></returns>
        private static async Task<byte[]> CompressRecord(string path, string recordingId)
        {
            byte[] bytes = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipStream = new ZipOutputStream(stream))
                    {
                        if (Directory.Exists(path))
                            await ZipUtilities.CompressFolderAsync(path, zipStream);
                        await ZipUtilities.AddFileToZipStreamAsync(zipStream,
                            $"{path}-activity.json", $"{recordingId}-activity.json");
                        await ZipUtilities.AddFileToZipStreamAsync(zipStream,
                            $"{path}-workplace.json", $"{recordingId}-workplace.json");
                    }
                    bytes = stream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"compression error: {e}");
            }
            return bytes;
        }

        public static async Task<(bool, Activity)> DownloadActivity(Session session)
        {
            var isTooBigForMemory = session.filesize > MAX_FILE_SIZE_FOR_MEMORY;

            bool result;
            Activity activity = null;
            var tempFilePath = Path.Combine(Application.persistentDataPath, Guid.NewGuid().ToString());
            var stream = isTooBigForMemory ? File.OpenWrite(tempFilePath) : (Stream)new MemoryStream();
            try
            {
                string error;
                (result, error) = await Network.DownloadRequestToStreamAsync(stream, session.contextid, session.component, session.filearea, session.itemid, session.filename, DBManager.domain);
                if (result)
                {
                    if (isTooBigForMemory)
                    {
                        stream.Dispose();
                        stream = File.OpenRead(tempFilePath);
                    }
                    await ZipUtilities.ExtractZipFileAsync(stream, Application.persistentDataPath);
                    var fileName = LocalFiles.GetActivityJsonFilename(session.sessionid, true);
                    var newActivity = await LocalFiles.ReadActivityAsync(fileName);
                    if (newActivity != null) activity = newActivity;
                }
                else
                {
                    Debug.LogError(error);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                result = false;
            }
            finally
            {
                stream.Dispose();
                if (isTooBigForMemory && File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }

            return (result, activity);
        }
    }
}