using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace MirageXR
{
    public class BrandConfiguration : EditorWindow
    {
        //The gap from right side of the window to the fields
        private readonly int _windowRightOffset = 20;
        private readonly Dictionary<string, bool> _augmentations = new ();
        private readonly ConfigEditor _cfEditor = new ();

        private string _compName = string.Empty;
        private string _prodName = string.Empty;
        private string _version = string.Empty;
        private string _termsOfUse = string.Empty;

        private Texture2D _splashScreen;
        private Texture2D _logo;
        private Color _splashBgColor;

        private Color _uiPrimaryColor;
        private Color _uiSecondaryColor;
        private Color _uiTextColor;
        private Color _uiIconColor;
        private Color _taskStationColor;
        private Color _pathColor;
        private Color _nextPathColor;

        //the fold out menus are open by default
        private bool _showAppSetting = true;
        private bool _showSplashAndIconSetting = true;
        private bool _infoSection = true;
        private bool _uiStyleSetting = true;

        private Vector2 _windowScrollPosition;
        private Vector2 _textareaScrollPosition;

        //for temporary check of change
        private string _termOfUseData;



        [MenuItem("Brand Manager/Settings")]
        private static void Init()
        {
            var window = GetWindow<BrandConfiguration>();
            window.InitiateConfigFile();
            window.AutoLoad();
            window.Show();
        }

        private void OnGUI()
        {
            //the height of each foldout menu
            const int layoutStartY = 10;
            var infoY = 80;
            var appSettingY = 770;
            var splashSettingT = 290;

            //create the foldout menu
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;

            _windowScrollPosition = EditorGUILayout.BeginScrollView(_windowScrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(1500));


            _infoSection = EditorGUI.Foldout(new Rect(5, layoutStartY, position.width - _windowRightOffset, 15), _infoSection, "Info", foldoutStyle);
            if (_infoSection)
            {
                //location labels
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.normal.textColor = Color.white;
                EditorGUI.LabelField(new Rect(5, layoutStartY + 20, position.width, 40),
                "Files location:\nConfig File: " + _cfEditor.ConfigFilePath() + "\nDefault terms of use: " + _cfEditor.TermsOfUseDefaultFilePath(), labelStyle);
            }
            else
            {
                infoY = 30;
            }

            var myStartY = layoutStartY + infoY;
            //App general settings
            _showAppSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - _windowRightOffset, 15), _showAppSetting, "App Setting", foldoutStyle);
            if (_showAppSetting)
            {
                _compName = EditorGUI.TextField(new Rect(3, myStartY + 20, position.width - _windowRightOffset, 20), "Company Name", _compName);
                _prodName = EditorGUI.TextField(new Rect(3, myStartY + 45, position.width - _windowRightOffset, 20), "Product Name", _prodName);
                _version = EditorGUI.TextField(new Rect(3, myStartY + 70, position.width - _windowRightOffset, 20), "Version", _version);

                //Available augmentation
                GUILayout.BeginArea(new Rect(3, myStartY + 100, position.width - _windowRightOffset, 370));
                GUILayout.Label("Available Augmentations");
                var counter = 0;
                foreach (var augmentation in BrandManager.Instance.SpareListOfAugmentations)
                {
                    if (_augmentations.TryGetValue(augmentation, out var toggleValue))
                    {
                    }
                    _augmentations[augmentation] = EditorGUI.Toggle(new Rect(5, 30 + (counter * 25), position.width - _windowRightOffset, 15), augmentation, toggleValue);
                    counter++;
                }
                GUILayout.EndArea();

                //terms of use
                //if user file not exist load default term of use file
                _termOfUseData = string.Empty;
                _termOfUseData = File.ReadAllText(File.Exists(_cfEditor.TermsOfUseUserFilePath()) ?
                    _cfEditor.TermsOfUseUserFilePath() : _cfEditor.TermsOfUseDefaultFilePath());


                GUILayout.BeginArea(new Rect(3, myStartY + 490, position.width - _windowRightOffset, 245));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Terms of use");
                if (GUILayout.Button("Reset Terms of Use"))
                {
                    File.Delete(_cfEditor.TermsOfUseUserFilePath());
                    _termOfUseData = File.ReadAllText(_cfEditor.TermsOfUseDefaultFilePath());
                }
                GUILayout.EndHorizontal();
                EditorGUI.TextArea(new Rect(3, 20, position.width - _windowRightOffset, 45), "Available Tags:<size><color><b><i>\nExample:<size=14></size><color=#ff0000ff></color>\nRemember to close tags", GUI.skin.GetStyle("HelpBox"));
                EditorGUILayout.Space(45);
                _textareaScrollPosition = GUILayout.BeginScrollView(_textareaScrollPosition, false, true, GUILayout.Width(position.width - _windowRightOffset), GUILayout.Height(180));
                _termsOfUse = EditorGUILayout.TextArea(_termOfUseData, GUILayout.ExpandHeight(true));
                //GUILayout.FlexibleSpace();
                GUILayout.EndArea();
                GUILayout.EndScrollView();
                EditorGUILayout.Space(20);
            }
            else
            {
                appSettingY = 30;
            }

            //Splash and icons settings
            myStartY = layoutStartY + infoY + appSettingY;
            _showSplashAndIconSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - _windowRightOffset, 15), _showSplashAndIconSetting, "Splash and Icons Setting", foldoutStyle);
            if (_showSplashAndIconSetting)
            {
                _splashScreen = CreateSplashTextureField("Splash Screen", _splashScreen, myStartY + 30);

                _logo = CreateSplashTextureField("Logo", _logo, myStartY + 125);

                EditorGUI.TextArea(new Rect(5, myStartY + 220, position.width - _windowRightOffset, 20), "Background color when no background image is used.", GUI.skin.GetStyle("HelpBox"));
                _splashBgColor = EditorGUI.ColorField(new Rect(5, myStartY + 245, position.width - _windowRightOffset, 15), "Splash Background Color", _splashBgColor);
            }
            else
            {
                splashSettingT = 30;
            }

            //UI settings at playmode
            myStartY = layoutStartY + infoY + appSettingY + splashSettingT;
            _uiStyleSetting = EditorGUI.Foldout(new Rect(5, myStartY, position.width - _windowRightOffset, 15), _uiStyleSetting, "User Interface(UI) Setting", foldoutStyle);
            if (_uiStyleSetting)
            {
                EditorGUI.TextArea(new Rect(5, myStartY + 30, position.width - _windowRightOffset, 20), "OBS! Remember to adjust alpha channels too.", GUI.skin.GetStyle("HelpBox"));
                _uiPrimaryColor = EditorGUI.ColorField(new Rect(5, myStartY + 50, position.width - _windowRightOffset, 15), "UI Primary Color", _uiPrimaryColor);
                _uiSecondaryColor = EditorGUI.ColorField(new Rect(5, myStartY + 70, position.width - _windowRightOffset, 15), "UI Secondary Color", _uiSecondaryColor);
                _uiTextColor = EditorGUI.ColorField(new Rect(5, myStartY + 90, position.width - _windowRightOffset, 15), "UI Text Color", _uiTextColor);
                _uiIconColor = EditorGUI.ColorField(new Rect(5, myStartY + 110, position.width - _windowRightOffset, 15), "UI Icon Color", _uiIconColor);
                _taskStationColor = EditorGUI.ColorField(new Rect(5, myStartY + 130, position.width - _windowRightOffset, 15), "TaskStation & Active path Color", _taskStationColor);
                _pathColor = EditorGUI.ColorField(new Rect(5, myStartY + 150, position.width - _windowRightOffset, 15), "Path Color", _pathColor);
                _nextPathColor = EditorGUI.ColorField(new Rect(5, myStartY + 170, position.width - _windowRightOffset, 15), "Next Path Color", _nextPathColor);
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                //if the file is deleted and the inspector is still open create the file again
                var configFilePath = _cfEditor.ConfigFilePath();
                if (!File.Exists(configFilePath))
                {
                    InitiateConfigFile();
                }

                SaveAugmentationSetting();
                AutoSave();
                AssetDatabase.Refresh();
            }
        }

        private void SaveAugmentationSetting()
        {
            var path = $"{Application.dataPath}/MirageXR/Resources/{BrandManager.Instance.AugmentationsListFile}.txt";

            //Create the file if not exist
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            //Read the lines
            var poiLists = File.ReadAllLines(path).ToList();

            foreach (var augmentation in _augmentations)
            {
                switch (augmentation.Value)
                {
                    case true when !poiLists.Contains(augmentation.Key):
                        poiLists.Add(augmentation.Key);
                        break;
                    case false when poiLists.Contains(augmentation.Key):
                        poiLists.Remove(augmentation.Key);
                        break;
                }
            }

            WriteAllLinesWithoutBlank(path, poiLists.ToArray());
        }


        private void LoadAugmentationSetting()
        {
            var path = $"{Application.dataPath}/MirageXR/Resources/{BrandManager.Instance.AugmentationsListFile}.txt";

            if (!File.Exists(path))
            {
                return;
            }

            foreach (var augmentation in File.ReadAllLines(path))
            {
                _augmentations[augmentation] = true;
            }
        }



        public static void WriteAllLinesWithoutBlank(string path, params string[] lines)
        {
            if (path == null)
            {
                throw new ArgumentNullException($"path");
            }
            if (lines == null)
            {
                throw new ArgumentNullException($"lines");
            }

            using var stream = File.OpenWrite(path);
            stream.SetLength(0);
            using var writer = new StreamWriter(stream);

            if (lines.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < lines.Length - 1; i++)
            {
                writer.WriteLine(lines[i]);
            }

            //Last line
            writer.Write(lines[^1]);
        }


        /// <summary>
        /// Initiating config file
        /// </summary>
        public void InitiateConfigFile()
        {
            var path = _cfEditor.ConfigFilePath();

            if (!Directory.Exists(_cfEditor.configFileDir))
            {
                Directory.CreateDirectory(_cfEditor.configFileDir);
            }

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            var lines = _cfEditor.ReadConfigFile();

            var properties = new Dictionary<string, string>
        {
            {"companyName", $"companyName:{PlayerSettings.companyName}"},
            {"productName", $"productName:{PlayerSettings.productName}"},
            {"version", $"version:{PlayerSettings.bundleVersion}"},
            {"splashScreen", $"splashScreen:{AssetDatabase.GetAssetPath(PlayerSettings.virtualRealitySplashScreen)}"},
            {"logo", $"logo:{AssetDatabase.GetAssetPath(PlayerSettings.virtualRealitySplashScreen)}"},
            {"SplashBackgroundColor", $"SplashBackgroundColor:{_cfEditor.ColorToString(new Color32(0, 0, 0, 255))}"},
            {"primaryColor", $"primaryColor:{_cfEditor.ColorToString(new Color32(46, 196, 182, 255))}"},
            {"secondaryColor", $"secondaryColor:{_cfEditor.ColorToString(new Color32(255, 159, 28, 255))}"},
            {"textColor", $"textColor:{_cfEditor.ColorToString(Color.white)}"},
            {"iconColor", $"iconColor:{_cfEditor.ColorToString(new Color32(255, 255, 255, 200))}"},
            {"taskStationColor", $"taskStationColor:{_cfEditor.ColorToString(new Color32(255, 159, 28, 255))}"},
            {"pathColor", $"pathColor:{_cfEditor.ColorToString(new Color32(255, 255, 255, 181))}"},
            {"nextPathColor", $"nextPathColor:{_cfEditor.ColorToString(new Color32(0, 255, 231, 200))}"},
        };

            foreach (var property in properties)
            {
                if (lines.Find(x => x.StartsWith(property.Key)) == null)
                {
                    lines.Add(property.Value);
                }
            }

            if (!File.Exists(_cfEditor.TermsOfUseUserFilePath()))
            {
                _termsOfUse = File.ReadAllText(_cfEditor.TermsOfUseDefaultFilePath());
            }

            _cfEditor.WriteConfigFile(lines);
        }


        private void AutoSave()
        {
            PlayerSettings.companyName = _compName;
            PlayerSettings.productName = _prodName;
            PlayerSettings.bundleVersion = _version;
            PlayerSettings.SplashScreen.backgroundColor = _splashBgColor;

            _cfEditor.EditLine("companyName", _compName);
            _cfEditor.EditLine("productName", _prodName);
            _cfEditor.EditLine("version", _version);

            if (_termsOfUse != string.Empty && _termsOfUse != _termOfUseData)
            {
                File.WriteAllText(_cfEditor.TermsOfUseUserFilePath(), _termsOfUse, Encoding.UTF8);
            }

            _cfEditor.EditLine("SplashBackgroundColor", _cfEditor.ColorToString(_splashBgColor));

            if (_splashScreen != null)
            {
                PlayerSettings.virtualRealitySplashScreen = _splashScreen;
                _cfEditor.EditLine("splashScreen", AssetDatabase.GetAssetPath(_splashScreen));
            }

            if (_logo != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { _logo });
                _cfEditor.EditLine("logo", AssetDatabase.GetAssetPath(_logo));
            }

            _cfEditor.EditLine("primaryColor", _cfEditor.ColorToString(_uiPrimaryColor));
            _cfEditor.EditLine("secondaryColor", _cfEditor.ColorToString(_uiSecondaryColor));
            _cfEditor.EditLine("textColor", _cfEditor.ColorToString(_uiTextColor));
            _cfEditor.EditLine("iconColor", _cfEditor.ColorToString(_uiIconColor));
            _cfEditor.EditLine("taskStationColor", _cfEditor.ColorToString(_taskStationColor));
            _cfEditor.EditLine("pathColor", _cfEditor.ColorToString(_pathColor));
            _cfEditor.EditLine("nextPathColor", _cfEditor.ColorToString(_nextPathColor));
        }


        private void AutoLoad()
        {
            var configItems = _cfEditor.ReadConfigFile();

            var properties = new Dictionary<string, Action<string>>
    {
        { "companyName", value => _compName = _cfEditor.GetValue(value) },
        { "productName", value => _prodName = _cfEditor.GetValue(value) },
        { "version", value => _version = _cfEditor.GetValue(value) },
        { "splashScreen", value => {
            var splashPath = _cfEditor.GetValue(value);
            _splashScreen = (Texture2D)AssetDatabase.LoadAssetAtPath(splashPath, typeof(Texture2D));
            PlayerSettings.virtualRealitySplashScreen = _splashScreen;
        }},
        { "logo", value => {
            var logoPath = _cfEditor.GetValue(value);
            _logo = (Texture2D)AssetDatabase.LoadAssetAtPath(logoPath, typeof(Texture2D));
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { _logo });
        }},
        { "SplashBackgroundColor", value => _splashBgColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "primaryColor", value => _uiPrimaryColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "secondaryColor", value => _uiSecondaryColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "textColor", value => _uiTextColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "iconColor", value => _uiIconColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "taskStationColor", value => _taskStationColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "pathColor", value => _pathColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
        { "nextPathColor", value => _nextPathColor = _cfEditor.StringToColor(_cfEditor.GetValue(value)) },
    };

            foreach (var property in properties)
            {
                string configItem = configItems.Find(x => x.StartsWith(property.Key));
                if (configItem != null)
                {
                    property.Value(configItem);
                }
            }

            LoadAugmentationSetting();
        }


        private Texture2D CreateSplashTextureField(string textureName, Texture2D texture, int y)
        {

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 90,
            };
            var result = (Texture2D)EditorGUI.ObjectField(new Rect(3, y, position.width - _windowRightOffset, 90), textureName, texture, typeof(Texture2D), false);
            return result;
        }
    }
}
