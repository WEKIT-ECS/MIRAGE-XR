using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class BrandManager : MonoBehaviour
    {
        [SerializeField] private bool prefabsOriginalColors;
        [SerializeField] private Color defaultPrimaryColor;
        [SerializeField] private Color defaultSecondaryColor;
        [SerializeField] private Color defaultTextColor;
        [SerializeField] private Color defaultIconColor;
        [SerializeField] private Color defaultUIPathColor;
        [SerializeField] private Color defaultNextPathColor;

        public static BrandManager Instance { get; private set; }

        private Color _newPrimaryColor;
        private Color _newSecondaryColor;
        private Color _newTextColor;
        private Color _newIconColor;
        private Color _newTaskStationColor;
        private Color _newUIPathColor;
        private Color _newNextPathColor;

        public bool Customizable { get; private set; }

        private readonly ConfigEditor CFEditor = new();

        public Color DefaultSecondaryColor => defaultSecondaryColor;

#if UNITY_ANDROID || UNITY_IOS

        private readonly string[] spareListOfAugmentations = { "image", "video", "audio", "ghost", "label", "act", "effects", "model", "character", "pickandplace", "imagemarker", "plugin" };
        private const string augmentationsListFile = "MobileAugmentationListFile";
#else
        private readonly string[] spareListOfAugmentations = { "image", "video", "audio", "ghost", "label", "act", "effects", "model", "character", "pick&place", "image marker", "plugin", "drawing" };
        private const string augmentationsListFile = "HololensAugmentationListFile";
#endif

        public string[] SpareListOfAugmentations => spareListOfAugmentations;

        public string AugmentationsListFile => augmentationsListFile;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {

            if (prefabsOriginalColors)
            {
                return;
            }

            // if config file isn't exist, disable color customization
            if (Resources.Load<TextAsset>(CFEditor.configFileName) == null)
            {
                prefabsOriginalColors = true;
                return;
            }
            else
            {
                Customizable = true;
            }

            AutoLoad();
            DontDestroyOnLoad(gameObject);

            WaitForActivityList();
        }


        /// <summary>
        /// Get the list of pois from json file depends on platform
        /// </summary>
        public string[] GetListOfAugmentations()
        {
            // If the file missing, use the spare array in this class
            var listOfAugmentations = spareListOfAugmentations;

            var augmentationListFile = Resources.Load<TextAsset>(augmentationsListFile);
            if (augmentationListFile != null)
            {
                listOfAugmentations = augmentationListFile.ToString().Split('\n');
            }
            else
            {
                return spareListOfAugmentations;
            }

            // if the file is empty return the default array
            if (listOfAugmentations.Length == 0 || (listOfAugmentations.Length == 1 && listOfAugmentations[0] == ""))
            {
                return spareListOfAugmentations;
            }

            string[] augFinalList = new string[listOfAugmentations.Length];
            for (int i = 0; i < listOfAugmentations.Length; i++)
            {
                augFinalList[i] = listOfAugmentations[i].Replace("\r", string.Empty);
            }

            return augFinalList;
        }


        private async void WaitForActivityList()
        {
            if (Customizable)
            {
                await RootObject.Instance.moodleManager.GetArlemList();

                AddCustomColors();
            }
        }

        private void ChangePrimaryAndIconColor()
        {
            foreach (Image img in FindObjectsOfType<Image>())
            {
                if (img.GetComponent<Button>()) continue; //not effect on buttons

                if (img.color == defaultPrimaryColor && _newPrimaryColor != null)
                    img.color = _newPrimaryColor;

                else if (img.color == defaultSecondaryColor && _newSecondaryColor != null)
                    img.color = _newSecondaryColor;

                else if (img.color == defaultIconColor && _newIconColor != null)
                    img.color = _newIconColor;
            }
        }


        private void ChangeSecondaryColors()
        {
            foreach (Button btn in FindObjectsOfType<Button>())
            {
                ColorBlock colors = btn.colors;
                if (colors.highlightedColor == defaultSecondaryColor && _newSecondaryColor != null)
                {
                    colors.highlightedColor = _newSecondaryColor;
                    var factor = 0.7f;
                    Color darkerColor = new Color(_newSecondaryColor.r * factor, _newSecondaryColor.g * factor, _newSecondaryColor.b * factor, _newSecondaryColor.a);
                    colors.pressedColor = darkerColor;
                    colors.selectedColor = _newSecondaryColor;
                    btn.colors = colors;
                }
            }
        }


        private void ChangeTextsColor()
        {
            foreach (Text txt in FindObjectsOfType<Text>())
            {
                if (txt.color == defaultTextColor && _newTextColor != null)
                    txt.color = _newTextColor;
            }
        }


        public void AddCustomColors()
        {
            ChangePrimaryAndIconColor();
            ChangeSecondaryColors();
            ChangeTextsColor();
        }



        public Color GetPrimaryColor()
        {
            return !prefabsOriginalColors ? _newPrimaryColor : defaultPrimaryColor;
        }


        public Color GetSecondaryColor()
        {
            return !prefabsOriginalColors ? _newSecondaryColor : defaultSecondaryColor;
        }

        public Color GetTextColor()
        {
            return !prefabsOriginalColors ? _newTextColor : defaultTextColor;
        }

        public Color GetIconColor()
        {
            return !prefabsOriginalColors ? _newIconColor : defaultIconColor;
        }

        public Color GetTaskStationColor()
        {
            return !prefabsOriginalColors ? _newTaskStationColor : defaultSecondaryColor;
        }

        public Color GetUIPathColor()
        {
            return !prefabsOriginalColors ? _newUIPathColor : defaultUIPathColor;
        }


        public Color GetNextPathColor()
        {
            return !prefabsOriginalColors ? _newNextPathColor : defaultNextPathColor;
        }


        private void AutoLoad()
        {
            var configFile = Resources.Load<TextAsset>(CFEditor.configFileName);

            var configItems = configFile.text.Split(new[] { '\r', '\n' }).ToList();

            var properties = new Dictionary<string, Action<string>>
            {
                { "primaryColor", value => _newPrimaryColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "secondaryColor", value => _newSecondaryColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "textColor", value => _newTextColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "iconColor", value => _newIconColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "taskStationColor", value => _newTaskStationColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "pathColor", value => _newUIPathColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
                { "nextPathColor", value => _newNextPathColor = CFEditor.StringToColor(CFEditor.GetValue(value)) },
            };

            foreach (var property in properties)
            {
                var configItem = configItems.Find(x => x.StartsWith(property.Key));
                if (configItem != null)
                {
                    property.Value(configItem);
                }
            }
        }
    }
}