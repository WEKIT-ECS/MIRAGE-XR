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
    /// <summary>
    /// Manager for centralized access to the Moodle API
    /// </summary>
    public class MoodleManager
    {
        private const long MAX_FILE_SIZE_FOR_MEMORY = 150 * 1024 * 1024; // 150 mb
        private static ActivityManager activityManager => RootObject.Instance.activityManager;

        private GameObject _progressText;   //TODO: remove ui logic

        /// <summary>
        /// Gets or sets the text that is currently displayed in the progress text label of the UI
        /// </summary>
        public string GetProgressText
        {
            get => _progressText.GetComponent<Text>().text;
            set => _progressText.GetComponent<Text>().text = value;
        }

        private void Update()    //TODO: remove ui logic
        {
            if (!DBManager.LoggedIn && _progressText)
            {
                _progressText.GetComponent<Text>().text = string.Empty;
            }
        }

        /// <summary>
        /// Logs in a user with the given credentials
        /// </summary>
        /// <param name="username">The username of the user which should be logged in</param>
        /// <param name="password">The password of the user which should be logged in</param>
        /// <returns>Returns true if the login was successful, otherwise false</returns>
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

        // Uploads a file to Moodle
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
            if (thumbnailExist)
            {
                thumbnail = File.ReadAllBytes(Path.Combine(activityManager.ActivityPath, thumbnailName));
            }

            var activityJson = File.ReadAllText(activityManager.ActivityPath + "-activity.json");
            var workplaceJson = File.ReadAllText(activityManager.ActivityPath + "-workplace.json");

            var (result, response) = await Network.UploadRequestAsync(DBManager.token, DBManager.userid, activityManager.SessionId,
                DBManager.publicUploadPrivacy, filename, activityManager.Activity.name, file, thumbnailName, thumbnail, DBManager.domain, activityJson, workplaceJson, updateMode);

            if (result && !response.Contains("Error"))
            {
                if (response.EndsWith("Saved."))
                {
                    Debug.Log(response);
                }
                else
                {
                    Debug.LogError(response);
                }

                Maggie.Speak("The upload is completed.");
                if (_progressText)
                {
                    _progressText.GetComponent<Text>().text = "Done";
                }

                return (true, response);
            }

            // The file handling response should be displayed as Log, not LogError
            if (response.Contains("File exist"))
            {
                Debug.Log($"Error on uploading: {response}");
            }
            else
            {
                Maggie.Speak("Uploading ARLEM failed. Check your system administrator.");
                Debug.LogError($"Error on uploading: {response}");
            }

            if (_progressText)
            {
                _progressText.GetComponent<Text>().text = "Error!";
            }

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
        /// Requests the user id of the currently logged-in user from Moodle
        /// </summary>
        /// <returns>Returns the user id of the currently logged-in user</returns>
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

        /// <summary>
        /// Requests the mail address of the currently logged-in user from Moodle
        /// </summary>
        /// <returns>Returns the mail address of the currently logged-in user</returns>
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
        /// <returns>Return the list of all ARLEM sessions that are stored on Moodle</returns>
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

        // returns a json string which contains all stored ARLEM sessions on the given Moodle server
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

        // parses the ARLEM list that is given as a JSON string and converts it to a list of sessions
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

        /// <summary>
        /// Deletes the given ARLEM session from the connected Moodle instance
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="sessionID"></param>
        /// <returns>Returns true if the delete session worked; otherwise false</returns>
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
        /// <param name="itemID">The id of the item for which the activity views should be increased</param>
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

        // Zips the workshop files
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
                        {
                            await ZipUtilities.CompressFolderAsync(path, zipStream);
                        }

                        await ZipUtilities.AddFileToZipStreamAsync(
                            zipStream,
                            $"{path}-activity.json",
                            $"{recordingId}-activity.json");
                        await ZipUtilities.AddFileToZipStreamAsync(
                            zipStream,
                            $"{path}-workplace.json",
                            $"{recordingId}-workplace.json");
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

        /// <summary>
        /// Download an ARLEM activity based on a given session
        /// </summary>
        /// <param name="session">The session data object that identifies the activity</param>
        /// <returns>Returns a tuple:
        /// The Bool is true if the download succeeded;
        /// In the second item, the downloaded activity can be found</returns>
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
                    if (newActivity != null)
                    {
                        activity = newActivity;
                    }
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
                if (isTooBigForMemory && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return (result, activity);
        }
    }
}