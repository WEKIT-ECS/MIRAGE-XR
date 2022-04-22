/*
 * Copyright(c) 2017-2018 Sketchfab Inc.
 * License: https://github.com/sketchfab/UnityGLTF/blob/master/LICENSE
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Static data and assets related to the plugin
public class SketchfabPlugin
{
	public static string VERSION = "0.0.85a";

	public struct Urls
	{
		public static string server = "https://api.sketchfab.com/v3/";
		public static string latestRelease = "https://github.com/sketchfab/UnityGLTF/releases/latest";
		public static string resetPassword = "https://sketchfab.com/login/reset-password";
		public static string createAccount = "https://sketchfab.com/signup";
		public static string reportAnIssue = "https://help.sketchfab.com/hc/en-us/requests/new?type=exporters&subject=Unity+Exporter";
		public static string privateInfo = "https://help.sketchfab.com/hc/en-us/articles/115000422206-Private-Models";
		public static string draftInfo = "https://help.sketchfab.com/hc/en-us/articles/115000472906-Draft-Mode";
		public static string latestReleaseCheck = "https://api.github.com/repos/sketchfab/UnityGLTF/releases";
		public static string plans = "https://sketchfab.com/plans";
		public static string categories = server + "categories";
		public static string userMe = server + "me";
		public static string userAccount = server + "me/account";
		public static string modelUrl = server + "models";
		public static string searchUrl = server + "search";

	}

	// Fields limits
	public const int NAME_LIMIT = 48;
	public const int DESC_LIMIT = 1024;
	public const int TAGS_LIMIT = 50;
	public const int PASSWORD_LIMIT = 64;
	public const int SPACE_SIZE = 5;

	// UI Elements
	public static Color RED_COLOR= new Color(0.8f, 0.0f, 0.0f);
	public static Color BLUE_COLOR = new Color(69 / 255.0f, 185 / 255.0f, 223 / 255.0f);
	public static Color GREY_COLOR = Color.white;
	public static string CLICKABLE_COLOR = "navy";
	public static Texture2D HEADER;
	public static GUIStyle SkfbTextArea;
	public static GUIStyle SkfbLabel;
	public static GUIStyle SkfbClickableLabel;

	public static void Initialize()
	{
		if(HEADER == null)
			HEADER = Resources.Load<Texture2D>("SketchfabHeader");

	}

	public static void ShowHeader()
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(HEADER);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void CheckValidity(int deskSizeX = 600, int descSizeY = 175)
	{
		if (SkfbLabel == null)
		{
            SkfbLabel = new GUIStyle(GUI.skin.label)
            {
                richText = true
            };
        }

		if (SkfbTextArea == null)
		{
            SkfbTextArea = new GUIStyle(GUI.skin.textArea)
            {
                fixedWidth = deskSizeX,
                fixedHeight = descSizeY
            };
        }

		if (SkfbClickableLabel == null)
		{
            SkfbClickableLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                richText = true
            };
        }
	}

	public static string ClickableTextColor(string text)
	{
		return "<color=" + SketchfabPlugin.CLICKABLE_COLOR + ">" + text + "</color>";
	}

	public static void DisplayVersionPopup()
	{
		bool update = EditorUtility.DisplayDialog("Plugin update", "A new version is available \n(you have version " + VERSION + ")\nIt's strongly recommended that you update now. The latest version may include important bug fixes and improvements", "Update", "Skip");
		if (update)
		{
			Application.OpenURL(Urls.latestRelease);
		}
	}

	public static void ShowVersionChecking()
	{
		GUILayout.Label("Checking plugin version ...", EditorStyles.centeredGreyMiniLabel);
	}

	public static void ShowVersionCheckError()
	{
		Color current = GUI.color;
		GUI.color = Color.red;
		GUILayout.Label("An error occured when looking for the latest exporter version\nYou might be using an old and not fully supported version", EditorStyles.centeredGreyMiniLabel);
		if (GUILayout.Button("Click here to be redirected to release page"))
		{
			Application.OpenURL(Urls.latestRelease);
		}
		GUI.color = current;
	}

	public static void ShowOutdatedVersionWarning(string latestVersion)
	{
		Color current = GUI.color;
		GUI.color = RED_COLOR;
		GUILayout.Label("New version " + latestVersion + " available (current version is " + VERSION + ")", EditorStyles.centeredGreyMiniLabel);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Go to release page", GUILayout.Width(150), GUILayout.Height(25)))
		{
			Application.OpenURL(Urls.latestRelease);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.color = current;
	}

	public static void ShowUpToDate(string latestVersion)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Exporter is up to date (version:" + latestVersion + ")", EditorStyles.centeredGreyMiniLabel);

		GUILayout.FlexibleSpace();
		if (GUILayout.Button(ClickableTextColor("Help"), SkfbClickableLabel, GUILayout.Height(20)))
		{
			Application.OpenURL(Urls.latestRelease);
		}

		if (GUILayout.Button(ClickableTextColor("Report an issue"), SkfbClickableLabel, GUILayout.Height(20)))
		{
			Application.OpenURL(Urls.reportAnIssue);
		}
		GUILayout.EndHorizontal();
	}
}
#endif