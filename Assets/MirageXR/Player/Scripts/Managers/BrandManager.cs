using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public static BrandManager Instance;
    
        private Color newPrimaryColor;
        private Color newSecondaryColor;
        private Color newTextColor;
        private Color newIconColor;
        private Color newTaskStationColor;
        private Color newUIPathColor;
        private Color newNextPathColor;

        public bool Customizable { get; private set; }

        ConfigEditor CFEditor = new ConfigEditor();

        public Color DefaultSecondaryColor => defaultSecondaryColor;


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void Start()
        {

            if (prefabsOriginalColors) return;

            //if config file doen't exist, disable color customization
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

        async void WaitForActivityList()
        {
            if (Customizable)
            {
                await MoodleManager.Instance.GetArlemList();

                AddCustomColors();
            }
        }

        void ChangePrimaryAndIconColor()
        {
            foreach (Image img in FindObjectsOfType<Image>())
            {
                if (img.GetComponent<Button>()) continue; //not effect on buttons

                if (img.color == defaultPrimaryColor && newPrimaryColor != null)
                    img.color = newPrimaryColor;

                else if (img.color == defaultSecondaryColor && newSecondaryColor != null)
                    img.color = newSecondaryColor;

                else if (img.color == defaultIconColor && newIconColor != null)
                    img.color = newIconColor;
            }
        }


        void ChangeSecodaryColors()
        {
            foreach (Button btn in FindObjectsOfType<Button>())
            {
                ColorBlock colors = btn.colors;
                if (colors.highlightedColor == defaultSecondaryColor && newSecondaryColor != null)
                {
                    colors.highlightedColor = newSecondaryColor;
                    var factor = 0.7f;
                    Color darkerColor = new Color(newSecondaryColor.r * factor, newSecondaryColor.g * factor, newSecondaryColor.b * factor, newSecondaryColor.a);
                    colors.pressedColor = darkerColor;
                    colors.selectedColor = newSecondaryColor;
                    btn.colors = colors;
                }
            }
        }


        void ChangeTextsColor()
        {
            foreach (Text txt in FindObjectsOfType<Text>())
            {
                if (txt.color == defaultTextColor && newTextColor != null)
                    txt.color = newTextColor;
            }
        }


        public void AddCustomColors()
        {
            ChangePrimaryAndIconColor();
            ChangeSecodaryColors();
            ChangeTextsColor();
        }



        public Color GetPrimaryColor()
        {
            return !prefabsOriginalColors ? newPrimaryColor : defaultPrimaryColor;
        }


        public Color GetSecondaryColor()
        {
            return !prefabsOriginalColors ? newSecondaryColor: defaultSecondaryColor;
        }

        public Color GetTextColor()
        {
            return !prefabsOriginalColors ? newTextColor: defaultTextColor;
        }

        public Color GetIconColor()
        {
            return !prefabsOriginalColors ? newIconColor : defaultIconColor;
        }

        public Color GetTaskStationColor()
        {
            return !prefabsOriginalColors ? newTaskStationColor: defaultSecondaryColor;
        }

        public Color GetUIPathColor()
        {
            return !prefabsOriginalColors ? newUIPathColor : defaultUIPathColor;
        }


        public Color GetNextPathColor()
        {
            return !prefabsOriginalColors ? newNextPathColor : defaultNextPathColor;
        }


        void AutoLoad()
        {
            TextAsset ConfigFile = Resources.Load<TextAsset>(CFEditor.configFileName);

            List<string> ConfigItems = ConfigFile.text.Split(new[] { '\r', '\n' }).ToList();

            string color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("primaryColor")));
            newPrimaryColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("secondaryColor")));
            newSecondaryColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("textColor")));
            newTextColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("iconColor")));
            newIconColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("taskStationColor")));
            newTaskStationColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("pathColor")));
            newUIPathColor = CFEditor.StringToColor(color);

            color = CFEditor.GetValue(ConfigItems.Find(x => x.StartsWith("nextPathColor")));
            newNextPathColor = CFEditor.StringToColor(color);
        }

    }
}