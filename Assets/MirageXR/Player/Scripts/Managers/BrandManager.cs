using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class BrandManager : MonoBehaviour
    {
        [SerializeField] private string defaultMoodleUrl = "https://learn.wekit-ecs.com";
        [SerializeField] private string defaultXApiUrl = "https://lrs.wekit-ecs.com/data/xAPI";

        [SerializeField] private bool prefabsOriginalColors;
        [SerializeField] private Color defaultPrimaryColor;
        [SerializeField] private Color defaultSecondaryColor;
        [SerializeField] private Color defaultTextColor;
        [SerializeField] private Color defaultIconColor;
        [SerializeField] private Color defaultUIPathColor;
        [SerializeField] private Color defaultNextPathColor;

        public static BrandManager Instance { get; private set; }

        public Color DefaultSecondaryColor => defaultSecondaryColor;

#if UNITY_ANDROID || UNITY_IOS

        private const string augmentationsListFile = "MobileAugmentationListFile";
#else
        private const string AugmentationsListFile = "HololensAugmentationListFile";
#endif

        public string MoodleUrl => !prefabsOriginalColors ? _newMoodleUrl : defaultMoodleUrl;

        public string XApiUrl => !prefabsOriginalColors ? _newXApiUrl : defaultXApiUrl;

        public Color PrimaryColor => !prefabsOriginalColors ? _newPrimaryColor : defaultPrimaryColor;

        public Color SecondaryColor => !prefabsOriginalColors ? _newSecondaryColor : defaultSecondaryColor;

        public Color TextColor => !prefabsOriginalColors ? _newTextColor : defaultTextColor;

        public Color IconColor => !prefabsOriginalColors ? _newIconColor : defaultIconColor;

        public Color TaskStationColor => !prefabsOriginalColors ? _newTaskStationColor : defaultSecondaryColor;

        public Color UIPathColor => !prefabsOriginalColors ? _newUIPathColor : defaultUIPathColor;

        public Color NextPathColor => !prefabsOriginalColors ? _newNextPathColor : defaultNextPathColor;

        private string _newMoodleUrl;
        private string _newXApiUrl;

        private Color _newPrimaryColor;
        private Color _newSecondaryColor;
        private Color _newTextColor;
        private Color _newIconColor;
        private Color _newTaskStationColor;
        private Color _newUIPathColor;
        private Color _newNextPathColor;

        public bool Customizable { get; private set; }

        /// <summary>
        /// Apply the custom colors to the app
        /// </summary>
        public void AddCustomColors()
        {
            ChangePrimaryAndIconColor();
            ChangeSecondaryColors();
            ChangeTextsColor();
        }



        /// <summary>
        /// Get the list of augmentations
        /// </summary>
        /// <returns>a list of ContentType</returns>
        public List<ContentType> GetListOfAugmentations()
        {
            var listOfAugmentations = Enum.GetValues(typeof(ContentType)).OfType<ContentType>().ToList();
            var augmentationListFile = Resources.Load<TextAsset>(augmentationsListFile);
            if (augmentationListFile != null)
            {
                var arrayOfAugmentations = augmentationListFile.ToString().Split('\n');
                var contentTypeList = arrayOfAugmentations
                    .Select(str => (ContentType?)ContentTypeExtenstion.ParsePredicate(str))
                    .Where(ct => ct.Value != ContentType.UNKNOWN)
                    .Select(ct => ct.Value)
                    .ToList();

                return contentTypeList;
            }

            return listOfAugmentations.Where(ct => ct != ContentType.UNKNOWN).ToList();
        }



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
            if (ConfigParser.ConfigFile == null)
            {
                prefabsOriginalColors = true;
                return;
            }

            Customizable = true;

            LoadConfiguration();
            DontDestroyOnLoad(gameObject);

            WaitForActivityList();
        }


        private async void WaitForActivityList()
        {
            if (!Customizable)
            {
                return;
            }

            await RootObject.Instance.moodleManager.GetArlemList();

            AddCustomColors();
        }


        private void ChangePrimaryAndIconColor()
        {
            foreach (var img in FindObjectsOfType<Image>())
            {
                //not effect on buttons
                if (img.GetComponent<Button>())
                {
                    continue;
                }

                if (img.color == defaultPrimaryColor)
                {
                    img.color = _newPrimaryColor;
                }
                else if (img.color == defaultSecondaryColor)
                {
                    img.color = _newSecondaryColor;
                }
                else if (img.color == defaultIconColor)
                {
                    img.color = _newIconColor;
                }
            }
        }


        private void ChangeSecondaryColors()
        {
            foreach (var btn in FindObjectsOfType<Button>())
            {
                var colors = btn.colors;
                if (colors.highlightedColor == defaultSecondaryColor)
                {
                    colors.highlightedColor = _newSecondaryColor;
                    const float factor = 0.7f;
                    var darkerColor = new Color(_newSecondaryColor.r * factor, _newSecondaryColor.g * factor, _newSecondaryColor.b * factor, _newSecondaryColor.a);
                    colors.pressedColor = darkerColor;
                    colors.selectedColor = _newSecondaryColor;
                    btn.colors = colors;
                }
            }
        }


        private void ChangeTextsColor()
        {
            foreach (var txt in FindObjectsOfType<Text>())
            {
                if (txt.color == defaultTextColor)
                {
                    txt.color = _newTextColor;
                }
            }
        }



        private void LoadConfiguration()
        {
            _newMoodleUrl = ConfigParser.MoodleUrl;
            _newXApiUrl = ConfigParser.XApiUrl;
            _newPrimaryColor = ConfigParser.Editor.StringToColor(ConfigParser.PrimaryColor);
            _newSecondaryColor = ConfigParser.Editor.StringToColor(ConfigParser.SecondaryColor);
            _newTextColor = ConfigParser.Editor.StringToColor(ConfigParser.TextColor);
            _newIconColor = ConfigParser.Editor.StringToColor(ConfigParser.IconColor);
            _newTaskStationColor = ConfigParser.Editor.StringToColor(ConfigParser.TaskStationColor);
            _newUIPathColor = ConfigParser.Editor.StringToColor(ConfigParser.UIPathColor);
            _newNextPathColor = ConfigParser.Editor.StringToColor(ConfigParser.NextPathColor);
        }
    }
}