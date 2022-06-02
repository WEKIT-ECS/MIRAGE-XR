/*
 * Copyright(c) 2017-2018 Sketchfab Inc.
 * License: https://github.com/sketchfab/UnityGLTF/blob/master/LICENSE
 */

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using System;

namespace Sketchfab
{
    public class SketchfabAPI
    {
        public const string authorizeEndpoint = "authorize/";
        public const string tokenEndpoint = "token/";
        public const string userInfoEndpoint = "userinfo/";
        public const string clientParam = "client_id";
        public const string redirectUriParam = "redirect_uri";
        public const string scopeParam = "scope";
        public const string stateParam = "state";

        private static string apiURL = "https://sketchfab.com/oauth2/";
        public static string ApiURL
        {
            get
            {
                return apiURL;
            }
            set
            {
                apiURL = value;
            }
        }

        public enum ExporterState
        {
            IDLE,
            CHECK_VERSION,
            REQUEST_CODE,
            // GET_CATEGORIES,
            USER_ACCOUNT_TYPE,
            CAN_PRIVATE,
            PUBLISH_MODEL
        }

        public class SketchfabPlan
        {
            public string label;
            public int maxSize;

            public SketchfabPlan(string lb, int ms)
            {
                label = lb;
                maxSize = ms;
            }
        }

        public SketchfabPlan GetPlan(string accountName)
        {
            switch (accountName)
            {
                case "pro":
                    int nbPro = 200 * 1024 * 1024;
                    return new SketchfabPlan("PRO", nbPro);
                case "prem":
                    int nbPrem = 500 * 1024 * 1024;
                    return new SketchfabPlan("PREMIUM", nbPrem);
                case "biz":
                    int nbBiz = 500 * 1024 * 1024;
                    return new SketchfabPlan("BUSINESS", nbBiz);
                case "ent":
                    int nbEnt = 500 * 1024 * 1024;
                    return new SketchfabPlan("ENTERPRISE", nbEnt);
            }

            int nbFree = 50 * 1024 * 1024;
            return new SketchfabPlan("BASIC", nbFree);
        }

        // Fields limits
        const int NAME_LIMIT = 48;
        const int DESC_LIMIT = 1024;
        const int TAGS_LIMIT = 50;
        const int PASSWORD_LIMIT = 64;
        const int SPACE_SIZE = 5;

        // Exporter objects and scripts
        public string _access_token = "";
        ExporterState _state;
        SketchfabRequest _publisher;
        public string _uploadSource = "Unity-exporter";

        // Dictionary<string, string> categories = new Dictionary<string, string>();
        // List<string> categoriesNames = new List<string>();
        private string _lastModelUrl;

        private bool _isUserPro;
        private string _userDisplayName;
        private SketchfabPlan _currentUserPlan = null;
        private bool _userCanPrivate = false;
        //int categoryIndex = 0;

        // Oauth stuff
        private float expiresIn = 0;
        private int lastTokenTime = 0;
        private string _latestVersion;
        private string _isLatestVersion;

        public delegate void Callback();
        public Callback _uploadSuccess;
        public Callback _uploadFailed;
        public Callback _tokenRequestSuccess;
        public Callback _tokenRequestFailed;
        public Callback _checkVersionSuccess;
        public Callback _checkVersionFailed;

        public Callback _checkUserAccountSuccess;
        public Callback _checkUserAccountFailure;

        private string _lastError = "";

        public SketchfabAPI(string uploadSource = "")
        {
            _publisher = new SketchfabRequest(uploadSource);
            _publisher.SetResponseCallback(HandleRequestResponse);
        }

        public bool IsUserAuthenticated()
        {
            return _access_token.Length > 0;
        }

        public bool IsLatestVersion()
        {
            return SketchfabPlugin.VERSION == _latestVersion;
        }

        // Setup callbacks
        public void SetUploadSuccessCb(Callback callback)
        {
            _uploadSuccess = callback;
        }

        public void SetUploadFailedCb(Callback callback)
        {
            _uploadFailed = callback;
        }

        public void SetTokenRequestSuccessCb(Callback callback)
        {
            _tokenRequestSuccess = callback;
        }

        public void SetTokenRequestFailedCb(Callback callback)
        {
            _tokenRequestFailed = callback;
        }

        public void SetCheckVersionSuccessCb(Callback callback)
        {
            _checkVersionSuccess = callback;
        }

        public void SetCheckVersionFailedCb(Callback callback)
        {
            _checkVersionFailed = callback;
        }

        public void SetCheckUserAccountSuccessCb(Callback callback)
        {
            _checkUserAccountSuccess = callback;
        }

        public void SetCheckUserAccountFailureCb(Callback callback)
        {
            _checkUserAccountFailure = callback;
        }

        public void LogoutUser()
        {
            _access_token = "";
            _publisher.SaveAccessToken(_access_token);
        }

        // public void AuthoriseUser(string client_id, string redirect_uri)
        // {
        // 	string url = System.String.Format(apiURL + authorizeEndpoint + "?client_id={0}&redirect_uri={1}&response_type={2}",
        // 		client_id, redirect_uri, "code");



        //}

        public void AuthenticateUser(string username, string password)
        {
            _state = ExporterState.REQUEST_CODE;
            _publisher.RequestAccessToken(username, password);
        }

        public void CheckLatestExporterVersion()
        {
            _state = ExporterState.CHECK_VERSION;
            _publisher.RequestExporterReleaseInfo();
        }

        public void RequestUserAccountInfo()
        {
            _state = ExporterState.USER_ACCOUNT_TYPE;
            _publisher.RequestAccountInfo();
        }

        public void RequestUserCanPrivate()
        {
            if (_currentUserPlan.label == "BASIC")
            {
                _userCanPrivate = false;
            }
            else
            {
                _state = ExporterState.CAN_PRIVATE;
                _publisher.RequestUserCanPrivate();
            }
        }

        public bool GetUserCanPrivate()
        {
            return _userCanPrivate;
        }

        public void PublishModel(Dictionary<string, string> parameters, string zipPath)
        {
            _state = ExporterState.PUBLISH_MODEL;
            _publisher.PostModel(parameters, zipPath);
        }

        int ConvertToSeconds(DateTime time)
        {
            return (int)(time.Hour * 3600 + time.Minute * 60 + time.Second);
        }

        public string GetLatestVersion()
        {
            return _latestVersion;
        }

        public string GetLastError()
        {
            return _lastError;
        }

        public SketchfabPlan GetUserPlan()
        {
            return _currentUserPlan;
        }

        public int GetCurrentUserMaxAllowedUploadSize()
        {
            if (_currentUserPlan == null)
                return -1;

            return _currentUserPlan.maxSize;
        }

        public string GetCurrentUserPlanLabel()
        {
            if (_currentUserPlan == null)
                return "Unknown";

            return _currentUserPlan.label;
        }

        public string GetCurrentUserDisplayName()
        {
            if (_userDisplayName == null)
                return "";

            return _userDisplayName;
        }

        // void relog()
        // {
        // 	if(publisher && publisher.getState() == ExporterState.REQUEST_CODE)
        // 	{
        // 		return;
        // 	}
        // 	if (user_name.Length == 0)
        // 	{
        // 		user_name = EditorPrefs.GetString(usernameEditorKey);
        // 		//user_password = EditorPrefs.GetString(passwordEditorKey);
        // 	}
           
        // 	if (publisher && user_name.Length > 0 && user_password.Length > 0)
        // 	{
        // 		publisher.oauth(user_name, user_password);
        // 	}
        // }

        private void CheckAccessTokenValidity()
        {
            float currentTimeSecond = ConvertToSeconds(DateTime.Now);
            if (_access_token.Length > 0 && currentTimeSecond - lastTokenTime > expiresIn)
            {
                _access_token = "";
                // relog();
            }
        }

        public void HandleRequestResponse()
        {
            WWW www = _publisher.GetResponse();

            if (www == null)
            {
                Debug.LogError("Request is empty (WWW object is null)");
                return;
            }

            JSONNode jsonResponse = ParseResponse(www);
            switch (_state)
            {
                case ExporterState.CHECK_VERSION:
                    if (jsonResponse != null && jsonResponse[0]["tag_name"] != null)
                    {
                        _latestVersion = jsonResponse[0]["tag_name"];
                        _checkVersionSuccess();
                    }
                    else
                    {
                        _latestVersion = "";
                        _checkVersionFailed();
                    }
                    break;
                case ExporterState.REQUEST_CODE:
                    if (jsonResponse["access_token"] != null)
                    {
                        _access_token = jsonResponse["access_token"];
                        expiresIn = jsonResponse["expires_in"].AsFloat;
                        lastTokenTime = ConvertToSeconds(DateTime.Now);
                        _publisher.SaveAccessToken(_access_token);
                        _tokenRequestSuccess?.Invoke();
                    }
                    else
                    {
                        _tokenRequestFailed?.Invoke();
                    }
                    break;
                case ExporterState.PUBLISH_MODEL:
                    if (www.responseHeaders["STATUS"].Contains("201") == true)
                    {
                        _lastModelUrl = SketchfabPlugin.Urls.modelUrl + "/" + GetUrlId(www.responseHeaders);
                        _uploadSuccess?.Invoke();
                    }
                    else
                    {
                        _lastError = www.responseHeaders["STATUS"];
                        _uploadFailed?.Invoke();
                    }
                    break;
                // case ExporterState.GET_CATEGORIES:
                // 	string jsonify = this.jsonify(www.text);
                // 	if (!jsonify.Contains("results"))
                // 	{
                // 		Debug.Log(jsonify);
                // 		Debug.Log("Failed to retrieve categories");
                // 		publisher.setIdle();
                // 		break;
                // 	}

                //	JSONArray categoriesArray = JSON.Parse(jsonify)["results"].AsArray;
                //	foreach (JSONNode node in categoriesArray)
                //	{
                //		categories.Add(node["name"], node["slug"]);
                //		categoriesNames.Add(node["name"]);
                //	}
                //	setIdle();
                //	break;
                case ExporterState.USER_ACCOUNT_TYPE:
                    string accountRequest = this.Jsonify(www.text);
                    if (!accountRequest.Contains("account"))
                    {
                        _lastError = "Failed to retrieve user account type";
                        _checkUserAccountFailure?.Invoke();
                        break;
                    }
                    else
                    {
                        var userSettings = JSON.Parse(accountRequest);
                        string account = userSettings["account"];
                        _currentUserPlan = GetPlan(account);
                        _userDisplayName = userSettings["displayName"];
                        _checkUserAccountSuccess?.Invoke();
                    }
                    break;
                case ExporterState.CAN_PRIVATE:
                    string canPrivateRequest = this.Jsonify(www.text);
                    if (!canPrivateRequest.Contains("canProtectModels"))
                    {
                        Debug.Log("Failed to retrieve if user can private");
                        SetIdle();
                        break;
                    }
                    _userCanPrivate = jsonResponse["canProtectModels"].AsBool;
                    break;
            }
        }
        private void SetIdle()
        {
            _state = ExporterState.IDLE;
        }

        private string GetUrlId(Dictionary<string, string> responseHeaders)
        {
            return responseHeaders["LOCATION"].Split('/')[responseHeaders["LOCATION"].Split('/').Length - 1];
        }

        public string GetModelUrl()
        {
            return _lastModelUrl;
        }

        private JSONNode ParseResponse(WWW www)
        {
            return JSON.Parse(this.Jsonify(www.text));
        }

        // Update is called once per frame
        public void Update()
        {
            // checkAccessTokenValidity();
            _publisher.Update();
        }

        public float GetUploadProgress()
        {
            if (_state == ExporterState.PUBLISH_MODEL && _publisher.GetResponse() != null)
            {
                return _publisher.GetUploadProgress();
            }
            else
            {
                return -1.0f; // No upload in progress
            }
        }

        private string Jsonify(string jsondata)
        {
            return jsondata.Replace("null", "\"null\"");
        }

        public string ValidateInputs(ref Dictionary<string, string> parameters)
        {
            string errors = "";

            if (parameters["name"].Length > NAME_LIMIT)
            {
                errors = "Model name is too long";
            }

            if (parameters["name"].Length == 0)
            {
                errors = "Please give a name to your model";
            }

            if (parameters["description"].Length > DESC_LIMIT)
            {
                errors = "Model description is too long";
            }


            if (parameters["tags"].Length > TAGS_LIMIT)
            {
                errors = "Model tags are too long";
            }

            return errors;
        }
    }
    class RequestManager
    {
        List<IEnumerator> _requests;
        IEnumerator _current = null;

        public RequestManager()
        {
            _requests = new List<IEnumerator>();
        }

        public void AddTask(IEnumerator task)
        {
            _requests.Add(task);
        }

        public void Clear()
        {
            _requests.Clear();
        }

        public bool Play()
        {
            if (_requests.Count > 0)
            {
                if (_current == null || !_current.MoveNext())
                {
                    _current = _requests[0];
                    _requests.RemoveAt(0);
                }
            }

            if (_current != null)
                _current.MoveNext();

            if (_current != null && !_current.MoveNext() && _requests.Count == 0)
                return false;

            return true;
        }
    }




    public class SketchfabRequest
    {
        bool _isDone = false;
        public WWW www;
        private string auth_code = "";
        private string access_token = "";
        private string uploadSource = "";
        public delegate void RequestResponseCallback();
        private RequestResponseCallback _callback;

        public SketchfabRequest(string source)
        {
            uploadSource = source;
        }

        public void SaveAccessToken(string token)
        {
            access_token = token;
        }

        public void SetResponseCallback(RequestResponseCallback responseCb)
        {
            _callback = responseCb;
        }

        public void Update()
        {
            if (!_isDone && www != null && www.isDone)
            {
                _isDone = true;
                _callback();
            }
        }


        // Request access_token
        public void RequestAccessToken(string user_name, string user_password)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "username", user_name },
                { "password", user_password }
            };
            // requestSketchfabAPI(SketchfabPlugin.Urls.oauth, parameters);
        }

        public WWW GetResponse()
        {
            return www;
        }

        public float GetUploadProgress()
        {
            if (www != null)
            {
                return 0.99f * www.uploadProgress + 0.01f * www.progress;
            }
            else
            {
                return -1.0f;
            }
        }

        public void RequestExporterReleaseInfo()
        {
            RequestSketchfabAPI(SketchfabPlugin.Urls.latestReleaseCheck);
        }

        public void RequestAccountInfo()
        {
            RequestSketchfabAPI(SketchfabPlugin.Urls.userMe);
        }

        public void RequestUserCanPrivate()
        {
            RequestSketchfabAPI(SketchfabPlugin.Urls.userAccount);
        }

        public void PostModel(Dictionary<string, string> parameters, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("Exported file not found. Aborting");
                return;
            }

            // byte[] data = File.ReadAllBytes(filePath);
            // requestSketchfabAPI(SketchfabPlugin.Urls.postModel, parameters, data, filePath);
        }

        public void RequestSketchfabAPI(string url)
        {
            _isDone = false;
            if (access_token.Length > 0)
            {
                WWWForm postForm = new WWWForm();
                Dictionary<string, string> headers = postForm.headers;
                if (access_token.Length > 0)
                    headers["Authorization"] = "Bearer " + access_token;


                Debug.Log("sending www");
                www = new WWW(url, null, headers);
                Debug.Log("finished www");
            }
            else
            {
                www = new WWW(url);
            }
        }

        public void RequestSketchfabAPI(string url, Dictionary<string, string> parameters)
        {
            _isDone = false;
            WWWForm postForm = new WWWForm();


            // Set parameters
            foreach (string param in parameters.Keys)
            {
                postForm.AddField(param, parameters[param]);
            }

            // Create and send request
            if (access_token.Length > 0)
            {
                Dictionary<string, string> headers = postForm.headers;
                if (access_token.Length > 0)
                    headers["Authorization"] = "Bearer " + access_token;

                www = new WWW(url, postForm.data, headers);
            }
            else
            {
                www = new WWW(url, postForm);
            }
        }

        public void RequestSketchfabAPI(string url, Dictionary<string, string> parameters, byte[] data, string fileName = "")
        {
            _isDone = false;
            WWWForm postForm = new WWWForm();
            // Set parameters
            foreach (string param in parameters.Keys)
            {
                postForm.AddField(param, parameters[param]);
            }

            // Add source
            postForm.AddField("source", uploadSource);

            // add data
            if (data.Length > 0)
            {
                postForm.AddBinaryData("modelFile", data, fileName, "application/zip");
            }

            Dictionary<string, string> headers = postForm.headers;
            headers["Authorization"] = "Bearer " + access_token;

            // Create and send request
            www = new WWW(url, postForm.data, headers);
        }
    }
}
#endif