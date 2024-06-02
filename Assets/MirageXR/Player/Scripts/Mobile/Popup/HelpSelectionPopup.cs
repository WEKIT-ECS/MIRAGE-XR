using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MirageXR
{
    /// <summary>
    /// Popup that presents instructions to the user in the mobile version
    /// of the tutorial. Uses the PopupBase to do so.
    /// </summary>
    public class HelpSelectionPopup : PopupBase
    {
        private const string FILE_NAME_ACTIVITY_SELECTION = "activitySelectionHelp.json";
        private const string FILE_NAME_ACTIVITY_STEPS = "activityStepsHelp.json";
        private const string FILE_NAME_ACTIVITY_INFO = "activityStepsInfo.json";
        private const string FILE_NAME_ACTIVITY_CALIBRATION = "activityCalibrationHelp.json";
        private const string FILE_NAME_STEP_AUGMENTATIONS = "stepAugmentationsHelp.json";
        private const string FILE_NAME_STEP_INFO = "stepInfoHelp.json";

        [SerializeField] private Button _selectionButton;
        [SerializeField] private Button _btnClose;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _btnClose.onClick.AddListener(Close);
        }

        public void LoadHelpSelection(RootView_v2.HelpPage page, bool editModeOn)
        {
            string neededFile = "";
            switch (page)
            {
                case RootView_v2.HelpPage.Home:
                    neededFile = FILE_NAME_ACTIVITY_SELECTION;
                    break;
                case RootView_v2.HelpPage.ActivitySteps:
                    neededFile = FILE_NAME_ACTIVITY_STEPS;
                    break;
                case RootView_v2.HelpPage.ActivityInfo:
                    neededFile = FILE_NAME_ACTIVITY_INFO;
                    break;
                case RootView_v2.HelpPage.ActivityCalibration:
                    neededFile = FILE_NAME_ACTIVITY_CALIBRATION;
                    break;
                case RootView_v2.HelpPage.ActionAugmentations:
                    neededFile = FILE_NAME_STEP_AUGMENTATIONS;
                    break;
                case RootView_v2.HelpPage.ActionInfo:
                    neededFile = FILE_NAME_STEP_INFO;
                    break;
                case RootView_v2.HelpPage.ActionMarker:
                    // TODO: maybe add something here?
                    break;
            }

            List<HelpSelectionModel> helpSelectionList = new List<HelpSelectionModel>();
            if (neededFile.Length > 0)
            {

                try
                {
                    string path = Path.Combine(Application.dataPath, "MirageXR", "Resources", neededFile);
                    string jsonString = File.ReadAllText(path);

                    helpSelectionList = JsonConvert.DeserializeObject<List<HelpSelectionModel>>(jsonString);
                    foreach (HelpSelectionModel model in helpSelectionList)
                    {
                        if (model.EditModeOnly && !editModeOn)
                        {
                            continue;
                        }

                        var button = CreateNewSelectionButton(model.SelectionText);
                        if (model.StartsTutorial.Count() > 0)
                        {
                            switch (model.StartsTutorial)
                            {
                                case "mobile_editing":
                                    button.onClick.AddListener(startMobileEditingTutorial);
                                    break;
                                case "mobile_viewing":
                                    button.onClick.AddListener(startMobileViewingTutorial);
                                    break;
                            }
                        }
                        else
                        {
                            button.onClick.AddListener(() => ShowShortMessageSequence(model.TutorialSteps));
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    Debug.LogError("File not found while loading help popup: " + e.FileName);
                }
                catch (JsonException e)
                {
                    Debug.LogError("JSON parsing error while loading help popup: " + e.Message);
                }
                catch (Exception e)
                {
                    Debug.LogError("An unexpected error occurred while loading help popup: " + e.Message);
                }
            }
        }

        public Button CreateNewSelectionButton(string title)
        {
            var button = Instantiate(_selectionButton, Vector3.zero, Quaternion.identity);
            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.SetParent(gameObject.transform);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 40);
            rectTransform.localScale = new Vector3(1, 1, 1);
            rectTransform.localPosition = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);
            rectTransform.GetComponent<TMP_Text>().text = title;

            return button;
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        private void ShowShortMessageSequence(List<TutorialModel> steps)
        {
            this.Close();
            var queue = new Queue<TutorialModel>(steps);
            TutorialManager.Instance.MobileTutorial.Show(queue);
        }

        private void startMobileEditingTutorial()
        {
            this.Close();
            TutorialManager.Instance.StartNewMobileEditingTutorial();
        }

        private void startMobileViewingTutorial()
        {
            this.Close();
            TutorialManager.Instance.StartNewMobileViewingTutorial();
        }
    }
}
