/*
 * Copyright(c) 2017-2018 Sketchfab Inc.
 * License: https://github.com/sketchfab/UnityGLTF/blob/master/LICENSE
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor.SceneManagement;
//using UnityGLTF;

public class SketchfabExporter : EditorWindow
{

    [MenuItem("Sketchfab/Publish to Sketchfab")]
    static void Init()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
		SketchfabExporter window = (SketchfabExporter)EditorWindow.GetWindow(typeof(SketchfabExporter));
		window.titleContent.text = "Sketchfab";
		window.Show();
#else // and error dialog if not standalone
        EditorUtility.DisplayDialog("Error", "Your build target must be set to standalone", "Okay");
#endif
    }

    // UI dimensions (to be cleaned)
    [SerializeField]
    Vector2 loginSize = new Vector2(603, 190);
    [SerializeField]
    Vector2 fullSize = new Vector2(603, 710);
    [SerializeField]
    Vector2 descSize = new Vector2(603, 175);

    Sketchfab.SketchfabAPI _api;
    private string exportPath;
    private string zipPath;

    // Login
    private string user_name = "";
    private string user_password = "";
    const string usernameEditorKey = "UnityExporter_username";

    // Upload params and options
    private bool opt_exportAnimation = true;
    private bool opt_exportSelection = false;
    private string param_name = "";
    private string param_description = "";
    private string param_tags = "";
    private bool param_autopublish = true;
    private bool param_private = false;
    private string param_password = "";

    // Exporter UI: dynamic elements
    private string status = "";
    Rect windowRect;

    //private List<String> tagList;
    void Awake()
    {
        zipPath = Application.temporaryCachePath + "/" + "Unity2Skfb.zip";
        exportPath = Application.temporaryCachePath + "/" + "Unity2Skfb.gltf";
        ResizeWindow(loginSize);
    }

    void SetupAPI()
    {
        _api = new Sketchfab.SketchfabAPI("Unity-exporter");

        //Setup callbacks
        _api.SetCheckVersionSuccessCb(OnCheckVersionSuccess);
        _api.SetCheckVersionFailedCb(OnCheckVersionFailure);
        _api.SetTokenRequestFailedCb(OnAuthenticationFail);
        _api.SetTokenRequestSuccessCb(OnAuthenticationSuccess);
        _api.SetCheckUserAccountSuccessCb(OnCheckUserAccountSuccess);
        _api.SetUploadSuccessCb(OnUploadSuccess);
        _api.SetUploadFailedCb(OnUploadFailed);

        _api.CheckLatestExporterVersion();
    }

    void OnEnable()
    {
        // Pre-fill model name with scene name if empty
        if (param_name.Length == 0)
        {
            param_name = EditorSceneManager.GetActiveScene().name;
        }

        SketchfabPlugin.Initialize();
        SetupAPI();

        ResizeWindow(loginSize);
        Relog();
    }

    int ConvertToSeconds(DateTime time)
    {
        return (int)(time.Hour * 3600 + time.Minute * 60 + time.Second);
    }

    void OnUploadSuccess()
    {
        Application.OpenURL(_api.GetModelUrl());
    }

    void OnUploadFailed()
    {
        EditorUtility.DisplayDialog("Upload Error", "An error occured when uploading the model:\n" + _api.GetLastError(), "Ok");
    }

    void OnCheckVersionSuccess()
    {
        if (!_api.IsLatestVersion())
        {
            SketchfabPlugin.DisplayVersionPopup();
        }
    }

    void OnCheckVersionFailure()
    {
        Debug.Log("Failed to retrieve Plugin version");
    }

    /*
	void OnSelectionChange()
	{
		// do nothing for now
	}
	*/

    void OnAuthenticationFail()
    {
        EditorUtility.DisplayDialog("Error", "Authentication failed: invalid email and/or password \n" + _api.GetLastError(), "Ok");
    }

    void OnAuthenticationSuccess()
    {
        _api.RequestUserAccountInfo();
        ResizeWindow(fullSize);
    }

    void OnCheckUserAccountSuccess()
    {
        _api.RequestUserCanPrivate();
    }

    void ResizeWindow(Vector2 size)
    {
        //this.maxSize = size;
        this.minSize = size;
    }

    void Relog()
    {
        if (user_name.Length == 0)
        {
            user_name = EditorPrefs.GetString(usernameEditorKey);
            //user_password = EditorPrefs.GetString(passwordEditorKey);
        }

        if (user_name.Length > 0 && user_password.Length > 0)
        {
            _api.AuthenticateUser(user_name, user_password);
        }
    }

    void ExpandWindow(bool expand)
    {
        windowRect = this.position;
        windowRect.height = expand ? fullSize.y : loginSize.y;
        position = windowRect;
    }

    private bool UpdateExporterStatus()
    {
        status = "";

        if (param_name.Length > SketchfabPlugin.NAME_LIMIT)
        {
            status = "Model name is too long";
            return false;
        }

        if (param_name.Length == 0)
        {
            status = "Please give a name to your model";
            return false;
        }


        if (param_description.Length > SketchfabPlugin.DESC_LIMIT)
        {
            status = "Model description is too long";
            return false;
        }


        if (param_tags.Length > SketchfabPlugin.TAGS_LIMIT)
        {
            status = "Model tags are too long";
            return false;
        }

        if (opt_exportSelection)
        {
            if (Selection.GetTransforms(SelectionMode.Deep).Length == 0)
            {
                status = "No object selected to export";
                return false;
            }
            else
            {
                status = "Upload selection to Sketchfab";
            }
        }
        else
        {
            status = "Upload scene to Sketchfab";
        }

        return true;
    }

    private void CheckValidity()
    {
        SketchfabPlugin.CheckValidity((int)descSize.x, (int)descSize.y);

        if (_api == null)
        {
            SetupAPI();
        }
    }

    private void Update()
    {
        if (_api != null)
        {
            _api.Update();
        }
    }

    void OnGUI()
    {
        CheckValidity();
        SketchfabPlugin.ShowHeader();

        // Account settings
        if (!_api.IsUserAuthenticated())
        {
            ShowLoginUi();
        }
        else
        {
            DisplayVersionInfo();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("Account: <b>" + _api.GetCurrentUserDisplayName() + (_api.GetCurrentUserPlanLabel().Length > 0 ? "</b> (" + _api.GetCurrentUserPlanLabel() + " account)" : ""), SketchfabPlugin.SkfbLabel);
            if (GUILayout.Button("Logout"))
            {
                _api.LogoutUser();
                ResizeWindow(loginSize);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(SketchfabPlugin.SPACE_SIZE);

            ShowModelProperties();
            GUILayout.Space(SketchfabPlugin.SPACE_SIZE);
            ShowPrivateSetting();
            ShowOptions();

            bool enable = UpdateExporterStatus();
            if (enable)
                GUI.color = SketchfabPlugin.BLUE_COLOR;
            else
                GUI.color = SketchfabPlugin.GREY_COLOR;

            if (_api.GetUploadProgress() >= 0.0f && _api.GetUploadProgress() < 1.0f)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, _api.GetUploadProgress(), "Upload progress");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUI.enabled = enable;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(status, GUILayout.Width(250), GUILayout.Height(40)))
                {
                    if (!enable)
                    {
                        EditorUtility.DisplayDialog("Error", status, "Ok");
                    }
                    else
                    {
                        //proceedToExportAndUpload();
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }

    public void DisplayVersionInfo()
    {
        if (_api.GetLatestVersion() == null)
        {
            SketchfabPlugin.ShowVersionChecking();
        }
        else if (_api.GetLatestVersion().Length == 0)
        {
            SketchfabPlugin.ShowVersionCheckError();
        }
        else if (_api.IsLatestVersion())
        {
            SketchfabPlugin.ShowUpToDate(_api.GetLatestVersion());
        }
        else
        {
            SketchfabPlugin.ShowOutdatedVersionWarning(_api.GetLatestVersion());
        }
    }

    private void ShowModelProperties()
    {
        // Model settings
        GUILayout.Label("Model properties", EditorStyles.boldLabel);

        // Model name
        GUILayout.Label("Name");
        param_name = EditorGUILayout.TextField(param_name);
        GUILayout.Label("(" + param_name.Length + "/" + SketchfabPlugin.NAME_LIMIT + ")", EditorStyles.centeredGreyMiniLabel);
        EditorStyles.textField.wordWrap = true;
        GUILayout.Space(SketchfabPlugin.SPACE_SIZE);

        GUILayout.Label("Description");
        param_description = EditorGUILayout.TextArea(param_description, SketchfabPlugin.SkfbTextArea);
        GUILayout.Label("(" + param_description.Length + " / 1024)", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(SketchfabPlugin.SPACE_SIZE);
        GUILayout.Label("Tags (separated by spaces)");
        param_tags = EditorGUILayout.TextField(param_tags);
        GUILayout.Label("'unity' and 'unity3D' added automatically (" + param_tags.Length + "/50)", EditorStyles.centeredGreyMiniLabel);
    }

    private void ShowPrivateSetting()
    {
        GUILayout.Label("Set the model to Private", EditorStyles.centeredGreyMiniLabel);
        if (_api.GetUserCanPrivate())
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            param_private = EditorGUILayout.Toggle("Private model", param_private);

            if (GUILayout.Button("( " + SketchfabPlugin.ClickableTextColor("more info") + ")", SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
            {
                Application.OpenURL(SketchfabPlugin.Urls.privateInfo);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.enabled = param_private;
            GUILayout.Label("Password");
            param_password = EditorGUILayout.TextField(param_password);
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }
        else
        {
            if (_api.GetCurrentUserPlanLabel() == "BASIC")
            {
                if (GUILayout.Button("(" + SketchfabPlugin.ClickableTextColor("Upgrade to a paid account to set your model to private") + ")", SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
                {
                    Application.OpenURL(SketchfabPlugin.Urls.plans);
                }
            }
            else
            {
                if (GUILayout.Button("(" + SketchfabPlugin.ClickableTextColor("You cannot set any other model to private (limit reached)") + ")", SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
                {
                    Application.OpenURL(SketchfabPlugin.Urls.plans);
                }
            }
        }
    }

    private void ShowOptions()
    {
        GUILayout.Label("Options", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        opt_exportAnimation = EditorGUILayout.Toggle("Export animation (beta)", opt_exportAnimation);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        opt_exportSelection = EditorGUILayout.Toggle("Export selection", opt_exportSelection);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        param_autopublish = EditorGUILayout.Toggle("Publish immediately ", param_autopublish);
        if (GUILayout.Button("(" + SketchfabPlugin.ClickableTextColor("more info") + ")", SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
        {
            Application.OpenURL(SketchfabPlugin.Urls.latestRelease);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    /*
	private void proceedToExportAndUpload()
	{
		if (System.IO.File.Exists(zipPath))
		{
			System.IO.File.Delete(zipPath);
		}

		// "Sketchfab Plugin (Unity " + Application.unityVersion + ")"
		var exporter = new GLTFSceneExporter(opt_exportSelection ? GLTFUtils.getSelectedTransforms() : GLTFUtils.getSceneTransforms());
		exporter.enableAnimation(opt_exportAnimation);
		exporter.SaveGLTFandBin(Path.GetDirectoryName(exportPath), Path.GetFileNameWithoutExtension(exportPath));

		GLTFUtils.buildZip(exporter.getExportedFilesList(), Path.Combine(Path.GetDirectoryName(exportPath), "Unity2Skfb.zip"), true);
		if (File.Exists(zipPath))
		{
			bool shouldUpload = checkFileSize(zipPath);

			if (!shouldUpload)
			{
				shouldUpload = EditorUtility.DisplayDialog("Error", "The export exceed the max file size allowed by your current account type", "Continue", "Cancel");
			}
			_api.publishModel(buildParameterDict(), zipPath);
		}
		else
		{
			Debug.Log("Zip file has not been generated. Aborting publish.");
		}
	}
	*/

    private void ShowLoginUi()
    {
        GUILayout.Label("Log in with your Sketchfab account", EditorStyles.centeredGreyMiniLabel);

        user_name = EditorGUILayout.TextField("Email", user_name);
        user_password = EditorGUILayout.PasswordField("Password", user_password);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(SketchfabPlugin.ClickableTextColor("Create an account"), SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
        {
            Application.OpenURL(SketchfabPlugin.Urls.createAccount);
        }
        if (GUILayout.Button(SketchfabPlugin.ClickableTextColor("Reset your password"), SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
        {
            Application.OpenURL(SketchfabPlugin.Urls.resetPassword);
        }
        if (GUILayout.Button(SketchfabPlugin.ClickableTextColor("Report an issue"), SketchfabPlugin.SkfbClickableLabel, GUILayout.Height(20)))
        {
            Application.OpenURL(SketchfabPlugin.Urls.reportAnIssue);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Login", GUILayout.Width(150), GUILayout.Height(25)))
        {
            _api.AuthenticateUser(user_name, user_password);
            EditorPrefs.SetString(usernameEditorKey, user_name);
        }

        GUILayout.EndHorizontal();
    }

    private bool CheckFileSize(string zipPath)
    {
        FileInfo file = new FileInfo(zipPath);
        status = "Uploading " + file.Length / (1024.0f * 1024.0f);
        return file.Length < _api.GetCurrentUserMaxAllowedUploadSize();
    }

    private Dictionary<string, string> BuildParameterDict()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>()
        {
            ["name"] = param_name,
            ["description"] = param_description,
            ["tags"] = "unity unity3D " + param_tags,
            ["private"] = param_private ? "1" : "0",
            ["isPublished"] = param_autopublish ? "1" : "0"
        };
        if (param_private)
            parameters["password"] = param_password;

        return parameters;
    }

    void OnDestroy()
    {
        if (System.IO.File.Exists(zipPath))
            System.IO.File.Delete(zipPath);
    }
}

#endif