using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class BrandConfiguration : EditorWindow
{
    #region App Settings Variables
    string compName = "";
    string prodName = "";
    string version = "";
    string termsOfUse = "";
    #endregion

    #region Splash and icons Settings Variables
    Texture2D SplashScreen;
    Texture2D logo;
    Color splashBGColor;
    #endregion

    #region Playmode UI settings variables
    Color UIPrimaryColor;
    Color UISecondaryColor;
    Color UITextColor;
    Color UIIconColor;
    Color TaskStationColor;
    Color PathColor;
    Color NextPathColor;
    #endregion

#if UNITY_ANDROID || UNITY_IOS

    private readonly string[] spareListOfAugmentations = { "image", "video", "audio", "ghost", "label", "act", "vfx",  "model", "character", "pickandplace", "imagemarker", "plugin", "erobson" };
    private const string augmentationsListFile = "MobileAugmentationListFile";
#else
    private readonly string[] spareListOfAugmentations = { "image", "video", "audio", "ghost", "label", "act", "vfx", "model", "character", "pick&place", "image marker", "plugin", "drawing", "erobson" };
    private const string augmentationsListFile = "HololensAugmentationListFile";
#endif

    //the foldout menus are open by default
    bool showAppSetting = true;
    bool showSplashAndIconSetting = true;
    bool infoSection = true;
    bool uiStyleSetting = true;

    Dictionary<string, bool> augmentations = new Dictionary<string, bool>();

    ConfigEditor CFEditor = new ConfigEditor();

    Vector2 windowScrollPosition;
    Vector2 textareaScrollPosition;

    //for temperory check of change
    string termOfUseData;

    //The gap from right side of the window to the fields
    int windowRightOffset = 20;

    [MenuItem("Brand Manager/Settings")]
    static void Init()
    {
        var window = GetWindow<BrandConfiguration>();
        window.InitiateConfigFile();
        window.AutoLoad();
        window.Show();
    }


    void OnGUI()
    {
        //the height of each foldout menu
        int layoutStartY = 10;
        int infoY = 80;
        int appSettingY = 770;
        int splashSettingT = 290;

        //create the foldout menu
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;

        windowScrollPosition = EditorGUILayout.BeginScrollView(windowScrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(1500));


        infoSection = EditorGUI.Foldout(new Rect(5, layoutStartY, position.width - windowRightOffset, 15), infoSection, "Info", foldoutStyle);
        if (infoSection)
        {
            //location labels
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.white;
            EditorGUI.LabelField(new Rect(5, layoutStartY + 20, position.width, 40),
            "Files location:\nConfig File: " + CFEditor.ConfigFilePath() + "\nDefault terms of use: " + CFEditor.TermsOfUseDefaultFilePath(), labelStyle);
        }
        else
            infoY = 30;

        var myStartY = layoutStartY + infoY;
        //App general settings
        showAppSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - windowRightOffset, 15), showAppSetting, "App Setting", foldoutStyle);
        if (showAppSetting)
        {
            compName = EditorGUI.TextField(new Rect(3, myStartY + 20, position.width - windowRightOffset, 20), "Company Name", compName);
            prodName = EditorGUI.TextField(new Rect(3, myStartY + 45, position.width - windowRightOffset, 20), "Product Name", prodName);
            version = EditorGUI.TextField(new Rect(3, myStartY + 70, position.width - windowRightOffset, 20), "Version", version);

            //Available augmentation
            GUILayout.BeginArea(new Rect(3, myStartY + 100, position.width - windowRightOffset, 370));
            GUILayout.Label("Available Augmentations");
            var counter = 0;
            foreach (var augmentation in spareListOfAugmentations)
            {
                bool toggleValue;
                if (augmentations.TryGetValue(augmentation, out toggleValue))
                {
                }
                augmentations[augmentation] = EditorGUI.Toggle(new Rect(5, 30 + (counter * 25), position.width - windowRightOffset, 15), augmentation, toggleValue);
                counter++;
            }
            GUILayout.EndArea();

            ///terms of use
            //if user file not exist load default term of use file
            termOfUseData = string.Empty;
            if (File.Exists(CFEditor.TermsOfUseUserFilePath()))
                termOfUseData = File.ReadAllText(CFEditor.TermsOfUseUserFilePath());
            else
                termOfUseData = File.ReadAllText(CFEditor.TermsOfUseDefaultFilePath());

            
            GUILayout.BeginArea(new Rect(3, myStartY + 490, position.width - windowRightOffset, 245));

            GUILayout.BeginHorizontal();
                GUILayout.Label("Terms of use");
                if (GUILayout.Button("Reset Terms of Use"))
                {
                    File.Delete(CFEditor.TermsOfUseUserFilePath());
                    termOfUseData = File.ReadAllText(CFEditor.TermsOfUseDefaultFilePath());
                }
            GUILayout.EndHorizontal();
            EditorGUI.TextArea(new Rect(3, 20 , position.width - windowRightOffset, 45), "Available Tags:<size><color><b><i>\nExample:<size=14></size><color=#ff0000ff></color>\nRemember to close tags", GUI.skin.GetStyle("HelpBox"));
            EditorGUILayout.Space(45);
            textareaScrollPosition = GUILayout.BeginScrollView(textareaScrollPosition, false, true, GUILayout.Width(position.width - windowRightOffset), GUILayout.Height(180));
            termsOfUse = EditorGUILayout.TextArea(termOfUseData, GUILayout.ExpandHeight(true));
            //GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            GUILayout.EndScrollView();
            EditorGUILayout.Space(20);
        }
        else
            appSettingY = 30;

        //Splash and icons settings
        myStartY = layoutStartY + infoY + appSettingY;
        showSplashAndIconSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - windowRightOffset, 15), showSplashAndIconSetting, "Splash and Icons Setting", foldoutStyle);
        if (showSplashAndIconSetting)
        {
            SplashScreen = CreateSplashTextureField("Splash Screen", SplashScreen, myStartY + 30);

            logo = CreateSplashTextureField("Logo", logo, myStartY + 125);

            EditorGUI.TextArea(new Rect(5, myStartY + 220, position.width - windowRightOffset, 20), "Background color when no background image is used.", GUI.skin.GetStyle("HelpBox"));
            splashBGColor = EditorGUI.ColorField(new Rect(5, myStartY + 245, position.width - windowRightOffset, 15), "Splash Background Color", splashBGColor);
        }
        else
            splashSettingT = 30;

        //UI settings at playmode
        myStartY = layoutStartY + infoY + appSettingY + splashSettingT;
        uiStyleSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - windowRightOffset, 15), uiStyleSetting, "User Interface(UI) Setting", foldoutStyle);
        if (uiStyleSetting)
        {
            EditorGUI.TextArea(new Rect(5, myStartY + 30, position.width - windowRightOffset, 20), "OBS! Remember to adjust alpha channels too.", GUI.skin.GetStyle("HelpBox"));
            UIPrimaryColor = EditorGUI.ColorField(new Rect(5, myStartY + 50, position.width - windowRightOffset, 15), "UI Primary Color", UIPrimaryColor);
            UISecondaryColor = EditorGUI.ColorField(new Rect(5, myStartY + 70, position.width - windowRightOffset, 15), "UI Secondary Color", UISecondaryColor);
            UITextColor = EditorGUI.ColorField(new Rect(5, myStartY + 90, position.width - windowRightOffset, 15), "UI Text Color", UITextColor);
            UIIconColor = EditorGUI.ColorField(new Rect(5, myStartY + 110, position.width - windowRightOffset, 15), "UI Icon Color", UIIconColor);
            TaskStationColor = EditorGUI.ColorField(new Rect(5, myStartY + 130, position.width - windowRightOffset, 15), "TaskStation & Active path Color", TaskStationColor);
            PathColor = EditorGUI.ColorField(new Rect(5, myStartY + 150, position.width - windowRightOffset, 15), "Path Color", PathColor);
            NextPathColor = EditorGUI.ColorField(new Rect(5, myStartY + 170, position.width - windowRightOffset, 15), "Next Path Color", NextPathColor);
        }


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            //if the file is deleted and the inspector is still open create the file again
            string configFilePath = CFEditor.ConfigFilePath();
            if (!File.Exists(configFilePath))
                InitiateConfigFile();

            SaveAugmentationSetting();
            AutoSave();
            AssetDatabase.Refresh();
        }
            

    }

    private void SaveAugmentationSetting()
    {
        var path = $"{Application.dataPath}/MirageXR/Resources/{augmentationsListFile}.txt";

        //Create the file if not exist
        if (!File.Exists(path)) { File.Create(path).Dispose(); }

        //Read the lines
        var poiLists = File.ReadAllLines(path).ToList();

        foreach (KeyValuePair<string, bool> augmentation in augmentations)
        {
            if (augmentation.Value && !poiLists.Contains(augmentation.Key))
            {
                poiLists.Add(augmentation.Key);
            }
            if (!augmentation.Value && poiLists.Contains(augmentation.Key))
            {
                poiLists.Remove(augmentation.Key);
            }
        }

        WriteAllLinesWithoutBlank(path, poiLists.ToArray());
    }


    private void LoadAugmentationSetting()
    {
        var path = $"{Application.dataPath}/MirageXR/Resources/{augmentationsListFile}.txt";

        if (File.Exists(path))
        {
            foreach (var augmentation in File.ReadAllLines(path))
            {
                augmentations[augmentation] = true;
            }
        }
    }

    public static void WriteAllLinesWithoutBlank(string path, params string[] lines)
    {
        if (path == null)
            throw new ArgumentNullException("path");
        if (lines == null)
            throw new ArgumentNullException("lines");

        using (var stream = File.OpenWrite(path))
        {
            stream.SetLength(0);
            using (var writer = new StreamWriter(stream))
            {
                if (lines.Length > 0)
                {
                    for (var i = 0; i < lines.Length - 1; i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                    writer.Write(lines[lines.Length - 1]);
                }
            }
        }
    }

    public void InitiateConfigFile()
    {

        string path = CFEditor.ConfigFilePath();

        if (!Directory.Exists(CFEditor.configFileDir))
            Directory.CreateDirectory(CFEditor.configFileDir);

        if (!File.Exists(path))
            File.Create(path).Dispose();

        List<string> lines = CFEditor.ReadConfigFile();

        if (lines.Find(x => x.StartsWith("companyName")) == null)
            lines.Add("companyName:" + PlayerSettings.companyName);

        if (lines.Find(x => x.StartsWith("productName")) == null)
            lines.Add("productName:" + PlayerSettings.productName);

        if (lines.Find(x => x.StartsWith("version")) == null)
            lines.Add("version:" + PlayerSettings.bundleVersion);

        if (!File.Exists(CFEditor.TermsOfUseUserFilePath()))
            termsOfUse = File.ReadAllText(CFEditor.TermsOfUseDefaultFilePath());

        if (lines.Find(x => x.StartsWith("splashScreen")) == null)
            lines.Add("splashScreen:" + AssetDatabase.GetAssetPath(PlayerSettings.virtualRealitySplashScreen));

        if (lines.Find(x => x.StartsWith("logo")) == null)
            lines.Add("logo:" + AssetDatabase.GetAssetPath(PlayerSettings.virtualRealitySplashScreen));

        if (lines.Find(x => x.StartsWith("SplashBackgroundColor")) == null)
            lines.Add("SplashBackgroundColor:" + CFEditor.ColorToString(new Color32(0, 0, 0, 255)));

        if (lines.Find(x => x.StartsWith("primaryColor")) == null)
            lines.Add("primaryColor:" + CFEditor.ColorToString(new Color32(46,196,182,255)));

        if (lines.Find(x => x.StartsWith("secondaryColor")) == null)
            lines.Add("secondaryColor:" + CFEditor.ColorToString(new Color32(255, 159, 28, 255)));

        if (lines.Find(x => x.StartsWith("textColor")) == null)
            lines.Add("textColor:" + CFEditor.ColorToString(Color.white));

        if (lines.Find(x => x.StartsWith("iconColor")) == null)
            lines.Add("iconColor:" + CFEditor.ColorToString(new Color32(255, 255, 255, 200)));

        if (lines.Find(x => x.StartsWith("taskStationColor")) == null)
            lines.Add("taskStationColor:" + CFEditor.ColorToString(new Color32(255, 159, 28, 255)));

        if (lines.Find(x => x.StartsWith("pathColor")) == null)
            lines.Add("pathColor:" + CFEditor.ColorToString(new Color32(255, 255, 255, 181)));

        if (lines.Find(x => x.StartsWith("nextPathColor")) == null)
            lines.Add("nextPathColor:" + CFEditor.ColorToString(new Color32(0, 255, 231, 200)));

        CFEditor.WriteConfigFile(lines);
    }


    void AutoSave()
    {
        PlayerSettings.companyName = compName;
        PlayerSettings.productName = prodName;
        PlayerSettings.bundleVersion = version;
        PlayerSettings.SplashScreen.backgroundColor = splashBGColor;

        CFEditor.EditLine("companyName", compName);
        CFEditor.EditLine("productName", prodName);
        CFEditor.EditLine("version", version);

        if(termsOfUse != "" && termsOfUse != termOfUseData)
            File.WriteAllText(CFEditor.TermsOfUseUserFilePath(), termsOfUse, Encoding.UTF8);

        CFEditor.EditLine("SplashBackgroundColor", CFEditor.ColorToString(splashBGColor));

        if (SplashScreen != null)
        {
            PlayerSettings.virtualRealitySplashScreen = SplashScreen;
            CFEditor.EditLine("splashScreen", AssetDatabase.GetAssetPath(SplashScreen));
        }

        if (logo != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { logo });
            CFEditor.EditLine("logo", AssetDatabase.GetAssetPath(logo));
        }

        CFEditor.EditLine("primaryColor", CFEditor.ColorToString(UIPrimaryColor));
        CFEditor.EditLine("secondaryColor", CFEditor.ColorToString(UISecondaryColor));
        CFEditor.EditLine("textColor", CFEditor.ColorToString(UITextColor));
        CFEditor.EditLine("iconColor", CFEditor.ColorToString(UIIconColor));
        CFEditor.EditLine("taskStationColor", CFEditor.ColorToString(TaskStationColor));
        CFEditor.EditLine("pathColor", CFEditor.ColorToString(PathColor));
        CFEditor.EditLine("nextPathColor", CFEditor.ColorToString(NextPathColor));
    }

    void AutoLoad()
    {
        List<string> ConfigItems = CFEditor.ReadConfigFile();

        compName = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("companyName")));
        prodName = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("productName")));
        version = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("version")));

        string splashPath = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("splashScreen")));
        SplashScreen = (Texture2D)AssetDatabase.LoadAssetAtPath(splashPath, typeof(Texture2D));
        PlayerSettings.virtualRealitySplashScreen = SplashScreen;

        string logoPath = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("logo")));
        logo = (Texture2D)AssetDatabase.LoadAssetAtPath(logoPath, typeof(Texture2D));
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { logo });

        var color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("SplashBackgroundColor")));
        splashBGColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("primaryColor")));
        UIPrimaryColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("secondaryColor")));
        UISecondaryColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("textColor")));
        UITextColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("iconColor")));
        UIIconColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("taskStationColor")));
        TaskStationColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("pathColor")));
        PathColor = CFEditor.StringToColor(color);

        color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("nextPathColor")));
        NextPathColor = CFEditor.StringToColor(color);

        LoadAugmentationSetting();
    }

    Texture2D CreateSplashTextureField(string name, Texture2D texture, int y)
    {

        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 90;
        var result = (Texture2D)EditorGUI.ObjectField(new Rect(3, y, position.width - windowRightOffset, 90), name, texture, typeof(Texture2D), false);
        return result;
    }
}
